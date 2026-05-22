# TARTARIA — Particle Prefabs for ParticleEffectPool

## Overview
ParticleEffectPool.cs manages pooled VFX instances for zero-GC combat/interaction feedback. This document defines the 12 core prefabs needed for Moon2-13 content.

---

## Core Hit/Impact VFX

### 1. **HitSpark_Generic**
**Path**: `Assets/_Project/VFX/Prefabs/HitSpark_Generic.prefab`
**Use**: Generic melee/projectile impact on surfaces
**Particle Settings**:
- Duration: 0.3s, Looping: No
- Start Color: Orange-yellow gradient (1, 0.6, 0.1) → (1, 0.3, 0)
- Start Size: 0.2-0.4 random
- Start Lifetime: 0.2-0.4s
- Emission: Burst 15-25 particles
- Shape: Sphere, Radius 0.1
- Velocity: 2-5 random, Radial
- Color over Lifetime: Alpha fade 1→0
- Size over Lifetime: 0.5→0 curve

### 2. **CrystalShatter_Purple**
**Path**: `Assets/_Project/VFX/Prefabs/CrystalShatter_Purple.prefab`
**Use**: Dissonance crystal destruction (Moon 2)
**Particle Settings**:
- Duration: 0.8s, Looping: No
- Start Color: Purple-black gradient (0.3, 0.1, 0.5) → (0.05, 0.05, 0.05)
- Start Size: 0.3-0.8 random (large crystal shards)
- Emission: Burst 40-60 particles
- Shape: Sphere, Radius 0.5
- Velocity: 3-8 random, Radial + gravity
- Rotation over Lifetime: 180-360° random
- Render Mode: Mesh (cube or shard mesh)

### 3. **MudSplatter_Brown**
**Path**: `Assets/_Project/VFX/Prefabs/MudSplatter_Brown.prefab`
**Use**: Mud Golem damage (Moon 3-4)
**Particle Settings**:
- Duration: 0.5s, Looping: No
- Start Color: Brown-gray (0.25, 0.2, 0.15) → (0.15, 0.12, 0.1)
- Start Size: 0.1-0.3 random
- Emission: Burst 20-30 particles
- Velocity: 1-4 random, Directional (impact normal)
- Gravity Modifier: 0.5 (mud weight)
- Collision: World, Bounce 0.3, Lifetime Loss 0.5

---

## Aura/Buff VFX

### 4. **ResonanceAura_Cyan**
**Path**: `Assets/_Project/VFX/Prefabs/ResonanceAura_Cyan.prefab`
**Use**: Player RS charge/buff indicator
**Particle Settings**:
- Duration: Looping
- Start Color: Cyan gradient (0.4, 0.9, 1.0, 0.5) → (0.6, 1.0, 1.0, 0.3)
- Start Size: 0.5-1.0 random
- Start Lifetime: 1.5-2.5s
- Emission: Rate 10 particles/sec
- Shape: Sphere, Radius 1.5 (around player)
- Velocity: 0.5-1.5 upward
- Color over Lifetime: Alpha pulse 0.3→0.6→0
- Render Mode: Additive

### 5. **TuningHarmonic_Gold**
**Path**: `Assets/_Project/VFX/Prefabs/TuningHarmonic_Gold.prefab`
**Use**: Successful building tuning (Moons 1-13)
**Particle Settings**:
- Duration: 2s, Looping: No
- Start Color: Gold (1, 0.85, 0.2, 0.8)
- Start Size: 1.0-2.0 random
- Emission: Burst 50 particles
- Shape: Sphere, Radius 2.0
- Velocity: 2-5 upward + outward
- Texture Sheet Animation: 4x4 sparkle atlas, Cycles 1
- Lights: Point light, Range 5, Intensity 2, Cyan

### 6. **LullabyWave_Blue**
**Path**: `Assets/_Project/VFX/Prefabs/LullabyWave_Blue.prefab`
**Use**: Lullaby pacification (Moon 3)
**Particle Settings**:
- Duration: 3s, Looping: No
- Start Color: Soft blue (0.6, 0.7, 1.0, 0.4)
- Start Size: 0.8-1.5 random
- Emission: Rate 20/sec for 3s
- Shape: Donut, Radius 3.0, Donut Radius 0.5
- Velocity: 1-2 radial outward
- Size over Lifetime: 0.5→2.0 expand
- Noise: Strength 0.5, Frequency 0.3 (wavy)

---

## Explosion/Area VFX

### 7. **FountainPurge_Cyan**
**Path**: `Assets/_Project/VFX/Prefabs/FountainPurge_Cyan.prefab`
**Use**: Ionized fountain restoration (Moon 2 climax)
**Particle Settings**:
- Duration: 5s, Looping: No
- Start Color: Cyan-white (0.6, 0.9, 1.0) → (1, 1, 1)
- Start Size: 2-5 random (large particles)
- Emission: Burst 500 particles at t=0
- Shape: Cone, Angle 45°, Radius 3.0 (fountain geyser)
- Velocity: 10-20 upward + radial
- Gravity Modifier: -0.5 (upward lift)
- Lights: Point light, Range 15, Intensity 5

