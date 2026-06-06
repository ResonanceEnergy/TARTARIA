// Moon1RuntimeCameraFollow.cs
// 2026-06-03 — Persistent camera follower for Moon 1.
// Wired by Moon1AutoEnterExploration when no other follow controller is detected
// on Main Camera. Smoothly tracks the player in third-person at offset (0, 4, -8).
//
// Per CLAUDE.md: this is a SAFETY NET, not a replacement for CameraController.cs
// (Tartaria.Camera). If a real follow controller is present, this never gets added.
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>Cheap third-person follow camera used by Moon1AutoEnterExploration's safety net.</summary>
    public class Moon1RuntimeCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 4f, -8f);
        public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
        [Range(1f, 30f)] public float positionLerp = 8f;
        [Range(1f, 30f)] public float rotationLerp = 10f;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * positionLerp);

            Vector3 lookPoint = target.position + lookAtOffset;
            Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationLerp);
        }
    }
}
