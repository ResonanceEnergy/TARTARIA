# AGENT 15: UI/UX POLISH — QUICK REFERENCE

## STATUS EFFECT DISPLAY API

### Basic Usage
```csharp
using Tartaria.UI;

// Add custom effect
StatusEffectDisplay.AddEffect("MyEffect", StatusEffectType.Buff, 30f, Color.cyan);

// Remove effect
StatusEffectDisplay.RemoveEffect("MyEffect");

// Check if effect is active
if (StatusEffectDisplay.HasEffect("Poison")) { /* ... */ }

// Get remaining time
float timeLeft = StatusEffectDisplay.GetRemainingTime("Poison");

// Clear all effects
StatusEffectDisplay.Clear();
```

### Common Effects (Built-in)
```csharp
// Debuffs (with screen tints)
StatusEffectDisplay.AddPoison(10f);     // Green tint
StatusEffectDisplay.AddBurn(8f);       // Red tint
StatusEffectDisplay.AddSlow(6f);       // Blue tint
StatusEffectDisplay.AddBleed(12f);     // No tint

// Buffs
StatusEffectDisplay.AddStrength(30f);
StatusEffectDisplay.AddDefense(30f);
StatusEffectDisplay.AddSpeed(20f);
StatusEffectDisplay.AddRegeneration(30f);

// Control effects
StatusEffectDisplay.AddStun(3f);
StatusEffectDisplay.AddSilence(5f);
StatusEffectDisplay.AddInvulnerability(5f);
StatusEffectDisplay.AddCorruption(15f);
```

### Custom Effect with Screen Tint
```csharp
StatusEffectDisplay.AddEffect(
    name: "Frozen",
    type: StatusEffectType.Debuff,
    duration: 8f,
    iconColor: Color.cyan,
    hasScreenTint: true,
    screenTintColor: new Color(0.3f, 0.7f, 1f),
    screenTintAlpha: 0.15f
);
```

---

## BOSS HEALTH BAR API

### Basic Usage
```csharp
using Tartaria.UI;

// Show boss health bar
BossHealthBar.Show("Ancient Leviathan", maxHealth: 5000f, totalPhases: 3);

// Update health (triggers damage flash)
BossHealthBar.UpdateHealth(4200f);

// Change phase
BossHealthBar.SetPhase(phase: 2, totalPhases: 3);

// Hide boss health bar
BossHealthBar.Hide();
```

### Example Boss Fight Integration
```csharp
public class BossFight : MonoBehaviour
{
    [SerializeField] float maxHealth = 5000f;
    [SerializeField] int totalPhases = 3;
    float currentHealth;
    int currentPhase = 1;

    void Start()
    {
        currentHealth = maxHealth;
        BossHealthBar.Show("Ancient Leviathan", maxHealth, totalPhases);
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
        BossHealthBar.UpdateHealth(currentHealth);

        // Phase transition at 66% and 33% HP
        if (currentHealth <= maxHealth * 0.66f && currentPhase == 1)
        {
            currentPhase = 2;
            BossHealthBar.SetPhase(2, totalPhases);
            Debug.Log("Boss entered Phase 2!");
        }
        else if (currentHealth <= maxHealth * 0.33f && currentPhase == 2)
        {
            currentPhase = 3;
            BossHealthBar.SetPhase(3, totalPhases);
            Debug.Log("Boss entered Phase 3 (Enrage)!");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        BossHealthBar.Hide();
        Debug.Log("Boss defeated!");
    }
}
```

---

## DAMAGE NUMBERS API

### Basic Usage
```csharp
using Tartaria.Gameplay;

// Regular damage (white)
DamageNumberPool.Spawn(damage: 42, worldPosition);

// Critical hit (yellow, larger, bounce, triggers hit marker)
DamageNumberPool.SpawnCritical(damage: 120, worldPosition);

// Player damage taken (red, screen shake)
DamageNumberPool.SpawnPlayerDamage(damage: 25, worldPosition);
```

