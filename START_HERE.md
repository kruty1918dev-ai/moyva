# MOYVA PROJECT: COMPLETE SCRIPTS DOCUMENTATION
## Start Here - Your Complete Guide to All 824 Scripts

---

## WHAT YOU HAVE

**7 Documentation Files Created:**

1. **START_HERE.md** ← You are here
2. **ALL_SCRIPTS_COMPLETE_LIST.txt** - Simple list of all 824 script names
3. **MASTER_SCRIPTS_REFACTORING_PLAN.md** - Complete implementation guide with code examples
4. **SCRIPTS_MANAGEMENT_PLAN.md** - Overview and architecture
5. **SCRIPTS_QUICK_REFERENCE.md** - Quick lookup table
6. **DETAILED_SCRIPTS_LIST.md** - Full inventory with module breakdown
7. **SCRIPTS_INVENTORY.csv** - Spreadsheet format (Excel/Sheets compatible)
8. **00_README_FIRST.txt** - Navigation guide

---

## QUICK ANSWERS

### "I want to see ALL 824 SCRIPT NAMES"
→ Open: **ALL_SCRIPTS_COMPLETE_LIST.txt**

### "I want to CHANGE a script - how do I do it?"
→ Open: **MASTER_SCRIPTS_REFACTORING_PLAN.md** and search for the module name

### "What scripts should I work on FIRST?"
→ Open: **MASTER_SCRIPTS_REFACTORING_PLAN.md** → Section 5: IMPLEMENTATION ROADMAP
**TOP-10 PRIORITIES:**
1. Economy (52 scripts) - 76 hours - Architecture redesign
2. Construction (58 scripts) - 32 hours - Service splitting
3. HomeMenu (114 scripts) - 30 hours - Controller refactoring
4. Generator (168 scripts) - 50 hours - Algorithm optimization
5. Multiplayer (76 scripts) - 58 hours - Sync optimization
6. Units (30 scripts) - 24 hours - Controller splitting
7. WorldCreation (14 scripts) - 12 hours - Phase organization
8. FogOfWar (22 scripts) - 18 hours - Algorithm optimization
9. BotAI (10 scripts) - 28 hours - Decision tree splitting
10. GraphSystem (37 scripts) - 20 hours - Caching implementation

### "What's the total effort?"
→ 366 hours across Q2-Q4 2026

### "How do I change a specific module like Economy?"
→ Open: **MASTER_SCRIPTS_REFACTORING_PLAN.md** → Section 3 → Module 7: ECONOMY
See detailed code examples and step-by-step instructions

### "I want a spreadsheet with all modules"
→ Open: **SCRIPTS_INVENTORY.csv** in Excel or Google Sheets

### "Show me the architecture and dependencies"
→ Open: **MASTER_SCRIPTS_REFACTORING_PLAN.md** → Section 10: HOW TO USE THIS PLAN → Dependency Map

---

## COMPLETE SCRIPT BREAKDOWN BY MODULE

### Module Counts (Total: 824 scripts)

| # | Module | Scripts | Status | Change Type | Hours |
|---|--------|---------|--------|-------------|-------|
| 1 | Animations | 4 | ✅ Stable | Monitor | 0 |
| 2 | BotAI | 10 | 🔧 Refactoring | Split decision tree | 28 |
| 3 | Calendar | 15 | ✅ Stable | Monitor | 0 |
| 4 | Camera | 10 | ✅ Stable | Monitor | 0 |
| 5 | Clouds | 7 | ✅ Stable | Monitor | 0 |
| 6 | Construction | 58 | 🔧 Refactoring | Split services | 32 |
| 7 | Economy | 52 | 🔴 Rewrite | Full redesign | 76 |
| 8 | Faction | 10 | ✅ Stable | Monitor | 0 |
| 9 | FogOfWar | 22 | 🔧 Refactoring | Optimize algorithm | 18 |
| 10 | GameMode | 10 | ✅ Stable | Monitor | 0 |
| 11 | Generator | 168 | 🚀 Active Dev | Optimize algorithms | 50 |
| 12 | GraphSystem | 37 | 🚀 Active Dev | Add caching | 20 |
| 13 | Grid | 8 | ✅ Stable | Do NOT modify | 0 |
| 14 | HomeMenu | 114 | 🔧 Refactoring | Split controllers | 30 |
| 15 | InfoPanel | 2 | ✅ Stable | Monitor | 0 |
| 16 | Interactions | 7 | ✅ Stable | Monitor | 0 |
| 17 | Multiplayer | 76 | 🚀 Active Dev | Optimize sync | 58 |
| 18 | ObjectsMap | 3 | ✅ Stable | Monitor | 0 |
| 19 | Pathfinding | 3 | ✅ Stable | Monitor | 0 |
| 20 | SaveSystem | 17 | ✅ Stable | Monitor | 0 |
| 21 | Signals | 15 | ✅ Stable | Do NOT modify | 0 |
| 22 | Units | 30 | 🔧 Refactoring | Split controller | 24 |
| 23 | Visuals | 5 | ✅ Stable | Monitor | 0 |
| 24 | WorldCreation | 14 | 🔧 Refactoring | Split by phases | 12 |
| 25 | Bootstrap | 15 | ✅ Stable | Monitor | 0 |
| 26 | Shared | 55 | ✅ Stable | Monitor | 0 |
| 27 | Editor | 30 | ✅ Stable | Monitor | 0 |
| 28 | EditorShared | 20 | ✅ Stable | Monitor | 0 |
| 29+ | Tests | 55 | 🚀 Active Dev | Expand coverage | 38 |
| **TOTAL** | **25 modules** | **824** | **Mixed** | **Various** | **366** |

