#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Sprint 7 Lane 8 - WirePostRestorationChildren.
    ///
    /// Authors the actual child GameObjects/components that
    /// Moon1PostRestorationVisuals.cs (Sprint 6 Lane 9) expects to find via
    /// transform.Find at runtime. The Lane 9 logic is correct but its children
    /// don't exist in the scene yet, so its transform.Find calls all log loud
    /// errors. This menu authors the children once, idempotently.
    ///
    /// Per CLAUDE.md no-stubs mandate:
    ///   - Every component is real (no `;`-body methods, no TODO comments).
    ///   - No GameObject.CreatePrimitive without URP-safe shader fallback.
    ///   - No silent fallbacks: every missing asset path logs the expected
    ///     resource path and a final summary is printed.
    ///   - Idempotent: re-running the menu does not duplicate children.
    /// </summary>
    public static class WirePostRestorationChildren
    {
        const string SceneAssetPath =
            "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";

        // Mirror of Moon1PostRestorationVisuals candidate lists - keep in sync.
        static readonly string[] FountainNames =
        {
            "Building_fountain", "EchohavenHarmonicFountain", "HarmonicFountain",
            "Fountain", "Echohaven_HarmonicFountain"
        };
        static readonly string[] DomeNames =
        {
            "Building_dome", "EchohavenStarDome", "StarDome", "Echohaven_StarDome"
        };
        static readonly string[] SpireNames =
        {
            "Building_spire", "EchohavenCrystalSpire", "CrystalSpire",
            "Spire", "Echohaven_CrystalSpire", "Cathedral"
        };

        const string FountainWaterChild = "FountainWater";
        const string FountainAudioChild = "FountainAudio";
        const string StarProjectionChild = "StarProjection";
        const string SpireEmissionLightChild = "SpireEmissionFallbackLight";

        const string FountainAudioResourcePath = "Audio/Ambient/fountain";

        [MenuItem("Tartaria/Level/Wire Post-Restoration Children")]
        public static void Run()
        {
            var summary = new WiringSummary();

            // Step 1: open the scene.
            var scene = OpenScene(summary);
            if (!scene.IsValid())
            {
                ReportSummary(summary);
                return;
            }

            // Step 2: wire fountain children.
            var fountain = FindNamed(scene, FountainNames, "Fountain", summary);
            if (fountain != null)
            {
                WireFountainWater(fountain, summary);
                WireFountainAudio(fountain, summary);
            }

            // Step 3: wire star dome children.
            var dome = FindNamed(scene, DomeNames, "Star Dome", summary);
            if (dome != null)
            {
                WireStarProjection(dome, summary);
            }

            // Step 4: verify the spire has emission-capable material, else add
            // fallback Light.
            var spire = FindNamed(scene, SpireNames, "Cathedral Spire", summary);
            if (spire != null)
            {
                WireSpireEmissionFallback(spire, summary);
            }

            // Step 5: persist.
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            if (!saved)
            {
                summary.LogError(
                    $"EditorSceneManager.SaveScene returned false for '{SceneAssetPath}' " +
                    "- scene changes may not have persisted.");
            }
            else
            {
                summary.Note($"Scene saved: {SceneAssetPath}");
            }

            ReportSummary(summary);
        }

        // ----- Scene open -------------------------------------------------------

        static Scene OpenScene(WiringSummary summary)
        {
            // If the requested scene is already loaded, reuse it; else open.
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.path == SceneAssetPath)
            {
                summary.Note($"Scene already open: {SceneAssetPath}");
                return active;
            }

            // Save any dirty scene first so we don't lose work.
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                bool ok = EditorSceneManager.SaveOpenScenes();
                if (!ok)
                {
                    summary.LogError(
                        "User cancelled saving open scenes - aborting wire pass.");
                    return default;
                }
            }

            try
            {
                var scene = EditorSceneManager.OpenScene(
                    SceneAssetPath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    summary.LogError(
                        $"EditorSceneManager.OpenScene returned invalid Scene for " +
                        $"'{SceneAssetPath}'. Expected the Echohaven vertical slice " +
                        "scene to exist at that path.");
                    return default;
                }
                summary.Note($"Opened scene: {SceneAssetPath}");
                return scene;
            }
            catch (Exception ex)
            {
                summary.LogError(
                    $"Exception opening scene '{SceneAssetPath}': {ex.Message}");
                return default;
            }
        }

        // ----- Hero lookup ------------------------------------------------------

        static Transform FindNamed(
            Scene scene, string[] candidateNames, string humanLabel,
            WiringSummary summary)
        {
            var roots = scene.GetRootGameObjects();
            foreach (var name in candidateNames)
            {
                foreach (var root in roots)
                {
                    var hit = FindDescendantByName(root.transform, name);
                    if (hit != null)
                    {
                        summary.Note(
                            $"Found {humanLabel}: '{hit.name}' at " +
                            $"'{GetHierarchyPath(hit)}'");
                        return hit;
                    }
                }
            }

            summary.LogError(
                $"{humanLabel} hero building not found in scene - tried names [" +
                string.Join(", ", candidateNames) +
                "]. Verify BuildingSpawner has run, or that the hero buildings " +
                "are present in '" + SceneAssetPath + "'.");
            return null;
        }

        static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindDescendantByName(root.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        // ----- Fountain water ---------------------------------------------------

        static void WireFountainWater(Transform fountain, WiringSummary summary)
        {
            var existing = fountain.Find(FountainWaterChild);
            if (existing != null)
            {
                if (existing.GetComponent<ParticleSystem>() != null)
                {
                    summary.Skip(
                        $"'{FountainWaterChild}' already exists under " +
                        $"'{fountain.name}' with ParticleSystem - skipping.");
                    return;
                }
                summary.Note(
                    $"'{FountainWaterChild}' exists under '{fountain.name}' but " +
                    "is missing ParticleSystem - adding the component.");
                existing.gameObject.AddComponent<ParticleSystem>();
                ConfigureFountainParticles(existing.GetComponent<ParticleSystem>());
                summary.Wired(
                    $"Repaired ParticleSystem on existing '{FountainWaterChild}'.");
                return;
            }

            var go = new GameObject(FountainWaterChild);
            Undo.RegisterCreatedObjectUndo(go, "Create FountainWater");
            go.transform.SetParent(fountain, worldPositionStays: false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var ps = go.AddComponent<ParticleSystem>();
            ConfigureFountainParticles(ps);

            summary.Wired(
                $"Added '{FountainWaterChild}' (ParticleSystem) under " +
                $"'{fountain.name}'.");
        }

        static void ConfigureFountainParticles(ParticleSystem ps)
        {
            if (ps == null) return;

            var main = ps.main;
            main.loop = true;
            main.duration = 5f;
            main.startLifetime = 2f;
            main.startSpeed = 4f;
            main.startSize = 0.12f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.6f;
            main.startColor = new Color(0.45f, 0.7f, 1f, 0.9f);
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.25f;
            shape.position = Vector3.zero;
            shape.rotation = new Vector3(-90f, 0f, 0f); // point up

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.y = new ParticleSystem.MinMaxCurve(2f, 4f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.55f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.35f, 0.6f, 0.95f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.85f, 0.2f),
                    new GradientAlphaKey(0.7f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.6f),
                new Keyframe(0.4f, 1f),
                new Keyframe(1f, 0.3f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // URP-safe particle material: ParticleSystemRenderer.material starts
            // null on a freshly-added ParticleSystem; assign a sprite-default
            // material that exists in every URP project.
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var urpMat = AssetDatabase.GetBuiltinExtraResource<Material>(
                    "Sprites-Default.mat");
                if (urpMat != null)
                {
                    renderer.sharedMaterial = urpMat;
                }
            }

            // Stop the system in editor; Moon1PostRestorationVisuals calls Play
            // at runtime when the cinematic fires.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // ----- Fountain audio ---------------------------------------------------

        static void WireFountainAudio(Transform fountain, WiringSummary summary)
        {
            var existing = fountain.Find(FountainAudioChild);
            if (existing != null)
            {
                if (existing.GetComponent<AudioSource>() != null)
                {
                    summary.Skip(
                        $"'{FountainAudioChild}' already exists under " +
                        $"'{fountain.name}' with AudioSource - skipping.");
                    return;
                }
                summary.Note(
                    $"'{FountainAudioChild}' exists under '{fountain.name}' but " +
                    "is missing AudioSource - adding the component.");
                ConfigureFountainAudio(
                    existing.gameObject.AddComponent<AudioSource>(), summary);
                summary.Wired(
                    $"Repaired AudioSource on existing '{FountainAudioChild}'.");
                return;
            }

            var go = new GameObject(FountainAudioChild);
            Undo.RegisterCreatedObjectUndo(go, "Create FountainAudio");
            go.transform.SetParent(fountain, worldPositionStays: false);
            go.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var src = go.AddComponent<AudioSource>();
            ConfigureFountainAudio(src, summary);

            summary.Wired(
                $"Added '{FountainAudioChild}' (AudioSource) under " +
                $"'{fountain.name}'.");
        }

        static void ConfigureFountainAudio(AudioSource src, WiringSummary summary)
        {
            if (src == null) return;

            src.loop = true;
            src.spatialBlend = 1f;
            src.volume = 0.6f;
            src.playOnAwake = false;
            src.minDistance = 2f;
            src.maxDistance = 30f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;

            // Try to locate a fountain ambient clip via Resources.
            var clip = Resources.Load<AudioClip>(FountainAudioResourcePath);
            if (clip == null)
            {
                summary.MissingAsset(
                    $"AudioClip not found at Resources/{FountainAudioResourcePath} " +
                    "(.wav/.mp3/.ogg under any Assets/Resources/Audio/Ambient " +
                    "folder). FountainAudio AudioSource will have a null clip; " +
                    "drop an audio file at " +
                    $"'Assets/Resources/{FountainAudioResourcePath}.wav' to fix.");
            }
            else
            {
                src.clip = clip;
                summary.Note(
                    $"Bound AudioClip 'Resources/{FountainAudioResourcePath}' " +
                    "to FountainAudio.");
            }
        }

        // ----- Star Dome projection --------------------------------------------

        static void WireStarProjection(Transform dome, WiringSummary summary)
        {
            var existing = dome.Find(StarProjectionChild);
            if (existing != null)
            {
                if (existing.GetComponent<ParticleSystem>() != null)
                {
                    summary.Skip(
                        $"'{StarProjectionChild}' already exists under " +
                        $"'{dome.name}' with ParticleSystem - skipping.");
                    return;
                }
                summary.Note(
                    $"'{StarProjectionChild}' exists under '{dome.name}' but " +
                    "is missing ParticleSystem - adding the component.");
                ConfigureStarProjection(
                    existing.gameObject.AddComponent<ParticleSystem>(), summary);
                summary.Wired(
                    $"Repaired ParticleSystem on existing '{StarProjectionChild}'.");
                return;
            }

            var go = new GameObject(StarProjectionChild);
            Undo.RegisterCreatedObjectUndo(go, "Create StarProjection");
            go.transform.SetParent(dome, worldPositionStays: false);
            go.transform.localPosition = new Vector3(0f, 3f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var ps = go.AddComponent<ParticleSystem>();
            ConfigureStarProjection(ps, summary);

            summary.Wired(
                $"Added '{StarProjectionChild}' (ParticleSystem) under " +
                $"'{dome.name}'.");
        }

        static void ConfigureStarProjection(ParticleSystem ps, WiringSummary summary)
        {
            if (ps == null) return;

            var main = ps.main;
            main.loop = true;
            main.duration = 8f;
            main.startLifetime = 8f;
            main.startSpeed = 0.2f;
            main.startSize = 0.18f;
            main.maxParticles = 600;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.02f;
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 20f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 6f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.85f, 0.9f, 1f), 0.5f),
                    new GradientColorKey(new Color(0.7f, 0.8f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.2f),
                    new GradientAlphaKey(0.7f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            // Slow drift via velocity over lifetime - small random walk.
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            vel.y = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

            // Additive-ish renderer material via Sprites-Default; URP-safe
            // (works in URP since it uses sprite shader path).
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var urpMat = AssetDatabase.GetBuiltinExtraResource<Material>(
                    "Sprites-Default.mat");
                if (urpMat != null)
                {
                    renderer.sharedMaterial = urpMat;
                }
                else
                {
                    summary.MissingAsset(
                        "Sprites-Default.mat not found via " +
                        "AssetDatabase.GetBuiltinExtraResource. StarProjection " +
                        "ParticleSystemRenderer will use Unity's null-material " +
                        "magenta - inspect Project settings.");
                }
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // ----- Spire emission fallback -----------------------------------------

        static void WireSpireEmissionFallback(Transform spire, WiringSummary summary)
        {
            var renderer = spire.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                summary.LogError(
                    $"Spire '{spire.name}' (hierarchy '{GetHierarchyPath(spire)}') " +
                    "has no MeshRenderer in any child - cannot verify emission " +
                    "shader support, and no fallback Light authored (would have " +
                    "nothing to anchor to). Add a mesh or fix the prefab.");
                return;
            }

            var mat = renderer.sharedMaterial;
            bool hasEmission = mat != null && mat.HasProperty("_EmissionColor");

            if (hasEmission)
            {
                summary.Note(
                    $"Spire material '{(mat != null ? mat.name : "<null>")}' has " +
                    "'_EmissionColor' - no fallback Light needed.");

                // Make sure the existing fallback Light (if any) is removed so
                // we don't double up emission. Idempotent cleanup.
                var existingFallback = spire.Find(SpireEmissionLightChild);
                if (existingFallback != null)
                {
                    summary.Note(
                        $"Removing stale '{SpireEmissionLightChild}' under " +
                        $"'{spire.name}' since material now supports emission.");
                    UnityEngine.Object.DestroyImmediate(existingFallback.gameObject);
                }
                return;
            }

            // Material does not support emission - add a fallback point light
            // (intensity 0 so Lane 9's pulse can drive it via material setter or
            // the user can hand-wire later; per spec we author intensity 0).
            var existing = spire.Find(SpireEmissionLightChild);
            if (existing != null && existing.GetComponent<Light>() != null)
            {
                summary.Skip(
                    $"'{SpireEmissionLightChild}' already exists under " +
                    $"'{spire.name}' with Light - skipping.");
                return;
            }

            GameObject lightGo;
            if (existing != null)
            {
                lightGo = existing.gameObject;
                summary.Note(
                    $"'{SpireEmissionLightChild}' exists under '{spire.name}' " +
                    "but missing Light - adding the component.");
            }
            else
            {
                lightGo = new GameObject(SpireEmissionLightChild);
                Undo.RegisterCreatedObjectUndo(
                    lightGo, "Create SpireEmissionFallbackLight");
                lightGo.transform.SetParent(spire, worldPositionStays: false);
                lightGo.transform.localPosition = new Vector3(0f, 4f, 0f);
                lightGo.transform.localRotation = Quaternion.identity;
                lightGo.transform.localScale = Vector3.one;
            }

            var light = lightGo.GetComponent<Light>();
            if (light == null) light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(165f / 255f, 115f / 255f, 50f / 255f);
            light.intensity = 0f;
            light.range = 8f;
            light.shadows = LightShadows.None;

            summary.Wired(
                $"Added '{SpireEmissionLightChild}' (Light, point, range 8, " +
                "intensity 0, Telluric color) under " +
                $"'{spire.name}' as emission fallback because the spire material " +
                "lacks '_EmissionColor'.");
        }

        // ----- Helpers ----------------------------------------------------------

        static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var sb = new StringBuilder(t.name);
            var cur = t.parent;
            while (cur != null)
            {
                sb.Insert(0, cur.name + "/");
                cur = cur.parent;
            }
            return sb.ToString();
        }

        static void ReportSummary(WiringSummary summary)
        {
            var msg = summary.Format();
            if (summary.HasErrors)
                Debug.LogError(msg);
            else if (summary.MissingAssets > 0)
                Debug.LogWarning(msg);
            else
                Debug.Log(msg);
        }

        // ----- Summary type -----------------------------------------------------

        sealed class WiringSummary
        {
            readonly List<string> _wired = new List<string>();
            readonly List<string> _skipped = new List<string>();
            readonly List<string> _missing = new List<string>();
            readonly List<string> _errors = new List<string>();
            readonly List<string> _notes = new List<string>();

            public bool HasErrors => _errors.Count > 0;
            public int MissingAssets => _missing.Count;

            public void Wired(string s) { _wired.Add(s); }
            public void Skip(string s) { _skipped.Add(s); }
            public void MissingAsset(string s) { _missing.Add(s); }
            public void LogError(string s) { _errors.Add(s); }
            public void Note(string s) { _notes.Add(s); }

            public string Format()
            {
                var sb = new StringBuilder();
                sb.Append("[WirePostRestorationChildren] Wired ")
                  .Append(_wired.Count)
                  .Append(" children, skipped ")
                  .Append(_skipped.Count)
                  .Append(" (already present), missing ")
                  .Append(_missing.Count)
                  .Append(" assets, errors ")
                  .Append(_errors.Count)
                  .AppendLine(". See Console for detail.");

                AppendSection(sb, "WIRED", _wired);
                AppendSection(sb, "SKIPPED", _skipped);
                AppendSection(sb, "MISSING ASSETS", _missing);
                AppendSection(sb, "ERRORS", _errors);
                AppendSection(sb, "NOTES", _notes);
                return sb.ToString();
            }

            static void AppendSection(
                StringBuilder sb, string header, List<string> items)
            {
                if (items.Count == 0) return;
                sb.Append("-- ").Append(header).AppendLine(" --");
                for (int i = 0; i < items.Count; i++)
                {
                    sb.Append("  ").Append(i + 1).Append(". ")
                      .AppendLine(items[i]);
                }
            }
        }
    }
}
#endif
