# com.kruty1918.vfx

Reusable VFX layer extracted from Moyva: `VfxPool` (pooled spawn, budgets, per-key cooldown), `VfxEffect`, `VfxDefinitionRegistry`, `VfxQualityState`, `VfxRendererFlash`, `VfxUnitSnapshotStore`, and contracts (`VfxSpawnRequest`, `IVfxSpawner`, `IVfxService`, `VfxEffectRule`, `VfxBudgetSettings`).

Host provides: event-id constants, the JSON/serialized catalog (`VfxCatalogConfig`-like), domain event wiring (e.g. `GameplayVfxService`), and quality-profile mapping.
