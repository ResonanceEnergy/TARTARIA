# 🎯 TARTARIA PREFAB CREATION & SCENE WIRING GUIDE
## Professional Production Workflow — ALL 13 MOONS

---

## PHILOSOPHY

**KayKit = Temporary Fallback Only**
- Use KayKit assets ONLY when proper prefabs don't exist yet
- Replace with custom art assets as soon as available
- This guide provides EXPLICIT specifications for every prefab

**One Guide, All Moons**
- Same 23-system architecture across all 13 Moons
- Prefab specs adapt per Moon's biome/theme
- Wire once per Moon, but understand the template

---

## PHASE 1: PREFAB CREATION WORKFLOW

### Step 1: Create Prefab Root Folders

```
Assets/_Project/Prefabs/
├── Collectibles/
│   ├── AetherShards/
│   └── LoreArtifacts/
├── Enemies/
│   ├── Moon1_MudGolems/
│   ├── Moon2_DissonanceDefenders/
│   └── (etc per Moon)
├── Interactive/
│   ├── TuningNodes/
│   ├── Doors/
│   └── Levers/
├── Environment/
│   ├── Props/
│   ├── Vegetation/
│   ├── Architecture/
│   └── Hazards/
├── VFX/
│   ├── Particles/
│   ├── Weather/
│   └── Ambient/
├── Audio/
│   └── Zones/
└── Landmarks/
```

---

## PHASE 2: EXPLICIT PREFAB SPECIFICATIONS

### 🔷 COLLECTIBLES

#### **Aether Shard Prefab**
**Path:** `Assets/_Project/Prefabs/Collectibles/AetherShards/AetherShard_Base.prefab`

**Components:**
1. **GameObject:** "AetherShard"
   - Tag: "Collectible"
   - Layer: "Interactable"

2. **Transform:**
   - Scale: (0.3, 0.3, 0.3)

3. **MeshFilter:**
   - Mesh: Unity Primitive Sphere OR custom crystal mesh

4. **MeshRenderer:**
   - Material: Create "M_AetherShard"
     - Shader: URP/Lit
     - Base Color: Cyan (0, 0.8, 1, 1)
     - Emission: Enabled, Cyan (0, 1.5, 2, 1)
     - Smoothness: 0.9
     - Metallic: 0.1

5. **Light Component:**
   - Type: Point
   - Color: Cyan (0, 0.8, 1)
   - Range: 5
   - Intensity: 2
   - Shadow Type: None (performance)

6. **SphereCollider:**
   - Radius: 0.5 (larger than visual for easy collection)
   - Is Trigger: ✓

7. **Rigidbody:**
   - Use Gravity: ✗
   - Is Kinematic: ✓

**Animation (Optional):**
- Add Animator with float/rotate loop
- OR use LeanTween in script (already implemented)

**Audio Source:**
- Clip: 432Hz chime (create simple sine wave in Audacity)
- Play On Awake: ✗
- Spatial Blend: 1.0 (3D)
- Volume: 0.5

---

#### **Lore Artifact Prefab**
**Path:** `Assets/_Project/Prefabs/Collectibles/LoreArtifacts/LoreArtifact_Base.prefab`

**Components:**
1. **GameObject:** "LoreArtifact"
   - Tag: "Collectible"
   - Layer: "Interactable"

2. **Transform:**
   - Scale: (0.4, 0.05, 0.3) — book shape

3. **MeshFilter:**
   - Mesh: Unity Cube OR custom book mesh

4. **MeshRenderer:**
   - Material: Create "M_LoreBook"
     - Base Color: Gold (0.8, 0.6, 0.2, 1)
     - Emission: Gold (1, 0.8, 0.3, 1) intensity 0.5
     - Normal Map: Optional parchment texture

5. **BoxCollider:**
   - Size: (1, 1, 1)
   - Is Trigger: ✓

6. **Light Component:**
   - Color: Gold (1, 0.8, 0.3)
   - Range: 4
   - Intensity: 1.5

---

### ⚔️ ENEMIES

#### **Moon 1: Mud Golem Prefab**
**Path:** `Assets/_Project/Prefabs/Enemies/Moon1_MudGolems/MudGolem.prefab`

