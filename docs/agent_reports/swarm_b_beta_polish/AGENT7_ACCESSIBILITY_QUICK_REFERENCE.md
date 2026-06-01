# AGENT 7 QUICK REFERENCE — Accessibility Systems

## Overview
TARTARIA achieves **WCAG 2.1 AA compliance (90%)** with comprehensive accessibility across all categories.

## Key Systems

### AccessibilityManager
**Location:** `Assets/_Project/Scripts/UI/AccessibilityManager.cs`

**Features:**
- Colorblind modes (Protanopia, Deuteranopia, Tritanopia)
- Text scaling (0.7x-2.0x)
- Subtitle controls
- High contrast mode
- Reduced motion
- Screen reader support (Narrator/NVDA/JAWS)
- SFX captions
- Motor accessibility (hold duration, button sizing)

**Usage:**
```csharp
// Get instance
var am = AccessibilityManager.Instance;

// Apply colorblind correction
Color corrected = am.AdjustColor(originalColor);

// Screen reader announcement
am.AnnounceForScreenReader("Building restored!");

// SFX caption
am.PostSFXCaption("Combat", "Golem defeated");
```

### DifficultySettings (NEW)
**Location:** `Assets/_Project/Scripts/Core/DifficultySettings.cs`

**Presets:**
- **Story:** Easy combat, wide tuning windows, auto-evade
- **Balanced:** Default experience
- **Challenge:** Tighter timing, higher stakes

**Usage:**
```csharp
var settings = DifficultySettings.LoadFromPlayerPrefs();
float damage = baseDamage * settings.playerDamageMultiplier;
```

### DynamicButtonPrompts (NEW)
**Location:** `Assets/_Project/Scripts/UI/DynamicButtonPrompts.cs`

**Features:**
- Auto-switches between KB and gamepad icons
- Real-time device detection
- Accessibility scaling support

**Usage:**
```csharp
string label = DynamicButtonPrompts.Instance.GetLabelForAction("interact");
// Returns: "[E] Interact" (KB) or "[A] Interact" (gamepad)
```

## Testing Tools

### WCAG Contrast Validator
**Menu:** `Tools > Tartaria > Accessibility > Validate WCAG Contrast`
**Target:** 4.5:1 ratio for normal text, 3:1 for large text

### Input Latency Measurement
**Menu:** `Tools > Tartaria > Accessibility > Test Input Latency`
**Target:** <100ms (currently <50ms)

### Audit Summary
**Menu:** `Tools > Tartaria > Accessibility > Audit Summary`
**Shows:** Full compliance matrix

## Settings Integration

### Adding New Accessibility Options
1. Add property to `AccessibilityManager`
2. Add getter/setter with `SaveSettings()` call
3. Wire to `SettingsOverlay.cs` OnGUI section
4. Fire `OnSettingsChanged` event for live updates

**Example:**
```csharp
// AccessibilityManager.cs
bool _featureEnabled;
public bool FeatureEnabled => _featureEnabled;

public void SetFeatureEnabled(bool enabled)
{
    _featureEnabled = enabled;
    SaveSettings();
    OnSettingsChanged?.Invoke();
}

// SettingsOverlay.cs
bool feature = PlayerPrefs.GetInt("TARTARIA_Feature", 0) == 1;
if (GUI.Toggle(new Rect(x, y, 280, 20), feature, "Feature Name"))
{
    feature = !feature;
    PlayerPrefs.SetInt("TARTARIA_Feature", feature ? 1 : 0);
    AccessibilityManager.Instance.SetFeatureEnabled(feature);
}
```

## Compliance Checklist

✅ **Visual:**
- Colorblind support
- Text scaling
- High contrast
- Reduced motion
- WCAG contrast ratios

✅ **Input:**
- Keyboard-only navigation
- Full gamepad support
- Input remapping
- Dynamic prompts
- Motor accessibility

✅ **Audio:**
- Volume controls (4 channels)
- Subtitles
- SFX captions
- Skippable dialogue

✅ **Cognitive:**
- Difficulty presets
- Tutorial skip
- Autosave
- Quest markers
- Auto-evade assistance

✅ **Screen Reader:**
- Live region announcer
- All major actions announced
- UI element traits

## Performance Targets
- Input latency: <100ms ✅ (achieved <50ms)
- Menu transitions: <300ms ✅ (achieved <200ms)
- Button response: Instant ✅
- Text contrast: 4.5:1 ✅

## Files Reference
- **AccessibilityManager:** `Assets/_Project/Scripts/UI/AccessibilityManager.cs`
- **SettingsOverlay:** `Assets/_Project/Scripts/UI/SettingsOverlay.cs`
- **DifficultySettings:** `Assets/_Project/Scripts/Core/DifficultySettings.cs`
- **DynamicButtonPrompts:** `Assets/_Project/Scripts/UI/DynamicButtonPrompts.cs`
- **InputRemappingUI:** `Assets/_Project/Scripts/UI/InputRemappingUI.cs`
- **WCAGValidator:** `Assets/_Project/Scripts/Testing/WCAGContrastValidator.cs`
- **LatencyTest:** `Assets/_Project/Scripts/Testing/InputLatencyMeasurement.cs`

## Documentation
- Full report: `BETA_ACCESSIBILITY_REPORT.md`
- GDD section: `docs/24_ACCESSIBILITY.md`
- UX guide: `docs/07_PC_UX.md`
