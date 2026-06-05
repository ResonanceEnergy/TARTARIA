// File: Assets/_Project/Scripts/Gameplay/PlayerRanged.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Gameplay
{
    public class PlayerRanged : MonoBehaviour
    {
        erializeField] private float projectileSpeed = 18f;
        erializeField] private float projectileLifetime = 4f;
        erializeField] private float damage = 12f;

        public void Fire(Vector3 origin, Vector3 direction)
        {
            // 1. Try Hovl VFX first
            var fx = Tartaria.Integration.HovlVFXBindings.Spawn(
                "crystal_attack", origin, parent: null, autoDestroyAfterSeconds: projectileLifetime);

            GameObject projectile = fx;
            if (projectile == null)
            {
                // 2. Fallback to a small URP-safe sphere
                projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                projectile.transform.localScale = Vector3.one * 0.3f;
                projectile.transform.position = origin;
                var sh = Shader.Find("Universal Render Pipeline/Lit");
                if (sh != null)
                {
                    var mat = new Material(sh);
                    mat.SetColor("_BaseColor", new Color(0.4f, 0.7f, 1f));
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.4f, 0.7f, 1f) * 1.4f);
                    projectile.GetComponent<Renderer>().sharedMaterial = mat;
                }
                Destroy(projectile, projectileLifetime);
            }

            // 3. Add motion via Rigidbody
            var rb = projectile.GetComponent<Rigidbody>() ?? projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = direction.normalized * projectileSpeed;

            // 4. Damage on contact
            var hitter = projectile.AddComponent<_RangedHit>();
            hitter.damage = (int)damage;

            // 5. Trigger collider if missing
            var col = projectile.GetComponent<SphereCollider>() ?? projectile.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    public class _RangedHit : MonoBehaviour
    {
        public int damage = 12;
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
