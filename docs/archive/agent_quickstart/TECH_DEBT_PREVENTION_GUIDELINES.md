# TECH DEBT PREVENTION GUIDELINES

**Purpose:** Prevent accumulation of new technical debt in TARTARIA codebase  
**Audience:** All developers, code reviewers, tech leads  
**Last Updated:** 2026-05-24  
**Related:** [Agent 8 Tech Debt Report](LIVEOPS_AGENT8_TECH_DEBT_REPORT.md)

---

## 🚫 ZERO-TOLERANCE VIOLATIONS (Auto-Fail PR)

These patterns MUST NOT appear in any new code. Code reviewers should immediately reject PRs containing these anti-patterns.

### 1. TODOs Without Linked Tickets

❌ **WRONG:**
```csharp
// TODO: Implement proper error handling
public void SaveGame() {
    // ...
}
```

✅ **RIGHT:**
```csharp
// TODO(TART-1234): Implement proper error handling - see https://jira.company.com/browse/TART-1234
public void SaveGame() {
    // ...
}
```

**Rule:** Every TODO must have a Jira ticket reference. Use format: `TODO(TICKET-ID): Description`

### 2. Magic Numbers

❌ **WRONG:**
```csharp
if (distance < 5f) {
    ApplyDamage(10f);
    yield return new WaitForSeconds(3f);
}
```

✅ **RIGHT:**
```csharp
// Option A: Local constants
private const float ATTACK_RANGE = 5f;
private const float BASE_DAMAGE = 10f;
private const float ATTACK_COOLDOWN = 3f;

if (distance < ATTACK_RANGE) {
    ApplyDamage(BASE_DAMAGE);
    yield return new WaitForSeconds(ATTACK_COOLDOWN);
}

// Option B: ScriptableObject config (preferred for tunable values)
[SerializeField] CombatConfig _config;

if (distance < _config.attackRange) {
    ApplyDamage(_config.baseDamage);
    yield return new WaitForSeconds(_config.attackCooldown);
}
```

**Rule:** No numeric literals except 0, 1, -1. Use named constants or config files.

**Exceptions:** Array indices (`array[0]`), boolean values (`x > 0`), Unity API calls (`Vector3.forward * 1f`)

### 3. God Objects (>200 Lines)

❌ **WRONG:**
```csharp
public class GameManager : MonoBehaviour {
    // 15 responsibilities: audio, UI, save, combat, progression, ...
    // 800 lines of tangled logic
}
```

✅ **RIGHT:**
```csharp
// Single Responsibility Principle - each class does ONE thing
public class AudioManager { /* 150 lines - audio only */ }
public class SaveManager { /* 200 lines - persistence only */ }
public class UICoordinator { /* 100 lines - UI routing only */ }
```

**Rule:** Max 200 lines per class. If you hit the limit, split into cohesive single-responsibility classes.

**Measurement:**
```bash
# Fails CI if any .cs file >200 lines (excluding tests)
find Assets/_Project/Scripts -name "*.cs" -exec wc -l {} \; | awk '$1 > 200 {print}'
```

### 4. Non-Thread-Safe Singletons

❌ **WRONG:**
```csharp
public static MyManager Instance { get; private set; }
void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;  // ❌ RACE CONDITION
}
```

✅ **RIGHT (Option A - Bootstrap):**
```csharp
public static MyManager Instance { get; private set; }

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Bootstrap() {
    if (Instance != null) return;
    var go = new GameObject("MyManager");
    DontDestroyOnLoad(go);
    Instance = go.AddComponent<MyManager>();
}
```

✅ **RIGHT (Option B - Lazy):**
```csharp
static readonly Lazy<MyManager> _instance = new(() => new MyManager());
public static MyManager Instance => _instance.Value;
```

**Rule:** Use bootstrap pattern for MonoBehaviours, `Lazy<T>` for pure C# classes.

### 5. Hardcoded Strings (Localization)

❌ **WRONG:**
```csharp
_dialogueText.text = "Welcome to Echohaven, traveler.";
```

✅ **RIGHT:**
```csharp
_dialogueText.text = LocalizationManager.Instance.GetString("dialogue.milo.intro_01");
```

