using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Simple bob + rotate animation for collectible motes.
    /// Placed on Golden Mote prefab at creation time.
    /// </summary>
    public class MoteBobAnimation : MonoBehaviour
    {
        // bobSpeed = 2π ÷ φ ≈ 3.883 rad/s → one full bob cycle every φ seconds (1.618 s).
        // This ties the visual rhythm of discovered markers to the golden-ratio identity
        // that permeates every 432 Hz resonance calculation in the game.
        [SerializeField] float bobHeight = 0.2f;
        [SerializeField] float bobSpeed = 3.8832f; // 2π / φ (golden-ratio period)
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
