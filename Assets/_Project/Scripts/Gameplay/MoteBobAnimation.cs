using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Simple bob + rotate animation for collectible motes.
    /// Placed on Golden Mote prefab at creation time.
    /// </summary>
    public class MoteBobAnimation : MonoBehaviour
    {
        [SerializeField] float bobHeight = 0.2f;
        [SerializeField] float bobSpeed = 2f;
        [SerializeField] float rotateSpeed = 90f;

        Vector3 _startPos;

        void Start()
        {
            _startPos = transform.position;
        }

        void Update()
        {
            transform.position = _startPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
