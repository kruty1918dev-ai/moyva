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

The default ScaffoldSimulationFactory has no gameplay world.
Its only candidate is Wait, which times out the episode without a gameplay command.
It does not fabricate turns, units, observations or success.
Do not train against this placeholder.
GameplayTrainingSimulationAdapter accepts the real TurnGateway, Capabilities and
Perception used by production, plus an explicit IGameplayTrainingReset boundary.
A scoped world factory/reset and opponent progression still need implementation.
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
No tests, trainer, model generation or PPO tuning are part of this integration pass.