**Rule:** All user-facing text MUST use localization keys. Use format: `category.subcategory.key`

### 6. Missing Unit Tests

❌ **WRONG:**
```csharp
// New class - no test file exists
public class NewFeatureSystem { /* ... */ }
```

✅ **RIGHT:**
```csharp
// Production code
public class NewFeatureSystem { /* ... */ }

// Test file: Tests/EditMode/NewFeatureSystemTests.cs
[TestFixture]
public class NewFeatureSystemTests {
    [Test]
    public void NewFeature_WhenConditionMet_ReturnsExpectedResult() { /* ... */ }
}
```

**Rule:** Every new class MUST have at least 1 unit test. Aim for 80% coverage.

---

## ⚠️ SOFT GUIDELINES (Comment But Don't Block)

These are best practices. Reviewers should comment but not block merge.

### 7. Use Existing Design Patterns

✅ **Factory Pattern** (when instantiating prefabs):
```csharp
// Instead of: Instantiate(_enemyPrefab)
IEntityFactory factory = ServiceLocator.GetService<IEntityFactory>();
Enemy enemy = factory.Create<Enemy>("goblin_warrior");
```

✅ **Strategy Pattern** (when implementing pluggable behaviors):
```csharp
// Instead of: switch (damageType) { case Physical: ... case Magic: ... }
public interface IDamageStrategy {
    float CalculateDamage(float baseDamage, DamageContext context);
}

_damageStrategy.CalculateDamage(10f, context);
```

✅ **Command Pattern** (when implementing undoable actions):
```csharp
public interface ICommand {
    void Execute();
    void Undo();
}

_commandQueue.Push(new UseAbilityCommand(_player, _ability));
```

### 8. Prefer Composition Over Inheritance

❌ **FRAGILE:**
```csharp
public class Enemy { /* 200 lines */ }
public class FlyingEnemy : Enemy { /* overrides 10 methods */ }
public class BossEnemy : FlyingEnemy { /* even more overrides */ }
```

✅ **FLEXIBLE:**
```csharp
public class Enemy {
    IMovementBehavior _movement;
    ICombatBehavior _combat;
    ILootBehavior _loot;
    
    public Enemy(IMovementBehavior move, ICombatBehavior combat, ILootBehavior loot) {
        _movement = move; _combat = combat; _loot = loot;
    }
}

// Boss = GroundMovement + AggressiveCombat + LegendaryLoot
// Goblin = GroundMovement + PassiveCombat + CommonLoot
```

### 9. Single Responsibility Principle

Each class should do ONE thing. Signs of violation:
- Class name contains "And" (e.g., `SaveAndLoadManager`)
- Class has >5 public methods
- Class imports from >3 assemblies

**Refactor Trigger:** If you can't explain the class purpose in 1 sentence, split it.

### 10. Dependency Injection Over Static Singletons

❌ **TIGHT COUPLING:**
```csharp
public void UpdateHUD() {
    HUDController.Instance.SetHealth(_health);  // Hard dependency
}
```

✅ **LOOSE COUPLING:**
```csharp
private readonly IHUDService _hud;

public PlayerHealth(IHUDService hud) {
    _hud = hud;  // Injected via constructor
}

public void UpdateHUD() {
    _hud.SetHealth(_health);
}
```

---

## 📋 CODE REVIEW CHECKLIST

Use this checklist for every PR:

### Pre-Review (Automated CI Gates)
- [ ] ✅ Build passes (CS:0 compiler errors)
- [ ] ✅ TODO count <100 total (debt budget enforced)
- [ ] ✅ No magic numbers >10 per file
- [ ] ✅ No God objects >200 lines
- [ ] ✅ Code duplication <5%
- [ ] ✅ Tests pass (all suites green)

### Manual Review (Human Reviewer)
- [ ] ✅ All TODOs have Jira ticket references
- [ ] ✅ No hardcoded strings (localization used)
- [ ] ✅ No non-thread-safe singletons
- [ ] ✅ New classes have unit tests (min 1 test)
- [ ] ✅ CHANGELOG.md updated
- [ ] ✅ Single Responsibility Principle followed
- [ ] ✅ Existing design patterns used (Factory, Strategy, Command)
- [ ] ✅ No duplicate code (DRY principle)
- [ ] ✅ Assembly boundaries respected (no circular deps)
- [ ] ⚠️ Performance impact assessed (if applicable)

