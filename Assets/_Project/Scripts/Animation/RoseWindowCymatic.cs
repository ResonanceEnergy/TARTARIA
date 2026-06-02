using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Animation
{
    /// <summary>
    /// Procedural cymatic pattern projection on the cathedral rose window.
    /// Subscribes to <see cref="GameEvents.OnBuildingRestored"/> and, when the
    /// restored building id contains "cathedral", drives the renderer's emission
    /// and base color via a MaterialPropertyBlock through a fade-in / sustain
    /// cycle. Pure code — no scene or .mat file mutation. Material must be
    /// URP/Lit with emission enabled (Cowork wires this in the scene).
    ///
    /// Per CLAUDE.md no-debt mandate:
    ///   - Logs subscribe / unsubscribe / activation with the GameObject path
    ///   - Logs warning + early-returns (no silent swallow) on null Renderer
    ///   - No try/catch suppression
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public class RoseWindowCymatic : MonoBehaviour
    {
        [Header("Cymatic projection")]
        [SerializeField] float waveSpeed = 0.5f;
        [SerializeField] float waveAmplitude = 1.2f;
        [SerializeField] Color baseEmission = new Color(0.9f, 0.85f, 0.55f);
        [SerializeField] float fadeInDuration = 2f;
        [SerializeField] float sustainDuration = 6f;

        [Tooltip("Substring (case-insensitive) the buildingId must contain to trigger. Default 'cathedral'.")]
        [SerializeField] string buildingIdFilter = "cathedral";

        Renderer _rend;
        MaterialPropertyBlock _mpb;
        bool _active;
        float _activeT;

        static readonly int _EmissionColorId = Shader.PropertyToID("_EmissionColor");
        static readonly int _BaseColorId = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            _rend = GetComponent<Renderer>();
            if (_rend == null)
            {
                // RequireComponent should prevent this, but be loud if Unity ever hands us null.
                Debug.LogError($"[RoseWindowCymatic] Renderer missing on '{GetHierarchyPath()}' — component disabled.");
                enabled = false;
                return;
            }

            _mpb = new MaterialPropertyBlock();
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            Debug.Log($"[RoseWindowCymatic] Subscribed to GameEvents.OnBuildingRestored on '{GetHierarchyPath()}' (filter='{buildingIdFilter}')");
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            Debug.Log($"[RoseWindowCymatic] Unsubscribed from GameEvents.OnBuildingRestored on '{GetHierarchyPath()}'");
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogWarning($"[RoseWindowCymatic] Received OnBuildingRestored with null/empty buildingId on '{GetHierarchyPath()}' — ignoring.");
                return;
            }

            if (string.IsNullOrEmpty(buildingIdFilter))
            {
                Debug.LogWarning($"[RoseWindowCymatic] buildingIdFilter is empty on '{GetHierarchyPath()}' — refusing to trigger on every restoration. Set filter in Inspector.");
                return;
            }

            if (!buildingId.ToLowerInvariant().Contains(buildingIdFilter.ToLowerInvariant()))
            {
                // Not our building — no log spam here (this fires for every restoration in the scene).
                return;
            }

            if (_active)
            {
                Debug.Log($"[RoseWindowCymatic] Already active on '{GetHierarchyPath()}', re-triggering sequence for buildingId='{buildingId}'");
                StopAllCoroutines();
            }
            else
            {
                Debug.Log($"[RoseWindowCymatic] Activating on '{GetHierarchyPath()}' for buildingId='{buildingId}' (fadeIn={fadeInDuration}s, sustain={sustainDuration}s)");
            }

            StartCoroutine(ProjectionSequence());
        }

        IEnumerator ProjectionSequence()
        {
            _active = true;
            _activeT = 0f;

            float t = 0f;
            while (t < fadeInDuration)
            {
                t += UnityEngine.Time.deltaTime;
                _activeT = t;
                yield return null;
            }

            // Clamp _activeT to the fade-in ceiling so Update's fadeIn computation stays at 1.
            _activeT = fadeInDuration;

            // Sustain window — we keep the pulse animation but no longer ramp.
            // After sustainDuration we DO NOT clear _active: the cathedral stays lit forever
            // once restored, matching narrative intent. Sustain just marks the intro complete.
            yield return new WaitForSeconds(sustainDuration);

            Debug.Log($"[RoseWindowCymatic] Intro complete on '{GetHierarchyPath()}' — entering perpetual sustain.");
        }

        void Update()
        {
            if (!_active) return;

            float fadeIn = Mathf.Clamp01(_activeT / Mathf.Max(0.0001f, fadeInDuration));
            float pulse = 0.5f + 0.5f * Mathf.Sin(UnityEngine.Time.time * waveSpeed * Mathf.PI * 2f);
            Color emission = baseEmission * (fadeIn * (1f + waveAmplitude * pulse));

            _rend.GetPropertyBlock(_mpb);
            _mpb.SetColor(_EmissionColorId, emission);
            _mpb.SetColor(_BaseColorId, Color.Lerp(Color.black, baseEmission, fadeIn));
            _rend.SetPropertyBlock(_mpb);
        }

        string GetHierarchyPath()
        {
            // Walk up the transform tree to produce 'Root/Child/Leaf' — used in logs per rule 4
            // ("Logging warnings with no value attached is useless").
            var t = transform;
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
