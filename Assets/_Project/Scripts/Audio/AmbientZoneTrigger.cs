// AmbientZoneTrigger.cs
// Sprint 6 Lane 4 — Per-zone ambient trigger volume.
//
// Attach to a GameObject with a Collider set to isTrigger. Assign an
// AmbientZoneProfile in the inspector. When the player (CompareTag("Player")
// or layer match) enters/exits the volume, the trigger forwards the profile
// to AmbientZoneController for cross-fade handling.
//
// The recommended tag for the GameObject itself is "AmbientZone" per the
// sprint spec. The tag is informational (for batch-find queries in Editor
// menus). Triggering is by collider + Player layer/tag, not by GameObject tag.
using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Trigger volume that pushes its <see cref="AmbientZoneProfile"/> onto the
    /// <see cref="AmbientZoneController"/> stack on player enter and pops it on exit.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class AmbientZoneTrigger : MonoBehaviour
    {
        [Header("Profile")]
        [Tooltip("ScriptableObject describing the ambient mix for this zone.")]
        [SerializeField] AmbientZoneProfile profile;

        [Header("Trigger Filter")]
        [Tooltip("Player tag used for collider.CompareTag(). Falls back to 'Player' if blank.")]
        [SerializeField] string playerTag = "Player";

        [Tooltip("Optional layer mask. If a layer is set on the entering collider that does not match, the enter is ignored. Default = Everything.")]
        [SerializeField] LayerMask triggerLayerMask = ~0;

        Collider _collider;
        bool _playerInside;

        void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                Debug.LogError($"[AmbientZone] AmbientZoneTrigger on '{GetHierarchyPath()}' has no Collider — RequireComponent should have prevented this. Disabling.");
                enabled = false;
                return;
            }
            if (!_collider.isTrigger)
            {
                Debug.LogWarning($"[AmbientZone] AmbientZoneTrigger on '{GetHierarchyPath()}' has Collider.isTrigger=false — forcing true.");
                _collider.isTrigger = true;
            }

            if (profile == null)
            {
                Debug.LogError($"[AmbientZone] AmbientZoneTrigger on '{GetHierarchyPath()}' has no AmbientZoneProfile assigned. Drag a profile asset from Assets/_Project/Data/Audio/Ambient/ into the Profile slot.");
            }

            if (string.IsNullOrEmpty(playerTag)) playerTag = "Player";
        }

        void OnTriggerEnter(Collider other)
        {
            if (!ShouldRespond(other)) return;
            if (_playerInside) return; // Edge case: nested colliders on player re-fire enter.
            _playerInside = true;

            var ctrl = AmbientZoneController.Instance;
            if (ctrl == null)
            {
                Debug.LogWarning($"[AmbientZone] AmbientZoneController.Instance is null on enter of '{GetHierarchyPath()}'. Bootstrap should have created it AfterSceneLoad; check scene load order.");
                return;
            }

            ctrl.EnterZone(profile, this);
        }

        void OnTriggerExit(Collider other)
        {
            if (!ShouldRespond(other)) return;
            if (!_playerInside) return;
            _playerInside = false;

            var ctrl = AmbientZoneController.Instance;
            if (ctrl == null)
            {
                Debug.LogWarning($"[AmbientZone] AmbientZoneController.Instance is null on exit of '{GetHierarchyPath()}'.");
                return;
            }

            ctrl.ExitZone(profile, this);
        }

        void OnDisable()
        {
            // If we go inactive while the player is inside, treat as an exit so the stack stays consistent.
            if (_playerInside)
            {
                _playerInside = false;
                var ctrl = AmbientZoneController.Instance;
                if (ctrl != null && profile != null)
                    ctrl.ExitZone(profile, this);
            }
        }

        bool ShouldRespond(Collider other)
        {
            if (other == null) return false;
            if (profile == null) return false;

            // Layer filter
            if (((1 << other.gameObject.layer) & triggerLayerMask) == 0)
                return false;

            // Tag filter (tag may not exist in TagManager → swallow + log)
            try
            {
                if (!other.CompareTag(playerTag))
                {
                    // Also accept root tag if a child collider is the actual trigger receiver.
                    var root = other.transform.root;
                    if (root == null || !root.CompareTag(playerTag))
                        return false;
                }
            }
            catch (UnityException ex)
            {
                Debug.LogWarning($"[AmbientZone] CompareTag('{playerTag}') threw on '{other.name}' — tag is probably missing from TagManager.asset. Add it under ProjectSettings/TagManager. Inner: {ex.Message}");
                return false;
            }

            return true;
        }

        string GetHierarchyPath()
        {
            var sb = new System.Text.StringBuilder(64);
            var t = transform;
            while (t != null)
            {
                if (sb.Length > 0) sb.Insert(0, '/');
                sb.Insert(0, t.name);
                t = t.parent;
            }
            return sb.ToString();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = profile != null && profile.activateTelluricOnEnter
                ? new Color(0.6f, 0.2f, 0.9f, 0.25f) // Telluric tie-in → purple
                : new Color(0.25f, 0.85f, 0.65f, 0.20f); // Standard ambient → teal
            Gizmos.matrix = transform.localToWorldMatrix;

            switch (col)
            {
                case BoxCollider box:
                    Gizmos.DrawCube(box.center, box.size);
                    break;
                case SphereCollider sphere:
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    break;
                case CapsuleCollider _:
                    // Capsule visualization is non-trivial; skip filled gizmo and let scene gizmos handle it.
                    break;
            }
        }
#endif
    }
}
