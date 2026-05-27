using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Arrow projectile: spawned by PlayerRanged, flies forward with velocity,
    /// raycast-checks for hits on FixedUpdate, applies damage via SendMessage.
    /// Auto-destroys on impact or after 5s timeout.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ArrowProjectile : MonoBehaviour
    {
        [SerializeField] float speed = 25f;
        [SerializeField] int damage = 15;
        [SerializeField] float lifetime = 5f;

        Rigidbody _rb;
        float _spawnTime;
        bool _hasHit;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = true;
            _spawnTime = Time.time;
        }

        public void Launch(Vector3 direction)
        {
            _rb.linearVelocity = direction.normalized * speed;
            transform.forward = direction;
        }

        void FixedUpdate()
        {
            if (_hasHit) return;

            // Raycast forward one physics step
            float stepDistance = _rb.linearVelocity.magnitude * Time.fixedDeltaTime;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stepDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                OnHit(hit);
            }

            // Timeout
            if (Time.time - _spawnTime >= lifetime)
                Destroy(gameObject);
        }

        void OnHit(RaycastHit hit)
        {
            _hasHit = true;
            _rb.linearVelocity = Vector3.zero;

            // Apply damage
            hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // Spawn damage number
            DamageNumberPool.Spawn(damage, hit.point);

            Debug.Log($"[Arrow] Hit {hit.collider.name} for {damage} damage");

            // Stick to surface briefly, then destroy
            transform.position = hit.point;
            transform.SetParent(hit.transform, true);
            Destroy(gameObject, 2f);
        }

        void OnDrawGizmos()
        {
            if (_rb == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + _rb.linearVelocity.normalized * 0.5f);
        }
    }
}
