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

        Vector3 _startLocalPos;
        Vector3 _bobOffset;

        void Start()
        {
            _startLocalPos = transform.localPosition;
        }

        void Update()
        {
            _bobOffset.y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = _startLocalPos + _bobOffset;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
