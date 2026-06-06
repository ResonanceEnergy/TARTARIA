using UnityEngine;
using TMPro;
using System.Collections;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Damage number pool: 32 worldspace TMP_Text instances, recycled.
    /// Spawned damage floats up and fades out over 1.2s.
    /// 
    /// Self-bootstraps via [RuntimeInitializeOnLoadMethod].
    /// Usage: DamageNumberPool.Spawn(amount, position)
    /// </summary>
    public class DamageNumberPool : MonoBehaviour
    {
        const int POOL_SIZE = 32;
        const float RISE_SPEED = 2.5f;
        const float LIFETIME = 1.2f;
        const float FONT_SIZE = 48f;

        static DamageNumberPool _instance;
        GameObject[] _pool;
        int _nextIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[DamageNumberPool]");
            _instance = go.AddComponent<DamageNumberPool>();
            DontDestroyOnLoad(go);
            _instance.InitializePool();
        }

        void InitializePool()
        {
            _pool = new GameObject[POOL_SIZE];
            
            for (int i = 0; i < POOL_SIZE; i++)
            {
                var go = new GameObject($"DamageNumber_{i}");
                go.transform.SetParent(transform, false);
                
                var tmp = go.AddComponent<TextMeshPro>();
                tmp.fontSize = FONT_SIZE;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.3f, 0.1f, 1f); // Orange-red
                tmp.fontStyle = FontStyles.Bold;
                tmp.sortingOrder = 100;
                
                go.SetActive(false);
                _pool[i] = go;
            }
        }

        public static void Spawn(int damage, Vector3 worldPosition)
        {
            if (_instance == null) Bootstrap();
            _instance.DoSpawn(damage, worldPosition);
        }

        void DoSpawn(int damage, Vector3 worldPosition)
        {
            var go = _pool[_nextIndex];
            _nextIndex = (_nextIndex + 1) % POOL_SIZE;

            go.transform.position = worldPosition + Vector3.up * 1.5f;
            go.transform.rotation = UnityEngine.Camera.main != null 
                ? UnityEngine.Camera.main.transform.rotation 
                : Quaternion.identity;

            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = damage.ToString();
            tmp.color = new Color(1f, 0.3f, 0.1f, 1f);

            go.SetActive(true);
            StartCoroutine(AnimateDamageNumber(go, tmp));
        }

        IEnumerator AnimateDamageNumber(GameObject go, TextMeshPro tmp)
        {
            float elapsed = 0f;
            Vector3 startPos = go.transform.position;
            Color startColor = tmp.color;

            while (elapsed < LIFETIME)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / LIFETIME;

                // Rise
                go.transform.position = startPos + Vector3.up * (RISE_SPEED * elapsed);
                
                // Fade
                Color c = startColor;
                c.a = 1f - t;
                tmp.color = c;

                // Billboard toward camera
                if (UnityEngine.Camera.main != null)
                    go.transform.rotation = UnityEngine.Camera.main.transform.rotation;

                yield return null;
            }

            go.SetActive(false);
        }
    }
}
