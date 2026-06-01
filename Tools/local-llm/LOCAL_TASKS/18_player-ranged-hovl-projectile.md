# TICKET: PlayerRanged — replace sphere projectile with Hovl VFX

## Output destination
`Assets/_Project/Scripts/Gameplay/PlayerRanged.cs`
**REPLACES the existing file.** Currently spawns a `PrimitiveType.Sphere` for the harmonic-staff projectile.

## Acceptance criteria
- Namespace: `Tartaria.Gameplay`
- One C# file, brace-balanced
- MUST keep ALL existing public API (other systems depend on it). If you don't know the existing API, assume at minimum: `void Fire(Vector3 origin, Vector3 direction, float damage)`.
- Replace the 2 `GameObject.CreatePrimitive` calls with `Tartaria.Integration.HovlVFXBindings.Spawn("crystal_attack", origin, parent: null, autoDestroyAfterSeconds: 4f)`.
- Keep a `// URP-safe` fallback primitive if `HovlVFXBindings.Spawn` returns null.
- Add a `Rigidbody` to the spawned VFX so it travels in `direction * speed`.
- Damage: on collider trigger with an `Enemy` tag, call `enemy.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);`. Destroy projectile after first hit.
- Range: auto-destroy projectile after 30m of travel OR 4 seconds.

## Spec

```csharp
namespace Tartaria.Gameplay
{
    public class PlayerRanged : MonoBehaviour
    {
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileLifetime = 4f;
        [SerializeField] private float damage = 12f;

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
```

## Do NOT
- Don't change `Fire` method signature without preserving the existing one as overload.
- Don't add new `using Tartaria.AI;` (asmdef boundary).
- Don't use `mat.color = ...` — URP needs `SetColor("_BaseColor", ...)`.
- Don't split string literals across lines.
