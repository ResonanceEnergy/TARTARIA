using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Procedural Tartarian architecture detail generator.
    /// Decorates greybox primitives with neoclassical features:
    /// - Ringed colonnades around domes
    /// - Pilasters & arched bases on fountains
    /// - Star-fort earthwork plinths under spires
    /// - Half-buried sink (mud-flood layer) so structures look excavated
    ///
    /// Pure procedural primitives only — no external assets required.
    /// </summary>
    public static class TartarianArchitectureBuilder
    {
        static Shader LitShader => Shader.Find("Universal Render Pipeline/Lit");

        public enum BuildingKind { Dome, Fountain, Spire }

        /// <summary>
        /// Adorns a greybox building with Tartarian detail.
        /// Returns the created decoration root (parented to building).
        /// </summary>
        public static GameObject Decorate(GameObject building, BuildingKind kind, Vector3 baseScale)
        {
            if (building == null) return null;
            string detailName = "TartarianDetail";
            var existing = building.transform.Find(detailName);
            if (existing != null) return existing.gameObject;

            var root = new GameObject(detailName);
            root.transform.SetParent(building.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            // Reverse parent scale so children are authored in world units
            var s = building.transform.lossyScale;
            root.transform.localScale = new Vector3(
                s.x != 0 ? 1f / s.x : 1f,
                s.y != 0 ? 1f / s.y : 1f,
                s.z != 0 ? 1f / s.z : 1f);

            // Half-bury all structures so they read as excavated ruins.
            // Sink ~35% of building height into terrain.
            float sinkY = baseScale.y * 0.35f;
            building.transform.position += Vector3.down * sinkY;

            switch (kind)
            {
                case BuildingKind.Dome:
                    BuildDomeColonnade(root, baseScale);
                    BuildDomeCap(root, baseScale);
                    BuildExposedArches(root, baseScale, count: 4);
                    break;
                case BuildingKind.Fountain:
                    BuildFountainPilasters(root, baseScale);
                    BuildFountainBasin(root, baseScale);
                    break;
                case BuildingKind.Spire:
                    BuildStarFortPlinth(root, baseScale, sides: 5);
                    BuildSpireBands(root, baseScale);
                    break;
            }

            BuildMudFloodDisc(root, baseScale);
            BuildRubblePile(root, baseScale);

            return root;
        }

        // ---------- DOME ----------

        static void BuildDomeColonnade(GameObject root, Vector3 scale)
        {
            int columns = 12;
            float radius = scale.x * 0.55f;
            float baseY = scale.y * 0.05f;
            float colHeight = scale.y * 0.45f;
            float colRadius = 0.18f;

            var stoneMat = MakeMat("M_Tart_Stone", new Color(0.85f, 0.82f, 0.75f), 0.55f);

            for (int i = 0; i < columns; i++)
            {
                float ang = (i / (float)columns) * Mathf.PI * 2f;
                Vector3 pos = new(Mathf.Cos(ang) * radius, baseY + colHeight * 0.5f, Mathf.Sin(ang) * radius);
                var col = MakePrimitive(PrimitiveType.Cylinder, root, $"Column_{i}", pos,
                    new Vector3(colRadius * 2f, colHeight * 0.5f, colRadius * 2f), stoneMat);
                // Capital
                var cap = MakePrimitive(PrimitiveType.Cube, root, $"Capital_{i}",
                    pos + Vector3.up * (colHeight * 0.5f + 0.1f),
                    new Vector3(colRadius * 3f, 0.15f, colRadius * 3f), stoneMat);
                // Suppress unused warning
                _ = col; _ = cap;
            }

            // Architrave ring (flat torus via thin cylinder)
            var ring = MakePrimitive(PrimitiveType.Cylinder, root, "Architrave",
                new Vector3(0f, baseY + colHeight + 0.18f, 0f),
                new Vector3(radius * 2.3f, 0.1f, radius * 2.3f), stoneMat);
            _ = ring;
        }

        static void BuildDomeCap(GameObject root, Vector3 scale)
        {
            var goldMat = MakeMat("M_Tart_Gold", new Color(0.95f, 0.78f, 0.35f), 0.85f, metallic: 0.9f,
                emission: new Color(0.6f, 0.4f, 0.1f) * 0.5f);
            var spire = MakePrimitive(PrimitiveType.Cylinder, root, "DomeFinial",
                new Vector3(0f, scale.y * 0.55f, 0f),
                new Vector3(0.25f, scale.y * 0.15f, 0.25f), goldMat);
            var orb = MakePrimitive(PrimitiveType.Sphere, root, "DomeOrb",
                new Vector3(0f, scale.y * 0.72f, 0f),
                Vector3.one * 0.5f, goldMat);
            _ = spire; _ = orb;
        }

        static void BuildExposedArches(GameObject root, Vector3 scale, int count)
        {
            var stoneMat = MakeMat("M_Tart_Stone", new Color(0.78f, 0.72f, 0.62f), 0.4f);
            float radius = scale.x * 0.62f;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f + 0.3f;
                Vector3 pos = new(Mathf.Cos(ang) * radius, -scale.y * 0.20f, Mathf.Sin(ang) * radius);
                Quaternion rot = Quaternion.LookRotation(new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)));
                var arch = MakePrimitive(PrimitiveType.Cube, root, $"BuriedArch_{i}",
                    pos, new Vector3(1.2f, 1.4f, 0.4f), stoneMat);
                arch.transform.localRotation = rot;
            }
        }

        // ---------- FOUNTAIN ----------

        static void BuildFountainPilasters(GameObject root, Vector3 scale)
        {
            var stoneMat = MakeMat("M_Tart_Marble", new Color(0.92f, 0.90f, 0.85f), 0.7f);
            int sides = 8;
            float radius = scale.x * 0.55f;
            float h = scale.y * 0.85f;
            for (int i = 0; i < sides; i++)
            {
                float ang = (i / (float)sides) * Mathf.PI * 2f;
                Vector3 pos = new(Mathf.Cos(ang) * radius, h * 0.5f, Mathf.Sin(ang) * radius);
                var p = MakePrimitive(PrimitiveType.Cube, root, $"Pilaster_{i}",
                    pos, new Vector3(0.25f, h * 0.5f, 0.15f), stoneMat);
                p.transform.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)));
            }
        }

        static void BuildFountainBasin(GameObject root, Vector3 scale)
        {
            var basinMat = MakeMat("M_Tart_Basin", new Color(0.55f, 0.55f, 0.58f), 0.55f);
            var basin = MakePrimitive(PrimitiveType.Cylinder, root, "Basin",
                new Vector3(0f, scale.y * 0.95f, 0f),
                new Vector3(scale.x * 1.4f, 0.15f, scale.z * 1.4f), basinMat);
            _ = basin;

            var rim = MakePrimitive(PrimitiveType.Cylinder, root, "BasinRim",
                new Vector3(0f, scale.y * 1.05f, 0f),
                new Vector3(scale.x * 1.5f, 0.08f, scale.z * 1.5f),
                MakeMat("M_Tart_RimGold", new Color(0.9f, 0.75f, 0.35f), 0.85f, metallic: 0.85f));
            _ = rim;
        }

        // ---------- SPIRE ----------

        static void BuildStarFortPlinth(GameObject root, Vector3 scale, int sides)
        {
            var earthMat = MakeMat("M_Tart_Earthwork", new Color(0.32f, 0.26f, 0.18f), 0.15f);
            float radius = scale.x * 1.6f;
            float h = 1.5f;
            for (int i = 0; i < sides; i++)
            {
                float ang = (i / (float)sides) * Mathf.PI * 2f;
                Vector3 pos = new(Mathf.Cos(ang) * radius, -h * 0.5f, Mathf.Sin(ang) * radius);
                var bastion = MakePrimitive(PrimitiveType.Cube, root, $"Bastion_{i}",
                    pos, new Vector3(2.4f, h, 1.2f), earthMat);
                bastion.transform.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)));
            }
        }

        static void BuildSpireBands(GameObject root, Vector3 scale)
        {
            var bandMat = MakeMat("M_Tart_SpireBand", new Color(0.7f, 0.85f, 1f), 0.9f, metallic: 0.6f,
                emission: new Color(0.3f, 0.5f, 0.8f) * 0.6f);
            int bands = 4;
            for (int i = 0; i < bands; i++)
            {
                float t = (i + 1) / (float)(bands + 1);
                var band = MakePrimitive(PrimitiveType.Cylinder, root, $"SpireBand_{i}",
                    new Vector3(0f, scale.y * (t - 0.5f), 0f),
                    new Vector3(scale.x * 1.15f, 0.08f, scale.z * 1.15f), bandMat);
                _ = band;
            }
            // Crystal cap
            var crystalMat = MakeMat("M_Tart_Crystal", new Color(0.6f, 0.85f, 1f), 0.95f, metallic: 0.2f,
                emission: new Color(0.4f, 0.7f, 1f) * 1.2f);
            var cap = MakePrimitive(PrimitiveType.Sphere, root, "SpireCrystal",
                new Vector3(0f, scale.y * 0.55f, 0f), Vector3.one * 0.9f, crystalMat);
            _ = cap;
        }

        // ---------- MUD FLOOD LAYER ----------

        static void BuildMudFloodDisc(GameObject root, Vector3 scale)
        {
            // Wet, dark mud disc surrounding the building base — sells the
            // "structure rising out of the flood" silhouette.
            var wetMat = MakeMat("M_Tart_WetMud", new Color(0.18f, 0.13f, 0.08f), 0.92f, metallic: 0.05f);
            float radius = Mathf.Max(scale.x, scale.z) * 1.5f;
            var disc = MakePrimitive(PrimitiveType.Cylinder, root, "MudFlood",
                new Vector3(0f, -scale.y * 0.05f, 0f),
                new Vector3(radius * 2f, 0.03f, radius * 2f), wetMat);
            _ = disc;

            // Cracked outer ring of drying mud
            var dryMat = MakeMat("M_Tart_DryMud", new Color(0.36f, 0.27f, 0.15f), 0.2f);
            var ring = MakePrimitive(PrimitiveType.Cylinder, root, "MudRing",
                new Vector3(0f, -scale.y * 0.04f, 0f),
                new Vector3(radius * 2.6f, 0.02f, radius * 2.6f), dryMat);
            _ = ring;
        }

        static void BuildRubblePile(GameObject root, Vector3 scale)
        {
            // Scattered broken stones around the base — excavation debris.
            var rubbleMat = MakeMat("M_Tart_Rubble", new Color(0.55f, 0.50f, 0.42f), 0.35f);
            int count = 14;
            float radius = Mathf.Max(scale.x, scale.z) * 1.1f;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f + (i * 0.37f);
                float r = radius * (0.85f + (i % 3) * 0.18f);
                Vector3 pos = new(Mathf.Cos(ang) * r, 0.05f, Mathf.Sin(ang) * r);
                float sz = 0.4f + (i % 4) * 0.18f;
                var stone = MakePrimitive(PrimitiveType.Cube, root, $"Rubble_{i}",
                    pos, new Vector3(sz, sz * 0.55f, sz * 0.85f), rubbleMat);
                stone.transform.localRotation = Quaternion.Euler(
                    (i * 17) % 30, (i * 53) % 360, (i * 31) % 30);
            }
        }

        // ---------- HELPERS ----------

        static GameObject MakePrimitive(PrimitiveType type, GameObject parent, string name,
            Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            // Strip collider — pure decoration
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<MeshRenderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        static Material MakeMat(string name, Color baseColor, float smoothness,
            float metallic = 0f, Color? emission = null)
        {
            var shader = LitShader ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", baseColor);
            mat.color = baseColor;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            return mat;
        }
    }
}
