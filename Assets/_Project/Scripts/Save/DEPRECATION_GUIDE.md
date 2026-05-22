# Data Schema Deprecation & Migration Guide

## Overview
TARTARIA uses a versioned schema system to ensure old saves/assets remain compatible with new game versions. This guide explains how to deprecate old fields and migrate to new schemas.

---

## When to Bump Schema Version

### ✅ BUMP VERSION FOR:
- **Field added** (new property in ScriptableObject or SaveData)
- **Field removed** (delete old property)
- **Field renamed** (change property name)
- **Field type changed** (int → float, string → enum, etc.)
- **Enum value added/removed** (ItemCategory, QuestCategory, etc.)
- **Breaking serialization change** (array → list, etc.)

### ❌ DO NOT BUMP FOR:
- **Field value changed** (data change, not schema change)
- **Documentation updated** (comments, tooltips)
- **Code refactor** (method rename, extract helper)
- **Bug fix** (logic change without schema impact)

---

## Step-by-Step Migration Process

### 1. Mark Old Field as Obsolete

```csharp
[System.Obsolete("Use 'durability' instead. Removed in v3.")]
public float oldHealthField;

public float durability = 100f; // New field
```

### 2. Bump Schema Version

Update `SchemaVersion.cs`:
```csharp
public const int ITEM_V1 = 1;
public const int ITEM_V2 = 2;  // New version
public const int CURRENT_ITEM = ITEM_V2; // Update current
```

### 3. Create Migrator

Create new file in `Scripts/Save/Migrators/`:
```csharp
public class ItemDataMigrator_V1_to_V2 : IDataMigrator<ItemData, ItemData>
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public ItemData Migrate(ItemData input)
    {
        var output = Object.Instantiate(input);
        output.durability = input.oldHealthField; // Copy old data
        return output;
    }

    public string GetChangeDescription()
    {
        return "V1→V2: Renamed oldHealthField to durability";
    }
}
```

### 4. Add OnAfterDeserialize Handler

In your data class (ItemData, QuestData, etc.):
```csharp
public void OnAfterDeserialize()
{
    int currentVersion = SchemaVersion.CURRENT_ITEM;
    
    if (schemaVersion < currentVersion)
    {
        Debug.Log($"[ItemData] {name}: Auto-migrating v{schemaVersion}→v{currentVersion}");
        
        // Apply migration logic
        if (schemaVersion < 2)
        {
            durability = oldHealthField; // V1→V2 migration
        }
        
        schemaVersion = currentVersion;
    }
}
```

### 5. Run Batch Migration Tool

1. Open: **Tools → Tartaria → Data Migration → Open Migration Tool**
2. Click: **Scan All Data Assets**
3. Enable: **Dry Run (Preview Only)**
4. Click: **Preview Migration** (verify changes)
5. Disable: **Dry Run**
6. Enable: **Create Backup First**
7. Click: **⚠ APPLY MIGRATION ⚠**

### 6. Test Migration

```csharp
[Test]
public void ItemData_V1_to_V2_Migration()
{
    var migrator = new ItemDataMigrator_V1_to_V2();
    var oldItem = CreateV1Item();
    oldItem.oldHealthField = 75f;

    var newItem = migrator.Migrate(oldItem);

    Assert.AreEqual(75f, newItem.durability);
    Assert.AreEqual(2, newItem.schemaVersion);
}
```

### 7. Update Changelog

In `SchemaVersion.GetChangelog()`:
```csharp
if (dataType == "ItemData")
{
    if (fromVersion < 2 && toVersion >= 2)
        log += "  • Renamed oldHealthField → durability\n";
}
```

---

## Common Migration Patterns

### Pattern 1: Field Rename
```csharp
// Old
public string itemName;

// New
public string displayName;

// Migration
output.displayName = input.itemName;
```

### Pattern 2: Type Change
```csharp
// Old
public string categoryString;

// New
public ItemCategory category;

// Migration
output.category = Enum.Parse<ItemCategory>(input.categoryString);
```