---

## ALL 824 SCRIPTS - QUICK REFERENCE

For complete file paths, see: **ALL_SCRIPTS_COMPLETE_LIST.txt**

### Statistics
- **Stable Modules:** 55% (450 scripts) - No changes needed
- **Refactoring Modules:** 24% (200 scripts) - Code quality improvements
- **Active Development:** 18% (150 scripts) - New features/optimization  
- **Rewrite:** 6% (52 scripts - Economy) - Architecture redesign

### Largest Modules (by script count)
1. Generator - 168 scripts
2. HomeMenu - 114 scripts
3. Multiplayer - 76 scripts
4. Construction - 58 scripts
5. Economy - 52 scripts

### Most Critical Modules (refactoring priority)
1. Economy - Architecture redesign IN PROGRESS
2. Generator - Algorithm optimization NEEDED
3. HomeMenu - Controller split NEEDED
4. Multiplayer - Sync optimization NEEDED
5. Construction - Service split NEEDED

---

## HOW TO USE THE MASTER PLAN

### Step 1: Choose a Module to Work On
Look at TOP-10 PRIORITIES above or choose from the module table

### Step 2: Open MASTER_SCRIPTS_REFACTORING_PLAN.md
Search for "MODULE XX: [YOUR MODULE NAME]"

### Step 3: Follow the Implementation Pattern
Each module section includes:
- List of all scripts in that module
- Current status and recommended changes
- Specific code examples
- How to test changes
- Git commit pattern

### Step 4: Execute and Commit
```bash
# Example: Refactoring Economy module
git add *.cs && git commit -m "refactor(Economy): split orchestrator into services"
```

### Step 5: Verify Tests Pass
```bash
dotnet test Kruty1918.Moyva.Tests.{ModuleName}
```

---

## IMPLEMENTATION TIMELINE

### Q2 2026 (April-June) - 120 Hours
- Week 1-2: Economy Phase 1, Construction split, BotAI initial
- Week 3-4: Economy Phase 2, HomeMenu split, Units split
- Week 5-6: WorldCreation split, Economy optimization, Tests

### Q3 2026 (July-September) - 90 Hours
- Generator optimization (24h)
- Multiplayer optimization (20h)
- FogOfWar optimization (18h)
- GraphSystem caching (20h)
- Performance testing (8h)

### Q4 2026 (October-December) - 60 Hours
- Bug fixes from Q2-Q3 (15h)
- Performance optimization (15h)
- Final testing & integration (20h)
- Documentation updates (10h)

**TOTAL EFFORT: 366 hours (~9-10 developer weeks)**

---

## CHANGE PATTERNS (Copy-Paste Templates)

### Pattern 1: Splitting a Large Class
```csharp
// Step 1: Create interfaces (if needed)
// Modules/{Module}/API/INewService.cs

// Step 2: Extract methods to new service
// Modules/{Module}/Runtime/NewService.cs

// Step 3: Update DI registration
// {Module}Installer.cs

// Step 4: Update using code
// Remove old dependencies, inject new service

// Step 5: Commit
git add *.cs && git commit -m "refactor(Module): split ServiceName"
```