**Temporary (KayKit Fallback):**
- Use: `Assets/KayKit_Skeletons_1.1_FREE/.../Skeleton_Minion.glb`
- Apply material tint: Brown (0.3, 0.2, 0.1)

**Proper Production Asset:**
1. **GameObject:** "MudGolem"
   - Tag: "Enemy"
   - Layer: "Enemy"

2. **Transform:**
   - Scale: (1.2, 1.5, 1.2) — bulky humanoid

3. **MeshFilter + Renderer:**
   - Mesh: Humanoid low-poly golem mesh
   - Material: "M_MudGolem"
     - Base Color: Dark brown (0.25, 0.18, 0.12)
     - Roughness: 0.9 (muddy, not reflective)
     - Normal Map: Cracked mud texture

4. **CapsuleCollider:**
   - Height: 2
   - Radius: 0.5
   - Center: (0, 1, 0)

5. **Rigidbody:**
   - Mass: 80
   - Drag: 1
   - Angular Drag: 5
   - Constraints: Freeze Rotation X, Z

6. **NavMeshAgent:**
   - Speed: 2.5
   - Angular Speed: 120
   - Acceleration: 8
   - Stopping Distance: 1.5
   - Auto Braking: ✓

7. **Animator:**
   - Controller: Create "AC_MudGolem"
   - States: Idle, Walk, Attack, Death
   - Transitions: Speed parameter

8. **Scripts:**
   - MudGolemAI.cs (already exists in codebase)
   - EnemyHealth.cs
   - DamageDealer.cs

**Audio Sources (3 required):**
- Footsteps (looping)
- Attack Grunt (one-shot)
- Death Groan (one-shot)

---

### 🎛️ INTERACTIVE OBJECTS

#### **Tuning Node Prefab**
**Path:** `Assets/_Project/Prefabs/Interactive/TuningNodes/TuningNode_Base.prefab`

**Components:**
1. **GameObject:** "TuningNode"
   - Tag: "Interactive"
   - Layer: "Interactable"

2. **Transform:**
   - Scale: (1, 4, 1) — tall crystalline pillar

3. **MeshFilter:**
   - Mesh: Unity Cylinder OR custom pillar mesh

4. **MeshRenderer:**
   - Material: "M_TuningNode"
     - Base Color: Dark gray (0.2, 0.2, 0.25, 1)
     - Emission: OFF by default, Cyan when activated
     - Metallic: 0.3
     - Smoothness: 0.7

5. **CapsuleCollider:**
   - Height: 4
   - Radius: 0.6
   - Is Trigger: ✓

6. **Light Component (Child Object):**
   - Name: "NodeGlow"
   - Position: (0, 3.5, 0) — top of pillar
   - Type: Point
   - Color: Cyan
   - Range: 8
   - Intensity: 0 (increases to 5 when tuned)
   - Render Mode: Important

7. **Particle System (Child Object):**
   - Name: "TuneParticles"
   - Position: (0, 3.5, 0)
   - Shape: Sphere, radius 0.5
   - Emission: 20/sec
   - Start Speed: 2
   - Start Size: 0.1
   - Start Color: Cyan
   - Start Lifetime: 2
   - Play On Awake: ✗ (triggers on tune)

**Audio Source:**
   - Clip: 432Hz sustained tone (Audacity sine wave)
   - Play On Awake: ✗
   - Loop: ✓ (when tuned)
   - Volume: 0.6
   - Spatial Blend: 1.0

---

#### **Resonance Door Prefab**
**Path:** `Assets/_Project/Prefabs/Interactive/Doors/ResonanceDoor.prefab`

**Components:**
1. **GameObject:** "ResonanceDoor"
   - Tag: "Door"
   - Layer: "Environment"

2. **Transform:**
   - Scale: (3, 4, 0.3) — standard door

3. **MeshFilter + Renderer:**
   - Mesh: Unity Cube OR custom door mesh
   - Material: "M_ResonanceDoor"
     - Base Color: Stone gray (0.4, 0.4, 0.45)
     - Emission: Locked = Red (1, 0, 0) / Unlocked = Cyan (0, 1, 1)

4. **BoxCollider:**
   - Size: (1, 1, 1)
   - Is Trigger: ✗ (blocks player when locked)

5. **Script:**
   - ResonanceDoor.cs component (add in Moon1InteractiveObjects)

