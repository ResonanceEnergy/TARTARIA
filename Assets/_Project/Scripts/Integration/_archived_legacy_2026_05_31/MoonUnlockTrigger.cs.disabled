using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon Unlock Trigger — auto-unlocks Moon content when player enters trigger.
    /// Simpler than manual QuestManager calls. Attach to trigger collider in zone.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MoonUnlockTrigger : MonoBehaviour
    {
        [Header("Moon Config")]
        [SerializeField, Range(2, 13)] int moonNumber = 2;
        [SerializeField] bool triggerOnce = true;

        bool _triggered;

        void Awake()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_triggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;

            UnlockMoon();
        }

        void UnlockMoon()
        {
            _triggered = true;

            Debug.Log($"[MoonUnlockTrigger] Unlocking Moon {moonNumber}");

            // Find appropriate spawner by moonNumber
            MonoBehaviour spawner = null;
            switch (moonNumber)
            {
                case 2:
                    spawner = FindFirstObjectByType<Moon2ContentSpawner>();
                    if (spawner != null) ((Moon2ContentSpawner)spawner).UnlockMoon2();
                    break;
                case 3:
                    spawner = FindFirstObjectByType<Moon3ContentSpawner>();
                    if (spawner != null) ((Moon3ContentSpawner)spawner).UnlockMoon3();
                    break;
                case 4:
                    spawner = FindFirstObjectByType<Moon4ContentSpawner>();
                    if (spawner != null) ((Moon4ContentSpawner)spawner).UnlockMoon4();
                    break;
                case 5:
                    spawner = FindFirstObjectByType<Moon5ContentSpawner>();
                    if (spawner != null) ((Moon5ContentSpawner)spawner).UnlockMoon5();
                    break;
                case 6:
                    spawner = FindFirstObjectByType<Moon6ContentSpawner>();
                    if (spawner != null) ((Moon6ContentSpawner)spawner).UnlockMoon6();
                    break;
                case 7:
                    spawner = FindFirstObjectByType<Moon7ContentSpawner>();
                    if (spawner != null) ((Moon7ContentSpawner)spawner).UnlockMoon7();
                    break;
                case 8:
                    spawner = FindFirstObjectByType<Moon8ContentSpawner>();
                    if (spawner != null) ((Moon8ContentSpawner)spawner).UnlockMoon8();
                    break;
                case 9:
                    spawner = FindFirstObjectByType<Moon9ContentSpawner>();
                    if (spawner != null) ((Moon9ContentSpawner)spawner).UnlockMoon9();
                    break;
                case 10:
                    spawner = FindFirstObjectByType<Moon10ContentSpawner>();
                    if (spawner != null) ((Moon10ContentSpawner)spawner).UnlockMoon10();
                    break;
                case 11:
                    spawner = FindFirstObjectByType<Moon11ContentSpawner>();
                    if (spawner != null) ((Moon11ContentSpawner)spawner).UnlockMoon11();
                    break;
                case 12:
                    spawner = FindFirstObjectByType<Moon12ContentSpawner>();
                    if (spawner != null) ((Moon12ContentSpawner)spawner).UnlockMoon12();
                    break;
                case 13:
                    spawner = FindFirstObjectByType<Moon13ContentSpawner>();
                    if (spawner != null) ((Moon13ContentSpawner)spawner).UnlockMoon13();
                    break;
            }

            if (spawner == null)
            {
                Debug.LogWarning($"[MoonUnlockTrigger] Moon {moonNumber} spawner not found in scene");
            }
        }

        void OnDrawGizmos()
        {
            // Draw trigger bounds with moon number label
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.4f);  // Golden for moon unlock
                Gizmos.matrix = transform.localToWorldMatrix;

                if (collider is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