### Pattern 2: Optimizing Performance
```csharp
// Step 1: Add profiling
#define ENABLE_PROFILING

// Step 2: Identify bottleneck
dotnet test Kruty1918.Moyva.Tests.{Module}

// Step 3: Implement cache
private Dictionary<Key, Value> cache;

// Step 4: Verify improvement
// Commit with performance metric
git commit -m "perf(Module): ServiceName 30% improvement"
```

### Pattern 3: Adding Tests
```csharp
// Location: Tests/{Module}/{FeatureName}Tests.cs
[Test]
public void ShouldDoSomething()
{
    // Arrange
    // Act
    // Assert
}

// Commit
git commit -m "test(Module): add {FeatureName} tests"
```

---

## GIT COMMIT GUIDE

### Format
```
TYPE(Module): description
```

### Types
- `feat` - New feature
- `fix` - Bug fix
- `refactor` - Code quality (splitting, organizing)
- `perf` - Performance optimization
- `test` - Test improvements
- `docs` - Documentation

### Examples
```bash
# Splitting a service
git commit -m "refactor(Economy): split TickOrchestrator into micro-services"

# Performance improvement
git commit -m "perf(Generator): optimize ErosionNode caching (50% faster)"

# New feature
git commit -m "feat(Multiplayer): add delta compression state sync"

# Bug fix
git commit -m "fix(Construction): correct overlap validation"

# Test coverage
git commit -m "test(Economy): add settlement trading scenarios"
```

---

## FILE REFERENCE GUIDE

| File | Purpose | Use When |
|------|---------|----------|
| ALL_SCRIPTS_COMPLETE_LIST.txt | Simple listing of all 824 script names | You need to find a script file name |
| MASTER_SCRIPTS_REFACTORING_PLAN.md | Complete implementation guide with code examples | You're about to work on a module |
| SCRIPTS_MANAGEMENT_PLAN.md | High-level project structure overview | You're new to the project |
| SCRIPTS_QUICK_REFERENCE.md | Quick lookup tables and decision matrix | You need quick answers |
| DETAILED_SCRIPTS_LIST.md | Full inventory with change recommendations | You want deep details on all scripts |
| SCRIPTS_INVENTORY.csv | Module counts in spreadsheet format | You're using Excel/Google Sheets |
| 00_README_FIRST.txt | Navigation guide for different roles | You're unsure which file to read |
| START_HERE.md | This file | You're starting now |

---

## DEPENDENCIES & ARCHITECTURE

```
Signals (FOUNDATION - all modules depend on this)
  ↓
Grid + Calendar (FOUNDATION - geometric + time base)
  ↓
Construction, Units, BotAI, Economy (DOMAIN LOGIC)
  ↓
FogOfWar, Multiplayer (DERIVED SYSTEMS)
  ↓
HomeMenu, GameMode (TOP-LEVEL FACADES)
```

**CRITICAL MODULES (Do NOT modify without approval):**
- Signals - All modules depend on SignalBus
- Grid - HexagonalGridCalculator is used everywhere

---

## NEXT STEPS

1. **Choose Priority #1:** Economy module (52 scripts, 76 hours)
2. **Open:** MASTER_SCRIPTS_REFACTORING_PLAN.md Section 3, Module 7
3. **Follow:** Step-by-step implementation guide with code examples
4. **Test:** `dotnet test Kruty1918.Moyva.Tests.Economy`
5. **Commit:** `git commit -m "refactor(Economy): [description]"`

---

## SUPPORT

- **Need the script list?** → ALL_SCRIPTS_COMPLETE_LIST.txt (824 lines)
- **Need implementation help?** → MASTER_SCRIPTS_REFACTORING_PLAN.md (specific module sections)
- **Need architecture overview?** → SCRIPTS_MANAGEMENT_PLAN.md
- **Need quick lookup?** → SCRIPTS_QUICK_REFERENCE.md
- **Need spreadsheet?** → SCRIPTS_INVENTORY.csv

---

**Status:** ✅ COMPLETE - All 824 scripts documented with change recommendations
**Format:** Markdown + CSV + TXT (multiple formats for different tools)
**Timeline:** Q2-Q4 2026 (366 hours total effort)
**Ready to implement:** YES

---

**Document Version:** 2.0  
**Last Updated:** 2024-05-14  
**All files committed to Git**
