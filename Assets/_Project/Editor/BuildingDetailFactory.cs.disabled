using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Adds detail geometry (buttress columns, emissive ring bands, finials) to the
    /// 3 Echohaven building prefabs so they read as built structures rather than
    /// primitive shapes. Idempotent — strips any existing "Detail_*" children first.
    /// </summary>
    public static class BuildingDetailFactory
    {
        const string MatDir = "Assets/_Project/Materials";

        public static void DecorateAllBuildings()
        {
            EnsureDetailMaterials();

            DecorateBuilding(
                "Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab",
                tint: new Color(0.55f, 0.85f, 1.0f),
                emissionStrength: 1.4f,
                buttressCount: 8,
                buttressHeight: 5.5f,
                buttressRadius: 5.5f,
                ringBands: new[] { 1.2f, 3.6f },
                ringRadius: 5.6f,
                finialHeight: 7.5f,
                finialKind: FinialKind.AntennaSpire);

            DecorateBuilding(
                "Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab",
                tint: new Color(1.0f, 0.85f, 0.45f),
                emissionStrength: 1.2f,
                buttressCount: 6,
                buttressHeight: 3.2f,
                buttressRadius: 3.4f,
                ringBands: new[] { 1.0f, 2.4f },
                ringRadius: 3.5f,
                finialHeight: 4.5f,
                finialKind: FinialKind.GlowingOrb);

            DecorateBuilding(
                "Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab",
                tint: new Color(0.85f, 0.55f, 1.0f),
                emissionStrength: 1.7f,
                buttressCount: 5,
                buttressHeight: 7.0f,
                buttressRadius: 2.6f,
                ringBands: new[] { 2.0f, 5.0f, 8.0f },
                ringRadius: 2.6f,
                finialHeight: 11f,
                finialKind: FinialKind.CrystalShard);

            AssetDatabase.SaveAssets();
            Debug.Log("[Tartaria] 3 building prefabs decorated with detail geometry.");
        }

        enum FinialKind { AntennaSpire, GlowingOrb, CrystalShard }

        static void DecorateBuilding(string prefabPath, Color tint, float emissionStrength,
            int buttressCount, float buttressHeight, float buttressRadius,
            float[] ringBands, float ringRadius,
            float finialHeight, FinialKind finialKind)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogWarning($"[Tartaria] Building prefab missing: {prefabPath}"); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            // Strip prior detail children (idempotent rebuild)
            for (int i = instance.transform.childCount - 1; i >= 0; i--)
            {
                var c = instance.transform.GetChild(i);
                if (c.name.StartsWith("Detail_")) Object.DestroyImmediate(c.gameObject);
            }

            var detailRoot = new GameObject("Detail_Root");
            detailRoot.transform.SetParent(instance.transform, false);

            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/M_Building_Stone.mat");
            var emissiveMat = MakeEmissiveVariant(tint, emissionStrength);

            // ── Buttress columns radially around the base ─────────────────────
            for (int i = 0; i < buttressCount; i++)
            {
                float angle = (i / (float)buttressCount) * Mathf.PI * 2f;
                var col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                col.name = $"Detail_Buttress_{i}";
                col.transform.SetParent(detailRoot.transform, false);
                col.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * buttressRadius,
                    buttressHeight * 0.5f,
                    Mathf.Cos(angle) * buttressRadius);
                col.transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
                col.transform.localScale = new Vector3(0.45f, buttressHeight, 0.45f);
                Object.DestroyImmediate(col.GetComponent<BoxCollider>());
                if (stoneMat != null) col.GetComponent<MeshRenderer>().sharedMaterial = stoneMat;
            }

            // ── Emissive ring bands at multiple heights ───────────────────────
            for (int b = 0; b < ringBands.Length; b++)
            {
                float h = ringBands[b];
                int seg = 24;
                for (int i = 0; i < seg; i++)
                {
                    float a = (i / (float)seg) * Mathf.PI * 2f;
                    var seg1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    seg1.name = $"Detail_Ring{b}_{i}";
                    seg1.transform.SetParent(detailRoot.transform, false);
                    seg1.transform.localPosition = new Vector3(
                        Mathf.Sin(a) * ringRadius,
                        h,
                        Mathf.Cos(a) * ringRadius);
                    seg1.transform.localRotation = Quaternion.Euler(0f, a * Mathf.Rad2Deg + 90f, 0f);
                    float chord = 2f * ringRadius * Mathf.Sin(Mathf.PI / seg) * 1.05f;
                    seg1.transform.localScale = new Vector3(chord, 0.18f, 0.12f);
                    Object.DestroyImmediate(seg1.GetComponent<BoxCollider>());
                    seg1.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
                }
            }

            // ── Finial on top ────────────────────────────────────────────────
            switch (finialKind)
            {
                case FinialKind.AntennaSpire:
                {
                    var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    spire.name = "Detail_AntennaSpire";
                    spire.transform.SetParent(detailRoot.transform, false);
                    spire.transform.localPosition = new Vector3(0f, finialHeight + 1.4f, 0f);
                    spire.transform.localScale = new Vector3(0.18f, 1.4f, 0.18f);
                    Object.DestroyImmediate(spire.GetComponent<CapsuleCollider>());
                    spire.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;

                    // Crossbars (Tesla-style coil)
                    for (int i = 0; i < 3; i++)
                    {
                        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        bar.name = $"Detail_AntennaBar_{i}";
                        bar.transform.SetParent(detailRoot.transform, false);
                        bar.transform.localPosition = new Vector3(0f, finialHeight + 0.4f + i * 0.6f, 0f);
                        bar.transform.localRotation = Quaternion.Euler(0f, i * 30f, 0f);
                        bar.transform.localScale = new Vector3(0.9f - i * 0.18f, 0.06f, 0.06f);
                        Object.DestroyImmediate(bar.GetComponent<BoxCollider>());
                        bar.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
                    }

                    var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tip.name = "Detail_AntennaTip";
                    tip.transform.SetParent(detailRoot.transform, false);
                    tip.transform.localPosition = new Vector3(0f, finialHeight + 2.9f, 0f);
                    tip.transform.localScale = Vector3.one * 0.35f;
                    Object.DestroyImmediate(tip.GetComponent<SphereCollider>());
                    tip.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
                    break;
                }
                case FinialKind.GlowingOrb:
                {
                    var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.name = "Detail_GlowOrb";
                    orb.transform.SetParent(detailRoot.transform, false);
                    orb.transform.localPosition = new Vector3(0f, finialHeight, 0f);
                    orb.transform.localScale = Vector3.one * 1.4f;
                    Object.DestroyImmediate(orb.GetComponent<SphereCollider>());
                    orb.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;

                    // Halo ring
                    int seg = 32;
                    for (int i = 0; i < seg; i++)
                    {
                        float a = (i / (float)seg) * Mathf.PI * 2f;
                        var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        s.name = $"Detail_OrbHalo_{i}";
                        s.transform.SetParent(detailRoot.transform, false);
                        s.transform.localPosition = new Vector3(Mathf.Sin(a) * 1.6f, finialHeight, Mathf.Cos(a) * 1.6f);
                        s.transform.localRotation = Quaternion.Euler(75f, a * Mathf.Rad2Deg + 90f, 0f);
                        float chord = 2f * 1.6f * Mathf.Sin(Mathf.PI / seg) * 1.05f;
                        s.transform.localScale = new Vector3(chord, 0.06f, 0.18f);
                        Object.DestroyImmediate(s.GetComponent<BoxCollider>());
                        s.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
                    }
                    break;
                }
                case FinialKind.CrystalShard:
                {
                    // Cluster of stretched octahedral crystals at the apex
                    Vector3[] offsets = {
                        Vector3.zero,
                        new Vector3(0.35f, -0.4f, 0.1f),
                        new Vector3(-0.3f, -0.6f, -0.2f),
                        new Vector3(0.05f, 0.6f, 0.3f),
                    };
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        crystal.name = $"Detail_Crystal_{i}";
                        crystal.transform.SetParent(detailRoot.transform, false);
                        crystal.transform.localPosition = new Vector3(0f, finialHeight, 0f) + offsets[i];
                        crystal.transform.localRotation = Quaternion.Euler(35f * i, 17f * i, 22f * i);
                        crystal.transform.localScale = new Vector3(0.45f, 1.6f - i * 0.25f, 0.45f);
                        Object.DestroyImmediate(crystal.GetComponent<BoxCollider>());
                        crystal.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
                    }
                    break;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }

        // ── Materials ────────────────────────────────────────────────────────
        static void EnsureDetailMaterials()
        {
            // Stone base material for buttresses (shared, dim)
            string stonePath = $"{MatDir}/M_Building_Stone.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(stonePath) == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                var mat = new Material(shader) { name = "M_Building_Stone" };
                mat.color = new Color(0.32f, 0.30f, 0.28f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0.0f);
                AssetDatabase.CreateAsset(mat, stonePath);
            }
        }

        static Material MakeEmissiveVariant(Color tint, float strength)
        {
            string safeName = $"M_BuildingEmissive_{Mathf.RoundToInt(tint.r * 255):X2}{Mathf.RoundToInt(tint.g * 255):X2}{Mathf.RoundToInt(tint.b * 255):X2}";
            string path = $"{MatDir}/{safeName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader) { name = safeName };
            mat.color = tint * 0.6f;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.65f);
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0.4f);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", tint * strength);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
