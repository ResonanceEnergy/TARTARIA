# 🎯 MOON 1 UNITY SCENE WIRING — Quick Reference

## Open Scene
**File:** `Assets/Scenes/Echohaven_VerticalSlice.unity`

---

## 14 Systems to Wire (In Order)

### 1. **Moon1EnemySpawners**
```
mudGolemPrefab: [Assign from KayKit or project prefabs]
spawnPoints: [Create empty GameObjects as spawn markers, drag to array]
maxActiveGolems: 5
spawnInterval: 45
```

### 2. **Moon1Collectibles**
```
aetherShardPrefab: [Create glowing crystal prefab]
loreArtifactPrefab: [Create book/scroll prefab]
shardPositions: [Optional - leave empty for procedural]
totalShards: 15
totalArtifacts: 5
```

### 3. **Moon1InteractiveObjects**
```
tuningNodePrefab: [Create tall crystalline node prefab]
resonanceDoors: [Array of door GameObjects in scene]
mechanicalLevers: [Array of lever GameObjects]
totalTuningNodes: 8
```

### 4. **Moon1WeatherSystem**
```
rainPrefab: [Unity Particle System - rain]
auroraEffectPrefab: [Particle System - cyan glow]
enableFog: true
enableRain: true
fogColor: (128, 128, 153) RGB
```

### 5. **Moon1AmbientAudio**
```
cathedralAmbience: [AudioClip - reverb ambience]
distantBells: [AudioClip - 432 Hz bell tones]
mechanicalHum: [AudioClip - clockwork]
resonanceHum: [AudioClip - aether drone]
explorationTheme: [AudioClip - background music]
combatTheme: [AudioClip - action music]
```

### 6. **Moon1AmbientParticles**
```
dustMotePrefab: [Particle System - small dust]
fireflyPrefab: [GameObject with Light component]
fogWispPrefab: [Particle System - ground fog]
aetherSparklePrefab: [Particle System - cyan sparkles]
maxDustMotes: 30
maxFireflies: 20
```

### 7. **Moon1AudioZones**
```
audioZones: [Configure array in Inspector]
  - Zone 0: Cathedral Interior
    - zoneName: "Cathedral"
    - center: (0, 0, 0)
    - radius: 20
    - ambientClip: [Reverb audio]
  - Zone 1: Courtyard
    - zoneName: "Courtyard"
    - center: (25, 0, 0)
    - radius: 15
    - ambientClip: [Outdoor wind]
  (Add more zones as needed)
```

### 8. **Moon1VisualLandmarks**
```
bellTowerPrefab: [Tall tower structure]
stainedGlassWindowPrefab: [Glass window mesh]
ancientStatuePrefab: [Stone statue]
resonanceObeliskPrefab: [Glowing pillar]
cathedralDomePrefab: [Dome structure]
bellTowerPosition: (0, 25, 40)
```

### 9. **Moon1NPCDialogues**
```
milo: [Drag Milo GameObject from scene hierarchy]
dialogueCooldown: 30
proximityTriggerDistance: 5
```

### 10. **Moon1QuestNodes**
```
(No prefabs needed - auto-initializes)
```

### 11. **Moon1Secrets**
```
secretMarkerPrefab: [Glowing marker with particle system]
discoveryRadius: 3
```

### 12. **Moon1PowerUps**
```
rsBoostPrefab: [Glowing cyan crystal]
combatBoostPrefab: [Glowing red crystal]
healingOrbPrefab: [Glowing green orb]
rsBoostCount: 8
combatBoostCount: 5
healingOrbCount: 10
```

### 13. **Moon1DynamicHazards**
```
mudPoolPrefab: [Plane with mud texture + trigger collider]
fallingDebrisPrefab: [Rock/debris mesh with Rigidbody]
dissonanceZonePrefab: [Particle system + trigger sphere]
collapsingFloorPrefab: [Floor mesh with collider]
```

### 14. **Moon1EnvironmentDecorator**
```
propPrefabs: [Array of furniture, barrels, crates, debris]
vegetationPrefabs: [Array of vines, moss, dead plants]
architecturalPrefabs: [Array of columns, arches, ornaments]
candlePrefabs: [Array of candle models]
propsCount: 40
vegetationCount: 30
architecturalCount: 20
candlesCount: 25
```

---

## Post-Wiring Checklist

### **NavMesh Setup:**
1. Window → AI → Navigation
2. Select all walkable surfaces
3. Mark as "Walkable"
4. Click "Bake"
5. Verify NavMesh covers floor areas

### **Lighting:**
1. Window → Rendering → Lighting
2. Generate Lighting (if not auto-bake)
3. Verify Adaptive Probe Volumes (APV) working

### **Testing:**
1. Press Play in Unity Editor
2. Verify player spawns correctly
3. Test movement (WASD)
4. Test combat (first Mud Golem encounter)
5. Test collection (pick up Aether Shard)
6. Test interaction (tune Tuning Node with E key)
7. Verify Milo dialogue triggers
8. Check performance (60 FPS target)

### **Build Test:**
1. File → Build Settings
2. Add Scene: Echohaven_VerticalSlice
3. Build and Run
4. Full 30-min playthrough test

---

## Quick Prefab Creation Tips

**If prefabs don't exist, create simple placeholders:**

- **Aether Shard:** Sphere (scale 0.3) + glowing cyan material + Light component
- **Tuning Node:** Cylinder (scale 1, 4, 1) + glowing material + capsule collider
- **Mud Golem:** Use KayKit character or capsule placeholder
- **Milo:** Use KayKit character with "Milo" tag
- **Candle:** Small cylinder + Point Light (orange color)
- **Props:** Use KayKit Forest Nature Pack assets
- **Particles:** Unity default particle systems with adjusted colors

---

## Save & Test Workflow

1. Wire 2-3 systems at a time
2. Press Play → Test those systems
3. Fix any issues
4. Continue to next systems
5. Full test after all 14 systems wired

**Priority Order:**
- Tier 1 first (combat, collection, interaction)
- Test gameplay loop
- Add atmospheric systems (weather, audio, particles)
- Polish with decoration

---

**You're ready to bring Moon 1 to life in Unity! 🎮**