### Optional (Nice-to-Have)
- [ ] XML documentation on public APIs
- [ ] Integration tests for new features
- [ ] Performance tests for hot paths
- [ ] Accessibility considerations (colorblind, text scale)

---

## 🤖 AUTOMATED ENFORCEMENT (CI/CD)

### GitHub Actions Workflow (`.github/workflows/debt-gate.yml`)

```yaml
name: Tech Debt Gate

on: [pull_request]

jobs:
  debt-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      # Gate 1: TODO Budget
      - name: Count TODOs
        run: |
          TODO_COUNT=$(grep -r "TODO\|FIXME\|HACK" --include="*.cs" Assets/_Project/Scripts | wc -l)
          echo "📝 Current TODOs: $TODO_COUNT / 100"
          if [ $TODO_COUNT -gt 100 ]; then
            echo "❌ FAIL: TODO budget exceeded (100 max)"
            exit 1
          fi
      
      # Gate 2: Magic Number Detection
      - name: Detect Magic Numbers
        run: |
          # Fails if any file has >10 numeric literals
          find Assets/_Project/Scripts -name "*.cs" | while read file; do
            MAGIC_COUNT=$(grep -oE '\b[0-9]+\.?[0-9]*f?\b' "$file" | grep -v -E '^(0|1|-1)$' | wc -l)
            if [ $MAGIC_COUNT -gt 10 ]; then
              echo "❌ FAIL: $file has $MAGIC_COUNT magic numbers (10 max)"
              exit 1
            fi
          done
      
      # Gate 3: God Object Detection
      - name: Detect God Objects
        run: |
          find Assets/_Project/Scripts -name "*.cs" | while read file; do
            LINES=$(wc -l < "$file")
            if [ $LINES -gt 200 ]; then
              echo "❌ FAIL: $file is $LINES lines (200 max)"
              exit 1
            fi
          done
      
      # Gate 4: Code Duplication (PMD CPD)
      - name: Check Code Duplication
        run: |
          pmd cpd --minimum-tokens 50 --files Assets/_Project/Scripts --format text
          # Fails if >5% duplication
      
      # Gate 5: TODO Ticket References
      - name: Validate TODO Tickets
        run: |
          grep -rn "TODO" Assets/_Project/Scripts --include="*.cs" | while read line; do
            if ! echo "$line" | grep -qE 'TODO\([A-Z]+-[0-9]+\)'; then
              echo "❌ FAIL: TODO without ticket: $line"
              exit 1
            fi
          done
```

### Pre-Commit Hook (`.git/hooks/pre-commit`)

```bash
#!/bin/bash
# Auto-runs before every commit (optional)

# Check for TODOs without tickets
if git diff --cached --name-only | grep -E '\.cs$' | xargs grep -l "TODO" | grep -v "TODO("; then
    echo "❌ COMMIT BLOCKED: Found TODOs without ticket references"
    echo "Use format: TODO(TART-1234): Description"
    exit 1
fi

# Check for magic numbers in staged files
if git diff --cached | grep -E '^\+.*\b[0-9]{2,}\.[0-9]+f\b' | grep -v "const"; then
    echo "⚠️  WARNING: Potential magic numbers detected"
    echo "Consider using named constants or config files"
    # Don't block, just warn
fi
```

---

## 📊 WEEKLY DEBT REPORT

Automated Slack post every Friday 5pm:

```
📊 Tech Debt Dashboard (Week 21 of 2026)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
TODO Count:        92 / 100  ✅ (-8 from last week)
Magic Numbers:    480 / 500  ✅ (-20 from last week)
Code Duplication:  3.2%      ✅ (-0.5% from last week)
God Objects:       1 / 3     🎉 (-2 from last week!)
Test Coverage:    42%        ✅ (+2% from last week)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔥 Top Debt Contributors This Sprint:
1. Moon11ContentSpawner.cs (+12 TODOs) - @alice
2. BossAI.cs (+8 magic numbers) - @bob
3. UIManager.cs (+150 lines, approaching limit) - @charlie

📅 Next Refactor Friday: 2026-05-31
Focus: Integration tests for Moon 6-10

Full report: https://github.com/company/tartaria/issues/1234
```

