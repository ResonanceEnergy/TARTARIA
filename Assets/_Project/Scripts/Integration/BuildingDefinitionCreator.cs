using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Building Definition Creator — Runtime utility to create BuildingDefinition assets
    /// Call from Unity Editor menu: Tartaria → Generate Moon 1 Building Definitions
    /// </summary>
    public static class BuildingDefinitionCreator
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tartaria/Generate Moon 1 Building Definitions")]
        public static void CreateMoon1Definitions()
        {
            CreateDomeDefinition();
            CreateFountainDefinition();
            CreateSpireDefinition();
            CreateVillageDefinitions();
            
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("[BuildingDefinitionCreator] Created 12 BuildingDefinition assets for Moon 1");
        }
        
        static void CreateDomeDefinition()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.buildingName = "Star Dome Observatory";
            def.loreDescription = "An ancient observatory where Tartarian scholars tracked celestial harmonics. The dome's geometry amplifies Aether flows, allowing precise measurement of the 432 Hz resonance field.";
            def.archetype = BuildingArchetype.Observatory;
            def.width = 16f;
            def.height = 16f * 1.618f; // Golden ratio
            def.goldenRatioTarget = 1.618f;
            def.aetherSourceStrength = 2.5f;
            def.aetherSourceRadius = 80f;
            def.outputBand = HarmonicBand.Harmonic;
            def.nodeCount = 3;
            def.nodePuzzles = CreatePuzzleConfigs(3);
            def.dissolutionDuration = 5.0f;
            def.baseIncome = 25;
            def.rsReward = 200;
            
            string path = "Assets/_Project/ScriptableObjects/Buildings/Moon1_StarDome.asset";
            UnityEditor.AssetDatabase.CreateAsset(def, path);
        }
        
        static void CreateFountainDefinition()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.buildingName = "Harmonic Fountain";
            def.loreDescription = "A ceremonial fountain that channels underground Aether currents. The water spray creates standing wave patterns at 432 Hz, purifying the surrounding field.";
            def.archetype = BuildingArchetype.Fountain;
            def.width = 12f;
            def.height = 12f * 1.618f;
            def.goldenRatioTarget = 1.618f;
            def.aetherSourceStrength = 1.8f;
            def.aetherSourceRadius = 60f;
            def.outputBand = HarmonicBand.Harmonic;
            def.nodeCount = 3;
            def.nodePuzzles = CreatePuzzleConfigs(3);
            def.dissolutionDuration = 4.5f;
            def.baseIncome = 20;
            def.rsReward = 150;
            
            string path = "Assets/_Project/ScriptableObjects/Buildings/Moon1_HarmonicFountain.asset";
            UnityEditor.AssetDatabase.CreateAsset(def, path);
        }
        
        static void CreateSpireDefinition()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.buildingName = "Crystal Resonance Spire";
            def.loreDescription = "A towering spire topped with a mercury-glass crystal. Acts as a relay node in the Tartarian grid, broadcasting harmonized Aether across the region.";
            def.archetype = BuildingArchetype.Spire;
            def.width = 8f;
            def.height = 8f * 1.618f * 1.618f; // Double golden ratio (extra tall)
            def.goldenRatioTarget = 1.618f;
            def.aetherSourceStrength = 3.0f;
            def.aetherSourceRadius = 100f;
            def.outputBand = HarmonicBand.Harmonic;
            def.nodeCount = 4;
            def.nodePuzzles = CreatePuzzleConfigs(4);
            def.dissolutionDuration = 6.0f;
            def.baseIncome = 30;
            def.rsReward = 250;
            
            string path = "Assets/_Project/ScriptableObjects/Buildings/Moon1_CrystalSpire.asset";
            UnityEditor.AssetDatabase.CreateAsset(def, path);
        }
        
        static void CreateVillageDefinitions()
        {
            string[] buildingTypes = { "House", "Tower", "Temple", "Workshop" };
            BuildingArchetype[] archetypes = { 
                BuildingArchetype.Residential, 
                BuildingArchetype.Defense, 
                BuildingArchetype.Temple, 
                BuildingArchetype.Workshop 
            };
            
            for (int i = 0; i < 9; i++)
            {
                int typeIndex = i % 4;
                string type = buildingTypes[typeIndex];
                
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.buildingName = $"Echohaven {type} {i + 1}";
                def.loreDescription = $"A small {type.ToLower()} in the village of Echohaven. Once home to Tartarian families who tended the Aether fields.";
                def.archetype = archetypes[typeIndex];
                
                // Golden ratio dimensions based on type
                float baseSize = type switch
                {
                    "House" => 8f,
                    "Tower" => 5f,
                    "Temple" => 12f,
                    "Workshop" => 10f,
                    _ => 8f
                };
                
                float heightMultiplier = type switch
                {
                    "Tower" => 1.618f * 1.618f, // Double phi
                    "Temple" => 0.618f, // Inverse phi (wide and low)
                    _ => 1.618f
                };
                
                def.width = baseSize;
                def.height = baseSize * heightMultiplier;
                def.goldenRatioTarget = 1.618f;
                def.aetherSourceStrength = 0.5f;
                def.aetherSourceRadius = 25f;
                def.outputBand = HarmonicBand.Harmonic;
                def.nodeCount = 2;
                def.nodePuzzles = CreatePuzzleConfigs(2);
                def.dissolutionDuration = 3.0f;
                def.baseIncome = 5;
                def.rsReward = 50;
                
                string path = $"Assets/_Project/ScriptableObjects/Buildings/Moon1_Village_{type}_{i + 1}.asset";
                UnityEditor.AssetDatabase.CreateAsset(def, path);
            }
        }
        
        static TuningPuzzleConfig[] CreatePuzzleConfigs(int count)
        {
            var configs = new TuningPuzzleConfig[count];
            
            for (int i = 0; i < count; i++)
            {
                configs[i] = new TuningPuzzleConfig
                {
                    variant = (TuningVariant)(i % 3), // Cycle through 3 variants
                    targetFrequency = 432f,
                    timeLimitSeconds = 20f - (i * 2f), // Gets harder (less time)
                    tolerancePercent = 0.08f + (i * 0.02f), // Gets harder (tighter tolerance)
                    difficultySpeed = 0.3f + (i * 0.15f) // Gets faster
                };
            }
            
            return configs;
        }
#endif
    }
}
