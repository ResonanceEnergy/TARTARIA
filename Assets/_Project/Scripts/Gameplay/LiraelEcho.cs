using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Lirael - Echo child, hums 432 Hz lullaby, appears translucent
    /// Key to Moon 3/6/7 crossover, manifests fully in Moon 13
    /// </summary>
    public class LiraelEcho : MonoBehaviour
    {
        [Header("Echo Properties")]
        public bool isFullyManifested = false;
        [Range(0f, 1f)] public float opacity = 0.4f;
        public Color echoTint = new Color(0.7f, 0.9f, 1f, 0.4f);
        
        [Header("Lullaby (432 Hz)")]
        public AudioClip lullaby432Hz;
        public float lullabyVolume = 0.3f;
        public bool isHumming = false;
        
        [Header("Dialogue")]
        public string[] crypticQuestions = new string[]
        {
            "Why do grown-ups build houses then live in the attic?",
            "The song... it'"'"'s breaking...",
            "I don'"'"'t remember my name, only the song.",
            "Can you hear the giants singing? They'"'"'re so sad...",
            "The spire used to shine. Will it shine again?"
        };
        
        private AudioSource audioSource;
        private MeshRenderer[] renderers;
        
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = lullaby432Hz;
            audioSource.loop = true;
            audioSource.volume = lullabyVolume;
            audioSource.spatialBlend = 0.8f; // 3D sound
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
            
            // Apply echo material properties
            renderers = GetComponentsInChildren<MeshRenderer>();
            SetOpacity(opacity);
            
            if (isHumming && lullaby432Hz != null)
            {
                audioSource.Play();
            }
        }
        
        void SetOpacity(float newOpacity)
        {
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    // Set to transparent render mode
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha blend
                    
                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = newOpacity;
                    mat.SetColor("_BaseColor", baseColor);
                    
                    // Apply echo tint
                    mat.SetColor("_EmissionColor", echoTint * 0.5f);
                }
            }
        }
        
        /// <summary>
        /// Manifests fully (becomes solid) during key story moments
        /// </summary>
        public void ManifestFully()
        {
            isFullyManifested = true;
            SetOpacity(1f);
            
            if (audioSource && lullaby432Hz)
            {
                audioSource.volume = lullabyVolume * 2f;
            }
            
            Debug.Log("[Lirael] Fully manifested. The song is complete.");
        }
        
        /// <summary>
        /// Trigger crying animation when dissonance appears
        /// </summary>
        public void ReactToDissonance()
        {
            if (audioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            Debug.Log("[Lirael] The song'"'"'s breaking...");
            // TODO: Play crying animation
        }
        
        public string GetRandomQuestion()
        {
            return crypticQuestions[Random.Range(0, crypticQuestions.Length)];
        }
    }
}