---

## 🛠️ REFACTOR FRIDAY WORKFLOW

**Every Friday afternoon (last 8 hours of sprint):**

### Week 1: Test Week
- Goal: Add tests for features shipped this sprint
- Minimum: 1 test per new class
- Target: 60% coverage

### Week 2: TODO Week
- Goal: Resolve 5 P2 TODOs from backlog
- Focus: Quick wins (1-2 hour fixes)
- Prioritize oldest TODOs first

### Week 3: Duplication Week
- Goal: Eliminate 1 duplicate pattern
- Examples: Extract base classes, consolidate validation logic
- Use PMD CPD report as input

### Week 4: Pattern Week
- Goal: Apply 1 missing design pattern
- Examples: Introduce Factory, Strategy, or Command
- Refactor existing code to use pattern

**Process:**
1. 2:00pm - Review weekly debt dashboard
2. 2:15pm - Assign refactor tickets (1 per dev)
3. 2:30pm - Start refactoring (pair programming encouraged)
4. 5:30pm - Code review + merge
5. 6:00pm - Update debt tracking spreadsheet

---

## 🚨 ESCALATION PROCEDURES

### Yellow Alert (Warning)
**Trigger:** Debt count >120 items OR 2 sprints over budget  
**Action:**
- Email tech lead with debt report
- Schedule 30-min debt review meeting
- Add debt item to next sprint planning

### Orange Alert (Mandatory Debt Sprint)
**Trigger:** Debt count >150 items OR 3 sprints over budget  
**Action:**
- Feature freeze for 1 week
- All devs assigned debt tickets only
- Daily debt standup (9am)
- Goal: Reduce debt by 30%

### Red Alert (All-Hands Debt Reduction)
**Trigger:** Debt count >200 items OR 6 sprints over budget  
**Action:**
- Feature freeze for 2 weeks
- 100% team focus on debt
- CTO escalation
- Post-mortem required

---

## 📚 TRAINING & ONBOARDING

### New Developer Checklist
- [ ] Read this document (15 min)
- [ ] Read [Agent 8 Tech Debt Report](LIVEOPS_AGENT8_TECH_DEBT_REPORT.md) (30 min)
- [ ] Review [Agent 6 Pattern Audit](AGENT6_DESIGN_PATTERNS_AUDIT_REPORT.md) (1 hour)
- [ ] Complete "Refactoring 101" course (3 hours)
- [ ] Shadow senior dev during Refactor Friday (4 hours)
- [ ] Pass debt prevention quiz (10 min)

### Quiz Questions
1. What is the maximum TODO count before CI fails? (Answer: 100)
2. What is the maximum lines per class? (Answer: 200)
3. What is the format for TODO comments? (Answer: `TODO(TICKET-ID): Description`)
4. Name 3 zero-tolerance violations. (Answer: Magic numbers, God objects, missing tests)
5. What day is Refactor Friday? (Answer: Last day of sprint)

---

## 🎯 SUCCESS METRICS

### Sprint Goals
- ≤10 new TODOs per sprint
- Zero P0 debt items introduced
- +2% test coverage per sprint
- Zero God objects introduced

### Quarterly Goals (Q3 2026)
- Reduce total debt from 237 → 150 items
- Eliminate all P0 blockers
- Achieve 60% test coverage
- Zero feature velocity regression

### Annual Goals (2027)
- Maintain debt <100 items
- Achieve 80% test coverage
- Zero tech debt-related production bugs
- 20% sprint capacity for continuous improvement

---

## 📖 FURTHER READING

- [Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882) by Robert C. Martin
- [Refactoring](https://refactoring.com/) by Martin Fowler
- [Design Patterns](https://refactoring.guru/design-patterns) by Gang of Four
- [Unity Best Practices](https://unity.com/how-to/organizing-your-project) by Unity Technologies

---

**Document Owner:** Tech Lead  
**Last Review:** 2026-05-24  
**Next Review:** 2026-08-01 (Post-Q3 retrospective)  
**Feedback:** Submit via Jira epic TART-5000 (Tech Debt Prevention)