**Child Objects:**
   - Lock Indicator (small sphere with glow)
   - Sound effect trigger point

---

### 🌦️ WEATHER & ATMOSPHERE

#### **Rain Particle System Prefab**
**Path:** `Assets/_Project/Prefabs/VFX/Weather/Rain_System.prefab`

**Components:**
1. **GameObject:** "RainSystem"

2. **ParticleSystem:**
   - **Main Module:**
     - Duration: 5 (looping)
     - Start Lifetime: 1-2 (randomize)
     - Start Speed: 15-20
     - Start Size: 0.05
     - Start Color: Light blue-gray (0.7, 0.8, 0.9, 0.4)
     - Gravity Modifier: 1
     - Max Particles: 2000

   - **Emission:**
     - Rate over Time: 400

   - **Shape:**
     - Shape: Box
     - Box Size: (50, 0.1, 50) — covers area around player

   - **Renderer:**
     - Render Mode: Stretched Billboard
     - Speed Scale: 0.2
     - Length Scale: 3

**Audio Source:**
   - Clip: Rain ambience loop
   - Loop: ✓
   - Volume: 0.3
   - Spatial Blend: 0 (2D global)

---

#### **Aurora Effect Prefab**
**Path:** `Assets/_Project/Prefabs/VFX/Weather/Aurora_Sky.prefab`

**Components:**
1. **GameObject:** "AuroraEffect"
   - Layer: "Effects"

2. **Particle System:**
   - **Main Module:**
     - Duration: 10 (looping)
     - Start Lifetime: 8-12
     - Start Speed: 0.5
     - Start Size: 20-40
     - Start Color: Gradient (Cyan → Green → Purple)
     - Start Rotation: Random 0-360
     - Gravity Modifier: 0
     - Simulation Space: World

   - **Emission:**
     - Rate over Time: 3

   - **Shape:**
     - Shape: Sphere
     - Radius: 100
     - Position: (0, 50, 0) — sky height

   - **Color over Lifetime:**
     - Gradient: Fade in/out with shimmer

   - **Renderer:**
     - Material: Create "M_Aurora"
       - Shader: Particles/Standard Unlit
       - Rendering Mode: Additive
       - Base Texture: Soft cloud texture

3. **Light Component:**
   - Type: Directional
   - Color: Cyan-green (0.2, 0.8, 0.6)
   - Intensity: 0.3 (subtle ambient wash)
   - Culling Mask: Everything

---

### 🎨 ENVIRONMENT DECORATION

#### **Candle Prefab (Flickering Light)**
**Path:** `Assets/_Project/Prefabs/Environment/Props/Candle_Flickering.prefab`

**Temporary (KayKit Fallback):**
- Use any small cylinder/candle from RPG Tools pack

**Proper Production Asset:**
1. **GameObject:** "Candle"

2. **Transform:**
   - Scale: (0.1, 0.3, 0.1)

3. **MeshFilter + Renderer:**
   - Mesh: Cylinder OR custom candle mesh
   - Material: "M_Candle"
     - Base Color: Off-white wax (0.9, 0.85, 0.7)

4. **Child: Flame (GameObject)**
   - Position: (0, 0.35, 0)
   - Scale: (0.5, 0.5, 0.5)

   **ParticleSystem:**
   - Start Color: Orange-yellow gradient
   - Start Size: 0.2
   - Emission: 20/sec
   - Shape: Cone, small angle

   **Light Component:**
   - Type: Point
   - Color: Orange (1, 0.7, 0.4)
   - Range: 6
   - Intensity: 0.8 (varies with FlickeringLight script)

5. **Script:**
   - FlickeringLight.cs (already implemented in Moon1EnvironmentDecorator)

---

### 💎 POWER-UPS

#### **RS Boost Prefab**
**Path:** `Assets/_Project/Prefabs/Collectibles/PowerUps/RSBoost.prefab`

**Components:**
1. **GameObject:** "RSBoost"
   - Tag: "PowerUp"
   - Layer: "Interactable"

2. **Transform:**
   - Scale: (0.5, 0.5, 0.5)

3. **MeshFilter + Renderer:**
   - Mesh: Unity Cube OR crystal shard
   - Material: "M_RSBoost"
     - Base Color: Cyan (0, 0.8, 1)
     - Emission: Cyan (0, 2, 3)
     - Transparency: Slight fade (alpha 0.9)

