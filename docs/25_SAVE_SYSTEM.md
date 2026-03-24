# TARTARIA WORLD OF WONDER — Save System Architecture
## Persistence, Cloud Sync, Offline-First Design & Data Integrity

---

> *"A civilization's memory is its greatest treasure. Every dome restored, every choice made, every friendship forged — preserved in the Aether forever."*

**Cross-References:**
- [09_TECHNICAL_SPEC.md §9](09_TECHNICAL_SPEC.md) — Save data structure, auto-save policy, save slots
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — Session interruption recovery, 10-second resume
- [22_DIALOGUE_BRANCHING.md](22_DIALOGUE_BRANCHING.md) — Choice state variables (WorldChoice, CompanionLoyalty, HarmonyProfile)
- [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) — Resource types and balances
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Skill tree and progression state
- [appendices/E_METRICS.md](appendices/E_METRICS.md) — Save-related analytics

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Save Data Schema](#2-save-data-schema)
3. [Auto-Save & Checkpoint System](#3-auto-save--checkpoint-system)
4. [Local Persistence Layer](#4-local-persistence-layer)
5. [Cloud Sync Architecture](#5-cloud-sync-architecture)
6. [Offline-First Design](#6-offline-first-design)
7. [Conflict Resolution](#7-conflict-resolution)
8. [Data Migration & Versioning](#8-data-migration--versioning)
9. [Integrity & Anti-Fraud](#9-integrity--anti-fraud)
10. [Storage Budget](#10-storage-budget)
11. [Disaster Recovery](#11-disaster-recovery)
12. [ECS Integration](#12-ecs-integration)

---

## 1. Design Philosophy

### Core Principles

1. **Never lose player progress.** A phone call, a crash, a dead battery — the player resumes exactly where they left off. Always.
2. **Offline-first, cloud-enhanced.** The game must function identically with no internet. Cloud is for sync, backup, and cross-device — not for operation.
3. **Invisible persistence.** Players should never think about saving. It just works.
4. **Tamper-resistant, not tamper-proof.** Protect against casual cheating and data corruption. Don't wage war on advanced modders — they're not our threat.
5. **Forward-compatible.** Save files from v1.0 must load in v5.0. Schema evolution, never schema destruction.

### Session Lifecycle

```
App Launch
│
├── Load local save (fastest path — <200ms)
├── UI immediately responsive
├── Background: check cloud for newer save
│   ├── Cloud newer → conflict resolution (see §7)
│   └── Local is latest → continue
│
├── GAMEPLAY (auto-save running)
│   ├── Every 10 seconds: dirty flag check
│   ├── Trigger events: zone transition, quest complete, build placed
│   ├── App backgrounding: emergency serialize (must complete in <2s)
│   └── Low battery (<10%): immediate full save + cloud push
│
└── App Terminate / Background
    ├── Final save written
    ├── Cloud push queued (NSURLSession background task)
    └── State: fully recoverable
```

---

## 2. Save Data Schema

### 2.1 Schema Overview

Expanding on the structure defined in [09_TECHNICAL_SPEC.md §9.1](09_TECHNICAL_SPEC.md):

```json
{
  "header": {
    "schema_version": 1,
    "game_version": "1.0.0",
    "platform": "ios",
    "device_model": "iPhone17,1",
    "save_slot": 0,
    "created_utc": "2027-01-15T14:32:00Z",
    "modified_utc": "2027-03-22T09:15:30Z",
    "play_time_seconds": 86400,
    "checksum": "sha256:abc123..."
  },
  "player": { },
  "world": { },
  "economy": { },
  "campaign": { },
  "meta": { }
}
```

### 2.2 Player Block

```json
{
  "player": {
    "position": { "zone": "echohaven", "x": 123.4, "y": 0.0, "z": 567.8 },
    "level": 12,
    "skill_tree": "0b1101001010...101",
    "equipped_cosmetics": {
      "headpiece": 0,
      "robe": 14,
      "aura": 3,
      "tool_skin": 0,
      "companion_outfit_milo": 2,
      "companion_outfit_lirael": 0,
      "companion_outfit_korath": 0,
      "companion_outfit_thorne": 0
    },
    "inventory": [
      { "item_id": "aether_shard", "count": 1250 },
      { "item_id": "blueprint_dome_med", "count": 3 },
      { "item_id": "harmonic_frequency_528", "count": 1 }
    ],
    "companions": [
      {
        "id": "milo",
        "affinity": 4,
        "arc_stage": "moon_7_post_betrayal",
        "loyalty": 3,
        "equipped_outfit": 2
      }
    ],
    "harmony_profile": {
      "builder": 0.72,
      "fighter": 0.45,
      "explorer": 0.88,
      "diplomat": 0.61
    }
  }
}
```

### 2.3 World Block

```json
{
  "world": {
    "buildings": [
      {
        "uid": "echohaven_dome_01",
        "archetype": "dome_medium",
        "restoration_pct": 1.0,
        "resonance_score": 87.3,
        "golden_ratio_match": 0.96,
        "upgrade_tier": 3,
        "placed_utc": "2027-02-01T10:00:00Z"
      }
    ],
    "aether_nodes": [
      {
        "uid": "node_ech_042",
        "intensity": 0.75,
        "band": "etheric",
        "last_harvest_utc": "2027-03-22T09:10:00Z"
      }
    ],
    "discovered_lore": "0b11011010...0110",
    "zone_states": [
      {
        "zone_id": "echohaven",
        "corruption_pct": 0.12,
        "unlocked": true,
        "fog_of_war": "0b111100...0011"
      }
    ],
    "npc_dialogue_states": {
      "milo": { "last_branch": "moon5_trust_fork_b" },
      "lirael": { "last_branch": "moon3_crystal_reveal" }
    }
  }
}
```

### 2.4 Campaign Block

```json
{
  "campaign": {
    "current_moon": 5,
    "moon_progress": [
      { "moon": 1, "phase": "completed", "day": 28, "flags": 7 },
      { "moon": 2, "phase": "completed", "day": 28, "flags": 15 },
      { "moon": 5, "phase": "active", "day": 14, "flags": 3 }
    ],
    "world_choices": {
      "W1_cassian_dilemma": "spare",
      "W2_star_fort_allegiance": null,
      "W3_koraths_sacrifice": null
    },
    "companion_loyalty": {
      "milo": 3,
      "lirael": 2,
      "korath": -1,
      "thorne": 0,
      "anastasia": 0
    },
    "quest_outcomes": 2147483647,
    "anastasia_flags": "0x00000000000000000000000000000003",
    "ending_seeds": {
      "harmony_weight": 0.72,
      "echo_weight": 0.45,
      "reset_weight": 0.15
    }
  }
}
```

### 2.5 Economy Block

```json
{
  "economy": {
    "primary_resources": {
      "aether_essence": 12500,
      "building_materials": 840,
      "star_points": 320,
      "resonance_shards": 55
    },
    "secondary_resources": {
      "harmonic_fragments": 120,
      "echo_memories": 45,
      "crystal_dust": 200,
      "frequency_tokens": 18
    },
    "premium": {
      "resonance_coins": 450
    },
    "owned_cosmetics": [0, 3, 14, 22, 45],
    "battle_pass": {
      "season": 1,
      "tier": 22,
      "xp": 4500,
      "is_premium": true,
      "claimed_rewards": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22]
    }
  }
}
```

### 2.6 Meta Block

```json
{
  "meta": {
    "settings_hash": "sha256:def456...",
    "total_aether_harvested": 95000.0,
    "buildings_restored": 42,
    "play_session_count": 87,
    "achievements_unlocked": "0b110101...1001",
    "tutorial_flags": 255,
    "last_daily_login_utc": "2027-03-22T00:00:00Z",
    "analytics_consent": true
  }
}
```

---

## 3. Auto-Save & Checkpoint System

### 3.1 Save Triggers

Expanding on [09_TECHNICAL_SPEC.md §9.2](09_TECHNICAL_SPEC.md):

| Trigger | Save Type | Scope | Priority |
|---|---|---|---|
| **App backgrounding** | Emergency full | All blocks | **Critical** (must complete <2s) |
| **Low battery (<10%)** | Emergency full + cloud push | All blocks | **Critical** |
| **Zone transition** | Full save | All blocks | High |
| **Quest complete** | Full save | All blocks | High |
| **World-Shaping choice** | Full save + cloud push | All blocks | High |
| **Building placed** | Incremental | World block only | Medium |
| **Combat encounter end** | Incremental | Player + World | Medium |
| **Every 10 seconds** | Dirty-flag check → incremental if dirty | Changed blocks only | Low |
| **Manual save** | Full save + cloud push | All blocks | High |
| **Pre-boss checkpoint** | Full save (separate checkpoint slot) | All blocks | High |

### 3.2 Dirty Flag Architecture

```
SaveStateSystem (ECS, LateSimulation Group)
│
├── Per-block dirty flags:
│   ├── isDirty_Player (position, inventory, skills)
│   ├── isDirty_World (buildings, aether nodes, zones)
│   ├── isDirty_Campaign (choices, moon progress)
│   ├── isDirty_Economy (resources, purchases)
│   └── isDirty_Meta (settings, analytics)
│
├── 10-second tick:
│   ├── Check all dirty flags
│   ├── If any dirty → serialize only dirty blocks
│   ├── Write to local serialized bytes buffer
│   ├── Write to disk (background thread via NativeArray)
│   └── Clear dirty flags
│
└── Emergency save (app backgrounding):
    ├── Serialize ALL blocks (ignore dirty flags)
    ├── Synchronous disk write (must complete)
    └── Queue cloud upload via NSURLSession background task
```

### 3.3 Save Slot Management

| Slot | Purpose | Overwrite Policy |
|---|---|---|
| **Auto-Save** | Rolling auto-save (latest state) | Always overwritten |
| **Checkpoint** | Pre-boss, pre-choice safety net | Overwritten at next checkpoint event |
| **Manual 1–3** | Player-controlled saves | Only overwritten by explicit player action |

**Total: 5 local save files** (1 auto + 1 checkpoint + 3 manual)

---

## 4. Local Persistence Layer

### 4.1 Storage Location

| Platform | Path | Backup |
|---|---|---|
| **iOS** | `Application Support/saves/` | iCloud backup included |
| **iOS (settings)** | `UserDefaults` (non-save settings only) | iCloud key-value store |

### 4.2 Serialization Format

| Stage | Format | Reason |
|---|---|---|
| **In-memory** | ECS component data (native structs) | Zero-copy gameplay access |
| **Serialization** | MessagePack binary | 40% smaller than JSON, faster parse |
| **Compression** | LZ4 block compression | Fast decompress (<5ms for 500KB) |
| **Encryption** | AES-256-GCM | Device-bound key from Keychain |
| **Disk** | Single file per save slot | Atomic write via temp + rename |

### 4.3 Write Safety

```
Atomic Save Write:
1. Serialize to byte buffer (background thread)
2. Compress with LZ4
3. Encrypt with AES-256-GCM (key from iOS Keychain)
4. Write to temp file: saves/slot_0.tmp
5. fsync() temp file
6. Rename temp → saves/slot_0.sav (atomic on APFS)
7. Delete temp if rename succeeded
```

**Why atomic?** If the app crashes mid-write, the old save is still intact. The temp file is discarded on next launch.

### 4.4 Storage Footprint

| Component | Size (uncompressed) | Size (compressed) |
|---|---|---|
| Header | 200 bytes | ~150 bytes |
| Player block | 5 KB | 2 KB |
| World block (Moon 13, 100% explored) | 300 KB | 120 KB |
| Campaign block | 8 KB | 3 KB |
| Economy block | 2 KB | 1 KB |
| Meta block | 1 KB | 500 bytes |
| **Total per save slot** | **~316 KB** | **~127 KB** |
| **5 slots total** | **~1.6 MB** | **~635 KB** |

---

## 5. Cloud Sync Architecture

### 5.1 Dual Backend Strategy

```
Local Save (MessagePack/LZ4/AES)
│
├── Primary Cloud: Firebase Firestore
│   ├── Document: users/{uid}/saves/{slot_id}
│   ├── Fields: header metadata (small, for quick query)
│   ├── Subcollection: save_blocks/{block_name}
│   ├── Binary payload: Cloud Storage gs://tartaria-saves/{uid}/{slot}.bin
│   └── Purpose: cross-device sync, server validation, analytics
│
└── Secondary Cloud: iCloud Key-Value Store
    ├── Key: "save_slot_{N}_hash"
    ├── Value: SHA-256 of latest save
    ├── Purpose: fast "is cloud newer?" check without full download
    └── Fallback: iCloud Documents for full save if Firebase unreachable
```

### 5.2 Sync Flow

```
On Save:
1. Write local save (§4)
2. If online:
   a. Upload save binary to Cloud Storage
   b. Update Firestore metadata document
   c. Update iCloud key-value hash
3. If offline:
   a. Queue upload in pending_sync table (SQLite)
   b. iOS Background Task will retry when connectivity returns

On Launch:
1. Load local save immediately (show game)
2. Background: fetch Firestore metadata
3. Compare modified_utc:
   a. Local newer → push to cloud
   b. Cloud newer → download + conflict resolution (§7)
   c. Equal → no action
4. If Firebase unreachable → check iCloud hash
   a. Hash mismatch → download from iCloud Documents
   b. Hash match → local is current
```

### 5.3 Bandwidth Optimization

| Technique | Saving |
|---|---|
| **Block-level sync** | Only upload changed blocks (not full save) |
| **Delta compression** | Binary diff against last synced version |
| **Hash comparison** | SHA-256 compare before download (avoid redundant pulls) |
| **Batch upload** | Aggregate incremental saves; push full save at checkpoint events only |
| **Background transfer** | iOS NSURLSession for large uploads (survives app suspend) |

**Target:** <50 KB per cloud sync event (average). <500 KB for full save push.

---

## 6. Offline-First Design

### 6.1 Offline Capability Matrix

| Feature | Offline Status | Notes |
|---|---|---|
| **Full campaign** | ✅ Fully playable | No degradation |
| **Building & restoration** | ✅ Fully playable | RS updates locally |
| **Combat & exploration** | ✅ Fully playable | |
| **Mini-games** | ✅ Fully playable | |
| **Cosmetic use** | ✅ Owned items work | Cannot purchase new |
| **IAP purchase** | ❌ Requires internet | Queued for retry |
| **Battle Pass progress** | ✅ Tracked locally | Server validates on reconnect |
| **Leaderboards** | 🔶 Cached (24h) | Updates on reconnect |
| **Events** | 🔶 Cached event data | New events require connectivity |
| **Cloud save sync** | ❌ Queued | Syncs on reconnect |

### 6.2 Offline Queue

```
pending_sync.db (SQLite, local)
│
├── Table: pending_uploads
│   ├── id INTEGER PRIMARY KEY
│   ├── save_slot INTEGER
│   ├── block_name TEXT
│   ├── payload BLOB (compressed)
│   ├── created_utc TEXT
│   └── retry_count INTEGER DEFAULT 0
│
└── On connectivity restored:
    ├── Sort by created_utc ASC
    ├── Upload oldest first
    ├── On success: delete row
    ├── On failure: increment retry_count
    └── Max retries: 5 (then alert player)
```

---

## 7. Conflict Resolution

### 7.1 Conflict Scenarios

| Scenario | Example | Resolution |
|---|---|---|
| **Same device, stale cloud** | Player was offline, cloud has old save | Local wins (newer modified_utc) |
| **Device switch, cloud newer** | Played on iPad, now on iPhone | Cloud wins (newer modified_utc) |
| **Simultaneous play** | Two devices played offline, both push | Merge by block (see §7.2) |
| **Corrupted local** | Checksum mismatch on local load | Cloud wins (automatic restore) |
| **Corrupted cloud** | Server-side data integrity failure | Local wins (report to analytics) |

### 7.2 Block-Level Merge Strategy

When both local and cloud have changes since last sync:

| Block | Merge Rule |
|---|---|
| **Player** | Highest play_time_seconds wins (more-progressed state) |
| **World** | Per-building: highest restoration_pct wins. Per-node: latest harvest time wins. |
| **Campaign** | Highest current_moon wins. Choices: first-written wins (immutable). |
| **Economy** | Server-authoritative for premium currency. Resources: highest value wins. |
| **Meta** | Union of achievements. Highest counters. Latest settings. |

### 7.3 Player-Facing Conflict UI

When automatic merge cannot resolve (rare — e.g., two World-Shaping choices made differently):

```
┌─────────────────────────────────────────────┐
│   ⚠️ Save Conflict Detected                 │
│                                              │
│   This Device         Cloud Save             │
│   Moon 7, Day 14      Moon 7, Day 19         │
│   42 buildings         45 buildings           │
│   Play Time: 24h       Play Time: 27h        │
│                                              │
│   [ Keep This Device ]  [ Keep Cloud ]       │
│                                              │
│   Both saves will be backed up.              │
└─────────────────────────────────────────────┘
```

**Rule:** The non-chosen save is archived (never deleted). Player can restore it from Settings > Save Management > Archived Saves.

---

## 8. Data Migration & Versioning

### 8.1 Schema Version History

| Version | Game Version | Changes |
|---|---|---|
| 1 | 1.0.0 | Initial schema |
| 2 | 1.1.0 | Added `anastasia_flags` to campaign block |
| 3 | 1.2.0 | Added `harmony_profile` to player block |
| ... | ... | ... |

### 8.2 Migration Pipeline

```
On Load:
1. Read header.schema_version
2. If schema_version < CURRENT_SCHEMA:
   a. Run migration chain: v1→v2→v3→...→current
   b. Each migration is a pure function: (old_data) → new_data
   c. Backup original save before migration
   d. Write migrated save as new version
3. If schema_version > CURRENT_SCHEMA:
   a. Old app trying to read new save — refuse with "update required" message
```

### 8.3 Migration Rules

| Rule | Rationale |
|---|---|
| **Migrations are additive only** | New fields get defaults; old fields never removed |
| **Each migration is tested** | Unit test per version pair in CI |
| **Backup before migrate** | Original save preserved as `slot_N_v{old}.bak` |
| **No breaking changes** | Renamed fields use aliased reads |
| **Max 5 version gap** | If save is >5 versions old, run full revalidation pass |

---

## 9. Integrity & Anti-Fraud

### 9.1 Local Integrity

| Protection | Mechanism |
|---|---|
| **Checksum** | SHA-256 of serialized payload stored in header |
| **Encryption** | AES-256-GCM, key in iOS Keychain (hardware-backed on Secure Enclave devices) |
| **Atomic writes** | Temp + rename prevents partial corruption |
| **Double buffer** | Previous save kept as `.bak` until next successful save |

### 9.2 Server-Side Validation

| Check | When | Action on Failure |
|---|---|---|
| **Play time sanity** | On cloud sync | Flag if play_time increases >24h between syncs |
| **Resource rate limits** | On Aether harvest sync | Reject if harvest exceeds theoretical maximum for time window |
| **IAP receipt verification** | On purchase | Apple receipt validation before granting premium currency |
| **Impossible state detection** | On cloud sync | Flag if Moon 10 reached but Moon 5 not completed |
| **Building count vs time** | On cloud sync | Flag if 100 buildings placed in 1 hour of play |
| **Leaderboard score bounds** | On RS submission | Reject scores exceeding mathematical maximum |

### 9.3 Anti-Fraud Response Tiers

| Severity | Detection | Response |
|---|---|---|
| **Low** | Unusual resource rates | Log to analytics, monitor |
| **Medium** | Impossible state detected | Flag account, defer leaderboard submission |
| **High** | Modified save binary (checksum mismatch + no valid migration path) | Restore last valid cloud save, notify player |
| **Critical** | IAP receipt forgery | Revoke items, report to Apple |

**Design Principle:** Never ban or punish without manual review. False positives are worse than undetected cheating. Log, flag, review.

---

## 10. Storage Budget

### 10.1 On-Device Storage

| Component | Size | Location |
|---|---|---|
| **5 save slots** | ~635 KB total | Application Support/saves/ |
| **1 backup per slot** | ~635 KB total | Application Support/saves/backup/ |
| **Pending sync queue** | <100 KB (SQLite) | Application Support/sync/ |
| **Settings** | <10 KB | UserDefaults |
| **Migration backups** | ~1.2 MB (2 versions retained) | Application Support/saves/migration/ |
| **Total save footprint** | **~2.6 MB** | |

### 10.2 Cloud Storage (per user)

| Component | Size | Backend |
|---|---|---|
| **Active save slots** | ~635 KB | Cloud Storage |
| **Firestore metadata** | ~5 KB | Firestore |
| **iCloud key-value** | <1 KB | iCloud KVS |
| **Historical backups (30 days)** | ~10 MB | Cloud Storage (lifecycle rule) |
| **Total cloud per user** | **~11 MB** | |
| **Cost at 500K users** | ~5.5 TB | ~$120/month (Cloud Storage) |

---

## 11. Disaster Recovery

### 11.1 Recovery Paths

| Disaster | Recovery Path |
|---|---|
| **Corrupted local save** | Load from cloud → if cloud also corrupt → load checkpoint → if all fail → archived backup |
| **Lost device** | Sign in on new device → pull cloud save → continue |
| **Cloud outage** | Local saves unaffected; queue sync for when cloud returns |
| **Accidental deletion (player)** | Archived deleted saves kept 30 days in cloud |
| **App reinstall** | Cloud save detected on first launch; prompt to restore |
| **Server migration** | Export/import tooling in admin dashboard |

### 11.2 Recovery Priority Chain

```
Load Attempt Order:
1. Local save (slot_N.sav)
2. Local backup (slot_N.bak)
3. Cloud primary (Firebase Cloud Storage)
4. Cloud secondary (iCloud Documents)
5. Migration backup (slot_N_v{X}.bak)
6. Checkpoint save (checkpoint.sav)
7. Last resort: new game with "We're sorry" bonus pack (100 Aether Shards + cosmetic)
```

---

## 12. ECS Integration

### 12.1 SaveStateSystem

```
SaveStateSystem (SystemBase, LateSimulationSystemGroup)
│
├── OnCreate():
│   ├── Register dirty-flag singleton entity
│   └── Load local save → hydrate ECS world
│
├── OnUpdate() (runs every frame):
│   ├── Check trigger events (zone transition, quest complete, etc.)
│   ├── If trigger OR 10-second timer expired:
│   │   ├── Read dirty flags
│   │   ├── Serialize dirty blocks from ECS queries
│   │   ├── Queue write job (Burst-compiled serialization)
│   │   └── Schedule disk I/O on worker thread
│   └── Check pending_sync queue (push on connectivity)
│
├── OnAppBackground():
│   ├── Force full serialize (all blocks, ignore dirty)
│   ├── Synchronous disk write (blocking)
│   └── Queue cloud upload via NSURLSession
│
└── OnDestroy():
    └── Final save + cleanup
```

### 12.2 Key ECS Components for Save

```csharp
// Singleton: tracks what needs saving
struct SaveDirtyFlags : IComponentData {
    bool Player;
    bool World;
    bool Campaign;
    bool Economy;
    bool Meta;
    double LastSaveTime;      // UnityEngine.Time.timeAsDouble
    double LastCloudPushTime;
}

// Tag: marks entities that participate in save serialization
struct SaveableEntity : IComponentData { }

// Shared: groups saveable entities by block
struct SaveBlock : ISharedComponentData {
    SaveBlockType Type; // Player, World, Campaign, Economy, Meta
}
```

### 12.3 Serialization Performance

| Operation | Target Time | Technique |
|---|---|---|
| **Incremental serialize (1 dirty block)** | <2ms | Burst-compiled NativeArray copy |
| **Full serialize (all blocks)** | <8ms | Parallel Burst jobs per block |
| **LZ4 compression** | <3ms | Native LZ4 via Unity.Collections |
| **AES encryption** | <2ms | CommonCrypto (hardware-accelerated on A-series) |
| **Disk write** | <5ms | Background thread, atomic rename |
| **Total emergency save** | **<20ms** | Well within 2-second background budget |

---

*What is saved cannot be truly lost. What is lost can always be rebuilt. That's the Tartarian way.*

---

**Document Status:** DRAFT
**Author:** Nathan / Resonance Energy
**Last Updated:** March 24, 2026
