// Moon1LiraelLullabyCinematic.cs
// ANIMATION ENGINEER · 2026-06-02
//
// 30-second scripted cinematic that plays when the cathedral light eruption fires
// the 17th-hour beat. Lirael walks from the cathedral entrance to the altar, sits,
// and hums four bars in 528 Hz. Visual companion to Moon1LiraelController (which
// owns the spoken Yarn dialogue line at the same beat).
//
// API references verified against canonical sources:
//   - GameEvents.cs:446  →  public static event Action OnSeventeenthHour;
//   - GameEvents.cs:623  →  RaiseHUDShowBanner(string title, string subtitle, float duration = 5f)
//   - TagManager.asset:14 →  "Lirael" tag exists
//   - TagManager.asset    →  NO "Cathedral_Altar" tag → name-lookup fallback used
//
// Per API_CONTRACT.md: UnityEngine.Time.deltaTime fully qualified to avoid any
// namespace shadow risk inside Tartaria.Integration.
// Per no-debt rule 3: reflection catch blocks rethrow after logging.
// Per no-debt rule 4: null lookups log error/warn with the path that was tried.

using System;
using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration {
    [DisallowMultipleComponent]
    public class Moon1LiraelLullabyCinematic : MonoBehaviour {
        public static Moon1LiraelLullabyCinematic Instance { get; private set; }

        // Timeline constants (total 30s).
        const float WALK_DURATION       = 6f;   // entrance → altar (SmoothStep lerp)
        const float SETTLE_DELAY        = 1f;   // pause before banner + animator trigger
        const float CINEMATIC_HOLD      = 18f;  // dolly camera holds on Lirael
        const float BANNER_DURATION     = 12f;  // matches RaiseHUDShowBanner third arg
        const float RESTORE_TAIL        = 5f;   // ease-out tail to total ~30s

        // Cinematic framing (rule of thirds: Lirael off-center on the left third).
        const float CAMERA_HEIGHT       = 1.6f;
        const float CAMERA_DISTANCE     = 3.2f;
        const float CAMERA_LATERAL      = 1.4f;  // shift camera right so Lirael lands on left third
        const float CAMERA_FOV          = 35f;   // mild tele for cinematic compression

        const string LIRAEL_TAG         = "Lirael";
        const string ALTAR_NAME_A       = "Cathedral_Altar";
        const string ALTAR_NAME_B       = "Altar";
        const string ENTRANCE_NAME      = "Cathedral_Entrance";
        const string ANIM_TRIGGER       = "Lullaby";

        bool _running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap() {
            if (Instance != null) return;
            var go = new GameObject("Moon1LiraelLullabyCinematic");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<Moon1LiraelLullabyCinematic>();
        }

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnSeventeenthHour += HandleSeventeenthHour;
        }

        void OnDestroy() {
            if (Instance == this) Instance = null;
            GameEvents.OnSeventeenthHour -= HandleSeventeenthHour;
        }

        void HandleSeventeenthHour() {
            if (_running) {
                Debug.LogWarning("[Moon1LiraelLullabyCinematic] OnSeventeenthHour fired while cinematic already running — ignoring re-entry.");
                return;
            }
            PlayNow();
        }

        /// <summary>
        /// Public entrypoint for QA / debug menu invocation. Safe to call directly.
        /// </summary>
        public void PlayNow() {
            if (_running) {
                Debug.LogWarning("[Moon1LiraelLullabyCinematic] PlayNow called while cinematic already running — ignoring.");
                return;
            }
            StartCoroutine(RunCinematic());
        }

        IEnumerator RunCinematic() {
            _running = true;
            Debug.Log("[Moon1LiraelLullabyCinematic] Starting 30s lullaby cinematic.");

            // ---- Locate Lirael --------------------------------------------------
            GameObject lirael = null;
            try { lirael = GameObject.FindGameObjectWithTag(LIRAEL_TAG); }
            catch (UnityException ex) {
                // Tag not defined in this build — log and rethrow per no-debt rule 3.
                Debug.LogError($"[Moon1LiraelLullabyCinematic] Tag '{LIRAEL_TAG}' lookup threw — tag missing from TagManager.asset? ex={ex}");
                throw;
            }
            if (lirael == null) {
                Debug.LogError($"[Moon1LiraelLullabyCinematic] Lirael NOT FOUND. Tried: GameObject.FindGameObjectWithTag(\"{LIRAEL_TAG}\"). Confirm a scene object has the 'Lirael' tag (TagManager.asset:14). Aborting cinematic.");
                _running = false;
                yield break;
            }

            // ---- Locate altar (tag fallback chain) ------------------------------
            GameObject altar = null;
            // No "Cathedral_Altar" tag exists in TagManager.asset, so go straight to name lookup.
            altar = GameObject.Find(ALTAR_NAME_A);
            if (altar == null) {
                Debug.LogWarning($"[Moon1LiraelLullabyCinematic] Altar name fallback: '{ALTAR_NAME_A}' not found, trying '{ALTAR_NAME_B}'.");
                altar = GameObject.Find(ALTAR_NAME_B);
            }
            if (altar == null) {
                Debug.LogError($"[Moon1LiraelLullabyCinematic] Altar NOT FOUND. Tried: GameObject.Find(\"{ALTAR_NAME_A}\"), GameObject.Find(\"{ALTAR_NAME_B}\"). Add a GameObject named '{ALTAR_NAME_A}' under the cathedral. Aborting cinematic.");
                _running = false;
                yield break;
            }

            // ---- Resolve entrance position (start of walk) ----------------------
            Vector3 entrancePos;
            var entrance = GameObject.Find(ENTRANCE_NAME);
            if (entrance != null) {
                entrancePos = entrance.transform.position;
            } else {
                // Use Lirael's current position as the entrance — log so we know the fallback engaged.
                Debug.LogWarning($"[Moon1LiraelLullabyCinematic] '{ENTRANCE_NAME}' GameObject not found — using Lirael's current position '{lirael.transform.position}' as walk start.");
                entrancePos = lirael.transform.position;
            }
            Vector3 altarPos = altar.transform.position;

            // Snap Lirael to entrance before the lerp so the walk reads correctly.
            lirael.transform.position = entrancePos;
            Vector3 walkVector = altarPos - entrancePos;
            walkVector.y = 0f;
            if (walkVector.sqrMagnitude > 0.0001f) {
                lirael.transform.rotation = Quaternion.LookRotation(walkVector.normalized, Vector3.up);
            }

            // ---- Capture original camera state ----------------------------------
            var cam = UnityEngine.Camera.main;
            Transform camFollowParent = null;
            Vector3 originalCamLocalPos = Vector3.zero;
            Quaternion originalCamLocalRot = Quaternion.identity;
            float originalCamFov = 60f;
            bool cameraCaptured = false;

            if (cam != null) {
                camFollowParent = cam.transform.parent;
                originalCamLocalPos = cam.transform.localPosition;
                originalCamLocalRot = cam.transform.localRotation;
                originalCamFov = cam.fieldOfView;
                cameraCaptured = true;
                // Detach from follow target so the cinematic dolly isn't fought by a follow script.
                cam.transform.SetParent(null, worldPositionStays: true);
            } else {
                Debug.LogWarning("[Moon1LiraelLullabyCinematic] UnityEngine.Camera.main is null — cinematic will run without camera dolly. Tag a Camera as 'MainCamera' to enable framing.");
            }

            // ---- PHASE 1 · Walk (6s) — SmoothStep lerp -------------------------
            float t = 0f;
            while (t < WALK_DURATION) {
                t += UnityEngine.Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / WALK_DURATION));
                lirael.transform.position = Vector3.Lerp(entrancePos, altarPos, k);

                // Camera tracks Lirael during the walk so the audience follows her.
                if (cam != null) {
                    DollyCameraToLirael(cam, lirael.transform, altarPos, kBlend: k * 0.6f);
                }
                yield return null;
            }
            lirael.transform.position = altarPos;

            // ---- PHASE 2 · Settle (1s) — face camera, fire Animator trigger -----
            yield return new WaitForSeconds(SETTLE_DELAY);

            if (cam != null) {
                Vector3 faceTarget = cam.transform.position;
                faceTarget.y = lirael.transform.position.y; // keep head level
                Vector3 faceDir = faceTarget - lirael.transform.position;
                if (faceDir.sqrMagnitude > 0.0001f) {
                    lirael.transform.rotation = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                }
            }

            var animator = lirael.GetComponentInChildren<Animator>();
            if (animator != null) {
                bool triggerExists = false;
                foreach (var p in animator.parameters) {
                    if (p.type == AnimatorControllerParameterType.Trigger && p.name == ANIM_TRIGGER) {
                        triggerExists = true;
                        break;
                    }
                }
                if (triggerExists) {
                    try { animator.SetTrigger(ANIM_TRIGGER); }
                    catch (Exception ex) {
                        Debug.LogError($"[Moon1LiraelLullabyCinematic] Animator.SetTrigger('{ANIM_TRIGGER}') threw on '{lirael.name}': {ex}");
                        throw;
                    }
                } else {
                    Debug.LogWarning($"[Moon1LiraelLullabyCinematic] Animator on '{lirael.name}' has no trigger parameter '{ANIM_TRIGGER}' — skipping animation. Add the trigger to her AnimatorController to enable the lullaby pose.");
                }
            } else {
                Debug.LogWarning($"[Moon1LiraelLullabyCinematic] No Animator component found on '{lirael.name}' or its children — skipping '{ANIM_TRIGGER}' trigger. Wire an Animator to enable the hum pose.");
            }

            // Banner — verified signature: RaiseHUDShowBanner(title, subtitle, duration).
            GameEvents.RaiseHUDShowBanner("Lirael", "(she hums four bars in 528 Hz)", BANNER_DURATION);

            // ---- PHASE 3 · Cinematic hold (18s) — dolly camera framing ----------
            float hold = 0f;
            while (hold < CINEMATIC_HOLD) {
                hold += UnityEngine.Time.deltaTime;
                if (cam != null) {
                    DollyCameraToLirael(cam, lirael.transform, altarPos, kBlend: 1f);
                }
                yield return null;
            }

            // ---- PHASE 4 · Restore (5s tail) — reattach follow camera -----------
            if (cameraCaptured && cam != null) {
                cam.transform.SetParent(camFollowParent, worldPositionStays: true);
                cam.transform.localPosition = originalCamLocalPos;
                cam.transform.localRotation = originalCamLocalRot;
                cam.fieldOfView = originalCamFov;
            }
            yield return new WaitForSeconds(RESTORE_TAIL);

            Debug.Log("[Moon1LiraelLullabyCinematic] Cinematic complete (30s elapsed).");
            _running = false;
        }

        /// <summary>
        /// Frame Lirael on the left third of the screen at altar, looking slightly down.
        /// kBlend ∈ [0,1] eases the camera into the cinematic angle during the walk.
        /// </summary>
        void DollyCameraToLirael(UnityEngine.Camera cam, Transform lirael, Vector3 altarPos, float kBlend) {
            // Pick a stable forward axis from Lirael toward camera-ish space.
            // We build the cinematic position behind & to the right of her facing direction.
            Vector3 lookAt = lirael.position + Vector3.up * 1.4f; // head height

            // Use altar's local +Z as the "front" of the altar; if zero, fall back to world +Z.
            Vector3 altarForward = lirael.forward;
            if (altarForward.sqrMagnitude < 0.0001f) altarForward = Vector3.forward;
            altarForward.y = 0f;
            altarForward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, altarForward).normalized;

            Vector3 cinematicPos =
                lookAt
                + altarForward * CAMERA_DISTANCE      // pull camera in front of Lirael
                + right * CAMERA_LATERAL              // shift right so subject lands on left third
                + Vector3.up * (CAMERA_HEIGHT - 1.4f); // adjust off head-height baseline

            // Blend from current camera state into cinematic during the walk.
            Vector3 blendedPos = Vector3.Lerp(cam.transform.position, cinematicPos, kBlend);
            cam.transform.position = blendedPos;

            Vector3 lookDir = lookAt - cam.transform.position;
            if (lookDir.sqrMagnitude > 0.0001f) {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRot, kBlend);
            }

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, CAMERA_FOV, kBlend);
        }
    }
}