4. **BoxCollider:**
   - Size: (1, 1, 1)
   - Is Trigger: ✓

5. **Light:**
   - Color: Cyan
   - Range: 5
   - Intensity: 3

6. **Particle Ring (Child):**
   - Orbit particles around pickup
   - Color: Cyan
   - Small sparkles

**Repeat for:**
- Combat Boost → Red color scheme
- Healing Orb → Green color scheme

---

### ⚠️ HAZARDS

#### **Mud Pool Prefab**
**Path:** `Assets/_Project/Prefabs/Environment/Hazards/MudPool.prefab`

**Components:**
1. **GameObject:** "MudPool"
   - Tag: "Hazard"
   - Layer: "Hazard"

2. **Transform:**
   - Scale: (3, 0.1, 3) — flat pool

3. **MeshFilter + Renderer:**
   - Mesh: Unity Plane
   - Material: "M_Mud"
     - Base Color: Dark brown (0.2, 0.15, 0.1)
     - Roughness: 1.0
     - Normal Map: Muddy texture
     - Emission: Slight dark red tint (damage indicator)

4. **BoxCollider:**
   - Size: (1, 0.5, 1)
   - Is Trigger: ✓

5. **Particle System (Child):**
   - Name: "MudBubbles"
   - Emission: 5/sec
   - Shape: Circle on surface
   - Particles: Small brown bubbles

6. **Audio Source:**
   - Clip: Bubbling/squelching loop
   - Loop: ✓
   - Volume: 0.2
   - Spatial Blend: 1.0

---

## PHASE 3: MOON-SPECIFIC ADAPTATIONS

### Moon 1 (Echohaven) — Cathedral Theme
**Color Palette:** Cyan, gray stone, warm candlelight
**Environment:** Gothic architecture, aged stone, sacred geometry
**Specific Prefabs:**
- Bell Tower (tall spire with bells)
- Stained Glass Windows (colored light sources)
- Ancient Statues (stone, weathered)
- Cathedral Dome (large structure piece)

### Moon 2 (Crystalline Caverns) — Crystal Theme
**Color Palette:** Purple, blue crystals, bioluminescent
**Environment:** Cave formations, crystal clusters
**Specific Prefabs:**
- DissonanceCrystal (12 large crystals for puzzles)
- CrystalCluster (environment decoration)
- Bioluminescent Fungi
- Crystal Defender Enemy (crystalline humanoid)

### Moon 3 (Windswept Highlands) — Wind/Rail Theme
**Color Palette:** Earth tones, oxidized metal, grass
**Environment:** Cliffs, railway, ruins
**Specific Prefabs:**
- Orphan Train (central narrative object)
- Rail Tracks (repeating segments)
- Wind Vanes (spinning indicators)
- Cliff Ruins (stone structures)

### Moons 4-13: (Same Prefab Creation Logic)
- Identify biome theme from docs
- Adapt base prefab specifications
- Maintain same component structure
- Adjust materials/colors per theme

---

## PHASE 4: UNITY SCENE WIRING

### Prerequisites Checklist:
- ✅ All prefabs created in Phase 2
- ✅ Materials created and assigned
- ✅ Audio clips imported (or generated via Audacity)
- ✅ Particle systems configured
- ✅ NavMesh baked for level geometry

---

### Wiring Process (Moon 1 Example)

#### Open Scene:
`Assets/Scenes/Echohaven_VerticalSlice.unity`

---

#### 1. Moon1EnemySpawners Component

**Find in Hierarchy:** "Moon1Systems" → "Moon1EnemySpawners"

**Inspector Assignments:**
```
mudGolemPrefab: [Drag] Assets/_Project/Prefabs/Enemies/Moon1_MudGolems/MudGolem.prefab
spawnPoints: [Create empty GameObjects, position them, add to array]
  - SpawnPoint_North (0, 0, 30)
  - SpawnPoint_East (30, 0, 0)
  - SpawnPoint_South (0, 0, -30)
  - SpawnPoint_West (-30, 0, 0)
maxActiveGolems: 5
spawnInterval: 45
spawnRadius: 50
```

**Validation:**
- Press Play
- Wait 5 seconds
- Verify Mud Golem spawns at one of the spawn points
- Check console for "[Moon1EnemySpawners] Spawned Mud Golem" log

