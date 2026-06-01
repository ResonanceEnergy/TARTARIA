# QUERY SYSTEM MIGRATION GUIDE

**Quick reference for migrating from O(n) linear searches to O(1) indexed queries**

---

## INITIALIZATION

Add `DataRegistryInitializer` to your startup scene:

```csharp
// In GameManager.Awake() or similar:
var initializer = gameObject.AddComponent<DataRegistryInitializer>();
initializer.InitializeAllRegistries();

// OR: Create GameObject with DataRegistryInitializer component in scene
```

---

## MIGRATION PATTERNS

### BEFORE: Item Queries (O(n))
```csharp
// Get item by ID
var item = itemDatabase.GetItem("aether_shard");

// Get items by category
var materials = itemDatabase.GetItemsByCategory(ItemCategory.Material);

// Get items by rarity
var rareItems = itemDatabase.GetItemsByRarity(ItemRarity.Rare);

// Complex filter
var items = itemDatabase.GetAllItems()
    .Where(i => i.category == ItemCategory.Equipment)
    .Where(i => i.rarity == ItemRarity.Epic)
    .Where(i => i.weight <= 5.0f)
    .OrderBy(i => i.value)
    .Take(10)
    .ToList();
```

### AFTER: Item Queries (O(1))
```csharp
using Tartaria.Data.Query;

// Get item by ID (O(1))
var item = ItemRegistry.Get("aether_shard");

// Get items by category (O(1))
var materials = ItemRegistry.GetByCategory(ItemCategory.Material);

// Get items by rarity (O(1))
var rareItems = ItemRegistry.GetByRarity(ItemRarity.Rare);

// Complex filter (cached)
var items = ItemRegistry.Query()
    .Where(i => i.category == ItemCategory.Equipment)
    .Where(i => i.rarity == ItemRarity.Epic)
    .Where(i => i.weight <= 5.0f)
    .OrderBy(i => i.value)
    .Take(10)
    .ToList();
```

---

### BEFORE: Quest Queries (O(n))
```csharp
// Get quests by moon
var moon1Quests = questDatabase.GetQuestsByMoon(1);

// Get quests by category
var mainQuests = questDatabase.GetQuestsByCategory(QuestCategory.Main);

// Get available quests
var available = questDatabase.GetMainQuestChain()
    .Where(q => q.rsRequirement <= playerRS)
    .ToArray();
```

### AFTER: Quest Queries (O(1))
```csharp
using Tartaria.Data.Query;

// Get quests by moon (O(1))
var moon1Quests = QuestRegistry.GetByMoon(1);

// Get quests by category (O(1))
var mainQuests = QuestRegistry.GetByCategory(QuestCategory.Main);

// Get available quests (cached)
var available = QuestRegistry.GetAvailableQuests(playerRS);
```

---

### BEFORE: Crafting Queries (O(n))
```csharp
// Get recipes by tier
var commonRecipes = new List<CraftingRecipeData>();
foreach (var recipe in craftingDatabase.recipes)
{
    if (recipe.requiredTier == MaterialTier.Common)
        commonRecipes.Add(recipe);
}

// Get recipes by station
var forgeRecipes = craftingDatabase.recipes
    .Where(r => r.requiredStation == StationType.Forge)
    .ToList();
```

### AFTER: Crafting Queries (O(1))
```csharp
using Tartaria.Data.Query;

// Get recipes by tier (O(1))
var commonRecipes = CraftingRecipeRegistry.GetByTier(MaterialTier.Common);

// Get recipes by station (O(1))
var forgeRecipes = CraftingRecipeRegistry.GetByStation(StationType.Forge);

// Get craftable recipes (complex query)
var craftable = CraftingRecipeRegistry.GetCraftableRecipes(inventory);
```

---

### BEFORE: Skill Queries (O(n))
```csharp
// Get skills by tier
var tier1Skills = skillTree.nodes
    .Where(n => n != null && n.tier == 1)
    .ToList();

// Get affordable skills
var affordable = skillTree.nodes
    .Where(n => n != null && n.rsCost <= playerRS)
    .OrderBy(n => n.tier)
    .ToList();
```

### AFTER: Skill Queries (O(1))
```csharp
using Tartaria.Data.Query;

// Get skills by tier (O(1))
var tier1Skills = SkillRegistry.GetByTier(1);

// Get affordable skills (cached)
var affordable = SkillRegistry.GetAffordableSkills(playerRS);

// Get blessings (O(1))
var blessings = SkillRegistry.GetBlessings();
```

