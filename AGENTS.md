# Moyva Agent Instructions

## Project Context

This is the Unity project **Moyva**.

Moyva is a turn-based strategy game built with:

- Unity 6.x
- C#
- URP
- Zenject
- TextMeshPro
- Odin Inspector where appropriate
- Square grid / tile-based procedural world systems

The project must stay maintainable, modular, and safe for long-term development.

---

## Core Rule

Before writing or modifying code, understand the local architecture.

Do not make broad rewrites unless explicitly requested.

Prefer small, safe, focused changes over large changes.

Every change must preserve existing Unity serialization unless a migration is explicitly requested.

---

## Architecture Principles

Follow these principles:

- SOLID
- DRY
- KISS
- YAGNI
- Composition over inheritance
- Dependency inversion
- Clear separation of responsibilities
- Explicit data flow
- Minimal coupling between systems
- High cohesion inside each feature module

Avoid:

- God classes
- static global state
- hardcoded dependencies
- hidden side effects
- duplicated logic
- oversized MonoBehaviours
- mixing runtime logic with editor logic
- mixing game logic with UI logic
- changing unrelated systems

---

## Unity-Specific Rules

### MonoBehaviour Rules

MonoBehaviours should be thin.

Use MonoBehaviours mainly for:

- Unity lifecycle entry points
- scene references
- view/presentation glue
- serialized configuration references
- forwarding events to services/controllers

Avoid putting complex business logic directly in MonoBehaviours.

Prefer plain C# services/classes for core logic.

---

### ScriptableObject Rules

Use ScriptableObjects for:

- configuration
- presets
- balance data
- tile/building/unit definitions
- editor-authored data
- reusable settings

Do not hardcode gameplay values directly in scripts if they should be configurable.

Bad:

```csharp
private const int MaxBuildings = 12;