using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player ranged weapon: bow mechanics.
    /// RMB or LT to aim (slows camera, shows reticle), release to fire arrow.
    /// Pools 16 arrow instances.
    /// Auto-attached by CharacterPrefabFactory.
    /// </summary>
    public class PlayerRanged : MonoBehaviour
    {
        [Header("Bow")]
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] Transform firePoint;
        [SerializeField] int poolSize = 16;
        [SerializeField] float aimFOVMultiplier = 0.7f;
        // TODO: aimSensitivityMultiplier should live in CameraController (Tartaria.Camera)
        // to avoid cross-assembly reference. PlayerRanged has no look input handling.
        [SerializeField] float aimSensitivityMultiplier = 0.5f;

        GameObject[] _arrowPool;
        int _nextArrowIndex;
        bool _isAiming;
        float _originalFOV;
        UnityEngine.Camera _camera;

        void Awake()
        {
            _camera = UnityEngine.Camera.main;
            if (_camera != null)
                _originalFOV = _camera.fieldOfView;

            // Create arrow pool
            _arrowPool = new GameObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var arrow = CreateArrow();
                arrow.SetActive(false);
                _arrowPool[i] = arrow;
            }
        }

        GameObject CreateArrow()
        {
            // Procedural arrow: cylinder shaft + cone tip
            var root = new GameObject("Arrow");
            root.transform.SetParent(transform, false);

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            shaft.transform.SetParent(root.transform, false);
            shaft.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(shaft.GetComponent<Collider>());

            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "Tip";
            tip.transform.SetParent(root.transform, false);
            tip.transform.localPosition = new Vector3(0f, 0f, 0.55f);
            tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.15f);
            Object.Destroy(tip.GetComponent<Collider>());

            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = 0.05f;
            capsule.height = 1.2f;
            capsule.direction = 2; // Z-axis
            capsule.isTrigger = true;

            root.AddComponent<ArrowProjectile>();
            return root;
        }

        void Update()
        {
            // Check aim input
            bool aimInput = false;
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed) aimInput = true;
            var pad = Gamepad.current;
            if (pad != null && pad.leftTrigger.isPressed) aimInput = true;

            if (aimInput && !_isAiming)
                StartAim();
            else if (!aimInput && _isAiming)
                ReleaseArrow();
        }

        void StartAim()
        {
            _isAiming = true;
            if (_camera != null)
                _camera.fieldOfView = _originalFOV * aimFOVMultiplier;
            Debug.Log("[PlayerRanged] Aiming...");
        }

        void ReleaseArrow()
        {
            _isAiming = false;
            if (_camera != null)
                _camera.fieldOfView = _originalFOV;

            FireArrow();
        }

        void FireArrow()
        {
            var arrow = _arrowPool[_nextArrowIndex];
            _nextArrowIndex = (_nextArrowIndex + 1) % poolSize;

            // Position at fire point (or player chest if no fire point)
            Vector3 spawnPos = firePoint != null 
                ? firePoint.position 
                : transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
            
            arrow.transform.position = spawnPos;
            arrow.transform.rotation = transform.rotation;
            arrow.transform.SetParent(null, true);
            arrow.SetActive(true);

            // Launch forward
            var projectile = arrow.GetComponent<ArrowProjectile>();
            if (projectile != null)
                projectile.Launch(transform.forward);

            Debug.Log("[PlayerRanged] Arrow fired!");
        }
    }
}