---

## QUERY BUILDER EXAMPLES

### Chained Filters
```csharp
// Find epic equipment under 100 RS value, sorted by weight
var items = ItemRegistry.Query()
    .Where(i => i.category == ItemCategory.Equipment)
    .Where(i => i.rarity == ItemRarity.Epic)
    .Where(i => i.value <= 100)
    .OrderBy(i => i.weight)
    .ToList();
```

### Pagination
```csharp
// Get page 2 of rare items (10 per page)
var page2 = ItemRegistry.Query()
    .Where(i => i.rarity == ItemRarity.Rare)
    .OrderBy(i => i.displayName)
    .Skip(10)
    .Take(10)
    .ToList();
```

### Existence Checks
```csharp
// Check if any epic items exist
bool hasEpic = ItemRegistry.Query()
    .Where(i => i.rarity == ItemRarity.Epic)
    .Any();

// Count rare items
int rareCount = ItemRegistry.Query()
    .Where(i => i.rarity == ItemRarity.Rare)
    .Count();
```

### First Match
```csharp
// Get first available quest at RS 100
var quest = QuestRegistry.Query()
    .Where(q => q.rsRequirement <= 100f)
    .OrderBy(q => q.moonId)
    .FirstOrDefault();
```

---

## PERFORMANCE TESTING

Add `QueryPerformanceBenchmark` component to test performance:

```csharp
// In Unity Editor:
1. Create GameObject → Add QueryPerformanceBenchmark component
2. Assign ItemDatabase, QuestDatabase, CraftingDatabase in Inspector
3. Right-click component → Run All Benchmarks
4. Check Console for results

// Expected output:
[Item GetByCategory]
  Old (O(n)): 12.5μs
  New (O(1)): 0.8μs
  Speedup: 15.6x faster
```

---

## COMMON PATTERNS

### Filtering + Sorting
```csharp
// Old (O(n * log n))
var items = itemDatabase.GetAllItems()
    .Where(i => i.value >= 100)
    .OrderByDescending(i => i.rarity)
    .ToList();

// New (O(1) + cached)
var items = ItemRegistry.Query()
    .Where(i => i.value >= 100)
    .OrderByDescending(i => (int)i.rarity)
    .ToList();
```

### Existence Checks
```csharp
// Old (O(n))
bool hasQuest = questDatabase.GetQuestsByMoon(1).Length > 0;

// New (O(1))
bool hasQuest = QuestRegistry.GetByMoon(1).Count > 0;
```

### Conditional Queries
```csharp
// Build query dynamically
var query = ItemRegistry.Query();

if (filterByCategory)
    query = query.Where(i => i.category == selectedCategory);

if (filterByRarity)
    query = query.Where(i => i.rarity >= minRarity);

var results = query.OrderBy(i => i.displayName).ToList();
```

---

## TROUBLESHOOTING

### Issue: "Registry not initialized"
**Solution**: Ensure `DataRegistryInitializer` runs before any registry calls.
```csharp
// Add to game startup:
DataRegistryInitializer.InitializeAllRegistries();
```

### Issue: "Fallback to O(n) search"
**Cause**: Registry not initialized or build configuration excludes registry code.
**Solution**: Check console for initialization logs. Registries must be initialized in Awake/Start.

### Issue: "Null reference exception"
**Cause**: Database asset not loaded from Resources.
**Solution**: Verify database paths in DataRegistryInitializer Inspector.

---

## BEST PRACTICES

1. **Initialize once at startup** — Don't initialize multiple times
2. **Use registries for runtime queries** — Databases for editor tools
3. **Cache complex queries** — QueryBuilder automatically caches
4. **Avoid in Update()** — Even O(1) queries have overhead
5. **Profile before/after** — Use QueryPerformanceBenchmark to verify gains

---

## NEED HELP?

- See `AGENT8_QUERY_SYSTEM_REPORT.md` for full documentation
- Check `QueryPerformanceBenchmark.cs` for example usage
- Examine existing registry classes for patterns
- Test with small datasets first, then scale up

---

**Migration Priority:**
1. ✅ Hot paths (Update/FixedUpdate loops)
2. ✅ UI queries (inventory filtering, quest tracking)
3. ✅ Save/load operations
4. ⚠️ Editor tools (optional — databases work fine)

Happy optimizing! 🚀
