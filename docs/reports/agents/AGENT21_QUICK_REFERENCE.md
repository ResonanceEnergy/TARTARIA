# AGENT 21: Moon 7-10 Dialogue — Quick Reference Guide

**Quick lookup for developers integrating dialogue calls**

---

## Moon 7: Resonant Moon (Korath Awakening)

### Discovery Phase
```csharp
DialogueManager.Instance?.PlayContextDialogue("moon7_discovery");
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_ice_voice");
```

### Thawing Progression
```csharp
// After each thaw session (3 total)
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_thaw_session");
```

### Awakening & Teaching
```csharp
// When Korath awakens
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_awakening");

// During 9-band training
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_teaching");

// When 9-band unlocked
DialogueManager.Instance?.PlayContextDialogue("moon7_9band_unlock");
```

### Cassian Confrontation (Branching)
```csharp
bool cassianTrusted = SaveManager.Instance?.GetMoonFlag(2, "cassianTrusted");

if (cassianTrusted) {
    // Player trusted Cassian in Moon 2 → Betrayal path
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_betrayal");
    // Player chooses redemption or purge
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_redemption"); // OR
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_purge");
} else {
    // Player doubted Cassian in Moon 2 → Confrontation path
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_confront");
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_redemption"); // OR
    DialogueManager.Instance?.PlayContextDialogue("moon7_cassian_purge");
}
```

### Climax
```csharp
// During golem siege
DialogueManager.Instance?.PlayContextDialogue("moon7_golem_siege");

// Korath's sacrifice sequence
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_sacrifice");

// After sacrifice (echo voice remains)
DialogueManager.Instance?.PlayContextDialogue("moon7_korath_echo");
```

### Revelation
```csharp
DialogueManager.Instance?.PlayContextDialogue("moon7_revelation");
```

---

## Moon 8: Galactic Moon (Airship Armada)

### Discovery Phase
```csharp
// Thorne landing
DialogueManager.Instance?.PlayContextDialogue("moon8_thorne_intro");

// Thorne idle comments
DialogueManager.Instance?.PlayContextDialogue("moon8_thorne_idle");
```

### Restoration Phase
```csharp
// After each airship repair (3 total)
DialogueManager.Instance?.PlayContextDialogue("moon8_airship_repair");

// When children board
DialogueManager.Instance?.PlayContextDialogue("moon8_children_board");
```

### Conflict Phase
```csharp
// During aerial combat
DialogueManager.Instance?.PlayContextDialogue("moon8_aerial_combat");
DialogueManager.Instance?.PlayContextDialogue("moon8_thorne_combat");
```

### Climax
```csharp
// Night flight under full moon
DialogueManager.Instance?.PlayContextDialogue("moon8_thorne_night_flight");
```

### Revelation
```csharp
// Continental connection truth
DialogueManager.Instance?.PlayContextDialogue("moon8_airship_lore_revelation");

// Korath echo during megalith transport
DialogueManager.Instance?.PlayContextDialogue("moon8_korath_echo");
```

---

## Moon 9: Solar Moon (Prophecy Stones)

### Discovery Phase
```csharp
// When first stone discovered
DialogueManager.Instance?.PlayContextDialogue("moon9_stone_discovered");

// After collecting each stone (6 total)
DialogueManager.Instance?.PlayContextDialogue("moon9_stone_collected");
```

### Zereth Contact
```csharp
// First contact (distorted voice)
DialogueManager.Instance?.PlayContextDialogue("moon9_zereth_contact");

// Voice stabilizes
DialogueManager.Instance?.PlayContextDialogue("moon9_zereth_speaks");

// Confession recordings (in aurora city)
DialogueManager.Instance?.PlayContextDialogue("moon9_zereth_confession");
```

### Codex Restoration
```csharp
// When all 12 pages restored
DialogueManager.Instance?.PlayContextDialogue("moon9_codex_complete");
```

### Aurora City
```csharp
// When floating city appears (3-minute window)
DialogueManager.Instance?.PlayContextDialogue("moon9_aurora_city");

// Milo's jaw-drop moment
DialogueManager.Instance?.PlayContextDialogue("moon9_milo_aurora_city");
```

