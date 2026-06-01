using UnityEngine;
using UnityEditor;
using System.IO;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// EquipmentAssetGenerator — Editor utility to create example equipment ScriptableObjects.
    /// Menu: Tartaria > Generate Equipment Assets
    /// Creates 6 starter equipment items in Assets/_Project/Resources/Equipment/
    /// 
    /// Generated equipment:
    /// - Iron Sword (Weapon) — STR +5, ARM +2
    /// - Leather Armor (Armor) — VIT +8, ARM +15
    /// - Steel Helmet (Helmet) — VIT +4, ARM +10
    /// - Work Gloves (Gloves) — STR +3, AGI +2
    /// - Leather Boots (Boots) — AGI +5, VIT +3
    /// - Resonance Amulet (Accessory) — RES +10, ATT +5
    /// </summary>
    public class EquipmentAssetGenerator
    {
        const string OUTPUT_PATH = "Assets/_Project/Resources/Equipment";

        [MenuItem("Tartaria/4 Generate Art/Equipment Assets", priority = 485)]
        public static void GenerateEquipmentAssets()
        {
            // Ensure output directory exists
            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
                AssetDatabase.Refresh();
            }

            // Create equipment assets
            CreateIronSword();
            CreateLeatherArmor();
            CreateSteelHelmet();
            CreateWorkGloves();
            CreateLeatherBoots();
            CreateResonanceAmulet();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EquipmentGen] Created 6 equipment assets in {OUTPUT_PATH}");
            EditorUtility.DisplayDialog(
                "Equipment Assets Created",
                "Created 6 starter equipment items:\n\n" +
                "• Iron Sword (Weapon)\n" +
                "• Leather Armor (Armor)\n" +
                "• Steel Helmet (Helmet)\n" +
                "• Work Gloves (Gloves)\n" +
                "• Leather Boots (Boots)\n" +
                "• Resonance Amulet (Accessory)\n\n" +
                $"Location: {OUTPUT_PATH}",
                "OK"
            );
        }

        static void CreateIronSword()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "iron_sword";
            item.itemName = "Iron Sword";
            item.slot = EquipSlot.Weapon;
            item.strengthBonus = 5;
            item.agilityBonus = 0;
            item.vitalityBonus = 0;
            item.resonanceBonus = 0;
            item.attunementBonus = 0;
            item.armorValue = 2;
            item.specialEffects = new string[] { "+10% Physical Damage", "Durability: 100" };
            item.description = "A well-crafted iron blade. Standard issue for Echohaven guards.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/IronSword.asset");
        }

        static void CreateLeatherArmor()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "leather_armor";
            item.itemName = "Leather Armor";
            item.slot = EquipSlot.Armor;
            item.strengthBonus = 0;
            item.agilityBonus = 0;
            item.vitalityBonus = 8;
            item.resonanceBonus = 0;
            item.attunementBonus = 0;
            item.armorValue = 15;
            item.specialEffects = new string[] { "+5% Health Regen", "Weight: Medium" };
            item.description = "Sturdy leather chest piece. Provides solid protection without sacrificing mobility.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/LeatherArmor.asset");
        }

        static void CreateSteelHelmet()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "steel_helmet";
            item.itemName = "Steel Helmet";
            item.slot = EquipSlot.Helmet;
            item.strengthBonus = 0;
            item.agilityBonus = 0;
            item.vitalityBonus = 4;
            item.resonanceBonus = 0;
            item.attunementBonus = 0;
            item.armorValue = 10;
            item.specialEffects = new string[] { "+5% Crit Resistance", "Blocks Headshot Damage" };
            item.description = "Reinforced steel helm. Essential for frontline combat.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/SteelHelmet.asset");
        }

        static void CreateWorkGloves()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "work_gloves";
            item.itemName = "Work Gloves";
            item.slot = EquipSlot.Gloves;
            item.strengthBonus = 3;
            item.agilityBonus = 2;
            item.vitalityBonus = 0;
            item.resonanceBonus = 0;
            item.attunementBonus = 0;
            item.armorValue = 3;
            item.specialEffects = new string[] { "+5% Crafting Speed", "Reduced Tool Durability Loss" };
            item.description = "Worn but reliable leather gloves. Perfect for construction work.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/WorkGloves.asset");
        }

        static void CreateLeatherBoots()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "leather_boots";
            item.itemName = "Leather Boots";
            item.slot = EquipSlot.Boots;
            item.strengthBonus = 0;
            item.agilityBonus = 5;
            item.vitalityBonus = 3;
            item.resonanceBonus = 0;
            item.attunementBonus = 0;
            item.armorValue = 5;
            item.specialEffects = new string[] { "+8% Movement Speed", "Reduces Fall Damage" };
            item.description = "Light and comfortable boots. Ideal for long journeys across the Moons.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/LeatherBoots.asset");
        }

        static void CreateResonanceAmulet()
        {
            var item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.itemID = "resonance_amulet";
            item.itemName = "Resonance Amulet";
            item.slot = EquipSlot.Accessory;
            item.strengthBonus = 0;
            item.agilityBonus = 0;
            item.vitalityBonus = 0;
            item.resonanceBonus = 10;
            item.attunementBonus = 5;
            item.armorValue = 0;
            item.specialEffects = new string[] { "+15% RS Regen Rate", "+5% Ability Power", "Resonance Vision Range +10m" };
            item.description = "An ancient amulet infused with Resonance Stone energy. Amplifies the bearer's connection to the Resonance Field.";

            AssetDatabase.CreateAsset(item, $"{OUTPUT_PATH}/ResonanceAmulet.asset");
        }
    }
}
