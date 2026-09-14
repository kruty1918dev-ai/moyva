using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    public sealed class AutonomousTrainingConfig
    {
        public bool enabled = true;
        public int evaluationEverySteps = 10000;
        public int evaluationEpisodes = 50;
        public float masteryThreshold = 0.80f;
        public float regressionThreshold = 0.65f;
        public int masteryChecksRequired = 3;
        public int currentSkillWeight = 60;
        public int masteredReviewWeight = 25;
        public int combinationWeight = 15;
        public int fullGameWeightAfterBasics = 50;
        public int weakSkillWeightAfterBasics = 30;
        public int reviewWeightAfterBasics = 20;
        public string statePath = "";

        public void Validate()
        {
            if (evaluationEverySteps < 1 || evaluationEpisodes < 1 || masteryChecksRequired < 1)
                throw new ArgumentException("Autonomous curriculum evaluation settings must be positive.");
            if (masteryThreshold <= 0 || masteryThreshold > 1 || regressionThreshold < 0 || regressionThreshold >= masteryThreshold)
                throw new ArgumentException("Invalid autonomous curriculum thresholds.");
            if (currentSkillWeight + masteredReviewWeight + combinationWeight <= 0
                || fullGameWeightAfterBasics + weakSkillWeightAfterBasics + reviewWeightAfterBasics <= 0)
                throw new ArgumentException("Curriculum sampling weights must have a positive sum.");
        }
    }

    [Serializable] public sealed class TrainingSkillState
    {
        public string scenarioId;
        public bool mastered;
        public int consecutivePasses;
        public int trainingEpisodes, trainingSuccesses;
        public int evaluationEpisodes, evaluationSuccesses;
        public float LastEvaluationRate => evaluationEpisodes == 0 ? 0 : evaluationSuccesses / (float)evaluationEpisodes;
    }

    [Serializable] public sealed class TrainingCurriculumSessionState
    {
        public int version = 2;
        public long totalDecisions;
        public long nextEvaluationStep;
        public string activeScenarioId;
        public string lastCheckpoint;
        public long lastCheckpointStep;
        public string bestVerifiedCheckpoint;
        public long bestVerifiedCheckpointStep;
        public float bestVerifiedRate = -1f;
        public string lastEvaluationId;
        public int lastEvaluationEpisodes;
        public int lastEvaluationSuccesses;
        public bool lastEvaluationMasteryChanged;
        public string[] opponentPool = Array.Empty<string>();
        public List<TrainingSkillState> skills = new List<TrainingSkillState>();
    }

    public sealed class TrainingCurriculumController : IDisposable
    {
        private readonly AutonomousTrainingConfig _config;
        private readonly TrainingScenarioCatalog _catalog;
        private readonly TrainingCurriculumSessionState _state;
        private readonly System.Random _random;
        private readonly string _statePath;
        public TrainingCurriculumSessionState State => _state;
        public int EvaluationEpisodes => _config.evaluationEpisodes;

        public TrainingCurriculumController(AutonomousTrainingConfig config, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config)); _config.Validate();
            _catalog = TrainingScenarioCatalog.BuiltIn();
            _statePath = ResolveStatePath(config.statePath);
            _state = Load(_statePath) ?? new TrainingCurriculumSessionState();
            if (_state.skills == null) _state.skills = new List<TrainingSkillState>();
            if (_state.opponentPool == null) _state.opponentPool = Array.Empty<string>();
            if (_state.nextEvaluationStep <= 0) _state.nextEvaluationStep = config.evaluationEverySteps;
            foreach (var scenario in _catalog.Items)
                if (_state.skills.All(s => s.scenarioId != scenario.id)) _state.skills.Add(new TrainingSkillState { scenarioId = scenario.id });
            _random = new System.Random(unchecked(seed ^ (int)_state.totalDecisions));
        }

        public TrainingScenarioDefinition ChooseNext()
        {
            var ordered = Ordered();
            var firstUnmastered = ordered.FirstOrDefault(s => !Skill(s.id).mastered && DependenciesMastered(s));
            bool basicsMastered = ordered.Where(s => !s.fullGame).All(s => Skill(s.id).mastered);
            if (!basicsMastered)
            {
                int roll = _random.Next(Math.Max(1, _config.currentSkillWeight + _config.masteredReviewWeight + _config.combinationWeight));
                if (roll < _config.currentSkillWeight && firstUnmastered != null) return Select(firstUnmastered);
                roll -= _config.currentSkillWeight;
                var mastered = ordered.Where(s => Skill(s.id).mastered && !s.fullGame).ToArray();
                if (roll < _config.masteredReviewWeight && mastered.Length > 0) return Select(mastered[_random.Next(mastered.Length)]);

                var combinations = _catalog.Items
                    .Where(s => s != null
                        && s.id.StartsWith("combo-", StringComparison.Ordinal)
                        && DependenciesMastered(s))
                    .ToArray();
                if (combinations.Length > 0)
                    return Select(combinations[_random.Next(combinations.Length)]);

                return Select(firstUnmastered ?? ordered.First());
            }
            int post = _random.Next(Math.Max(1, _config.fullGameWeightAfterBasics + _config.weakSkillWeightAfterBasics + _config.reviewWeightAfterBasics));
            var full = _catalog.Get("full-game");
            if (post < _config.fullGameWeightAfterBasics && full != null) return Select(full);
            post -= _config.fullGameWeightAfterBasics;
            var weak = ordered.Where(s => !s.fullGame).OrderBy(s => Skill(s.id).LastEvaluationRate).FirstOrDefault();
            if (post < _config.weakSkillWeightAfterBasics && weak != null) return Select(weak);
            var review = ordered.Where(s => Skill(s.id).mastered).ToArray();
            return Select(review.Length == 0 ? full : review[_random.Next(review.Length)]);
        }

        public TrainingScenarioDefinition GetScenario(string scenarioId) => _catalog.Get(scenarioId);
        public TrainingSkillState GetSkill(string scenarioId) => Skill(scenarioId);

        public void RecordTrainingEpisode(string scenarioId, bool success, int decisions)
        {
            var skill = Skill(scenarioId); if (skill == null) return;
            skill.trainingEpisodes++; if (success) skill.trainingSuccesses++;
            _state.totalDecisions += Math.Max(0, decisions);
            Save();
        }

        public bool EvaluationDue => _state.totalDecisions >= _state.nextEvaluationStep;

        // Returns true only when the mastered flag changed. A partial evaluation is
        // rejected without touching any mastery/evaluation state.
        public bool RecordEvaluation(string scenarioId, int successes, int episodes, long checkpointStep = -1,
            string verifiedCheckpoint = null, string evaluationId = null)
        {
            var skill = Skill(scenarioId);
            if (skill == null || episodes != _config.evaluationEpisodes) return false;
            int boundedSuccesses = Math.Max(0, Math.Min(episodes, successes));
            if (!string.IsNullOrWhiteSpace(evaluationId) && _state.lastEvaluationId == evaluationId)
            {
                if (_state.lastEvaluationEpisodes != episodes || _state.lastEvaluationSuccesses != boundedSuccesses)
                    throw new InvalidOperationException("Duplicate frozen evaluation id produced a different result.");
                return _state.lastEvaluationMasteryChanged;
            }
            bool wasMastered = skill.mastered;
            skill.evaluationEpisodes = episodes;
            skill.evaluationSuccesses = boundedSuccesses;
            float rate = skill.LastEvaluationRate;
            if (rate >= _config.masteryThreshold)
            {
                skill.consecutivePasses++;
                if (skill.consecutivePasses >= _config.masteryChecksRequired) skill.mastered = true;
            }
            else
            {
                skill.consecutivePasses = 0;
                if (rate < _config.regressionThreshold) skill.mastered = false;
            }
            long verifiedStep = checkpointStep >= 0 ? checkpointStep : _state.totalDecisions;
            _state.nextEvaluationStep = Math.Max(_state.totalDecisions, verifiedStep) + _config.evaluationEverySteps;
            bool masteryChanged = wasMastered != skill.mastered;
            ConsiderBestVerifiedCheckpoint(verifiedCheckpoint, verifiedStep, rate);
            if (!string.IsNullOrWhiteSpace(evaluationId))
            {
                _state.lastEvaluationId = evaluationId;
                _state.lastEvaluationEpisodes = episodes;
                _state.lastEvaluationSuccesses = boundedSuccesses;
                _state.lastEvaluationMasteryChanged = masteryChanged;
            }
            Save();
            return masteryChanged;
        }

        public void SetLatestCheckpoint(string checkpoint, long step)
        {
            _state.lastCheckpoint = checkpoint;
            _state.lastCheckpointStep = Math.Max(0, step);
            Save();
        }

        private void ConsiderBestVerifiedCheckpoint(string checkpoint, long step, float successRate)
        {
            if (string.IsNullOrWhiteSpace(checkpoint) || float.IsNaN(successRate) || float.IsInfinity(successRate)) return;
            if (_state.bestVerifiedCheckpoint == null || successRate > _state.bestVerifiedRate
                || (Math.Abs(successRate - _state.bestVerifiedRate) < 0.000001f && step > _state.bestVerifiedCheckpointStep))
            {
                _state.bestVerifiedCheckpoint = checkpoint;
                _state.bestVerifiedCheckpointStep = Math.Max(0, step);
                _state.bestVerifiedRate = Mathf.Clamp01(successRate);
                Save();
            }
        }

        // Kept for source compatibility. A training checkpoint is never promoted to
        // bestVerified here; only a completed RecordEvaluation + Consider... may do it.
        public void SetCheckpoints(string latest, string bestVerified)
        {
            SetLatestCheckpoint(latest, _state.totalDecisions);
        }

        public void SetOpponentPool(IEnumerable<string> checkpoints)
        { _state.opponentPool = (checkpoints ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray(); Save(); }
        public void Dispose() => Save();

        private TrainingScenarioDefinition Select(TrainingScenarioDefinition scenario)
        { _state.activeScenarioId = scenario.id; Save(); return scenario; }
        private TrainingSkillState Skill(string id) => _state.skills.FirstOrDefault(s => s.scenarioId == id);
        private bool DependenciesMastered(TrainingScenarioDefinition scenario)
            => (scenario.prerequisites ?? Array.Empty<string>()).All(id => Skill(id)?.mastered == true);
        private TrainingScenarioDefinition[] Ordered()
        {
            string[] ids = { "castle", "production", "stable-economy", "recruitment", "movement-scouting", "combat-defense", "capture", "full-game" };
            return ids.Select(_catalog.Get).Where(x => x != null).ToArray();
        }
        private static string ResolveStatePath(string configured)
        {
            if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured);
            return Path.Combine(Application.persistentDataPath, "MoyvaTraining", "curriculum-state.json");
        }
        private static TrainingCurriculumSessionState Load(string path)
        {
            try { return File.Exists(path) ? JsonUtility.FromJson<TrainingCurriculumSessionState>(File.ReadAllText(path)) : null; }
            catch (Exception e) { Debug.LogWarning("Cannot load curriculum state: " + e.Message); return null; }
        }
        private void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(_statePath); if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                string temporary = _statePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(_state, true));
                if (File.Exists(_statePath)) File.Delete(_statePath);
                File.Move(temporary, _statePath);
            }
            catch (Exception e) { Debug.LogWarning("Cannot save curriculum state: " + e.Message); }
        }
    }
}