### Revelation
```csharp
// 17th hour timestamp mystery
DialogueManager.Instance?.PlayContextDialogue("moon9_mystery_deepens");
```

---

## Moon 10: Planetary Moon (Rail Network)

### Discovery Phase
```csharp
// Rails reactivate
DialogueManager.Instance?.PlayContextDialogue("moon10_rails_hum");
```

### Restoration Phase
```csharp
// Orphan children tune rail network
DialogueManager.Instance?.PlayContextDialogue("moon10_orphans_success");

// Full continental journey
DialogueManager.Instance?.PlayContextDialogue("continental_train_journey");
```

### Conflict Phase
```csharp
// Trigger Room discovered
DialogueManager.Instance?.PlayContextDialogue("trigger_room_discovery");

// Examining control console
DialogueManager.Instance?.PlayContextDialogue("trigger_room_analysis");

// Rail Leviathan boss
DialogueManager.Instance?.PlayContextDialogue("moon10_leviathan_spawn");
DialogueManager.Instance?.PlayContextDialogue("moon10_leviathan_phase2");
DialogueManager.Instance?.PlayContextDialogue("moon10_leviathan_defeated");
```

### Revelation
```csharp
// Three operators mystery
DialogueManager.Instance?.PlayContextDialogue("moon10_revelation");
```

---

## Common Patterns

### Story Beat Contexts
```csharp
// Available for all moons
DialogueContext.Discovery
DialogueContext.TuningStart
DialogueContext.TuningSuccess
DialogueContext.TuningFail
DialogueContext.Restoration
DialogueContext.CombatStart
DialogueContext.CombatVictory
DialogueContext.ExplorationIdle
DialogueContext.AetherWake
DialogueContext.ZoneShift
DialogueContext.ZoneComplete
DialogueContext.CorruptionDetected
DialogueContext.CorruptionPurged
```

### Korath Echo Voice (Moon 8-13)
```csharp
// Korath can be called anywhere after Moon 7 sacrifice
DialogueManager.Instance?.PlayLineById("korath_echo_01");
```

### Character-Specific Idle
```csharp
// Milo
DialogueManager.Instance?.PlayContextDialogue("milo_chat");

// Thorne
DialogueManager.Instance?.PlayContextDialogue("thorne_strategy");

// Cassian (if redeemed)
DialogueManager.Instance?.PlayContextDialogue("cassian_intel");
```

---

## Audio Cue Integration

### Moon 7 Audio
```csharp
AudioManager.Instance?.PlaySFX3D("Moon7_KorathVoice", stasisVaultCenter);
AudioManager.Instance?.PlaySFX3D("Korath_Awakening", stasisVaultCenter);
AudioManager.Instance?.PlaySFX3D("Korath_Sacrifice", starFortClusterCenter);
AudioManager.Instance?.PlaySFX2D("Moon7_GolemSiege");
AudioManager.Instance?.PlaySFX3D("Cassian_Confrontation", cassianPosition);
```

### Moon 8 Audio
```csharp
AudioManager.Instance?.PlaySFX3D("Thorne_Landing", whiteCityDock);
AudioManager.Instance?.PlaySFX3D("Moon8_AirshipRepair", airshipPosition);
AudioManager.Instance?.PlaySFX2D("Moon8_AerialCombat");
AudioManager.Instance?.PlaySFX3D("Moon8_NightFlight", airshipPosition);
```

### Moon 9 Audio
```csharp
AudioManager.Instance?.PlaySFX3D("Moon9_StoneCollect", stonePosition);
AudioManager.Instance?.PlaySFX3D("Moon9_ProphecyVision", stonePosition);
AudioManager.Instance?.PlaySFX3D("Zereth_Distorted", visionCenter);
AudioManager.Instance?.PlaySFX3D("Moon9_AuroraCity", auroraCityCenter);
```

