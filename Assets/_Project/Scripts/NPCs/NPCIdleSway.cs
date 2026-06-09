using UnityEngine;

namespace Tartaria.NPCs
{
    /// <summary>
    /// R234 — lightweight idle animation for un-rigged NPCs. Adds subtle Y-bob (breathing) +
    /// yaw sway (looking around) so static MeshRenderer NPCs feel alive without needing a
    /// full Mecanim humanoid rig. Drop-in until Sprint D ships proper rigs.
    ///
    /// Per CLAUDE.md R171: no static T-pose capsules — at minimum, NPCs must breathe.
    /// Per NATRIX 2026-06-03 mandate: build content beats, don't release-frame.
    /// </summary>
    [DisallowMultipleComponent]
    public class NPCIdleSway : MonoBehaviour
    {
        [Header("Breathing (Y-axis)")]
        [Tooltip("Vertical bob amplitude in meters.")]
        public float bobAmplitude = 0.025f;
        [Tooltip("Bob cycles per second.")]
        public float bobFrequency = 0.6f;

        [Header("Yaw Sway")]
        [Tooltip("Degrees of look-around sway.")]
        public float yawAmplitude = 4f;
        [Tooltip("Yaw cycles per second.")]
        public float yawFrequency = 0.18f;

        [Header("Phase Randomization")]
        [Tooltip("Random phase offset so NPCs don't sync.")]
        public bool randomizePhase = true;

        Vector3 _basePosition;
        Quaternion _baseRotation;
        float _phaseY;
        float _phaseYaw;
        bool _initialized;

        void OnEnable()
        {
            if (!_initialized)
            {
                _basePosition = transform.localPosition;
                _baseRotation = transform.localRotation;
                if (randomizePhase)
                {
                    var seed = (uint)gameObject.GetInstanceID();
                    var rng = new System.Random(unchecked((int)seed));
                    _phaseY = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                    _phaseYaw = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                }
                _initialized = true;
            }
        }

        void Update()
        {
            if (!_initialized) return;
            float t = Time.time;
            float bob = Mathf.Sin(t * bobFrequency * Mathf.PI * 2f + _phaseY) * bobAmplitude;
            float yaw = Mathf.Sin(t * yawFrequency * Mathf.PI * 2f + _phaseYaw) * yawAmplitude;
            transform.localPosition = _basePosition + new Vector3(0, bob, 0);
            transform.localRotation = _baseRotation * Quaternion.Euler(0, yaw, 0);
        }

        void OnDisable()
        {
            if (_initialized)
            {
                transform.localPosition = _basePosition;
                transform.localRotation = _baseRotation;
            }
        }
    }
}