### Pattern 3: Field Split
```csharp
// Old
public string itemStats; // JSON string

// New
public int attack;
public int defense;

// Migration
var stats = JsonUtility.FromJson<ItemStats>(input.itemStats);
output.attack = stats.atk;
output.defense = stats.def;
```

### Pattern 4: Field Merge
```csharp
// Old
public int healthMin;
public int healthMax;

// New
public Vector2Int healthRange;

// Migration
output.healthRange = new Vector2Int(input.healthMin, input.healthMax);
```

### Pattern 5: Enum Value Added
```csharp
// Old enum: Common, Uncommon, Rare

// New enum: Common, Uncommon, Rare, Epic, Legendary

// Migration
if (input.rarity == ItemRarity.Rare && input.value > 5000)
{
    output.rarity = ItemRarity.Epic; // Upgrade high-value Rare → Epic
}
```

---

## Backward Compatibility Rules

1. **Support 10 versions back** (configurable in `SchemaVersion.IsCompatible`)
2. **Never delete data** (deprecated fields kept for migration)
3. **Always provide migration path** (no breaking changes)
4. **Test all migration paths** (unit tests required)
5. **Document all changes** (changelog + deprecation notices)

---

## Deprecation Timeline

| Phase | Duration | Action |
|-------|----------|--------|
| **Deprecation Announced** | 2 releases | Mark field `[Obsolete]`, add migration |
| **Warning Phase** | 1 release | Log warnings on load, guide users to migrate |
| **Removal** | After v+3 | Delete field, keep migration for 10 versions |

Example:
- v1.0: Field `itemName` added
- v2.0: Field `displayName` added, `itemName` marked `[Obsolete]`
- v3.0: `itemName` still present but warned
- v4.0: `itemName` removed from schema (migration still works)
- v14.0: Migration for `itemName` removed (>10 versions old)

---

## Troubleshooting

### "No migration path found"
**Cause:** Missing migrator for version jump  
**Fix:** Register all intermediate migrators in MigrationPipeline

### "Validation failed"
**Cause:** Data corruption or missing required field  
**Fix:** Check `Validate()` method, add null checks

### "Migration too slow"
**Cause:** Complex migration logic or large asset count  
**Fix:** Optimize migrator, use batch processing

### "Old saves not loading"
**Cause:** Save version too old (>10 versions back)  
**Fix:** Provide upgrade path instructions to user

---

## Best Practices

1. ✅ **Always backup before migration** (automated in tool)
2. ✅ **Test migrations in dry-run mode first**
3. ✅ **Use descriptive changelog messages**
4. ✅ **Keep migration logic simple** (< 100ms per asset)
5. ✅ **Version all data types independently** (ITEM_V2 ≠ QUEST_V2)
6. ✅ **Document breaking changes in release notes**
7. ✅ **Preserve player progress at all costs** (robust error handling)

---

## API Reference

### SchemaVersion
```csharp
SchemaVersion.CURRENT_SAVE    // Latest save version
SchemaVersion.CURRENT_ITEM    // Latest item version
SchemaVersion.GetChangelog()  // Get version changelog
SchemaVersion.IsCompatible()  // Check version compatibility
```

### IDataMigrator
```csharp
int FromVersion { get; }
int ToVersion { get; }
TTo Migrate(TFrom input);
bool Validate(TFrom input);
string GetChangeDescription();
```

### MigrationPipeline
```csharp
pipeline.Register(migrator);
pipeline.Migrate(data, from, to, dryRun);
pipeline.CanMigrate(from, to);
```

---

## Examples

### SaveData Migration (v17→v18)
See: `SaveDataMigrators.cs`

### ItemData Migration (v1→v2, future)
See: `ItemDataMigrators.cs`

### QuestData Migration (v1→v2, future)
See: `QuestDataMigrators.cs`

---

## Support

**Questions?** Contact Dr. Vex Aurelian's data architecture team  
**Issues?** File bug report with save file + migration log  
**Requests?** Propose new migration patterns via pull request
