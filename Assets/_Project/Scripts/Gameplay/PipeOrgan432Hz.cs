using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Pipe Organ - 432 Hz tuning minigame
    /// 3-note sequences create cymatic patterns, power dome
    /// </summary>
    public class PipeOrgan432Hz : MonoBehaviour
    {
        [Header("Tuning Minigame")]
        public AudioClip[] organNotes432Hz = new AudioClip[12]; // 12-tone scale at 432 Hz
        public float[] targetSequence = { 0f, 4f, 7f }; // C-E-G major triad
        public float tuningTolerance = 0.1f;
        
        [Header("Dome Connection")]
        public GameObject dome;
        public Light domeLight;
        public Material domeMaterial;
        
        [Header("Rose Window Cymatic Projection")]
        public GameObject roseWindow;
        public Projector cymaticProjector;
        public Texture[] cymaticPatterns;
        
        private List<float> playerSequence = new List<float>();
        private AudioSource audioSource;
        private bool isTuned = false;
        
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0.8f;
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 50f;
        }
        
        /// <summary>
        /// Player presses organ key (0-11 for 12-tone scale)
        /// </summary>
        public void PlayNote(int noteIndex)
        {
            if (noteIndex < 0 || noteIndex >= organNotes432Hz.Length) return;
            if (organNotes432Hz[noteIndex] == null) return;
            
            audioSource.PlayOneShot(organNotes432Hz[noteIndex]);
            playerSequence.Add(noteIndex);
            
            Debug.Log($"[Organ] Played note {noteIndex}. Sequence: {string.Join(", ", playerSequence)}");
            
            // Check if sequence matches target
            if (playerSequence.Count == targetSequence.Length)
            {
                CheckTuning();
            }
            else if (playerSequence.Count > targetSequence.Length)
            {
                // Too many notes, reset
                ResetSequence();
            }
        }
        
        void CheckTuning()
        {
            bool matches = true;
            
            for (int i = 0; i < targetSequence.Length; i++)
            {
                if (Mathf.Abs(playerSequence[i] - targetSequence[i]) > tuningTolerance)
                {
                    matches = false;
                    break;
                }
            }
            
            if (matches)
            {
                OnTuningSuccess();
            }
            else
            {
                Debug.Log("[Organ] Sequence incorrect. Try again!");
                ResetSequence();
            }
        }
        
        void OnTuningSuccess()
        {
            isTuned = true;
            Debug.Log("[Organ] ✅ PERFECT TUNING! 432 Hz resonance achieved!");
            
            // Power up dome
            if (dome && domeMaterial)
            {
                domeMaterial.SetColor("_EmissionColor", Color.cyan * 2f);
                domeMaterial.EnableKeyword("_EMISSION");
            }
            
            if (domeLight)
            {
                domeLight.intensity = 3f;
                domeLight.color = Color.cyan;
            }
            
            // Project cymatic pattern
            if (cymaticProjector && cymaticPatterns.Length > 0)
            {
                cymaticProjector.material.mainTexture = cymaticPatterns[Random.Range(0, cymaticPatterns.Length)];
                cymaticProjector.enabled = true;
            }
            
            // TODO: Trigger Aether Energy generation
            // TODO: Play success fanfare
            // TODO: Unlock ley line connection
        }
        
        void ResetSequence()
        {
            playerSequence.Clear();
        }
        
        public bool IsTuned()
        {
            return isTuned;
        }
    }
}