---

#### 2. Moon1Collectibles Component

**Inspector Assignments:**
```
aetherShardPrefab: [Drag] Assets/_Project/Prefabs/Collectibles/AetherShards/AetherShard_Base.prefab
loreArtifactPrefab: [Drag] Assets/_Project/Prefabs/Collectibles/LoreArtifacts/LoreArtifact_Base.prefab
shardPositions: [Leave empty - uses procedural placement]
totalShards: 15
totalArtifacts: 5
autoCollectRadius: 2.5
```

**Validation:**
- Press Play
- Look for glowing cyan shards scattered around level
- Walk near one (within 2.5m)
- Verify auto-collection + "+2 RS" message
- Check GameStateManager RS counter increases

---

#### 3. Moon1InteractiveObjects Component

**Inspector Assignments:**
```
tuningNodePrefab: [Drag] Assets/_Project/Prefabs/Interactive/TuningNodes/TuningNode_Base.prefab
resonanceDoors: [Create door GameObjects in scene, add to array]
  - Cathedral_MainDoor
  - Courtyard_Gate
  - SecretChamber_Door
mechanicalLevers: [Create lever GameObjects, add to array]
  - Lever_BellTower
  - Lever_Fountain
totalTuningNodes: 8
```

**Validation:**
- Press Play
- Find a Tuning Node (tall cyan-emitting pillar)
- Approach and press E key
- Verify node glows brighter + plays 432Hz tone
- Check progress: "Tuning Nodes: 1/8"

---

#### (Continue for all 14 systems...)

---

## PHASE 5: TESTING & VALIDATION

### System-by-System Tests:

**Combat:**
- [ ] Mud Golems spawn correctly
- [ ] NavMesh pathfinding works
- [ ] Combat damage applies
- [ ] Enemy death triggers correctly

**Collection:**
- [ ] Shards auto-collect within radius
- [ ] RS reward applies (+2 per shard)
- [ ] Lore artifacts unlock lore entries
- [ ] Save/load persistence works

**Interaction:**
- [ ] Tuning Nodes respond to E key
- [ ] Resonance doors unlock at threshold
- [ ] Progress tracking updates correctly

**Atmosphere:**
- [ ] Weather systems trigger at milestones
- [ ] Audio zones crossfade smoothly
- [ ] Particles don't tank framerate
- [ ] Lighting looks good day/night

**Performance:**
- [ ] 60 FPS maintained
- [ ] No memory leaks
- [ ] LOD systems active
- [ ] NavMesh doesn't stutter

---

## PHASE 6: EXPORT TEMPLATE FOR MOONS 2-13

### Create Prefab Variant Workflow:

1. **Duplicate Moon 1 prefabs**
2. **Rename for target Moon**
3. **Adjust materials/colors per biome**
4. **Update component references in Moon systems**
5. **Test in target scene**
6. **Repeat for all 13 Moons**

### Time Estimate:
- Moon 1 (first time): 8-12 hours (prefab creation + wiring + testing)
- Moons 2-13 (using template): 2-3 hours each (mostly material swaps + testing)

---

## ✅ DELIVERABLE CHECKLIST

**Prefabs:**
- [ ] All collectibles created
- [ ] All enemies created per Moon
- [ ] All interactive objects created
- [ ] All VFX systems created
- [ ] All environment props created

**Materials:**
- [ ] M_AetherShard
- [ ] M_LoreBook
- [ ] M_MudGolem
- [ ] M_TuningNode
- [ ] M_ResonanceDoor
- [ ] (etc for all systems)

**Audio:**
- [ ] 432Hz tone clips (Tuning Nodes, Shards)
- [ ] Rain ambience loop
- [ ] Combat sounds (enemy grunts, footsteps)
- [ ] Ambient soundscapes per zone

**Scene Setup:**
- [ ] All 14 systems wired in Moon 1 scene
- [ ] NavMesh baked
- [ ] Lighting configured
- [ ] Performance validated (60 FPS)

---

**THIS IS THE PROFESSIONAL STANDARD.**

**No "assign from KayKit or whatever" — EXPLICIT specifications, CONCRETE workflows, TESTABLE outcomes.**

**Ready for Unity. Ready for production. Ready for ALL 13 MOONS.** 🎯
