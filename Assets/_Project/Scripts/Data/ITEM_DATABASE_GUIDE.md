# ItemDatabase System — Usage Guide

## Overview
Data-driven item system for TARTARIA using ScriptableObjects. All item metadata (icons, descriptions, stats, categories) stored in reusable assets.

## Architecture

### Core Classes
- **ItemData.cs** — Individual item definition (ScriptableObject)
- **ItemDatabase.cs** — Collection of all items with lookup API
- **ItemDatabaseEditor.cs** — Editor tools for database management

### Integration Points
- **InventorySystem.cs** — Validates item IDs against database
- **InventoryUIPanel.cs** — Displays item icons, names, descriptions from database

## Quick Start

### 1. Create ItemDatabase Asset
**Menu:** `Assets → Create → Tartaria → Setup ItemDatabase`

This creates: `Assets/_Project/Resources/ItemDatabase.asset`

### 2. Create Example Items
**Menu:** `Assets → Create → Tartaria → Setup Example Items`

This creates 5 example items in `Assets/_Project/Resources/Items/`:
- `aether_shard.asset` — Rare crafting material (150 RS)
- `golem_core.asset` — Uncommon enemy drop (85 RS)
- `resonance_crystal.asset` — Epic quest reward (500 RS)
- `repair_kit.asset` — Common consumable (30 RS)
- `health_potion.asset` — Common consumable (25 RS)

### 3. Populate Database
1. Select `ItemDatabase.asset` in Project window
2. Click **"Auto-Populate from Assets"** button in Inspector
3. Verify items appear in the list

### 4. Create New Items
**Menu:** `Assets → Create → Tartaria → Item Data`

Fill in:
- **itemID** — Unique identifier (e.g., `"flame_sword"`)
- **displayName** — UI name (e.g., `"Flame Sword"`)
- **description** — Tooltip text
- **icon** — Drag sprite from Assets
- **category** — Consumable, Equipment, Material, QuestItem, Currency, Misc
- **rarity** — Common, Uncommon, Rare, Epic, Legendary, Mythic
- **stackSize** — Max stack count (1 = non-stackable)
- **weight** — Encumbrance value (kg)
- **value** — Vendor price (RS)

### 5. Add to Database
Two methods:
- **Auto:** Click "Auto-Populate from Assets" in ItemDatabase Inspector
- **Manual:** Drag ItemData asset into "Items" list in ItemDatabase Inspector

## Code Usage

### Get Item Data
```csharp
var inventory = InventorySystem.Instance;
ItemData item = inventory.GetItemData("aether_shard");

if (item != null)
{
    Debug.Log($"Name: {item.displayName}");
    Debug.Log($"Desc: {item.description}");
    Debug.Log($"Value: {item.value} RS");
    Debug.Log($"Rarity: {item.rarity}");
}
```

### Check if Item Exists
```csharp
var db = ItemDatabase.LoadDatabase();
if (db.HasItem("golem_core"))
{
    // Item is registered
}
```

### Get Items by Category
```csharp
var db = ItemDatabase.LoadDatabase();
var consumables = db.GetItemsByCategory(ItemCategory.Consumable);
foreach (var item in consumables)
{
    Debug.Log($"Consumable: {item.displayName}");
}
```

### Add Item to Inventory (with validation)
```csharp
var inventory = InventorySystem.Instance;

// This now validates against ItemDatabase
bool success = inventory.AddItem("aether_shard", 5);

if (success)
{
    Debug.Log("Added 5 aether shards");
}
else
{
    Debug.LogWarning("Failed — either full or invalid item ID");
}
```

## InventorySystem Integration

### Validation Toggle
InventorySystem Inspector has a **"Validate Item IDs"** checkbox:
- **Enabled (default):** AddItem() rejects unknown itemIDs
- **Disabled:** Allows any string (legacy mode)

### Item Data Access
```csharp
// From InventorySystem
var inventory = InventorySystem.Instance;
ItemData item = inventory.GetItemData("health_potion");
```

## UI Integration

### InventoryUIPanel Features
- Auto-loads item icons from ItemDatabase
- Tooltips show displayName, description, rarity, category, value, weight
- Rarity color-coded (yellow text)
- Falls back to Resources/Items/{itemId}.png for legacy support

### Tooltip Format
```
Aether Shard x5
A crystalline fragment pulsing with temporal energy. Essential for resonance rituals.

Rare | Material
Value: 150 RS | Weight: 0.2 kg
```

## Editor Tools

### ItemDatabase Inspector Buttons
- **Auto-Populate from Assets** — Finds all ItemData assets in project
- **Validate Item IDs** — Checks for duplicates, missing fields, invalid data
- **Sort Items by ID** — Alphabetizes item list

