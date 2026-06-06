using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Hit-stop system: brief Time.timeScale pause on hit-confirmed for visceral impact.
    /// Scaled by damage magnitude. Auto-restores after duration.
    /// 
    /// Self-bootstraps via [RuntimeInitializeOnLoadMethod].
    /// Usage: HitStopController.Trigger(damage) from PlayerCombat/enemy hit.
    /// </summary>
    public class HitStopController : MonoBehaviour
    {
        const float BASE_DURATION = 0.06f;
        const float SCALE_PER_DAMAGE = 0.001f;
        const float MAX_DURATION = 0.10f;
        const float HIT_STOP_TIMESCALE = 0.05f;

        static HitStopController _instance;
        float _restoreTime = -1f;
        float _originalTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[HitStopController]");
            _instance = go.AddComponent<HitStopController>();
            DontDestroyOnLoad(go);
        }

        public static void Trigger(int damage)
        {
            if (_instance == null) Bootstrap();
            _instance.DoHitStop(damage);
        }

        void DoHitStop(int damage)
        {
            float duration = Mathf.Min(BASE_DURATION + damage * SCALE_PER_DAMAGE, MAX_DURATION);
            
            _originalTimeScale = Time.timeScale;
            Time.timeScale = HIT_STOP_TIMESCALE;
            _restoreTime = Time.realtimeSinceStartup + duration;
        }

        void Update()
        {
            if (_restoreTime > 0f && Time.realtimeSinceStartup >= _restoreTime)
            {
                Time.timeScale = _originalTimeScale;
                _restoreTime = -1f;
            }
        }
    }
}
