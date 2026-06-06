# TARTARIA Moon 11-13 Quest Definitions Required

## Quest IDs Referenced in Code (Need Implementation)

### Moon 11 Quests
```csharp
// QuestManager.Instance?.ActivateQuest()
"moon11_aquifer_discovery"           // Discover corrupted aquifer
"moon11_aquifer_purification"        // Purify 5 aquifer nodes
"moon11_memory_echoes"               // View all 7 memory echo visions
"moon11_fountain_chain_complete"     // All 10 fountains activated

// QuestManager.Instance?.CompleteQuest()
"moon11_memory_echoes_complete"      // All echoes viewed
```

### Moon 12 Quests
```csharp
// QuestManager.Instance?.ActivateQuest()
"moon12_bell_synchronization"        // Synchronize all 12 bell towers
"moon12_defend_bell_network"         // Defend towers from Reset assault
"moon12_cymatic_tuning"              // Complete cymatic tuning puzzle

// QuestManager.Instance?.CompleteQuest()
"moon12_bell_network_synchronized"   // All 12 towers ringing in harmony
```

### Moon 13 Quests
```csharp
// QuestManager.Instance?.ActivateQuest()
"moon13_final_node_discovery"        // Discover final node beneath New Chicago
"moon13_echo_realms"                 // Visit all 3 echo realm gates
"moon13_zereth_resonance_dialogue"   // Zereth confrontation

// QuestManager.Instance?.CompleteQuest()
"moon13_zereth_resonance_complete"   // Zereth echo calmed
"moon13_cosmic_alignment_complete"   // Final node activated (any path)

// EndCardController triggers (auto-created by ending execution)
"moon13_harmony_ending"              // Harmony path chosen
"moon13_echo_ending"                 // Echo path chosen
"moon13_reset_ending"                // Reset path chosen
```

## Quest Objectives Referenced

### Moon 11
- `QuestManager.Instance?.ProgressObjective("moon11_aquifer_purification", 0, 1)` - Per node purified
- `QuestManager.Instance?.ProgressObjective("moon11_memory_echoes", 0, 1)` - Per echo viewed

### Moon 12
- `QuestManager.Instance?.ProgressObjective("moon12_bell_synchronization", 0, 1)` - Per tower synced
- `QuestManager.Instance?.ProgressObjective("moon12_cymatic_tuning", 0, 1)` - Per puzzle solved

### Moon 13
- `QuestManager.Instance?.ProgressObjective("moon13_echo_realms", 0, 1)` - Per realm visited

## Achievement IDs Referenced

```csharp
// Moon 11
"planetary_fountain_restoration"     // Complete fountain chain
"memory_archivist"                   // View all memory echoes

// Moon 12
"planetary_bell_harmony"             // All 12 bells synchronized
"harmonic_master"                    // Complete cymatic tuning

// Moon 13
"harmony_ending_golden_age"          // Achieve Harmony ending
"echo_ending_parallel_worlds"        // Achieve Echo ending
"reset_ending_controlled_power"      // Achieve Reset ending
```

## Dialogue Context IDs Referenced

### Moon 11
```csharp
"lirael_aquifer_sensing"             // Aquifer discovered
"lirael_water_remembers"             // Core purified
"lirael_echoes_complete"             // All echoes viewed
"echo_aquifer_1" through "echo_aquifer_7"  // 7 memory echo dialogues
```

### Moon 12
```csharp
"korath_bells_were_first"            // Moon 12 unlocked
"reset_commander_final_assault"      // Reset assault triggered
"korath_feel_dawn_again"             // Planetary ring moment
"moon12_prophecy_stone_promise"      // Stone #12 revealed
"korath_frequencies_aligned"         // Cymatic puzzle complete
```

### Moon 13
```csharp
"zereth_you_deserve_truth"           // Moon 13 unlocked
"echo_realm_golden_age"              // Golden Age realm visited
"echo_realm_dissonant"               // Dissonant timeline visited
"echo_realm_flood_moment"            // Flood moment witnessed
"zereth_wanted_more"                 // Zereth manifests
"lirael_we_hear_you_now"             // Lirael joins confrontation

// Harmony ending dialogues
"lirael_lullaby_finale"
"milo_remembering_more"
"thorne_skys_ours_again"
"korath_song_resumes"
"zereth_at_last"

// Echo ending
"echo_ending_threshold"

// Reset ending
"reset_ending_control"
```

## Implementation Priority

### High Priority (Blocking Gameplay):
1. Moon 11: moon11_aquifer_discovery, moon11_aquifer_purification
2. Moon 12: moon12_bell_synchronization
3. Moon 13: moon13_final_node_discovery, moon13_echo_realms

### Medium Priority (Completion Rewards):
1. All _complete quests
2. Ending trigger quests

### Low Priority (Polish):
1. Detailed quest descriptions
2. Dialogue VO recording
3. Context-specific triggers

## Quest Definition Template

```csharp
// Add to QuestManager quest dictionary
new Quest
{
    id = "moon11_aquifer_discovery",
    title = "Ancient Aquifer",
    description = "Investigate the corrupted water source beneath the oldest star fort.",
    objectives = new[]
    {
        new QuestObjective { description = "Locate aquifer entrance", required = 1 },
        new QuestObjective { description = "Descend to core chamber", required = 1 }
    },
    rewards = new QuestRewards
    {
        experience = 2000,
        aetherShards = 500
    },
    moonNumber = 11
}
```

---

**Total IDs to Implement:** 26 quest IDs + 6 achievements + 30 dialogue contexts = 62 content references
