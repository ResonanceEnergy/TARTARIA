#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Inspector for EnemyData — Designer-friendly enemy editor.
    /// Features: model preview, stat comparison chart, loot table editor,
    /// AI behavior setup, validation, quick actions.
    /// </summary>
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataEditor : UnityEditor.Editor
    {
        private EnemyData _enemy;
        private bool _showBasic = true;
        private bool _showStats = true;
        private bool _showCombat = true;
        private bool _showLoot = false;
        private bool _showAudio = false;
        private bool _showDebug = false;
        private List<ValidationResult> _validationResults = new();

        // Comparison reference enemy (for stat comparison)
        private EnemyData _comparisonEnemy;

        void OnEnable()
        {
            _enemy = (EnemyData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ─── Header ────────────────────────────────────────────
            EditorGUILayout.Space(10);
            EditorUtils.DrawColoredLabel($"Enemy: {_enemy.displayName}", GetArchetypeColor(_enemy.archetype), FontStyle.Bold);
            EditorGUILayout.LabelField($"{_enemy.archetype} | {_enemy.enemyID}", EditorStyles.centeredGreyMiniLabel);
            EditorUtils.DrawSeparator();

            // ─── Preview Panel ─────────────────────────────────────
            if (_enemy.icon != null)
            {
                EditorUtils.DrawSpritePreview(_enemy.icon, 128f);
                EditorGUILayout.LabelField("Enemy Icon", EditorStyles.centeredGreyMiniLabel);
            }

            if (_enemy.prefab != null)
            {
                EditorGUILayout.Space(10);
                EditorUtils.DrawPrefabPreview(_enemy.prefab, 128f);
                EditorGUILayout.LabelField("Enemy Prefab", EditorStyles.centeredGreyMiniLabel);
            }

            EditorUtils.DrawSeparator();

            // ─── Stat Summary Card ─────────────────────────────────
            DrawStatSummaryCard();

            // ─── Quick Actions ─────────────────────────────────────
            EditorUtils.DrawQuickActions(
                ("Validate", ValidateEnemy),
                ("Duplicate", DuplicateEnemy),
                ("Export JSON", ExportEnemy),
                ("Test Spawn", TestSpawn)
            );

            EditorUtils.DrawSeparator();

            // ─── Collapsible Sections ──────────────────────────────
            _showBasic = EditorUtils.DrawFoldoutSection("Basic Properties", _showBasic, DrawBasicSection);
            _showStats = EditorUtils.DrawFoldoutSection("Stats & Attributes", _showStats, DrawStatsSection);
            _showCombat = EditorUtils.DrawFoldoutSection("Combat Behavior", _showCombat, DrawCombatSection);
            _showLoot = EditorUtils.DrawFoldoutSection("Loot & Rewards", _showLoot, DrawLootSection);
            _showAudio = EditorUtils.DrawFoldoutSection("Audio", _showAudio, DrawAudioSection);
            _showDebug = EditorUtils.DrawFoldoutSection("Debug Info", _showDebug, DrawDebugSection);

            // ─── Stat Comparison Tool ──────────────────────────────
            DrawStatComparison();

            // ─── Validation Results ────────────────────────────────
            if (_validationResults.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
                EditorUtils.DrawValidationResults(_validationResults);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawStatSummaryCard()
        {
            EditorUtils.DrawBoxGroup("Combat Stats", () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                // HP bar
                EditorUtils.DrawProgressBar(_enemy.maxHealth, 1000f, "HP", Color.green);
                
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"⚔️ ATK: {_enemy.attackDamage:F0}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"🏃 SPD: {_enemy.moveSpeed:F1} m/s", GUILayout.Width(150));
                EditorGUILayout.LabelField($"🎯 RNG: {_enemy.attackRange:F1}m");
                EditorGUILayout.EndHorizontal();
            });

            EditorGUILayout.Space(5);
        }

        void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyID"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("archetype"));
        }

        void DrawStatsSection()
        {
            // Health
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
            EditorUtils.DrawProgressBar(_enemy.maxHealth, 10000f, "HP Scale", Color.green);

            EditorGUILayout.Space(5);

            // Movement
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
            EditorUtils.DrawProgressBar(_enemy.moveSpeed, 20f, "Speed Scale", Color.cyan);

            EditorGUILayout.Space(5);

            // Attack stats
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackCooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionRange"));
        }

        void DrawCombatSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("specialAbilities"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Damage Resistances", EditorStyles.boldLabel);
            
            var resistancesProp = serializedObject.FindProperty("resistances");
            EditorGUILayout.PropertyField(resistancesProp.FindPropertyRelative("physical"));
            EditorUtils.DrawProgressBar(_enemy.resistances.physical, 100f, "Physical", 
                _enemy.resistances.physical > 0 ? EditorUtils.ColorBuffed : EditorUtils.ColorNerfed);

            EditorGUILayout.PropertyField(resistancesProp.FindPropertyRelative("resonance"));
            EditorUtils.DrawProgressBar(_enemy.resistances.resonance, 100f, "Resonance", 
                _enemy.resistances.resonance > 0 ? EditorUtils.ColorBuffed : EditorUtils.ColorNerfed);

            EditorGUILayout.PropertyField(resistancesProp.FindPropertyRelative("environmental"));
            EditorUtils.DrawProgressBar(_enemy.resistances.environmental, 100f, "Environmental", 
                _enemy.resistances.environmental > 0 ? EditorUtils.ColorBuffed : EditorUtils.ColorNerfed);
        }

        void DrawLootSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rsReward"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xpReward"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Loot Table", EditorStyles.boldLabel);
            
            var lootTableProp = serializedObject.FindProperty("lootTable");
            
            if (lootTableProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No loot drops defined", MessageType.Info);
                if (GUILayout.Button("Add First Loot Drop"))
                {
                    lootTableProp.InsertArrayElementAtIndex(0);
                }
            }
            else
            {
                for (int i = 0; i < lootTableProp.arraySize; i++)
                {
                    var lootProp = lootTableProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Drop {i + 1}", EditorStyles.boldLabel, GUILayout.Width(80));
                    
                    if (GUILayout.Button("✖", GUILayout.Width(25)))
                    {
                        lootTableProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(lootProp.FindPropertyRelative("itemID"));
                    EditorGUILayout.PropertyField(lootProp.FindPropertyRelative("dropChance"));
                    
                    float dropChance = lootProp.FindPropertyRelative("dropChance").floatValue;
                    EditorUtils.DrawProgressBar(dropChance, 1f, "Drop Rate", Color.yellow);

                    EditorGUILayout.PropertyField(lootProp.FindPropertyRelative("minQuantity"));
                    EditorGUILayout.PropertyField(lootProp.FindPropertyRelative("maxQuantity"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }

                if (GUILayout.Button("+ Add Loot Drop"))
                {
                    lootTableProp.InsertArrayElementAtIndex(lootTableProp.arraySize);
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnMoons"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minPlayerLevel"));
        }

        void DrawAudioSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackSound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deathSound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientSound"));
        }

        void DrawDebugSection()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Asset Path", AssetDatabase.GetAssetPath(_enemy));
            EditorGUILayout.TextField("Enemy ID", _enemy.enemyID);
            EditorGUILayout.TextField("Stat Summary", _enemy.GetStatSummary());
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy Stat Summary"))
            {
                EditorGUIUtility.systemCopyBuffer = _enemy.GetStatSummary();
                Debug.Log($"[EnemyDataEditor] Copied stat summary: {_enemy.GetStatSummary()}");
            }
        }

        void DrawStatComparison()
        {
            EditorGUILayout.Space(10);
            EditorUtils.DrawBoxGroup("Stat Comparison Tool", () =>
            {
                _comparisonEnemy = (EnemyData)EditorGUILayout.ObjectField("Compare With", _comparisonEnemy, typeof(EnemyData), false);

                if (_comparisonEnemy != null && _comparisonEnemy != _enemy)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Comparison Results:", EditorStyles.boldLabel);

                    CompareStatFloat("Max Health", _enemy.maxHealth, _comparisonEnemy.maxHealth);
                    CompareStatFloat("Attack Damage", _enemy.attackDamage, _comparisonEnemy.attackDamage);
                    CompareStatFloat("Move Speed", _enemy.moveSpeed, _comparisonEnemy.moveSpeed);
                    CompareStatFloat("Attack Range", _enemy.attackRange, _comparisonEnemy.attackRange);
                    CompareStatFloat("RS Reward", _enemy.rsReward, _comparisonEnemy.rsReward);
                    CompareStatInt("XP Reward", _enemy.xpReward, _comparisonEnemy.xpReward);
                }
            });
        }

        void CompareStatFloat(string label, float value1, float value2)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));
            EditorGUILayout.LabelField($"{value1:F1}", GUILayout.Width(60));
            
            float diff = value1 - value2;
            string diffText = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
            Color diffColor = diff > 0 ? EditorUtils.ColorBuffed : diff < 0 ? EditorUtils.ColorNerfed : EditorUtils.ColorDefault;
            
            EditorUtils.DrawColoredLabel(diffText, diffColor);
            EditorGUILayout.EndHorizontal();
        }

        void CompareStatInt(string label, int value1, int value2)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));
            EditorGUILayout.LabelField($"{value1}", GUILayout.Width(60));
            
            int diff = value1 - value2;
            string diffText = diff > 0 ? $"+{diff}" : $"{diff}";
            Color diffColor = diff > 0 ? EditorUtils.ColorBuffed : diff < 0 ? EditorUtils.ColorNerfed : EditorUtils.ColorDefault;
            
            EditorUtils.DrawColoredLabel(diffText, diffColor);
            EditorGUILayout.EndHorizontal();
        }

        void ValidateEnemy()
        {
            _validationResults.Clear();

            // Validate enemy ID
            if (string.IsNullOrWhiteSpace(_enemy.enemyID))
            {
                _validationResults.Add(new ValidationResult("Enemy ID is empty", ValidationSeverity.Error, _enemy));
            }

            // Validate display name
            if (string.IsNullOrWhiteSpace(_enemy.displayName))
            {
                _validationResults.Add(new ValidationResult("Display name is empty", ValidationSeverity.Error, _enemy));
            }

            // Validate prefab
            if (_enemy.prefab == null)
            {
                _validationResults.Add(new ValidationResult("No prefab assigned", ValidationSeverity.Error, _enemy));
            }

            // Validate stats
            if (_enemy.maxHealth <= 0)
            {
                _validationResults.Add(new ValidationResult("Max health must be greater than 0", ValidationSeverity.Error, _enemy));
            }

            if (_enemy.attackDamage <= 0)
            {
                _validationResults.Add(new ValidationResult("Attack damage must be greater than 0", ValidationSeverity.Warning, _enemy));
            }

            // Validate spawn settings
            if (_enemy.spawnMoons == null || _enemy.spawnMoons.Count == 0)
            {
                _validationResults.Add(new ValidationResult("No spawn moons defined", ValidationSeverity.Warning, _enemy));
            }

            // Validate loot
            if (_enemy.lootTable != null)
            {
                foreach (var loot in _enemy.lootTable)
                {
                    if (loot.dropChance < 0f || loot.dropChance > 1f)
                    {
                        _validationResults.Add(new ValidationResult($"Invalid drop chance for {loot.itemID}", ValidationSeverity.Error, _enemy));
                    }
                }
            }

            if (_validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Success", "✓ All checks passed!", "OK");
            }

            Repaint();
        }

        void DuplicateEnemy()
        {
            if (EditorUtils.ConfirmAction("Duplicate Enemy", $"Create a copy of '{_enemy.displayName}'?"))
            {
                var clone = EditorUtils.DuplicateAsset(_enemy, $"{_enemy.enemyID}_copy");
                if (clone != null)
                {
                    clone.enemyID = $"{_enemy.enemyID}_copy";
                }
            }
        }

        void ExportEnemy()
        {
            EditorUtils.ExportToJSON(_enemy, $"{_enemy.enemyID}.json");
        }

        void TestSpawn()
        {
            if (_enemy.prefab == null)
            {
                EditorUtility.DisplayDialog("Test Spawn", "No prefab assigned to spawn!", "OK");
                return;
            }

            if (EditorUtils.ConfirmAction("Test Spawn", $"Spawn '{_enemy.displayName}' in current scene?"))
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(_enemy.prefab);
                instance.name = $"{_enemy.displayName} (Test)";
                Selection.activeGameObject = instance;
                EditorGUIUtility.PingObject(instance);
                Debug.Log($"[EnemyDataEditor] Spawned test instance of {_enemy.displayName}");
            }
        }

        Color GetArchetypeColor(EnemyArchetype archetype)
        {
            return archetype switch
            {
                EnemyArchetype.Boss => new Color(1f, 0.2f, 0.2f),      // Red
                EnemyArchetype.Elite => new Color(0.7f, 0.2f, 0.9f),  // Purple
                EnemyArchetype.Tank => new Color(0.5f, 0.5f, 0.5f),   // Gray
                EnemyArchetype.Swarm => new Color(1f, 0.9f, 0.2f),    // Yellow
                EnemyArchetype.Caster => new Color(0.2f, 0.5f, 1f),   // Blue
                _ => Color.white
            };
        }
    }
}
#endif
