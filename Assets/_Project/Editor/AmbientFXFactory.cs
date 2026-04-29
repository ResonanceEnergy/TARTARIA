using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Adds particle systems to the scene + character/building prefabs:
    ///   - Floating "aether motes" volume that drifts around the playable area
    ///   - Footstep dust trigger on the Player prefab (always-on emit, low rate)
    ///   - Glowing aura around each Echohaven building
    /// All configured via the built-in ParticleSystem (no VFX Graph dependency).
    /// </summary>
    public static class AmbientFXFactory
    {
        /// <summary>
        /// Build prefab-level effects (Player footstep, building auras).
        /// Run during Phase 6/9 after prefabs exist.
        /// </summary>
        public static void DecorateAllPrefabs()
        {
            DecoratePlayerPrefab();
            DecorateBuildingPrefabs();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Add scene-level ambient particle volume. Run during Phase 9.
        /// </summary>
        public static void AddAmbientToScene()
        {
            var existing = GameObject.Find("AmbientAetherMotes");
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject("AmbientAetherMotes");
            go.transform.position = new Vector3(0f, 8f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ConfigureMotes(ps);
            Debug.Log("[Tartaria] AmbientAetherMotes added to scene (drift volume).");
        }

        // ── Player footstep dust ──────────────────────────────────────────────
        static void DecoratePlayerPrefab()
        {
            const string path = "Assets/_Project/Prefabs/Characters/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogWarning("[Tartaria] Player prefab not found, skipping FX."); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            // Footstep dust at feet
            var feetFx = instance.transform.Find("FootstepDust");
            if (feetFx != null) Object.DestroyImmediate(feetFx.gameObject);
            var fx = new GameObject("FootstepDust");
            fx.transform.SetParent(instance.transform, false);
            fx.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var ps = fx.AddComponent<ParticleSystem>();
            ConfigureFootstep(ps);

            // PlayerAnimator component (procedural limb animator)
            var animType = System.Type.GetType("Tartaria.Gameplay.PlayerAnimator, Tartaria.Gameplay");
            if (animType != null && instance.GetComponent(animType) == null)
                instance.AddComponent(animType);

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            Debug.Log("[Tartaria] Player prefab decorated (FootstepDust + PlayerAnimator).");
        }

        // ── Building auras ────────────────────────────────────────────────────
        static void DecorateBuildingPrefabs()
        {
            string[] buildings = {
                "Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab",
                "Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab",
                "Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab",
            };
            Color[] tints = {
                new Color(0.5f, 0.8f, 1.0f),   // dome — cyan
                new Color(1.0f, 0.85f, 0.4f),  // fountain — gold
                new Color(0.8f, 0.5f, 1.0f),   // spire — violet
            };

            for (int i = 0; i < buildings.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(buildings[i]);
                if (prefab == null) continue;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                var existing = instance.transform.Find("Aura");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);

                var fx = new GameObject("Aura");
                fx.transform.SetParent(instance.transform, false);
                fx.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                var ps = fx.AddComponent<ParticleSystem>();
                ConfigureAura(ps, tints[i]);

                // Add a soft point light too
                var lightGo = instance.transform.Find("AuraLight");
                if (lightGo != null) Object.DestroyImmediate(lightGo.gameObject);
                lightGo = new GameObject("AuraLight").transform;
                lightGo.SetParent(instance.transform, false);
                lightGo.localPosition = new Vector3(0f, 4f, 0f);
                var light = lightGo.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = tints[i];
                light.intensity = 3f;
                light.range = 18f;
                light.shadows = LightShadows.Soft;

                PrefabUtility.SaveAsPrefabAsset(instance, buildings[i]);
                Object.DestroyImmediate(instance);
            }
            Debug.Log("[Tartaria] 3 building prefabs decorated (Aura + Light).");
        }

        // ── Particle configurations ───────────────────────────────────────────
        static void ConfigureMotes(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 8f;
            main.startSpeed = 0.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = new Color(0.7f, 0.9f, 1.0f, 0.7f);
            main.maxParticles = 600;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f;

            var emission = ps.emission;
            emission.rateOverTime = 60f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(120f, 8f, 120f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.7f, 0.9f, 1.0f), 0f), new GradientColorKey(new Color(0.85f, 1.0f, 0.95f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.7f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            color.color = new ParticleSystem.MinMaxGradient(grad);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = LoadOrCreateAdditiveMaterial();
        }

        static void ConfigureFootstep(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 0.6f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = new Color(0.8f, 0.75f, 0.6f, 0.6f);
            main.gravityModifier = -0.1f;
            main.maxParticles = 80;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 6f; // emits as the player moves

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.85f, 0.8f, 0.65f), 0f), new GradientColorKey(new Color(0.7f, 0.65f, 0.5f), 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            color.color = new ParticleSystem.MinMaxGradient(grad);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = LoadOrCreateAdditiveMaterial();
        }

        static void ConfigureAura(ParticleSystem ps, Color tint)
        {
            var main = ps.main;
            main.duration = 4f;
            main.loop = true;
            main.startLifetime = 3f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            main.startColor = new Color(tint.r, tint.g, tint.b, 0.85f);
            main.gravityModifier = -0.2f;
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 18f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Donut;
            shape.radius = 4f;
            shape.donutRadius = 0.6f;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(tint, 0f), new GradientColorKey(tint * 0.8f, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            color.color = new ParticleSystem.MinMaxGradient(grad);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0.2f));
            size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = LoadOrCreateAdditiveMaterial();
        }

        // ── Shared additive material for particles ────────────────────────────
        static Material LoadOrCreateAdditiveMaterial()
        {
            const string path = "Assets/_Project/Materials/M_Particle_Additive.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            mat = new Material(shader);
            mat.name = "M_Particle_Additive";
            // Additive blend
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 1f); // Additive
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            mat.renderQueue = 3000;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
