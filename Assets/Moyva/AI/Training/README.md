# Moyva bot integration

Production: BotController -> BotDecisionOrchestrator -> policy -> capability -> game API.
Training: MoyvaStrategyAgent -> the same frame, masks, orchestrator and capabilities.
Rewards and episode reset remain training-only.

The shared contract is in Bot/Core/Contracts. It has 128 candidate slots,
64 global features, 8x8x8 spatial features and 24 features per candidate.
Policies receive encoded semantics; runtime IDs never enter the observation vector.
Candidate order/reduction is deterministic. EndTurn and category diversity survive
reduction; remaining critical candidates precede ordinary candidates.
Slot meanings are per-frame candidates, not the legacy TrainingActionType enum.
The former seven-action training layout is obsolete; do not reuse its models.
No training has been run for this contract.

Production config: Assets/Moyva/Presets/AI/Resources/MoyvaBotRuntime.json.
Model profile: Assets/Moyva/Presets/AI/Resources/MoyvaBotModelProfile.json.
Default policy is Heuristic, preferring EndTurn. Movement is exposed but no
strategy is implemented.
To use a future model, set policyMode=2, enable the matching profile, and add
MoyvaBotPolicyBinding to an object injected by the gameplay SceneContext.
Assign its ModelAsset in the Inspector. Never attach it to the ProjectContext.
The binding sets InferenceOnly; mismatch/missing model falls back to Heuristic.
Change profile/binding at game initialization or swap policies between turns.
No model is provided or loaded by default.

Open Moyva/AI/Bot Monitor in Play Mode for production or training telemetry.
It shows frames, candidates, last result, fallback reason and bounded episode metrics.
Learning health is a heuristic diagnostic, not proof of learning.
Confidence is N/A. Dump Last Decision logs a compact trace.
Historical training metrics use the MoyvaAI/ StatsRecorder prefix.

Training scene: Assets/Moyva/AI/Training/Scenes/MoyvaTraining.unity.
It is not in production build settings. Build only this scene for training players.
Training config: Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json.
Default behavior is Default; HeuristicOnly is behaviorType=2.
Training configuration only controls episodes, rewards and pacing.
Curriculum masks capabilities through the shared contract.

The default GameplayTrainingSimulationFactory fails closed. **Real training is blocked.**
MoyvaTraining does not yet bind ITrainingGameplayScopeFactory. The existing generator
requires scene-owned graph/TWC assets and asynchronous startup; BootstrapInstaller also
installs input, HUD, saves and multiplayer. No complete episode reset is available at
the inspected public boundary. A reset adapter has deliberately not been fabricated.
The scope composition must initialize the real world, players, ownership, units, fog,
turns and opponent progression, own their lifetime, and supply IGameplayTrainingReset.
That reset must finish world initialization, drain/cancel prior commands, restore all
episode state and consume the full TrainingResetContext before returning true.
Scene reload would require an asynchronous reset lifecycle; it is not implemented.
TrainingGameplayScope resolves the production turn/movement APIs, creates the shared
MovementBotCapability and MoyvaBotPerceptionSource, and requires gameplay authority.
It is a service adapter boundary, not a completed world composition.
TrainingGameplayEventBridge translates GameEndedSignal into real terminal rewards when
an owned scope supplies its SignalBus and ITurnHistoryQuery. Empty winners are draws
only with no surviving participants; cancellation becomes InvalidState. Unit destruction
signals lack owner/attacker data, so unit/building/objective shaping remains disconnected.
No real event subscription is active in the current training scene.
Explicit allowScaffoldSimulation=true selects the debugging scaffold, prominently marked
SCAFFOLD / NOT REAL GAMEPLAY; it can never receive READY_FOR_REAL_TRAINING.
No initialization exception falls back to scaffold.
Each environment owns its orchestrator, policy, counters and telemetry.
N scaffold environments are not N independent gameplay worlds.

Integrated capabilities:
- EndTurn: ITurnEndQuery.CanEndTurn and ITurnService.TryEndTurn share validation.
- Movement: owner-filtered IUnitService, IUnitMovementQuery, owner fog visibility,
  and IUnitMovementService.MoveUnitAsync; completion verifies the final position.
- Combat: unavailable until a visible combat entity/owner/position gateway connects
  unit/building IDs to ICombatCommandService. No combat rule is copied.
- Recruitment: IUnitRecruitmentService lacks a non-mutating enqueue/options query.
- Construction: EvaluatePlacement exists; player-scoped catalog and bounded
  candidate-location gateway are not connected.
- Capture: no public candidate/legality/command gateway identified.
Perception currently includes actual turn state, own unit count and visible
non-owned unit count. Economy/building/territory/spatial groups remain zero.
No team relation is inferred from the non-owned count.

A model profile must match the contract version and SHA-256 signature.
Internal implementation, prefab and UI changes alone need not change the contract.
Feature meaning/order, dimensions or new decision semantics require a version bump
and retraining/fine-tuning. Significant balance/strategy changes may also need it.

Future trainer: ML-Agents 4.0.0 / Python mlagents 1.1.0.
Baseline: Config/moyva_ppo.yaml; short check: Config/moyva_ppo_fast_test.yaml.
Once real reset/capabilities are connected:
mlagents-learn Assets/Moyva/AI/Training/Config/moyva_ppo.yaml --run-id=moyva-001
Press Play in MoyvaTraining, or add --env=<training-player> --no-graphics.
Use --num-envs=4 for trainer-managed player processes; custom launchers need
distinct worker_id values. Separate trainer jobs need distinct ports/run IDs.
No trainer, model generation or PPO tuning was run in this pass.

## Presentation and fast training

Open Moyva/AI/Training Monitor for readiness blockers, bounded episode graphs, completed
action distribution, heuristic learning health and Unity-measured throughput. Bot Monitor
remains available for decision details. TensorBoard remains the long-term history.
Visual (presentationMode=0) observes the same environment and enables a scene overlay;
MetricsOnly (1) disables cameras/overlay while retaining live metrics; HeadlessFast (2)
also skips telemetry views, retains four decision traces and disables VSync/frame limits.
The current scene has no real map/units to watch until the scope blocker is resolved.
Action actor/target coordinates are shown as text; world-space highlighting is not connected.
visualTimeScale and headlessTimeScale are separate, clamped to 0.1–20. Settings and camera
states are restored at shutdown or initialization failure. Legacy trainingTimeScale and
disableRenderingWhenPossible are retained for compatibility but no longer control mode.
Batch mode forces HeadlessFast when autoHeadlessInBatchMode=true, including when a CLI
mode is specified. Otherwise -moyvaTrainingMode Visual|MetricsOnly|HeadlessFast overrides JSON.

After readiness blockers are fixed, intended short smoke usage is:
```text
mlagents-learn Assets/Moyva/AI/Training/Config/moyva_ppo_fast_test.yaml --run-id=moyva-smoke --env=<training-player> --no-graphics
```
A directly launched training player accepts `-batchmode -nographics -moyvaTrainingMode HeadlessFast`.
Keep environmentCount=1 for real gameplay. Multiple Unity processes with distinct worker IDs
are the parallelization path until independent scopes exist. The fast YAML is unchanged
(512 steps); no unsupported engine YAML fields were added. Visual/headless gameplay parity
and runtime initialization remain unverified; presentation never selects or executes actions.