### Integration with Combat System
```csharp
void OnEnemyHit(Enemy enemy, int damage, bool isCritical)
{
    Vector3 hitPosition = enemy.transform.position + Vector3.up * 1.5f;
    
    if (isCritical)
        DamageNumberPool.SpawnCritical(damage, hitPosition);
    else
        DamageNumberPool.Spawn(damage, hitPosition);
}

void OnPlayerDamaged(int damage)
{
    Vector3 playerPosition = Player.transform.position + Vector3.up * 1.5f;
    DamageNumberPool.SpawnPlayerDamage(damage, playerPosition);
}
```

---

## HUD INTERACTION PROMPTS

### Show/Hide Prompts
```csharp
using Tartaria.UI;

// Show prompt
PlayerHUDOverlay.ShowInteractionPrompt("[E] Open Chest");

// Hide prompt
PlayerHUDOverlay.HideInteractionPrompt();
```

### Integration with Interactables
```csharp
public class Chest : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHUDOverlay.ShowInteractionPrompt("[E] Open Chest");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHUDOverlay.HideInteractionPrompt();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerNearby())
        {
            Open();
            PlayerHUDOverlay.HideInteractionPrompt();
        }
    }
}
```

---

## CROSSHAIR HIT MARKER

### Trigger Hit Marker Expansion
```csharp
using Tartaria.UI;

// Called when player hits enemy
PlayerHUDOverlay.OnHitMarker();
```

### Auto-triggered by Critical Hits
```csharp
// DamageNumberPool.SpawnCritical() automatically triggers hit marker
DamageNumberPool.SpawnCritical(damage, position); // Hit marker auto-triggered
```

---

## SCREEN SHAKE

### Trigger Screen Shake
```csharp
using Tartaria.Core;

// Requires ServiceLocator.CameraShake to be registered
if (ServiceLocator.CameraShake != null)
{
    ServiceLocator.CameraShake.TriggerShake(
        intensity: 0.5f,   // 0.0 - 1.0 (higher = more violent)
        duration: 0.6f     // seconds
    );
}
```

### Common Use Cases
```csharp
// Player takes damage
ServiceLocator.CameraShake?.TriggerShake(0.25f, 0.4f);

// Boss slams ground
ServiceLocator.CameraShake?.TriggerShake(1.0f, 0.8f);

// Explosion
ServiceLocator.CameraShake?.TriggerShake(0.75f, 0.5f);

// Player lands from high fall
ServiceLocator.CameraShake?.TriggerShake(0.4f, 0.3f);
```

---

## HIT-STOP EFFECT

### Trigger Hit-Stop
```csharp
using Tartaria.Gameplay;

// Brief time freeze on hit (scaled by damage)
HitStopController.Trigger(damage: 100);
```

### Auto-triggered by Combat System
```csharp
// PlayerCombat.cs already calls this on melee hits
// No manual integration needed for basic combat
```

---

## ENEMY HEALTH BARS

### Attach to Enemy Prefabs
1. Add `EnemyHealthBar` component to enemy prefab
2. Assign references in Inspector:
   - Fill Image (child Image component)
   - Background Image (child Image component)
   - Canvas Group (on root)
3. Call `Initialize()` on spawn

### Dynamic Creation
```csharp
using Tartaria.UI;

public class EnemyHealth : MonoBehaviour
{
    EnemyHealthBar healthBar;
    float maxHealth = 100f;
    float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Create health bar dynamically
        var barGO = new GameObject("HealthBar");
        barGO.transform.SetParent(transform, false);
        healthBar = barGO.AddComponent<EnemyHealthBar>();
        healthBar.Initialize(transform, maxHealth);
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.UpdateHealth(currentHealth);
    }
}
```

---

## TESTING UI/UX SYSTEMS

