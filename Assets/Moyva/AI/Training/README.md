# Moyva training scaffold

Scene: `Assets/Moyva/AI/Training/Scenes/MoyvaTraining.unity`.
The scene is deliberately absent from production build settings.
For a training player, build only this scene.

Configuration: `Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json`,
referenced as a TextAsset by TrainingBootstrap.
JSON is the single editable source; runtime receives validated private snapshots.
There is no duplicate ScriptableObject configuration asset.
Behavior name: `MoyvaStrategy`; default: Default, no model.
Default without a Python connection uses the safe heuristic.

Unity package: ML-Agents 4.0.0; matching Python trainer: mlagents 1.1.0
(official release_23). No Python dependencies or models are installed here.

Future training command (do not run until gameplay adapters exist):
`mlagents-learn Assets/Moyva/AI/Training/Config/moyva_ppo.yaml --run-id=moyva-001`
Then press Play in the training scene.
For a short trainer check, substitute `moyva_ppo_fast_test.yaml`.
For a built player add `--env=<training-player-path> --no-graphics`.
Use `--num-envs=4` for multiple player processes managed by the trainer.
Python UnityEnvironment launchers must assign different worker_id values;
different trainer jobs also need different base ports/run IDs.

No-trainer smoke: set behaviorType to 2 (HeuristicOnly) in JSON,
open the scene and press Play; inspect TrainingEnvironmentManager.Environments.
One safe NoOp decision runs every decisionInterval physics steps.
Episodes time out and reset; autoReset=false stops after the first episode.
Restore behaviorType=0 (Default) afterwards.
Pure tests: EditMode filter `Kruty1918.Moyva.AI.Training.Tests`.

Current limitations:
- ScaffoldSimulationFactory has no Moyva world or turn simulation.
  Its N instances isolate training state only; they are not N gameplay worlds.
- Implement ITrainingSimulationFactory to create a scoped gameplay session and
  ITrainingSimulation.Reset to reset map, players, turns, and gameplay services.
  ITurnService exposes no complete independent session reset API.
- ITrainingSimulation.CanEndTurn needs a non-mutating legality query covering
  authority and all blockers. CanOwnerAct alone does not check EndTurn blockers.
- When connected, TrainingActionExecutor delegates EndTurn to ITurnService.TryEndTurn.
  Until then only NoOp is unmasked, and turn count stays zero.
- Agent represents one faction; opponent progression is future simulation work.
- Other actions need legal candidate enumeration and service adapters.
- Observation layout v1 reserves Global(12), Player(2), Units(8),
  Buildings(6), Map(6). Values are deterministic zeros until a
  player-visible adapter is supplied. No hidden enemy information is queried.
- Reward events must carry trusted validation, stable event/entity IDs and owners.
  Each objective/event category is rewarded once per episode; shaping absolute
  sum is capped. Terminal rewards bypass that cap. Invalid episodes are interrupted
  and accumulated reward is cancelled; they halt automatic resets.
- Curriculum stages are labels only. No strategy, inference, or training is run.
