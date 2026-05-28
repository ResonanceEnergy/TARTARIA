# SHADER GRAPH QUICK REFERENCE
## Copy-paste node configurations for Unity Shader Graph Editor

---

## 🪨 STONE_TARTARIAN SHADER GRAPH

### Properties (Blackboard)
```
BaseColor_Texture    : Texture2D    (Albedo map)
NormalMap_Texture    : Texture2D    (Bump detail)
Roughness            : Float        (0.7 default, range 0-1)
EmissionColor        : Color (HDR)  (#FFD700 default)
EmissionIntensity    : Float        (1.5 default, range 0-5)
EmissionMask_Texture : Texture2D    (Geometric pattern)
```

### Node Chain
```
UV → BaseColor_Texture (Texture Sample)
     └→ Multiply (EmissionColor property)
        └→ Fragment.BaseColor

UV → NormalMap_Texture (Normal From Texture)
     └→ Normal Strength (1.0)
        └→ Fragment.Normal

Roughness (property)
     └→ One Minus
        └→ Fragment.Smoothness

EmissionColor × EmissionIntensity × EmissionMask_Texture (Texture Sample Alpha)
     └→ Fragment.Emission

Connect all to: Fragment (Lit Master Node)
```

---

## 🔩 METAL_ORNATE SHADER GRAPH

### Properties (Blackboard)
```
BaseColor_Texture : Texture2D    (Bronze/Gold texture)
NormalMap_Texture : Texture2D    (Engraving detail)
Metallic          : Float        (0.9 default, range 0-1)
Roughness         : Float        (0.3 default, range 0-1)
FresnelColor      : Color (HDR)  (#FFAA44 default)
FresnelPower      : Float        (3.0 default, range 1-5)
```

### Node Chain
```
UV → BaseColor_Texture (Texture Sample)
     └→ Fragment.BaseColor

UV → NormalMap_Texture (Normal From Texture)
     └→ Fragment.Normal

Metallic (property)
     └→ Fragment.Metallic

Roughness (property)
     └→ One Minus
        └→ Fragment.Smoothness

Fresnel Effect (View Direction + Normal)
     └→ Power (FresnelPower)
        └→ Multiply (FresnelColor)
           └→ Fragment.Emission
```

---

## 💎 CRYSTAL_AETHER SHADER GRAPH

### Properties (Blackboard)
```
BaseColor        : Color         (#66B3FF default, with alpha 0.4)
EmissionColor    : Color (HDR)   (#FFBF00 default)
PulseSpeed       : Float         (0.5 default, range 0-2)
FresnelPower     : Float         (2.5 default, range 1-5)
Transparency     : Float         (0.4 default, range 0-1)
```

### Node Chain
```
BaseColor (property)
     └→ Fragment.BaseColor

Time → Sine Wave
     └→ Multiply (PulseSpeed)
        └→ Remap (0 to 1 → 0.5 to 1.0)
           └→ Multiply (EmissionColor × 2.0)
              └→ Add (Fresnel rim glow)
                 └→ Fragment.Emission

Fresnel Effect
     └→ Power (FresnelPower)
        └→ Multiply (EmissionColor × 0.5)
           └→ Add to main Emission

Transparency (property)
     └→ Fragment.Alpha

Surface Type: Transparent
Blend Mode: Alpha
Rendering Queue: Transparent
```

---

## 📐 NODE PLACEMENT TIPS

1. **Texture Samples**: Top-left corner (UV inputs)
2. **Properties**: Left side (Blackboard variables)
3. **Math Operations**: Middle area (Multiply, Add, Power)
4. **Special Effects**: Right-middle (Fresnel, Time, Sine)
5. **Fragment Node**: Far right (final output)

## 🎨 MOON 1 COLOR PALETTE

```csharp
// Copy these hex codes into Unity Color Picker (HDR mode for emission)
Amber Stone Base:     #FFA726 (RGB: 255, 167, 38)
Golden Emission:      #FFD700 (RGB: 255, 215, 0, Intensity: 1.5)
Bronze Metal:         #CD7F32 (RGB: 205, 127, 50)
Warm Metal Glow:      #FFAA44 (RGB: 255, 170, 68, Intensity: 0.5)
Amber Crystal Base:   #FFBF00 (RGB: 255, 191, 0, Alpha: 0.4)
Amber Crystal Glow:   #FFBF00 (RGB: 255, 191, 0, Intensity: 2.0)
```

## ⚡ QUICK CREATE WORKFLOW

1. **Assets → Create → Shader Graph → URP → Lit Shader Graph**
2. **Name:** Stone_Tartarian.shadergraph
3. **Open Shader Graph window** (double-click)
4. **Add Properties** (Blackboard panel, right side)
5. **Drag Properties** onto graph canvas (creates nodes)
6. **Connect nodes** following chain above
7. **Connect to Fragment node** (Master output, right side)
8. **Save** (Ctrl+S)
9. **Create Material:** Assets → Create → Material
10. **Assign Shader:** Shader dropdown → Shader Graphs → Stone_Tartarian
11. **Configure Properties** in Material Inspector
12. **Save material** with Moon1 suffix

## 🔧 TROUBLESHOOTING

**Emission not glowing?**
→ Enable HDR on Color properties (checkbox next to color picker)
→ Set Intensity > 1.0

**Transparency not working?**
→ Fragment Node: Surface Type = Transparent
→ Blend Mode = Alpha
→ Rendering Queue = Transparent

**Fresnel rim too bright?**
→ Lower FresnelPower (try 2.0 instead of 3.0)
→ Reduce FresnelColor intensity

**Texture not showing?**
→ Check UV node connected to Texture Sample
→ Verify texture imported correctly (Assets folder)
→ Check material has textures assigned

---

**Time to create all 3:** ~30 minutes with this reference ⚡