### In Play Mode
1. Attach `UIUXPolishValidator` to any GameObject
2. Enter Play Mode
3. Press test keys:
   - **1** = Test Health Bar (damage)
   - **2** = Test XP Bar (gain XP)
   - **3** = Test Level Up
   - **4** = Test Damage Numbers
   - **5** = Test Crosshair Hit Marker
   - **6** = Test Interaction Prompt
   - **7** = Test Boss Health Bar
   - **8** = Test Status Effects (Debuffs)
   - **9** = Test Status Effects (Buffs)
   - **0** = Test Screen Shake
   - **H** = Test Hit-Stop
   - **C** = Clear All Effects

---

## COMMON INTEGRATION PATTERNS

### Combat Hit Flow
```csharp
void OnMeleeHit(Enemy enemy, int damage, bool isCritical)
{
    // 1. Apply damage
    enemy.TakeDamage(damage);
    
    // 2. Show damage number
    Vector3 hitPos = enemy.transform.position + Vector3.up * 1.5f;
    if (isCritical)
        DamageNumberPool.SpawnCritical(damage, hitPos);
    else
        DamageNumberPool.Spawn(damage, hitPos);
    
    // 3. Trigger hit-stop
    HitStopController.Trigger(damage);
    
    // 4. Screen shake (optional, for heavy hits)
    if (damage > 50)
        ServiceLocator.CameraShake?.TriggerShake(0.3f, 0.4f);
}
```

### Player Damage Flow
```csharp
void OnPlayerDamaged(int damage)
{
    // 1. Apply damage (PlayerHealth handles this + screen shake)
    playerHealth.TakeDamage(damage);
    
    // 2. Show damage number
    Vector3 playerPos = player.transform.position + Vector3.up * 1.5f;
    DamageNumberPool.SpawnPlayerDamage(damage, playerPos);
    
    // 3. Apply debuff (if applicable)
    if (isPoisoned)
        StatusEffectDisplay.AddPoison(10f);
}
```

### Boss Fight Flow
```csharp
void StartBossFight(Boss boss)
{
    // 1. Show boss health bar
    BossHealthBar.Show(boss.name, boss.maxHealth, boss.phaseCount);
    
    // 2. Subscribe to boss damage events
    boss.OnDamaged += (damage) => BossHealthBar.UpdateHealth(boss.currentHealth);
    boss.OnPhaseChange += (phase) => BossHealthBar.SetPhase(phase, boss.phaseCount);
    boss.OnDeath += () => BossHealthBar.Hide();
}
```

---

## PERFORMANCE TIPS

1. **Damage Numbers:** Pool size = 32. If you need more, increase `POOL_SIZE` in DamageNumberPool.cs
2. **Status Effects:** Max 8 icons per row. Use expiration timers to prevent clutter.
3. **Enemy Health Bars:** Create a pool manager if >20 enemies on screen.
4. **Boss Health Bar:** Only one instance active at a time (singleton).
5. **Screen Shake:** Don't call more than once per frame. Queue shakes if needed.

---

## TROUBLESHOOTING

**Q: Damage numbers not showing?**  
A: Check that UnityEngine.Camera.main is assigned in scene.

**Q: Status effect icons overlapping?**  
A: Max 8 per row. Clear old effects or increase spacing.

**Q: Boss health bar not showing?**  
A: Ensure BossHealthBar.Show() called before damage. Check for canvas render issues.

**Q: Screen shake not working?**  
A: Verify ServiceLocator.CameraShake is registered (CameraController must implement ICameraShakeService).

**Q: Hit-stop freezing game?**  
A: Check for multiple HitStop triggers per frame. Add cooldown if needed.

**Q: HUD elements not visible?**  
A: IMGUI renders in OnGUI(). Check that MonoBehaviour is active and DontDestroyOnLoad is set.

---

**Last Updated:** 2026-05-24  
**Agent:** 15 (UI/UX Polish)  
**Version:** 1.0
