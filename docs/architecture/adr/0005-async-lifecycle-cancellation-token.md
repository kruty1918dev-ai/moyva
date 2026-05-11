# 0005 Async Lifecycle via CancellationToken

- Status: Accepted
- Date: 2026-05-11

## Context

HomeMenu flows had several long async operations (join, startup, delayed UI actions) that could continue after UI state changed, provider switched, or services were disposed.

Symptoms:
- "Hanging" operations waiting longer than needed.
- Fire-and-forget tasks outliving panel/service lifecycle.
- Cancellation behavior inconsistent between services.

## Decision

Unify async lifecycle with cooperative cancellation:

1. Every long async flow must accept and propagate `CancellationToken` through all awaited calls.
2. Service-owned async flows must use service-owned `CancellationTokenSource` and cancel it on:
- `Dispose()`
- mode/provider change
- start of a newer mutually exclusive operation
3. `OperationCanceledException` is handled as a normal lifecycle outcome (not an error path).
4. Transport/lobby/startup operations must pass token to network, lobby, and polling APIs.

Applied in this step:
- `JoinRoomPanelService`: one join-operation token and full token propagation through password/lobby/transport flow.
- `JoinRoomTransportAdapter`: token propagation in provider switch, query/poll, join, leave.
- `HomeMenuInitializer`: lifecycle token for initialization and delayed overlay actions.
- `LobbyPanelService`, `WorldCreationPanelService`, `ContinuePanelService`, `GameStartListenerService`: tokenized `StartGameAsync` and cancellation on dispose/new run.

## Consequences

Positive:
- Fewer dangling tasks during panel transitions and provider switches.
- Faster recovery from user navigation changes.
- Predictable async shutdown semantics.

Trade-offs:
- More token plumbing in method signatures.
- Extra lifecycle state (`CancellationTokenSource`) per service.

## Rollback / Alternative

Rollback:
- Revert tokenized signatures and lifecycle CTS fields per service.

Alternative:
- Introduce a common `ILifecycleCancellation` abstraction to reduce repeated CTS management while preserving the same cancellation policy.
