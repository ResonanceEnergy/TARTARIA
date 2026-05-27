using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tartarian Architecture Enhancer — Adds golden ratio details to procedural buildings
    /// Creates columns, arches, domes, ornamental bands, and sacred geometry patterns
    /// All proportions follow φ = 1.618 (golden ratio)
    /// </summary>
    public static class TartarianArchitectureEnhancer
    {
        const float PHI = 1.618f;
        
        /// <summary>
        /// Enhance a building with Tartarian architectural details
        /// </summary>
        public static void EnhanceBuilding(GameObject building, ArchitecturalStyle style)
        {
            var bounds = GetBuildingBounds(building);
            var position = building.transform.position;
            
            switch (style)
            {
                case ArchitecturalStyle.Classical:
                    AddClassicalColumns(building, bounds);
                    AddGoldenBand(building, bounds);
                    break;
                    
                case ArchitecturalStyle.Dome:
                    AddDomeStructure(building, bounds);
                    AddGoldenSpiral(building, bounds);
                    break;
                    
                case ArchitecturalStyle.Spire:
                    AddSpireTop(building, bounds);
                    AddHelicalBands(building, bounds);
                    break;
                    
                case ArchitecturalStyle.Fountain:
                    AddBasinStructure(building, bounds);
                    AddWaterSpout(building, bounds);
                    break;
            }
        }
        
        static void AddClassicalColumns(GameObject parent, Bounds bounds)
        {
            Material columnMaterial = CreateMaterial(new Color(0.85f, 0.82f, 0.75f));
            
            int columnCount = 6;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.9f;
            float columnHeight = bounds.size.y * 0.8f;
            float columnWidth = bounds.size.x * 0.08f;
            
            for (int i = 0; i < columnCount; i++)
            {
                float angle = (360f / columnCount) * i;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);
                
                var column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                column.name = $"Column_{i}";
                column.transform.SetParent(parent.transform);
                column.transform.localPosition = offset + Vector3.up * columnHeight * 0.5f;
                column.transform.localScale = new Vector3(columnWidth, columnHeight * 0.5f, columnWidth);
                column.GetComponent<Renderer>().material = columnMaterial;
                Object.Destroy(column.GetComponent<Collider>());
                
                // Add column capital (top decoration)
                var capital = GameObject.CreatePrimitive(PrimitiveType.Cube);
                capital.name = $"Capital_{i}";
                capital.transform.SetParent(column.transform);
                capital.transform.localPosition = Vector3.up * 0.95f;
                capital.transform.localScale = new Vector3(1.3f, 0.1f, 1.3f);
                capital.GetComponent<Renderer>().material = CreateGoldMaterial();
                Object.Destroy(capital.GetComponent<Collider>());
            }
        }
        
        static void AddGoldenBand(GameObject parent, Bounds bounds)
        {
            Material goldMaterial = CreateGoldMaterial();
            
            // Add band at golden ratio height
            float bandHeight = bounds.size.y / PHI;
            
            var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "GoldenBand";
            band.transform.SetParent(parent.transform);
            band.transform.localPosition = Vector3.up * bandHeight;
            
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.05f;
            band.transform.localScale = new Vector3(radius * 2f, 0.2f, radius * 2f);
            band.GetComponent<Renderer>().material = goldMaterial;
            Object.Destroy(band.GetComponent<Collider>());
        }
        
        static void AddDomeStructure(GameObject parent, Bounds bounds)
        {
            Material domeMaterial = CreateMaterial(new Color(0.9f, 0.88f, 0.82f));
            
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Dome";
            dome.transform.SetParent(parent.transform);
            dome.transform.localPosition = Vector3.up * bounds.size.y;
            
            float domeRadius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.2f;
            dome.transform.localScale = Vector3.one * domeRadius * 2f;
            dome.GetComponent<Renderer>().material = domeMaterial;
            Object.Destroy(dome.GetComponent<Collider>());
            
            // Add golden capstone
            var capstone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            capstone.name = "Capstone";
            capstone.transform.SetParent(dome.transform);
            capstone.transform.localPosition = Vector3.up * 0.5f;
            capstone.transform.localScale = Vector3.one * 0.15f;
            capstone.GetComponent<Renderer>().material = CreateGoldMaterial();
            Object.Destroy(capstone.GetComponent<Collider>());
        }
        
        static void AddGoldenSpiral(GameObject parent, Bounds bounds)
        {
            // Create ornamental spiral pattern on dome surface
            Material goldMaterial = CreateGoldMaterial();
            int spiralSegments = 12;
            
            for (int i = 0; i < spiralSegments; i++)
            {
                float t = (float)i / spiralSegments;
                float angle = t * 360f * 2f; // Two full rotations
                float height = t * bounds.size.y * 0.8f;
                float radius = Mathf.Lerp(bounds.extents.x * 0.9f, bounds.extents.x * 0.3f, t);
                
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, height, 0f);
                
                var segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                segment.name = $"SpiralSegment_{i}";
                segment.transform.SetParent(parent.transform);
                segment.transform.localPosition = offset;
                segment.transform.localScale = Vector3.one * 0.3f;
                segment.GetComponent<Renderer>().material = goldMaterial;
                Object.Destroy(segment.GetComponent<Collider>());
            }
        }
        
        static void AddSpireTop(GameObject parent, Bounds bounds)
        {
            Material goldMaterial = CreateGoldMaterial();
            
            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "SpireTop";
            spire.transform.SetParent(parent.transform);
            spire.transform.localPosition = Vector3.up * (bounds.size.y + bounds.size.y * 0.3f);
            spire.transform.localScale = new Vector3(
                bounds.extents.x * 0.2f,
                bounds.size.y * 0.3f,
                bounds.extents.z * 0.2f
            );
            spire.GetComponent<Renderer>().material = goldMaterial;
            Object.Destroy(spire.GetComponent<Collider>());
        }
        
        static void AddHelicalBands(GameObject parent, Bounds bounds)
        {
            Material bandMaterial = CreateGoldMaterial();
            int bandCount = 5;
            
            for (int i = 0; i < bandCount; i++)
            {
                float height = bounds.size.y * (0.2f + i * 0.15f);
                
                var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                band.name = $"HelicalBand_{i}";
                band.transform.SetParent(parent.transform);
                band.transform.localPosition = Vector3.up * height;
                
                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.03f;
                band.transform.localScale = new Vector3(radius * 2f, 0.15f, radius * 2f);
                band.GetComponent<Renderer>().material = bandMaterial;
                Object.Destroy(band.GetComponent<Collider>());
            }
        }
        
        static void AddBasinStructure(GameObject parent, Bounds bounds)
        {
            Material stoneMaterial = CreateMaterial(new Color(0.75f, 0.72f, 0.68f));
            
            // Outer basin ring
            var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.name = "Basin";
            basin.transform.SetParent(parent.transform);
            basin.transform.localPosition = Vector3.up * bounds.size.y * 0.3f;
            basin.transform.localScale = new Vector3(
                bounds.extents.x * 2.5f,
                bounds.size.y * 0.2f,
                bounds.extents.z * 2.5f
            );
            basin.GetComponent<Renderer>().material = stoneMaterial;
            Object.Destroy(basin.GetComponent<Collider>());
        }
        
        static void AddWaterSpout(GameObject parent, Bounds bounds)
        {
            // Central water spout column
            var spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spout.name = "WaterSpout";
            spout.transform.SetParent(parent.transform);
            spout.transform.localPosition = Vector3.up * bounds.size.y * 0.5f;
            spout.transform.localScale = new Vector3(
                bounds.extents.x * 0.3f,
                bounds.size.y * 0.6f,
                bounds.extents.z * 0.3f
            );
            spout.GetComponent<Renderer>().material = CreateGoldMaterial();
            Object.Destroy(spout.GetComponent<Collider>());
        }
        
        static Bounds GetBuildingBounds(GameObject building)
        {
            var renderers = building.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(building.transform.position, Vector3.one * 10f);
            
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            
            return bounds;
        }
        
        static Material CreateMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.4f);
            return mat;
        }
        
        static Material CreateGoldMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.85f, 0.3f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.9f);
            return mat;
        }
        
        public enum ArchitecturalStyle
        {
            Classical,
            Dome,
            Spire,
            Fountain
        }
    }
}
