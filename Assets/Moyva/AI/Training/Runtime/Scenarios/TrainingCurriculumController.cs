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
        public bool enabled;
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
        public int version = 1;
        public long totalDecisions;
        public long nextEvaluationStep;
        public string activeScenarioId;
        public string lastCheckpoint;
        public string bestVerifiedCheckpoint;
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

        public TrainingCurriculumController(AutonomousTrainingConfig config, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config)); _config.Validate();
            _catalog = TrainingScenarioCatalog.BuiltIn();
            _statePath = ResolveStatePath(config.statePath);
            _state = Load(_statePath) ?? new TrainingCurriculumSessionState();
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

        public void RecordTrainingEpisode(string scenarioId, bool success, int decisions)
        {
            var skill = Skill(scenarioId); if (skill == null) return;
            skill.trainingEpisodes++; if (success) skill.trainingSuccesses++;
            _state.totalDecisions += Math.Max(0, decisions);
            Save();
        }

        public bool EvaluationDue => _state.totalDecisions >= _state.nextEvaluationStep;
        public void RecordEvaluation(string scenarioId, int successes, int episodes)
        {
            var skill = Skill(scenarioId); if (skill == null || episodes <= 0) return;
            skill.evaluationEpisodes = episodes; skill.evaluationSuccesses = Math.Max(0, Math.Min(episodes, successes));
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
            _state.nextEvaluationStep = _state.totalDecisions + _config.evaluationEverySteps;
            Save();
        }

        public void SetCheckpoints(string latest, string bestVerified)
        { _state.lastCheckpoint = latest; _state.bestVerifiedCheckpoint = bestVerified; Save(); }
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
