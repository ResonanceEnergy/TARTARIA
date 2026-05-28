using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Giant-Mode System - Player grows to 15ft tall for 60 seconds
    /// Triggered by interacting with Giant Skeleton or consuming Aether burst
    /// </summary>
    public class GiantModeController : MonoBehaviour
    {
        [Header("Giant-Mode Settings")]
        public float giantHeight = 4.572f; // 15 feet in meters
        public float normalHeight = 1.8f; // 6 feet in meters
        public float giantDuration = 60f;
        public float giantStrengthMultiplier = 5f;
        
        [Header("Scale Transition")]
        public float transitionSpeed = 2f;
        
        [Header("VFX")]
        public ParticleSystem giantActivationVFX;
        public Color giantAuraColor = new Color(0.4f, 0.7f, 1f, 0.5f);
        
        private bool isGiantMode = false;
        private float giantModeTimer = 0f;
        private Vector3 targetScale;
        private Vector3 normalScale;
        
        void Start()
        {
            normalScale = transform.localScale;
            targetScale = normalScale;
        }
        
        void Update()
        {
            // Smooth scale transition
            if (transform.localScale != targetScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, transitionSpeed * Time.deltaTime);
            }
            
            // Count down giant mode
            if (isGiantMode)
            {
                giantModeTimer -= Time.deltaTime;
                
                if (giantModeTimer <= 0f)
                {
                    DeactivateGiantMode();
                }
            }
        }
        
        /// <summary>
        /// Activates Giant-Mode for 60 seconds
        /// </summary>
        public void ActivateGiantMode()
        {
            if (isGiantMode)
            {
                // Extend duration if already active
                giantModeTimer = Mathf.Min(giantModeTimer + giantDuration, giantDuration * 2f);
                return;
            }
            
            isGiantMode = true;
            giantModeTimer = giantDuration;
            
            float scaleMultiplier = giantHeight / normalHeight;
            targetScale = normalScale * scaleMultiplier;
            
            if (giantActivationVFX)
            {
                giantActivationVFX.Play();
            }
            
            Debug.Log($"[GiantMode] ACTIVATED! 15ft tall for {giantDuration} seconds. Strength ×{giantStrengthMultiplier}");
            // TODO: Play giant roar sound
            // TODO: Enable giant throw mechanics
        }
        
        void DeactivateGiantMode()
        {
            isGiantMode = false;
            targetScale = normalScale;
            
            Debug.Log("[GiantMode] Returned to normal size");
        }
        
        /// <summary>
        /// Called when player attacks/throws in giant mode
        /// </summary>
        public float GetAttackDamage(float baseDamage)
        {
            return isGiantMode ? baseDamage * giantStrengthMultiplier : baseDamage;
        }
        
        public bool IsGiantMode()
        {
            return isGiantMode;
        }
        
        public float GetRemainingTime()
        {
            return giantModeTimer;
        }
    }
}