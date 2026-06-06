# TARTARIA — AudioMixer + URP Volumes Setup Guide

## AudioMixer Asset Creation (Unity GUI required)

### Create AudioMixer
1. Project window → Right-click → Create → Audio Mixer
2. Name: `MasterAudioMixer`
3. Location: `Assets/_Project/Audio/`

### Exposed Parameters
Configure these groups + exposed parameters for AudioMixerController.cs to control:

**Group Structure:**
```
Master
├── Music
├── SFX
└── Voice
```

**Exposed Parameters** (right-click param → "Expose to script"):
- `MasterVolume` → Master attenuation
- `MusicVolume` → Music attenuation
- `SFXVolume` → SFX attenuation
- `VoiceVolume` → Voice attenuation

**Default Values:**
- Master: 0 dB
- Music: -10 dB
- SFX: -5 dB
- Voice: 0 dB

### Wire to AudioManager
In AudioManager.cs (lines 50-80), set mixer reference:
```csharp
[SerializeField] AudioMixerGroup masterGroup;
[SerializeField] AudioMixerGroup musicGroup;
[SerializeField] AudioMixerGroup sfxGroup;
[SerializeField] AudioMixerGroup voiceGroup;
```

### Apply to AudioSources
All AudioManager-spawned AudioSources should route through mixer:
```csharp
audioSource.outputAudioMixerGroup = sfxGroup;  // or musicGroup, voiceGroup
```

---

## URP Post-Processing Volumes (Unity GUI required)

### Global Volume
1. Hierarchy → Right-click → Volume → Global Volume
2. Name: `GlobalPostProcessing`
3. Profile: Create new → `GlobalPPProfile`
4. Add Overrides:
   - Bloom (intensity 0.3, threshold 1.0)
   - Tonemapping (Mode: ACES)
   - Color Adjustments (post-exposure +0.2)
   - Vignette (intensity 0.25, smoothness 0.4)

### Moon-Specific Volumes (Scene-local)

#### Moon 2: Crystalline Caverns
- **Profile**: `Moon2_DissonancePP`
- **Weight**: 1.0
- **Priority**: 10
- **Overrides**:
  - Color Adjustments: Saturation -20%, Hue Shift +5° (cyan tint)
  - Chromatic Aberration: Intensity 0.4 (dissonance distortion)
  - Bloom: Intensity 0.5, Scatter 0.7 (crystal glow)
  - Vignette: Intensity 0.4 (oppressive caverns)

#### Moon 4: Deep Forge
- **Profile**: `Moon4_ForgePP`
- **Overrides**:
  - Color Grading: Temperature +15 (warm forge heat)
  - Bloom: Intensity 0.7, Threshold 0.8 (lava glow)
  - Depth of Field: Aperture f/5.6, Focus Distance 10m (giant scale)

#### Moon 10: Planetary Rail
- **Profile**: `Moon10_RailPP`
- **Overrides**:
  - Motion Blur: Intensity 0.3 (speed sensation)
  - Color Adjustments: Contrast +10%
  - Lens Distortion: Intensity -0.15 (wide-angle rail view)

#### Moon 13: Echo Realm
- **Profile**: `Moon13_EchoPP`
- **Overrides**:
  - Color Adjustments: Saturation +30%, Post-Exposure +0.5 (ethereal brightness)
  - Bloom: Intensity 0.8, Scatter 0.9 (divine light)
  - White Balance: Temperature +10, Tint +5 (golden hour)
  - Depth of Field: Gaussian, Aperture f/2.8 (dreamlike)

### Trigger Volumes
Use Box Collider + Volume component:
```csharp
[RequireComponent(typeof(BoxCollider))]
public class VolumeActivationTrigger : MonoBehaviour
{
    [SerializeField] UnityEngine.Rendering.Volume targetVolume;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetVolume.weight = 1f;  // Blend in
        }
    }
}
```

---

## Integration Checklist

- [ ] MasterAudioMixer created with exposed params
- [ ] AudioMixerController.cs wired to exposed params
- [ ] AudioManager routes sources through mixer groups
- [ ] GlobalPostProcessing volume created
- [ ] Moon2-13 scene-specific volumes created (8 total)
- [ ] Volume activation triggers placed at Moon entrances
- [ ] Player camera has "Post Processing" enabled
- [ ] URP Asset has "Post Processing" feature enabled

---

## Testing Commands

**Audio Mixer:**
```csharp
AudioMixerController.Instance?.SetMasterVolume(0.8f);  // 80%
AudioMixerController.Instance?.SetMusicVolume(0.5f);   // 50%
```

**Post-Processing:**
```csharp
// Runtime volume weight adjustment
var volume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
volume.weight = 0.5f;  // 50% blend
```

**Debug Commands** (Tartaria Console):
- `/audio mixer master 0.8` → Set master volume
- `/post moon2` → Activate Moon 2 PP profile
- `/post global` → Reset to global profile

---

_Last Updated: 2026-05-22 | Vex_