### Validation Checks
- Empty itemID detection
- Duplicate itemID detection
- Missing displayName warnings
- Missing icon warnings
- Invalid stackSize warnings

## File Locations

### Required
- `Assets/_Project/Resources/ItemDatabase.asset` — Main database (singleton)
- `Assets/_Project/Resources/Items/*.asset` — Item definitions

### Scripts
- `Assets/_Project/Scripts/Data/ItemData.cs`
- `Assets/_Project/Scripts/Data/ItemDatabase.cs`
- `Assets/_Project/Scripts/Editor/ItemDatabaseEditor.cs`

### Modified
- `Assets/_Project/Scripts/Gameplay/InventorySystem.cs`
- `Assets/_Project/Scripts/UI/InventoryUIPanel.cs`

## Item Categories

| Category   | Use Case                           |
|------------|-----------------------------------|
| Consumable | Potions, food, single-use items   |
| Equipment  | Weapons, armor, tools             |
| Material   | Crafting resources, enemy drops   |
| QuestItem  | Quest-specific items              |
| Currency   | Resonance Shards, special coins   |
| Misc       | Everything else                   |

## Item Rarity

| Rarity    | Color Hint    | Use Case                |
|-----------|--------------|-------------------------|
| Common    | White/Gray   | Basic items             |
| Uncommon  | Green        | Enemy drops             |
| Rare      | Blue         | Special resources       |
| Epic      | Purple       | Boss drops, quest items |
| Legendary | Orange/Gold  | Unique artifacts        |
| Mythic    | Red/Crimson  | Endgame legendary items |

## Adding Icons

### Option 1: Assign in Inspector
1. Import sprite to `Assets/_Project/Textures/Items/`
2. Select ItemData asset
3. Drag sprite to "Icon" field

### Option 2: Auto-Assignment (future)
- Place sprites in `Assets/_Project/Textures/Items/{itemID}.png`
- Editor script can auto-assign by matching itemID

## Performance Notes

- Database lookup cached after first access (Dictionary)
- No per-frame overhead
- Item icons cached in InventoryUIPanel
- Database loaded once in InventorySystem.Awake()

## Migration from Old System

### Old Code
```csharp
inventory.AddItem("aether_shard", 5);  // No validation
```

### New Code
```csharp
// Same API, now validated against ItemDatabase
inventory.AddItem("aether_shard", 5);

// Get rich metadata
ItemData item = inventory.GetItemData("aether_shard");
if (item != null)
{
    icon = item.icon;
    tooltip = item.description;
}
```

### No Breaking Changes
- Existing AddItem/RemoveItem/GetItemCount still work
- String IDs still used internally
- Database validation is opt-in (toggle in Inspector)

## Example Workflow

### Creating a New Quest Item
1. Create ItemData: `Assets → Create → Tartaria → Item Data`
2. Name asset: `ancient_key.asset`
3. Fill in:
   - itemID: `"ancient_key"`
   - displayName: `"Ancient Key"`
   - description: `"A rusted key bearing strange glyphs."`
   - category: `QuestItem`
   - rarity: `Rare`
   - stackSize: `1` (unique)
   - value: `0` (quest items usually worthless to vendors)
4. Assign icon sprite
5. Select ItemDatabase → "Auto-Populate from Assets"
6. Use in code: `inventory.AddItem("ancient_key")`

## Troubleshooting

### "Failed to load from Resources/ItemDatabase.asset"
**Solution:** Run `Assets → Create → Tartaria → Setup ItemDatabase`

### "Item 'xyz' not found in database"
**Solution:** 
1. Create ItemData asset for 'xyz'
2. Select ItemDatabase
3. Click "Auto-Populate from Assets"

### Icons not showing in UI
**Check:**
1. ItemData has icon assigned (Inspector)
2. ItemDatabase contains the item
3. InventorySystem.validateItemIDs is enabled

### Validation disabled after load
**Cause:** ItemDatabase failed to load from Resources
**Fix:** Ensure ItemDatabase.asset exists at `Assets/_Project/Resources/ItemDatabase.asset`

## CS:0 Status
✓ All files compile cleanly
✓ No missing references
✓ Editor scripts use `#if UNITY_EDITOR` guards
✓ Database loads in Awake(), no runtime exceptions

## Next Steps
- Add custom item behaviors (IUsable interface)
- Implement equipment slots (weapon/armor)
- Create vendor system with value-based pricing
- Add item crafting recipes
- Implement weight-based encumbrance

---

**Created:** 2026-05-22 by Agent 2  
**Status:** Production-ready  
**Maintainer:** Tartaria Core Systems Team
