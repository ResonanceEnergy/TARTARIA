using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Very light, non-threatening first "conflict" enemy for Moon 2 vertical slice.
    /// Spawns after the first vein purge as the immediate Conflict hook.
    /// Slowly moves away from the player and despawns after lifetime.
    /// Teaches the player that "purging brings the shadow's attention".
    /// </summary>
    public class Moon2LightFleeingWraith : MonoBehaviour
    {
        public float lifetime = 9f;
        private float _timer;
        private Transform _player;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            _timer += Time.deltaTime;

            if (_player != null)
            {
                Vector3 dir = (transform.position - _player.position).normalized;
                // Slow flee
                transform.position += dir * 2.2f * Time.deltaTime;
                transform.position = new Vector3(transform.position.x, 1.2f, transform.position.z);
            }

            // Gentle bobbing + fade out near end of life
            float bob = Mathf.Sin(Time.time * 3.5f) * 0.08f;
            transform.position += Vector3.up * bob * Time.deltaTime;

            if (_timer > lifetime - 2.5f)
            {
                var rend = GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    Color c = rend.material.color;
                    c.a = Mathf.Lerp(1f, 0f, (_timer - (lifetime - 2.5f)) / 2.5f);
                    rend.material.color = c;
                }
            }
        }
    }
}