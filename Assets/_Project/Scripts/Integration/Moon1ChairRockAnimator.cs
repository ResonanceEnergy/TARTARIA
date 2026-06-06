using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Gentle back-and-forth rock animation for a rocking-chair GameObject.
    /// Authored onto the chair root inside AnastasiaRocker.prefab by Moon1AnastasiaRockerBake.
    /// Split into its own file (2026-06-06) so Unity generates a MonoScript asset —
    /// classes sharing a file can never be serialized into prefabs (m_Script fileID 0 bug).
    /// </summary>
    public class Moon1ChairRockAnimator : MonoBehaviour
    {
        public float amplitudeDeg = 6f;
        public float speed = 1.2f;
        float _phase;

        void Awake() { _phase = Random.Range(0f, Mathf.PI * 2f); }

        void Update()
        {
            float angle = Mathf.Sin(Time.time * speed + _phase) * amplitudeDeg;
            var e = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(angle, e.y, e.z);
        }
    }
}
