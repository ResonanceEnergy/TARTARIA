# TICKET: Tartaria.AI URP-safe color helper — batch-fix 25+ .color magenta sites

## Output destination
`Assets/_Project/Scripts/AI/AIMaterialHelper.cs`

## Acceptance criteria
- Namespace: `Tartaria.AI`
- Single static utility class, brace-balanced
- Compiles against Unity 6 LTS, assembly `Tartaria.AI`
- No `using` of Tartaria.Integration or Tartaria.UI (asmdef boundaries)
- Provides ONE method other AI scripts can call instead of `renderer.material.color = c`

## Spec

The codebase has 25+ sites in Phase 2/3 enemy AI files that do:
```csharp
_renderer.material.color = Color.red;        // damage flash
mat.color = new Color(...);                   // construction tint
```

Both fail silently on URP (URP material has no `_Color` property — needs `_BaseColor`). Today only MudGolemAI + MudGolemHealth are patched. Need a shared helper that ALL `Tartaria.AI` files can switch to via a one-line replacement.

Public API:

```csharp
namespace Tartaria.AI
{
    public static class AIMaterialHelper
    {
        /// <summary>
        /// URP-safe color set on a Renderer's material. Falls back to .color
        /// if the material's shader doesn't have _BaseColor (Built-in Standard).
        /// Safe on null renderer or null material.
        /// </summary>
        public static void SetColor(Renderer r, Color c) { /* ... */ }

        /// <summary>
        /// URP-safe color set on a Material directly.
        /// </summary>
        public static void SetColor(Material m, Color c) { /* ... */ }

        /// <summary>
        /// URP-safe emission setup. Enables _EMISSION keyword and sets the color.
        /// </summary>
        public static void SetEmission(Material m, Color c, float intensity = 1f) { /* ... */ }

        /// <summary>
        /// Build a URP/Lit material with given base + optional emission. Falls
        /// back to Standard if URP shader not found. Returns null only if both
        /// shaders are missing (Unity in a very broken state).
        /// </summary>
        public static Material BuildUrpLitMaterial(Color baseColor, Color? emissionColor = null, float emissionIntensity = 1.2f) { /* ... */ }
    }
}
```

## Sample implementation

```csharp
using UnityEngine;

namespace Tartaria.AI
{
    public static class AIMaterialHelper
    {
        public static void SetColor(Renderer r, Color c)
        {
            if (r == null) return;
            var m = r.material;
            SetColor(m, c);
        }

        public static void SetColor(Material m, Color c)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else m.color = c;
        }

        public static void SetEmission(Material m, Color c, float intensity = 1f)
        {
            if (m == null) return;
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * intensity);
        }

        public static Material BuildUrpLitMaterial(Color baseColor, Color? emissionColor = null, float emissionIntensity = 1.2f)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (sh == null) return null;
            var m = new Material(sh);
            SetColor(m, baseColor);
            if (emissionColor.HasValue) SetEmission(m, emissionColor.Value, emissionIntensity);
            return m;
        }
    }
}
```

## Do NOT
- Do not modify any other AI file in this ticket (caller-side refactors are a follow-up).
- Do not reference UnityEngine.Rendering.Universal or other URP namespaces directly — `_BaseColor` works via plain Material.SetColor.
- Do not invent helpers for non-color material properties (smoothness/metallic are out of scope).