### Moon 10 Audio
```csharp
AudioManager.Instance?.PlaySFX3D("RailNetworkHum", centralStationPoint, 0.3f);
AudioManager.Instance?.PlaySFX3D("Moon10_TrainJourney", trainPosition);
AudioManager.Instance?.PlaySFX3D("TriggerRoomAmbience", triggerRoomCenter);
AudioManager.Instance?.PlaySFX3D("RailLeviathan_Roar", leviathanPosition);
```

---

## SaveData Flags

### Moon 7 Flags
```csharp
// Save Cassian choice
SaveManager.Instance?.SetMoonFlag(7, "cassianRedeemed", redeemed ? 1 : 0);

// 9-band unlock (global flag)
SaveManager.Instance?.SetGlobalFlag("9BandUnlocked", true);

// Korath sacrifice complete
SaveManager.Instance?.SetMoonFlag(7, "korathSacrificed", 1);
```

### Moon 9 Flags
```csharp
// Prophecy stones collected
SaveManager.Instance?.SetMoonFlag(9, "stonesCollected", stoneCount);

// Aurora city witnessed
SaveManager.Instance?.SetMoonFlag(9, "auroraCityWitnessed", 1);

// Zereth confession accessed
SaveManager.Instance?.SetMoonFlag(9, "zerethConfession", 1);
```

### Moon 10 Flags
```csharp
// Rail network complete
SaveManager.Instance?.SetMoonFlag(10, "railNetworkComplete", true);

// Trigger room discovered
SaveManager.Instance?.SetMoonFlag(10, "triggerRoomDiscovered", 1);

// Leviathan defeated
SaveManager.Instance?.SetMoonFlag(10, "railLeviathanDefeated", 1);
```

---

## Debugging

### Test Specific Dialogue
```csharp
// From Unity console:
DialogueManager.Instance?.PlayLineById("korath_wake_01");
DialogueManager.Instance?.PlayLineById("thorne_night_01");
DialogueManager.Instance?.PlayLineById("zereth_contact_01");
DialogueManager.Instance?.PlayLineById("moon10_leviathan_defeat_01");
```

### Check Dialogue Database
```csharp
// Total lines in context
var lines = DialogueManager.Instance?._contextLines["moon7_korath_awakening"];
Debug.Log($"Korath awakening lines: {lines?.Count}");

// Verify line exists
var line = DialogueManager.Instance?._lineById["korath_wake_01"];
Debug.Log($"Line text: {line?.text}");
```

---

## Performance Notes

- **Dialogue Load Time:** All 114 lines loaded in BuildDatabase() (~5ms overhead)
- **Memory Impact:** ~15KB string data (minimal)
- **PlayContextDialogue Calls:** O(1) lookup via dictionary
- **Audio Streaming:** All SFX use streaming (not preloaded)

---

## Common Issues & Solutions

### Issue: Dialogue Not Playing
```csharp
// Check if DialogueManager exists
if (DialogueManager.Instance == null)
    Debug.LogError("DialogueManager not in scene!");

// Check if context exists
if (!DialogueManager.Instance._contextLines.ContainsKey("moon7_korath_awakening"))
    Debug.LogError("Context key not found in database!");
```

### Issue: Audio Not Playing
```csharp
// Check AudioManager
if (AudioManager.Instance == null)
    Debug.LogError("AudioManager not in scene!");

// Check ProceduralSFXLibrary
var clip = ProceduralSFXLibrary.Get("Korath_Awakening");
if (clip == null)
    Debug.LogWarning("Audio clip not found, using placeholder");
```

### Issue: Cassian Fork Not Working
```csharp
// Verify Moon 2 trust was saved
int cassianTrust = SaveManager.Instance?.GetMoonFlag(2, "cassianTrusted", -1) ?? -1;
Debug.Log($"Cassian trust value: {cassianTrust}");
// Should be 1 (trusted) or 0 (doubted), not -1 (not set)
```

---

**End of Quick Reference**  
For full dialogue text and story context, see: `AGENT21_DIALOGUE_POLISH_MOON7_10_REPORT.md`