### 8. **GolemSmash_Dust**
**Path**: `Assets/_Project/VFX/Prefabs/GolemSmash_Dust.prefab`
**Use**: Guardian Golem ground pound (Moon 4)
**Particle Settings**:
- Duration: 2s, Looping: No
- Start Color: Gray-brown dust (0.5, 0.45, 0.4, 0.6)
- Start Size: 3-8 random (huge dust cloud)
- Emission: Burst 100 particles
- Shape: Ring, Radius 5.0
- Velocity: 5-10 radial outward
- Size over Lifetime: 1→3 expand
- Force over Lifetime: Drag 0.5 (slow dust settle)

---

## Restoration/Divine VFX

### 9. **BellTowerChime_Gold**
**Path**: `Assets/_Project/VFX/Prefabs/BellTowerChime_Gold.prefab`
**Use**: Bell tower activation (Moons 1, 2, 12)
**Particle Settings**:
- Duration: 4s, Looping: No
- Start Color: Gold (1, 0.9, 0.3, 0.7)
- Start Size: 1-3 random
- Emission: 5 bursts over 4s, 30 particles each
- Shape: Sphere, Radius 10.0 (tower-scale)
- Velocity: 3-8 radial outward
- Texture Sheet Animation: Ring ripple atlas

### 10. **AetherCollect_Sparkle**
**Path**: `Assets/_Project/VFX/Prefabs/AetherCollect_Sparkle.prefab`
**Use**: Aether crystal/mote pickup
**Particle Settings**:
- Duration: 0.5s, Looping: No
- Start Color: White-cyan (1, 1, 1) → (0.5, 0.9, 1)
- Start Size: 0.2-0.5 random
- Emission: Burst 20 particles
- Shape: Sphere, Radius 0.5
- Velocity: Toward player (attraction), Speed 5
- Sub Emitters: Trail of smaller sparkles

---

## Special/Giant Mode VFX

### 11. **GiantMode_GlowRing**
**Path**: `Assets/_Project/VFX/Prefabs/GiantMode_GlowRing.prefab`
**Use**: Giant mode activation indicator
**Particle Settings**:
- Duration: Looping
- Start Color: Gold (1, 0.8, 0.2, 0.5)
- Start Size: 5-8 (large human-scale ring)
- Emission: Rate 15/sec
- Shape: Circle, Radius 3.0, Emit from Edge
- Velocity: 0 (static ring)
- Rotation over Lifetime: 45°/sec
- Render Mode: Additive

### 12. **PortalSwirl_Blue**
**Path**: `Assets/_Project/VFX/Prefabs/PortalSwirl_Blue.prefab`
**Use**: Echo realm portals (Moon 13)
**Particle Settings**:
- Duration: Looping
- Start Color: Blue-purple (0.4, 0.5, 1.0, 0.6)
- Start Size: 1-3 random
- Emission: Rate 30/sec
- Shape: Donut, Radius 2.0, Donut Radius 0.3
- Velocity: 2-4 tangential (swirl)
- Noise: Strength 1.0, Scroll Speed 0.5
- Render Mode: Additive

---

## ParticleEffectPool Integration

Wire prefabs into ParticleEffectPool.cs (lines 25-45):
```csharp
[Header("VFX Prefabs")]
[SerializeField] GameObject hitSparkPrefab;
[SerializeField] GameObject crystalShatterPrefab;
[SerializeField] GameObject mudSplatterPrefab;
[SerializeField] GameObject resonanceAuraPrefab;
// ... etc for all 12

void Awake()
{
    RegisterPrefab("hit_generic", hitSparkPrefab, 10);  // Pool size 10
    RegisterPrefab("crystal_shatter", crystalShatterPrefab, 5);
    RegisterPrefab("mud_splatter", mudSplatterPrefab, 8);
    // ... etc
}
```

---

## Usage Examples

**Combat Hit:**
```csharp
ParticleEffectPool.Instance?.Play("hit_generic", hitPoint, hitNormal);
```

**Moon 2 Crystal Purge:**
```csharp
ParticleEffectPool.Instance?.Play("crystal_shatter", crystal.transform.position, Vector3.up);
AudioManager.Instance?.PlaySFX("CrystalShatter", crystal.transform.position);
```

**Giant Mode Activation:**
```csharp
var aura = ParticleEffectPool.Instance?.Play("giant_glow_ring", player.position, Vector3.up);
// Keep reference, stop when giant mode ends
```

---

## Testing Commands

**Spawn VFX** (Tartaria Console):
- `/vfx hit_generic` → Spawn at player
- `/vfx crystal_shatter 10 5 0` → Spawn at (10,5,0)
- `/vfx list` → List all registered prefabs

**Pool Stats:**
- `/pool stats` → Show active/pooled counts
- `/pool clear` → Return all to pool

---

_Last Updated: 2026-05-22 | Vex_
