#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Regenerates the two Moon 1 character prefabs that were renamed with a
    /// .corrupt suffix to keep Unity from importing busted assets.
    ///
    /// After invoking this menu, NATRIX can safely delete:
    ///   Assets/_Project/Prefabs/Characters/Lirael.prefab.corrupt
    ///   Assets/_Project/Prefabs/Characters/Cassian.prefab.corrupt
    ///
    /// Idempotent — overwrites existing prefabs at the destination paths.
    /// </summary>
    public static class Moon1RegenCorruptCharacters
    {
        private const string CharactersDir = "Assets/_Project/Prefabs/Characters";
        private const string LiraelPath   = CharactersDir + "/Lirael.prefab";
        private const string CassianPath  = CharactersDir + "/Cassian.prefab";

        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";

        [MenuItem("Tartaria/1 Build/Regenerate Corrupt Character Prefabs (Lirael + Cassian)", priority = 110)]
        public static void Run()
        {
            EnsureFolder(CharactersDir);

            var report = new StringBuilder();
            report.AppendLine("Moon 1 character prefab regeneration");
            report.AppendLine("-------------------------------------");

            // ---- Lirael ----
            string liraelResult = BuildLirael(report);

            // ---- Cassian ----
            string cassianResult = BuildCassian(report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            report.AppendLine();
            report.AppendLine("Lirael:  " + liraelResult);
            report.AppendLine("Cassian: " + cassianResult);
            report.AppendLine();
            report.AppendLine("You may now delete the .corrupt sidecars manually.");

            Debug.Log("[Moon1RegenCorruptCharacters] " + report);
            EditorUtility.DisplayDialog(
                "Regenerate Corrupt Character Prefabs",
                report.ToString(),
                "OK");
        }

        // -----------------------------------------------------------------
        // LIRAEL — small, glowing, cyan, child-spirit at the fountain
        // -----------------------------------------------------------------
        private static string BuildLirael(StringBuilder report)
        {
            GameObject root = new GameObject("Lirael");
            try
            {
                // CharacterController — small child-like
                var cc = root.AddComponent<CharacterController>();
                cc.radius = 0.3f;
                cc.height = 1.4f;
                cc.center = new Vector3(0f, 0.7f, 0f);

                // Visual body — emissive cyan sphere
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                body.transform.localScale = Vector3.one * 0.8f;
                StripCollider(body);

                Color cyanBase     = new Color(0.5f, 0.9f, 1.0f, 0.7f);
                Color cyanEmission = new Color(0.3f, 0.6f, 1.0f, 1.0f) * 1.5f;
                ApplyUrpLitMaterial(body, "Lirael_BodyMat", cyanBase, cyanEmission, emissive: true);

                // Child point light
                GameObject lightGo = new GameObject("Glow");
                lightGo.transform.SetParent(root.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                var light = lightGo.AddComponent<Light>();
                light.type      = LightType.Point;
                light.color     = new Color(0.5f, 0.85f, 1.0f, 1.0f);
                light.range     = 5f;
                light.intensity = 0.8f;

                // Optional behaviour component via reflection — avoids hard
                // asmdef dependency from the Editor assembly.
                bool addedLullaby = TryAddComponentByType(
                    root,
                    "Tartaria.Integration.LiraelLullaby, Tartaria.Integration",
                    out string lullabyMsg);
                report.AppendLine("  Lirael component: " + lullabyMsg);

                // Overwrite if it exists
                bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(LiraelPath) != null;
                PrefabUtility.SaveAsPrefabAsset(root, LiraelPath);

                long size = FileSizeBytes(LiraelPath);
                return string.Format(
                    "{0} → {1} ({2} bytes){3}",
                    existed ? "OVERWROTE" : "CREATED",
                    LiraelPath,
                    size,
                    addedLullaby ? " [+LiraelLullaby]" : " [no LiraelLullaby found]");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Moon1RegenCorruptCharacters] Lirael build failed: " + ex);
                return "FAILED — " + ex.Message;
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // -----------------------------------------------------------------
        // CASSIAN — adult, mortal-looking, dark purple, hooded
        // -----------------------------------------------------------------
        private static string BuildCassian(StringBuilder report)
        {
            GameObject root = new GameObject("Cassian");
            try
            {
                // CharacterController — adult proportions
                var cc = root.AddComponent<CharacterController>();
                cc.radius = 0.4f;
                cc.height = 1.85f;
                cc.center = new Vector3(0f, 0.925f, 0f);

                Color darkPurple = new Color(0.25f, 0.15f, 0.3f, 1.0f);

                // Body — capsule (more humanoid than a cube)
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                body.transform.localScale = new Vector3(0.55f, 0.85f, 0.55f);
                StripCollider(body);
                ApplyUrpLitMaterial(body, "Cassian_BodyMat", darkPurple, Color.black, emissive: false);

                // Hooded head — slightly smaller sphere, same dark colour
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "HoodedHead";
                head.transform.SetParent(root.transform, false);
                head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
                head.transform.localScale = Vector3.one * 0.35f;
                StripCollider(head);
                ApplyUrpLitMaterial(head, "Cassian_HoodMat", darkPurple, Color.black, emissive: false);

                bool addedController = TryAddComponentByType(
                    root,
                    "Tartaria.Integration.CassianController, Tartaria.Integration",
                    out string ctrlMsg);
                report.AppendLine("  Cassian component: " + ctrlMsg);

                bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(CassianPath) != null;
                PrefabUtility.SaveAsPrefabAsset(root, CassianPath);

                long size = FileSizeBytes(CassianPath);
                return string.Format(
                    "{0} → {1} ({2} bytes){3}",
                    existed ? "OVERWROTE" : "CREATED",
                    CassianPath,
                    size,
                    addedController ? " [+CassianController]" : " [no CassianController found]");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Moon1RegenCorruptCharacters] Cassian build failed: " + ex);
                return "FAILED — " + ex.Message;
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf   = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void StripCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);
        }

        /// <summary>
        /// URP-safe material setup. Uses Universal Render Pipeline/Lit and the
        /// _BaseColor / _EmissionColor properties so it survives a build.
        /// </summary>
        private static void ApplyUrpLitMaterial(
            GameObject go,
            string materialName,
            Color baseColor,
            Color emissionColor,
            bool emissive)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) return;

            Shader shader = Shader.Find(UrpLitShaderName);
            if (shader == null)
            {
                // Fallback — Standard so we at least get a colour. URP-safe path
                // is the primary; this is the safety net.
                shader = Shader.Find("Standard");
            }

            var mat = new Material(shader) { name = materialName };

            // URP-safe color assignment
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", baseColor);

            // Transparency for Lirael (alpha < 1) — flip Surface Type to Transparent on URP/Lit
            if (baseColor.a < 0.999f && shader != null && shader.name == UrpLitShaderName)
            {
                if (mat.HasProperty("_Surface"))  mat.SetFloat("_Surface", 1f); // 1 = Transparent
                if (mat.HasProperty("_Blend"))    mat.SetFloat("_Blend", 0f);   // Alpha
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite"))   mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
                mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (emissive)
            {
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }

            renderer.sharedMaterial = mat;
        }

        private static bool TryAddComponentByType(GameObject go, string assemblyQualifiedName, out string message)
        {
            try
            {
                Type t = Type.GetType(assemblyQualifiedName);
                if (t == null)
                {
                    message = "type not found (" + assemblyQualifiedName + ")";
                    return false;
                }
                if (!typeof(Component).IsAssignableFrom(t))
                {
                    message = "type is not a Component (" + t.FullName + ")";
                    return false;
                }
                go.AddComponent(t);
                message = "added " + t.FullName;
                return true;
            }
            catch (Exception ex)
            {
                message = "exception: " + ex.Message;
                return false;
            }
        }

        private static long FileSizeBytes(string assetPath)
        {
            try
            {
                string abs = Path.Combine(
                    Path.GetDirectoryName(Application.dataPath) ?? "",
                    assetPath.Replace('/', Path.DirectorySeparatorChar));
                return new FileInfo(abs).Length;
            }
            catch
            {
                return -1;
            }
        }
    }
}
#endif
