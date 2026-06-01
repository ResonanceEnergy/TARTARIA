# 🎨 VFX PREFAB WIRING REFERENCE
## Quick lookup for which VFX to use where

---

## 🔥 HOVL STUDIO MAGIC EFFECTS (50+ prefabs)

### **Combat VFX:**

**Resonance Pulse (Player primary attack):**
- `Hovl Studio/Magic effects pack/Prefabs/Fireball/Fireball01.prefab`
- Blue/cyan variant, projectile

**Enemy Attacks:**
- `Hovl Studio/Magic effects pack/Prefabs/Impact/Impact01.prefab`
- `Hovl Studio/Magic effects pack/Prefabs/Magic/MagicMissile01.prefab`

**Hit Effects:**
- `Hovl Studio/Magic effects pack/Prefabs/Hit/Hit01.prefab`
- `Hovl Studio/Magic effects pack/Prefabs/Explosion/Explosion01.prefab`

### **Collectible VFX:**

**Aether Shard Collection:**
- `Hovl Studio/Magic effects pack/Prefabs/Buff/Buff_Aura.prefab`
- Cyan/blue glow

**Lore Artifact Pickup:**
- `Hovl Studio/Magic effects pack/Prefabs/Sparkle/Sparkle01.prefab`
- Golden sparkles

**Power-Up Glow:**
- `Hovl Studio/Magic effects pack/Prefabs/Buff/BuffWave.prefab`
- Continuous aura

### **Building Restoration VFX:**

**Cathedral Repair:**
- `Hovl Studio/Magic effects pack/Prefabs/Heal/HealingAura.prefab`
- Green/golden healing glow

**Stone Reassembly:**
- `Hovl Studio/Magic effects pack/Prefabs/Magic/MagicCircle.prefab`
- Ritual circle effect

### **Tuning Node VFX:**

**Activation Pulse:**
- `Hovl Studio/Magic effects pack/Prefabs/Magic/MagicCircle02.prefab`
- Purple resonance wave

**Active State Glow:**
- `Hovl Studio/Magic effects pack/Prefabs/Buff/Buff_Aura02.prefab`
- Continuous purple glow

### **Boss Abilities:**

**Phase Transition:**
- `Hovl Studio/Magic effects pack/Prefabs/Explosion/ExplosionLarge.prefab`

**Ultimate Attack:**
- `Hovl Studio/Magic effects pack/Prefabs/Lightning/Lightning01.prefab`

---

## 💥 UNITY PARTICLE EFFECTS (30+ prefabs)

### **Environment VFX:**

**Dust/Ambient:**
- `EffectExamples/Prefabs/Dust/FloatingDust.prefab`
- Ambient particles for Moon 1

**Fire (Torches):**
- `EffectExamples/Fire and Explosions Prefabs/Fire.prefab`
- Torch flames, campfires

**Smoke:**
- `EffectExamples/Fire and Explosions Prefabs/Smoke.prefab`
- Chimney smoke, fog

### **Combat VFX:**

**Blood Spray:**
- `EffectExamples/Blood FX Prefabs/BloodSpray.prefab`
- Enemy hit effect

**Explosion:**
- `EffectExamples/Fire and Explosions Prefabs/Explosion.prefab`
- Large impacts

### **Weather VFX:**

**Rain (Moon 3 - Wind Moon):**
- `EffectExamples/Water FX Prefabs/Rain.prefab`

**Water Splash:**
- `EffectExamples/Water FX Prefabs/WaterSplash.prefab`

---

## 🎯 WIRING INSTRUCTIONS (Manual Process)

### **Step-by-Step:**

1. **Open Unity Editor**

2. **Open Moon System Component:**
   - Navigate to `Assets/_Project/Scripts/Integration/`
   - Open scene or find GameObject with component
   - Select in Hierarchy

3. **Find SerializedField in Inspector:**
   - Look for public fields like:
     - `collectionVFX`
     - `activationVFX`
     - `restorationVFX`
     - `ambientParticles`
     - etc.

4. **Drag VFX Prefab to Field:**
   - Navigate to VFX prefab in Project window
   - Drag into Inspector field
   - Release

5. **Test:**
   - Press Play
   - Trigger the action (collect shard, activate node, etc.)
   - Verify VFX plays

6. **Repeat for All Systems:**
   - Moon1EnemySpawners
   - Moon1Collectibles
   - Moon1InteractiveObjects
   - Moon1WeatherSystem
   - Moon1AmbientParticles
   - etc.

---

## 📋 CHECKLIST: MOON 1 VFX WIRING

### **Moon1Collectibles.cs:**
- [ ] `aetherShardPrefab` → Add Buff_Aura.prefab as child
- [ ] `loreArtifactPrefab` → Add Sparkle01.prefab as child
- [ ] `collectionVFX` → Buff_Aura.prefab (plays on collection)

### **Moon1InteractiveObjects.cs:**
- [ ] `tuningNodeActivationVFX` → MagicCircle02.prefab
- [ ] `tuningNodeActiveGlow` → Buff_Aura02.prefab (continuous)
- [ ] `resonanceDoorOpen` → MagicCircle.prefab

### **Moon1EnemySpawners.cs:**
- [ ] `mudGolemSpawnVFX` → Smoke.prefab
- [ ] `mudGolemDeathVFX` → Explosion.prefab

### **Moon1WeatherSystem.cs:**
- [ ] `fogEffect` → Smoke.prefab (continuous, low to ground)

### **Moon1AmbientParticles.cs:**
- [ ] `dustParticles` → FloatingDust.prefab

### **Player Combat (separate component):**
- [ ] `resonancePulseVFX` → Fireball01.prefab
- [ ] `playerHitVFX` → BloodSpray.prefab

---

## ⏱️ TIME ESTIMATE

- **Moon 1 VFX wiring:** 2-3 hours (13 systems × ~10 min each)
- **All 13 Moons VFX wiring:** 8-12 hours

---

## 💡 TIPS

1. **Preview VFX before wiring:**
   - Drag prefab into scene
   - Press Play
   - See if it looks right
   - Delete if wrong

2. **Duplicate prefabs for color variants:**
   - Copy Buff_Aura.prefab
   - Rename to Buff_Aura_Red.prefab
   - Change Particle System color in Inspector
   - Use for different collectible types

3. **Adjust scale/duration:**
   - Select VFX prefab
   - In Particle System Inspector:
     - Start Size: scale effect
     - Duration: how long it plays
     - Loop: continuous vs one-shot

4. **Layer VFX:**
   - Combine multiple VFX on same object
   - Example: Tuning Node = MagicCircle + Buff_Aura + Sparkle

---

**This is manual work. Put on music. It takes 2-3 hours. Just drag and drop. 🎨**
