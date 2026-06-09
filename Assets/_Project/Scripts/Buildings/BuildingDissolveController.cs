using System.Collections;
using UnityEngine;

namespace Tartaria.Buildings
{
    /// <summary>
    /// R296 — drives the mud dissolution shader animation per spec §8.
    /// Lerps Building_MudDissolve material's _DissolveProgress from 0 → 1 over 5 seconds.
    /// Triggers Restoration Burst particle on completion.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingDissolveController : MonoBehaviour
    {
        [Tooltip("Duration of dissolution animation (seconds). Spec §8 = 5.0")]
        public float dissolveDuration = 5.0f;

        [Tooltip("Optional: ParticleSystem to trigger when dissolve completes (RestorationBurst)")]
        public ParticleSystem onCompleteParticle;

        [Tooltip("Optional: AudioClip to play at start of dissolution (crystalline 432Hz tone)")]
        public AudioClip dissolveAudio;

        Renderer[] _renderers;
        bool _isDissolving;
        float _progress;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
        }

        public void StartDissolve()
        {
            if (_isDissolving) return;
            _isDissolving = true;
            StartCoroutine(DissolveRoutine());
        }

        IEnumerator DissolveRoutine()
        {
            if (dissolveAudio != null)
            {
                AudioSource.PlayClipAtPoint(dissolveAudio, transform.position);
            }

            float elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                _progress = Mathf.Clamp01(elapsed / dissolveDuration);
                SetProgressOnRenderers(_progress);
                yield return null;
            }

            _progress = 1f;
            SetProgressOnRenderers(_progress);

            if (onCompleteParticle != null)
            {
                onCompleteParticle.gameObject.SetActive(true);
                onCompleteParticle.Play();
            }

            _isDissolving = false;
            Debug.Log($"[BuildingDissolveController] {gameObject.name} dissolution complete");
        }

        void SetProgressOnRenderers(float p)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                var mats = r.sharedMaterials;
                foreach (var m in mats)
                {
                    if (m != null && m.HasProperty("_DissolveProgress"))
                    {
                        m.SetFloat("_DissolveProgress", p);
                    }
                }
            }
        }

        public float Progress => _progress;
        public bool IsDissolving => _isDissolving;
    }
}
