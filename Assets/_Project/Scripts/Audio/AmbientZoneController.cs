// AmbientZoneController.cs
// Sprint 6 Lane 4 — Singleton ambient zone manager.
//
// Owns three persistent AudioSources (PrimaryA, PrimaryB, Secondary) and
// cross-fades the live "primary" pair on every zone change. Secondary layer
// is mixed in/out independently. All fades use Time.unscaledDeltaTime so
// they continue while the game is paused (consistent with the Pause-menu
// dim-but-don't-cut policy in MixerSnapshotController).
//
// Coexists with:
//   * AdaptiveMusicController (MUSIC bus, RS-reactive, owns its own zone-ambient A/B)
//   * CymaticMusicEngine      (CYMATIC bus, 3-band drones — soft-bound via reflection)
//   * AmbientAudioZone, AmbienceZone (legacy single-clip triggers; safe to leave in scene)
//
// Reads no GameEvents; subscribers in MusicController/CymaticEngine already cover
// OnBuildingRestored / OnMoonCompleted. This controller is strictly trigger-driven.
//
// Public surface used by AmbientZoneTrigger:
//   Instance.EnterZone(profile, trigger)
//   Instance.ExitZone (profile, trigger)
//
// Zone stack semantics:
//   * Triggers can overlap. The MOST-RECENTLY-ENTERED profile is the live mix.
//   * On exit, the controller pops the stack and resumes the previous profile
//     (or fades to silence if the stack is empty).
//
// CLAUDE.md compliance notes:
//   * Uses UnityEngine.Time.unscaledDeltaTime (no namespace shadow).
//   * No silent catches anywhere — every guard logs WHY it bailed.
//   * Missing AudioClip refs log a warning naming the zoneId AND the expected path.
//   * No FindObjectOfType<T>() — uses FindFirstObjectByType<T>(FindObjectsInactive).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Singleton manager for Moon 1 ambient zones. Cross-fades a primary/secondary
    /// loop pair on player Enter / Exit of <see cref="AmbientZoneTrigger"/> volumes.
    /// </summary>
    [DisallowMultipleComponent]
    public class AmbientZoneController : MonoBehaviour
    {
        public static AmbientZoneController Instance { get; private set; }

        const float MASTER_AMBIENT_VOLUME = 1.0f;

        // A/B cross-fade pair for the primary layer.
        AudioSource _primaryA;
        AudioSource _primaryB;
        AudioSource _secondary;

        // Tracks which of A/B is currently the "live" source.
        bool _primaryUseA = true;

        // Currently-running cross-fade coroutines (one per layer).
        Coroutine _primaryFadeRoutine;
        Coroutine _secondaryFadeRoutine;

        // Stack of active profiles (LIFO). Top = current live mix.
        readonly List<AmbientZoneProfile> _activeStack = new List<AmbientZoneProfile>(8);

        // Maps trigger -> profile so re-entries from the same trigger replace cleanly.
        readonly Dictionary<int, AmbientZoneProfile> _triggerToProfile = new Dictionary<int, AmbientZoneProfile>(8);

        // Resolved mixer group (cached after first AudioManager probe).
        AudioMixerGroup _resolvedMixerGroup;
        bool _mixerGroupProbed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var existing = Object.FindFirstObjectByType<AmbientZoneController>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Instance = existing;
                return;
            }
            var go = new GameObject("AmbientZoneController");
            DontDestroyOnLoad(go);
            go.AddComponent<AmbientZoneController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[AmbientZone] Duplicate AmbientZoneController on '{name}' — destroying. Existing instance lives on '{Instance.name}'.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            EnsureSources();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Source creation ─────────────────────────

        void EnsureSources()
        {
            if (_primaryA == null) _primaryA = CreateSource("AmbientZone_PrimaryA", loop: true);
            if (_primaryB == null) _primaryB = CreateSource("AmbientZone_PrimaryB", loop: true);
            if (_secondary == null) _secondary = CreateSource("AmbientZone_Secondary", loop: true);
        }

        AudioSource CreateSource(string sourceName, bool loop)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.volume = 0f;
            src.spatialBlend = 0f;
            src.ignoreListenerPause = true; // honor unscaled-time fade-while-paused policy
            return src;
        }

        AudioMixerGroup ResolveMixerGroup(AmbientZoneProfile profile)
        {
            if (profile != null && profile.mixerGroupOverride != null)
                return profile.mixerGroupOverride;

            if (_mixerGroupProbed) return _resolvedMixerGroup;
            _mixerGroupProbed = true;
            var am = AudioManager.Instance;
            if (am == null)
            {
                Debug.LogWarning("[AmbientZone] AudioManager.Instance is null — ambient sources will play unrouted (Master bus). Cathedral/Mixer snapshot ducking will not apply.");
                _resolvedMixerGroup = null;
                return null;
            }
            _resolvedMixerGroup = am.AmbienceGroup;
            if (_resolvedMixerGroup == null)
            {
                Debug.LogWarning("[AmbientZone] AudioManager.AmbienceGroup is null — assign the Ambience mixer group in AudioManager's inspector. Falling back to unrouted playback.");
            }
            return _resolvedMixerGroup;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Player entered a trigger volume bound to <paramref name="profile"/>.
        /// Pushes the profile onto the live stack and cross-fades the mix.
        /// </summary>
        public void EnterZone(AmbientZoneProfile profile, AmbientZoneTrigger trigger)
        {
            if (profile == null)
            {
                Debug.LogError($"[AmbientZone] EnterZone called with null profile from trigger '{(trigger != null ? trigger.name : "<null>")}'. Fix the AmbientZoneTrigger inspector reference.");
                return;
            }

            if (trigger != null)
            {
                int id = trigger.GetInstanceID();
                if (_triggerToProfile.TryGetValue(id, out var existing) && existing == profile)
                {
                    // Re-fired enter on the same trigger (Unity can do this on collider toggles).
                    return;
                }
                _triggerToProfile[id] = profile;
            }

            if (profile.IsCompletelyEmpty)
            {
                Debug.LogWarning($"[AmbientZone] Zone profile '{profile.zoneId}' has neither primary nor secondary clip — entering will fade ambient to silence. Assign primaryClip in the inspector or check the expected path: '{profile.primaryExpectedPath}'.");
            }

            // Re-stack: remove any existing instance of this profile, push to top.
            _activeStack.Remove(profile);
            _activeStack.Add(profile);

            ApplyLiveProfile(profile);

            if (profile.activateTelluricOnEnter)
            {
                InvokeTelluricTieIn(profile.zoneId);
            }

            float liveLoopTime = GetLiveLoopTime();
            string primaryName = profile.primaryClip != null ? profile.primaryClip.name : "<none>";
            string secondaryName = profile.secondaryClip != null ? profile.secondaryClip.name : "<none>";
            Debug.Log($"[AmbientZone] Entered {profile.zoneId}, mix={primaryName} {secondaryName} loop_time={liveLoopTime:F2}");
        }

        /// <summary>
        /// Player exited a trigger volume bound to <paramref name="profile"/>. Pops the
        /// profile from the stack and either resumes the next-down profile or fades to silence.
        /// </summary>
        public void ExitZone(AmbientZoneProfile profile, AmbientZoneTrigger trigger)
        {
            if (profile == null)
            {
                Debug.LogError($"[AmbientZone] ExitZone called with null profile from trigger '{(trigger != null ? trigger.name : "<null>")}'.");
                return;
            }

            if (trigger != null)
            {
                int id = trigger.GetInstanceID();
                if (_triggerToProfile.TryGetValue(id, out var bound) && bound == profile)
                    _triggerToProfile.Remove(id);
            }

            bool removed = _activeStack.Remove(profile);
            if (!removed)
            {
                // Exit without enter — log so we notice mismatched colliders.
                Debug.LogWarning($"[AmbientZone] ExitZone '{profile.zoneId}' but profile was not on the active stack. Trigger '{(trigger != null ? trigger.name : "<null>")}' may have fired Exit without Enter.");
                return;
            }

            var next = _activeStack.Count > 0 ? _activeStack[_activeStack.Count - 1] : null;
            if (next != null)
            {
                ApplyLiveProfile(next);
                float liveLoopTime = GetLiveLoopTime();
                string primaryName = next.primaryClip != null ? next.primaryClip.name : "<none>";
                string secondaryName = next.secondaryClip != null ? next.secondaryClip.name : "<none>";
                Debug.Log($"[AmbientZone] Exited {profile.zoneId} -> resumed {next.zoneId}, mix={primaryName} {secondaryName} loop_time={liveLoopTime:F2}");
            }
            else
            {
                FadeAllToSilence(profile.crossfadeSeconds);
                Debug.Log($"[AmbientZone] Exited {profile.zoneId}, mix=<silence> <silence> loop_time=0.00 (stack empty — fading to silence)");
            }
        }

        // ─── Internal fade plumbing ──────────────────

        void ApplyLiveProfile(AmbientZoneProfile profile)
        {
            EnsureSources();

            var mixerGroup = ResolveMixerGroup(profile);
            if (_primaryA  != null) _primaryA.outputAudioMixerGroup  = mixerGroup;
            if (_primaryB  != null) _primaryB.outputAudioMixerGroup  = mixerGroup;
            if (_secondary != null) _secondary.outputAudioMixerGroup = mixerGroup;

            // ─ Primary layer ─
            if (profile.primaryClip == null)
            {
                Debug.LogWarning($"[AmbientZone] Zone '{profile.zoneId}' primaryClip is null. Expected at '{profile.primaryExpectedPath}'. Fading primary layer to silence.");
                if (_primaryFadeRoutine != null) StopCoroutine(_primaryFadeRoutine);
                _primaryFadeRoutine = StartCoroutine(CrossfadePrimary(null, 0f, profile.crossfadeSeconds));
            }
            else
            {
                if (_primaryFadeRoutine != null) StopCoroutine(_primaryFadeRoutine);
                _primaryFadeRoutine = StartCoroutine(CrossfadePrimary(profile.primaryClip, profile.primaryVolume * MASTER_AMBIENT_VOLUME, profile.crossfadeSeconds));
            }

            // ─ Secondary layer ─
            if (profile.secondaryClip == null)
            {
                if (profile.secondaryRequired)
                {
                    Debug.LogWarning($"[AmbientZone] Zone '{profile.zoneId}' secondaryClip is null but secondaryRequired=true. Expected at '{profile.secondaryExpectedPath}'. Fading secondary layer to silence.");
                }
                if (_secondaryFadeRoutine != null) StopCoroutine(_secondaryFadeRoutine);
                _secondaryFadeRoutine = StartCoroutine(FadeSecondary(null, 0f, profile.crossfadeSeconds));
            }
            else
            {
                if (_secondaryFadeRoutine != null) StopCoroutine(_secondaryFadeRoutine);
                _secondaryFadeRoutine = StartCoroutine(FadeSecondary(profile.secondaryClip, profile.secondaryVolume * MASTER_AMBIENT_VOLUME, profile.crossfadeSeconds));
            }
        }

        void FadeAllToSilence(float seconds)
        {
            if (_primaryFadeRoutine != null) StopCoroutine(_primaryFadeRoutine);
            _primaryFadeRoutine = StartCoroutine(CrossfadePrimary(null, 0f, seconds));
            if (_secondaryFadeRoutine != null) StopCoroutine(_secondaryFadeRoutine);
            _secondaryFadeRoutine = StartCoroutine(FadeSecondary(null, 0f, seconds));
        }

        IEnumerator CrossfadePrimary(AudioClip target, float targetVolume, float duration)
        {
            EnsureSources();

            var fromSrc = _primaryUseA ? _primaryA : _primaryB;
            var toSrc   = _primaryUseA ? _primaryB : _primaryA;

            float fromStartVol = fromSrc != null ? fromSrc.volume : 0f;

            // If the target clip is already live on fromSrc, just adjust volume without flipping.
            if (target != null && fromSrc != null && fromSrc.clip == target && fromSrc.isPlaying && Mathf.Approximately(targetVolume, fromStartVol))
            {
                yield break;
            }

            if (toSrc != null)
            {
                toSrc.clip = target;
                toSrc.volume = 0f;
                if (target != null)
                {
                    // Restart from 0 — short loop_time is the expected fresh-enter behavior.
                    toSrc.time = 0f;
                    toSrc.Play();
                }
            }

            duration = Mathf.Max(0.05f, duration);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (fromSrc != null) fromSrc.volume = Mathf.Lerp(fromStartVol, 0f, k);
                if (toSrc   != null) toSrc.volume   = Mathf.Lerp(0f, targetVolume, k);
                yield return null;
            }

            if (fromSrc != null)
            {
                fromSrc.volume = 0f;
                fromSrc.Stop();
                fromSrc.clip = null;
            }
            if (toSrc != null) toSrc.volume = targetVolume;

            _primaryUseA = !_primaryUseA;
            _primaryFadeRoutine = null;
        }

        IEnumerator FadeSecondary(AudioClip target, float targetVolume, float duration)
        {
            EnsureSources();
            if (_secondary == null)
            {
                Debug.LogError("[AmbientZone] _secondary AudioSource is null after EnsureSources() — internal invariant violation.");
                yield break;
            }

            float startVol = _secondary.volume;
            bool needRestart = target != null && _secondary.clip != target;

            if (needRestart)
            {
                // Fade out current then swap clip; we run as a single coroutine for simplicity.
                float halfDur = Mathf.Max(0.025f, duration * 0.5f);
                float t1 = 0f;
                while (t1 < halfDur)
                {
                    t1 += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t1 / halfDur);
                    _secondary.volume = Mathf.Lerp(startVol, 0f, k);
                    yield return null;
                }
                _secondary.Stop();
                _secondary.clip = target;
                _secondary.time = 0f;
                _secondary.Play();
                startVol = 0f;

                float t2 = 0f;
                while (t2 < halfDur)
                {
                    t2 += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t2 / halfDur);
                    _secondary.volume = Mathf.Lerp(0f, targetVolume, k);
                    yield return null;
                }
                _secondary.volume = targetVolume;
            }
            else
            {
                duration = Mathf.Max(0.05f, duration);
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / duration);
                    _secondary.volume = Mathf.Lerp(startVol, targetVolume, k);
                    yield return null;
                }
                _secondary.volume = targetVolume;
                if (target == null && Mathf.Approximately(targetVolume, 0f))
                {
                    _secondary.Stop();
                    _secondary.clip = null;
                }
            }

            _secondaryFadeRoutine = null;
        }

        float GetLiveLoopTime()
        {
            var live = _primaryUseA ? _primaryA : _primaryB;
            return live != null && live.clip != null && live.isPlaying ? live.time : 0f;
        }

        // ─── Cymatic tie-in (soft-bound via reflection) ─

        // Reflection is used here so this controller compiles on branches where
        // CymaticMusicEngine has not yet been merged (e.g. feature/consolidate-moon-architecture
        // pre-Sprint-5). When the type is present the call resolves and a Grotto
        // enter pings DebugActivateTelluric(); when absent, we log a clear warning.
        static System.Type s_cymaticType;
        static System.Reflection.PropertyInfo s_cymaticInstanceProp;
        static System.Reflection.MethodInfo s_cymaticActivateTelluricMethod;
        static bool s_cymaticProbed;

        static void ProbeCymaticReflection()
        {
            if (s_cymaticProbed) return;
            s_cymaticProbed = true;

            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("Tartaria.Audio.CymaticMusicEngine", throwOnError: false, ignoreCase: false);
                if (t != null)
                {
                    s_cymaticType = t;
                    s_cymaticInstanceProp = t.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    s_cymaticActivateTelluricMethod = t.GetMethod("DebugActivateTelluric",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    break;
                }
            }
        }

        void InvokeTelluricTieIn(string zoneId)
        {
            ProbeCymaticReflection();

            if (s_cymaticType == null)
            {
                Debug.LogWarning($"[AmbientZone] Zone '{zoneId}' requested Telluric tie-in but Tartaria.Audio.CymaticMusicEngine is not in any loaded assembly on this branch. Merge Sprint-5 cymatic engine to enable, or disable activateTelluricOnEnter on this profile.");
                return;
            }
            if (s_cymaticInstanceProp == null)
            {
                Debug.LogWarning($"[AmbientZone] Zone '{zoneId}' Telluric tie-in: CymaticMusicEngine type was found but no static 'Instance' property exists. API drift — update reflection probe.");
                return;
            }
            if (s_cymaticActivateTelluricMethod == null)
            {
                Debug.LogWarning($"[AmbientZone] Zone '{zoneId}' Telluric tie-in: CymaticMusicEngine.Instance found but no 'DebugActivateTelluric()' method exists. API drift — update reflection probe.");
                return;
            }

            var instance = s_cymaticInstanceProp.GetValue(null);
            if (instance == null)
            {
                Debug.LogWarning($"[AmbientZone] Zone '{zoneId}' Telluric tie-in: CymaticMusicEngine.Instance is null. Add the engine to the scene or bootstrap it before player can enter the grotto.");
                return;
            }

            s_cymaticActivateTelluricMethod.Invoke(instance, null);
            Debug.Log($"[AmbientZone] Zone '{zoneId}' Telluric tie-in -> CymaticMusicEngine.DebugActivateTelluric() invoked (reflection).");
        }

        // ─── Debug helpers ───────────────────────────

        /// <summary>Live snapshot of the controller mix for QA / Editor menus.</summary>
        public string DescribeMix()
        {
            string top = _activeStack.Count > 0 ? _activeStack[_activeStack.Count - 1].zoneId : "<none>";
            float pa = _primaryA != null ? _primaryA.volume : 0f;
            float pb = _primaryB != null ? _primaryB.volume : 0f;
            float s  = _secondary != null ? _secondary.volume : 0f;
            return $"top='{top}' stack={_activeStack.Count} pA={pa:F2} pB={pb:F2} sec={s:F2} loop_time={GetLiveLoopTime():F2}";
        }
    }
}
