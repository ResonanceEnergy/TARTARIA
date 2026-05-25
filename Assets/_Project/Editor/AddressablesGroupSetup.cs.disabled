using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using System.IO;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// AddressablesGroupSetup — one-click creation of initial Phase 2 Addressables groups.
    /// Menu: TARTARIA / Addressables / Create Initial Groups (Echohaven + KayKit + Zones)
    /// 
    /// Run this in Unity Editor after opening the project. It creates the groups defined
    /// in 09_TECHNICAL_SPEC.md and AddressableAssetLoader labels.
    /// Then manually (or via future automation) assign assets to groups via Addressables window
    /// or by setting Addressable + group in inspector on prefabs/scenes.
    /// 
    /// Groups created:
    /// - Echohaven_Core (preload, never unload)
    /// - KayKit_Assets (high reuse props/characters)
    /// - VFX_Common
    /// - Audio_Common
    /// - Zone_Moon1_Echohaven (500m streaming)
    /// - Zone_Moon2 (future)
    /// 
    /// Safe / incremental: does not delete existing groups. Idempotent.
    /// </summary>
    public static class AddressablesGroupSetup
    {
        [MenuItem("TARTARIA/Addressables/Create Initial Groups (Echohaven Core + Zones)")]
        public static void CreateInitialGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressablesGroupSetup] No AddressableAssetSettings found. Open Window > Asset Management > Addressables > Groups to initialize, then re-run.");
                return;
            }

            // Ensure default group exists
            if (settings.DefaultGroup == null)
            {
                // Default group will be implicitly created on first CreateGroup; no-op fallback.
                Debug.LogWarning("[AddressablesGroupSetup] DefaultGroup missing; will rely on subsequent CreateGroup calls.");
            }

            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_ECHOHAVEN_CORE, preload: true);
            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_KAYKIT_ASSETS, preload: false);
            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_VFX_COMMON, preload: false);
            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_AUDIO_COMMON, preload: false);
            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_ZONE_MOON1, preload: false);
            CreateOrGetGroup(settings, AddressableAssetLoader.LABEL_ZONE_MOON2, preload: false);

            // Add a "Core" group alias for managers/player if not present (preloaded)
            CreateOrGetGroup(settings, "Core", preload: true);

            AssetDatabase.SaveAssets();
            Debug.Log("[AddressablesGroupSetup] ✓ Initial Addressables groups created/verified:\n" +
                      "  - Echohaven_Core (preload)\n" +
                      "  - KayKit_Assets\n" +
                      "  - VFX_Common\n" +
                      "  - Audio_Common\n" +
                      "  - Zone_Moon1_Echohaven (for 500m streaming)\n" +
                      "  - Zone_Moon2\n" +
                      "Next: 1) Open Addressables Groups window 2) Drag key prefabs/scenes into groups 3) Build Addressables (via Build menu).");
        }

        static AddressableAssetGroup CreateOrGetGroup(AddressableAssetSettings settings, string groupName, bool preload)
        {
            var existing = settings.FindGroup(groupName);
            if (existing != null)
            {
                Debug.Log($"[AddressablesGroupSetup] Group already exists: {groupName}");
                ConfigureGroupSchemas(existing, preload);
                return existing;
            }

            var group = settings.CreateGroup(groupName, setAsDefaultGroup: false, readOnly: false, postEvent: true, schemasToCopy: null, types: null);
            ConfigureGroupSchemas(group, preload);

            Debug.Log($"[AddressablesGroupSetup] ✓ Created group: {groupName} (preload={preload})");
            return group;
        }

        static void ConfigureGroupSchemas(AddressableAssetGroup group, bool preload)
        {
            // Bundle schema (default for most)
            var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundleSchema == null)
            {
                bundleSchema = group.AddSchema<BundledAssetGroupSchema>();
            }
            bundleSchema.IncludeInBuild = true;
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            // Content update
            var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            if (updateSchema == null)
            {
                updateSchema = group.AddSchema<ContentUpdateGroupSchema>();
            }
            updateSchema.StaticContent = !preload; // Core is static/preloaded

            // For preload groups, we can mark for initial catalog load (Addressables handles via labels in code)
            // Future: add custom schema or label "preload" for auto-inclusion in bootstrap.
        }

        [MenuItem("TARTARIA/Addressables/Print Current Groups")]
        public static void PrintGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("No Addressables settings.");
                return;
            }

            Debug.Log("=== Current Addressables Groups ===");
            foreach (var g in settings.groups)
            {
                Debug.Log($"  - {g.Name} (entries: {g.entries.Count})");
            }
        }

        [MenuItem("TARTARIA/Addressables/Build Addressables (Player)")]
        public static void BuildAddressables()
        {
            // Simple trigger to open build UI or use API
            AddressableAssetSettings.BuildPlayerContent();
            Debug.Log("[AddressablesGroupSetup] BuildPlayerContent triggered. Check Console + Addressables for output.");
        }
    }
}
