// MixerSnapshotController.cs
// Runtime auto-bootstrap singleton that swaps AudioMixer snapshots
// (Normal <-> Paused) in response to the global pause toggle event.
//
// Owner: agent/audio/mixer-snapshot-system
// Asmdef: Tartaria.Audio (references Tartaria.Core only --- this file
//          subscribes to Tartaria.Input.PlayerInputHandler.OnPauseToggled
//          via REFLECTION to avoid introducing a new asmdef dependency).
//
// Spec:
//   - Loads UnityEngine.Audio.AudioMixer via Resources.Load("Audio/Mixers/EchohavenMaster")
//   - Caches snapshots "Normal" + "Paused" via mixer.FindSnapshot
//   - Subscribes to Tartaria.Input.PlayerInputHandler.OnPauseToggled static event
//   - On pause: PausedSnapshot.TransitionTo(0.3f)
//   - On resume: NormalSnapshot.TransitionTo(0.3f)
//   - Null-guard everything; log a single warning + skip if mixer missing
//   - Works with or without the mixer asset present (cowork-editor pass may create it)

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    public sealed class MixerSnapshotController : MonoBehaviour
    {
        const string MixerResourcePath = "Audio/Mixers/EchohavenMaster";
        const string NormalSnapshotName = "Normal";
        const string PausedSnapshotName = "Paused";
        const float TransitionSeconds = 0.3f;

        public static MixerSnapshotController Instance { get; private set; }

        AudioMixer _mixer;
        AudioMixerSnapshot _normalSnapshot;
        AudioMixerSnapshot _pausedSnapshot;
        bool _isPaused;
        bool _warnedMissing;

        // Reflection handles for Tartaria.Input.PlayerInputHandler.OnPauseToggled.
        // Held so we can unsubscribe on destroy.
        EventInfo _pauseEventInfo;
        Delegate _pauseHandler;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[MixerSnapshotController]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.DontSave;
            go.AddComponent<MixerSnapshotController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            LoadMixer();
            SubscribeToPauseEvent();
        }

        void OnDestroy()
        {
            UnsubscribeFromPauseEvent();
            if (Instance == this) Instance = null;
        }

        void LoadMixer()
        {
            try
            {
                _mixer = Resources.Load<AudioMixer>(MixerResourcePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MixerSnapshotController] Exception loading mixer '{MixerResourcePath}': {ex.Message}");
                _mixer = null;
            }

            if (_mixer == null)
            {
                if (!_warnedMissing)
                {
                    Debug.LogWarning(
                        $"[MixerSnapshotController] AudioMixer not found at Resources/'{MixerResourcePath}'. " +
                        "Snapshot transitions disabled. Run Tartaria/5 Audio/Create EchohavenMaster Mixer in the Editor to author it.");
                    _warnedMissing = true;
                }
                return;
            }

            _normalSnapshot = SafeFindSnapshot(NormalSnapshotName);
            _pausedSnapshot = SafeFindSnapshot(PausedSnapshotName);

            if (_normalSnapshot == null || _pausedSnapshot == null)
            {
                Debug.LogWarning(
                    $"[MixerSnapshotController] EchohavenMaster loaded, but snapshots " +
                    $"('{NormalSnapshotName}'={_normalSnapshot != null}, '{PausedSnapshotName}'={_pausedSnapshot != null}) " +
                    "are missing. Re-run Tartaria/5 Audio/Create EchohavenMaster Mixer or add the snapshots in the Editor.");
            }
        }

        AudioMixerSnapshot SafeFindSnapshot(string name)
        {
            if (_mixer == null) return null;
            try
            {
                return _mixer.FindSnapshot(name);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MixerSnapshotController] FindSnapshot('{name}') threw: {ex.Message}");
                return null;
            }
        }

        // === Pause event wiring (reflection to avoid Tartaria.Input asmdef ref) ===

        void SubscribeToPauseEvent()
        {
            try
            {
                var type = Type.GetType("Tartaria.Input.PlayerInputHandler, Tartaria.Input");
                if (type == null)
                {
                    Debug.LogWarning("[MixerSnapshotController] Tartaria.Input.PlayerInputHandler type not found. Pause snapshot transitions disabled.");
                    return;
                }
                _pauseEventInfo = type.GetEvent("OnPauseToggled", BindingFlags.Public | BindingFlags.Static);
                if (_pauseEventInfo == null)
                {
                    Debug.LogWarning("[MixerSnapshotController] PlayerInputHandler.OnPauseToggled event not found. Pause snapshot transitions disabled.");
                    return;
                }

                var handlerMethod = typeof(MixerSnapshotController).GetMethod(
                    nameof(HandlePauseToggled), BindingFlags.Instance | BindingFlags.NonPublic);
                if (handlerMethod == null)
                {
                    Debug.LogWarning("[MixerSnapshotController] Could not locate HandlePauseToggled handler method.");
                    return;
                }

                _pauseHandler = Delegate.CreateDelegate(_pauseEventInfo.EventHandlerType, this, handlerMethod);
                _pauseEventInfo.AddEventHandler(null, _pauseHandler);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MixerSnapshotController] Subscribe to OnPauseToggled failed: {ex.Message}");
                _pauseEventInfo = null;
                _pauseHandler = null;
            }
        }

        void UnsubscribeFromPauseEvent()
        {
            try
            {
                if (_pauseEventInfo != null && _pauseHandler != null)
                {
                    _pauseEventInfo.RemoveEventHandler(null, _pauseHandler);
                }
            }
            catch
            {
                // best-effort; static event may already be cleared by domain reload
            }
            finally
            {
                _pauseEventInfo = null;
                _pauseHandler = null;
            }
        }

        void HandlePauseToggled()
        {
            _isPaused = !_isPaused;
            ApplySnapshot(_isPaused);
        }

        void ApplySnapshot(bool paused)
        {
            if (_mixer == null) return;
            var target = paused ? _pausedSnapshot : _normalSnapshot;
            if (target == null) return;
            try
            {
                target.TransitionTo(TransitionSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MixerSnapshotController] TransitionTo failed: {ex.Message}");
            }
        }

        // === Public test hooks (manual overrides; safe no-ops when mixer missing) ===

        public void ForcePaused() { _isPaused = true; ApplySnapshot(true); }
        public void ForceNormal() { _isPaused = false; ApplySnapshot(false); }
        public bool MixerLoaded => _mixer != null;
        public bool SnapshotsResolved => _normalSnapshot != null && _pausedSnapshot != null;
    }
}
