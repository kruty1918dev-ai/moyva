using System;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    [Serializable]
    public sealed class BotAnalyzerSettings
    {
        private const string Prefix = "Moyva.BotAnalyzer.";

        [Title("Sampling"), MinValue(0.05f)] public float CoreSampleInterval = 0.10f;
        [MinValue(0.05f)] public float TraceSampleInterval = 0.10f;
        [MinValue(0.10f)] public float EconomySampleInterval = 0.20f;
        [MinValue(0.20f)] public float FogSampleInterval = 0.50f;

        [Title("History"), MinValue(50)] public int MaxFrames = 500;
        [MinValue(100)] public int MaxTimelineEvents = 5000;
        [MinValue(10)] public int MaxDisplayedEvents = 500;

        [Title("Scene Overlay")]
        public bool ShowFogVisible = true;
        public bool ShowFogExplored = true;
        public bool ShowOwnUnits = true;
        public bool ShowOwnBuildings = true;
        public bool ShowVisibleEnemies = true;
        public bool ShowMovementTrails = true;
        public bool ShowLastActions = true;
        public bool ShowCurrentGoal = true;
        public bool ShowTopCandidate = true;
        public bool ShowCandidateLabels;
        public bool ShowCellCoordinates;
        [Range(0.02f, 0.60f)] public float VisibleFogAlpha = 0.20f;
        [Range(0.01f, 0.40f)] public float ExploredFogAlpha = 0.08f;
        [MinValue(1f)] public float MovementTrailSeconds = 8f;
        [MinValue(0.5f)] public float ActionMarkerSeconds = 5f;

        [Title("Export")]
        public bool IncludeFramesInExport = true;
        public bool IncludeFogCellsInExport = true;
        public bool IncludeMemoryInExport = true;

        public static BotAnalyzerSettings CreateDefault() => new();

        public static BotAnalyzerSettings Load()
        {
            var s = new BotAnalyzerSettings();
            s.CoreSampleInterval = EditorPrefs.GetFloat(Prefix + nameof(CoreSampleInterval), s.CoreSampleInterval);
            s.TraceSampleInterval = EditorPrefs.GetFloat(Prefix + nameof(TraceSampleInterval), s.TraceSampleInterval);
            s.EconomySampleInterval = EditorPrefs.GetFloat(Prefix + nameof(EconomySampleInterval), s.EconomySampleInterval);
            s.FogSampleInterval = EditorPrefs.GetFloat(Prefix + nameof(FogSampleInterval), s.FogSampleInterval);
            s.MaxFrames = EditorPrefs.GetInt(Prefix + nameof(MaxFrames), s.MaxFrames);
            s.MaxTimelineEvents = EditorPrefs.GetInt(Prefix + nameof(MaxTimelineEvents), s.MaxTimelineEvents);
            s.MaxDisplayedEvents = EditorPrefs.GetInt(Prefix + nameof(MaxDisplayedEvents), s.MaxDisplayedEvents);
            s.ShowFogVisible = EditorPrefs.GetBool(Prefix + nameof(ShowFogVisible), s.ShowFogVisible);
            s.ShowFogExplored = EditorPrefs.GetBool(Prefix + nameof(ShowFogExplored), s.ShowFogExplored);
            s.ShowOwnUnits = EditorPrefs.GetBool(Prefix + nameof(ShowOwnUnits), s.ShowOwnUnits);
            s.ShowOwnBuildings = EditorPrefs.GetBool(Prefix + nameof(ShowOwnBuildings), s.ShowOwnBuildings);
            s.ShowVisibleEnemies = EditorPrefs.GetBool(Prefix + nameof(ShowVisibleEnemies), s.ShowVisibleEnemies);
            s.ShowMovementTrails = EditorPrefs.GetBool(Prefix + nameof(ShowMovementTrails), s.ShowMovementTrails);
            s.ShowLastActions = EditorPrefs.GetBool(Prefix + nameof(ShowLastActions), s.ShowLastActions);
            s.ShowCurrentGoal = EditorPrefs.GetBool(Prefix + nameof(ShowCurrentGoal), s.ShowCurrentGoal);
            s.ShowTopCandidate = EditorPrefs.GetBool(Prefix + nameof(ShowTopCandidate), s.ShowTopCandidate);
            s.ShowCandidateLabels = EditorPrefs.GetBool(Prefix + nameof(ShowCandidateLabels), s.ShowCandidateLabels);
            s.ShowCellCoordinates = EditorPrefs.GetBool(Prefix + nameof(ShowCellCoordinates), s.ShowCellCoordinates);
            s.VisibleFogAlpha = EditorPrefs.GetFloat(Prefix + nameof(VisibleFogAlpha), s.VisibleFogAlpha);
            s.ExploredFogAlpha = EditorPrefs.GetFloat(Prefix + nameof(ExploredFogAlpha), s.ExploredFogAlpha);
            s.MovementTrailSeconds = EditorPrefs.GetFloat(Prefix + nameof(MovementTrailSeconds), s.MovementTrailSeconds);
            s.ActionMarkerSeconds = EditorPrefs.GetFloat(Prefix + nameof(ActionMarkerSeconds), s.ActionMarkerSeconds);
            s.IncludeFramesInExport = EditorPrefs.GetBool(Prefix + nameof(IncludeFramesInExport), s.IncludeFramesInExport);
            s.IncludeFogCellsInExport = EditorPrefs.GetBool(Prefix + nameof(IncludeFogCellsInExport), s.IncludeFogCellsInExport);
            s.IncludeMemoryInExport = EditorPrefs.GetBool(Prefix + nameof(IncludeMemoryInExport), s.IncludeMemoryInExport);
            s.Normalize();
            return s;
        }

        public void Save()
        {
            Normalize();
            EditorPrefs.SetFloat(Prefix + nameof(CoreSampleInterval), CoreSampleInterval);
            EditorPrefs.SetFloat(Prefix + nameof(TraceSampleInterval), TraceSampleInterval);
            EditorPrefs.SetFloat(Prefix + nameof(EconomySampleInterval), EconomySampleInterval);
            EditorPrefs.SetFloat(Prefix + nameof(FogSampleInterval), FogSampleInterval);
            EditorPrefs.SetInt(Prefix + nameof(MaxFrames), MaxFrames);
            EditorPrefs.SetInt(Prefix + nameof(MaxTimelineEvents), MaxTimelineEvents);
            EditorPrefs.SetInt(Prefix + nameof(MaxDisplayedEvents), MaxDisplayedEvents);
            EditorPrefs.SetBool(Prefix + nameof(ShowFogVisible), ShowFogVisible);
            EditorPrefs.SetBool(Prefix + nameof(ShowFogExplored), ShowFogExplored);
            EditorPrefs.SetBool(Prefix + nameof(ShowOwnUnits), ShowOwnUnits);
            EditorPrefs.SetBool(Prefix + nameof(ShowOwnBuildings), ShowOwnBuildings);
            EditorPrefs.SetBool(Prefix + nameof(ShowVisibleEnemies), ShowVisibleEnemies);
            EditorPrefs.SetBool(Prefix + nameof(ShowMovementTrails), ShowMovementTrails);
            EditorPrefs.SetBool(Prefix + nameof(ShowLastActions), ShowLastActions);
            EditorPrefs.SetBool(Prefix + nameof(ShowCurrentGoal), ShowCurrentGoal);
            EditorPrefs.SetBool(Prefix + nameof(ShowTopCandidate), ShowTopCandidate);
            EditorPrefs.SetBool(Prefix + nameof(ShowCandidateLabels), ShowCandidateLabels);
            EditorPrefs.SetBool(Prefix + nameof(ShowCellCoordinates), ShowCellCoordinates);
            EditorPrefs.SetFloat(Prefix + nameof(VisibleFogAlpha), VisibleFogAlpha);
            EditorPrefs.SetFloat(Prefix + nameof(ExploredFogAlpha), ExploredFogAlpha);
            EditorPrefs.SetFloat(Prefix + nameof(MovementTrailSeconds), MovementTrailSeconds);
            EditorPrefs.SetFloat(Prefix + nameof(ActionMarkerSeconds), ActionMarkerSeconds);
            EditorPrefs.SetBool(Prefix + nameof(IncludeFramesInExport), IncludeFramesInExport);
            EditorPrefs.SetBool(Prefix + nameof(IncludeFogCellsInExport), IncludeFogCellsInExport);
            EditorPrefs.SetBool(Prefix + nameof(IncludeMemoryInExport), IncludeMemoryInExport);
        }

        public void Normalize()
        {
            CoreSampleInterval = Math.Max(0.05f, CoreSampleInterval);
            TraceSampleInterval = Math.Max(0.05f, TraceSampleInterval);
            EconomySampleInterval = Math.Max(0.10f, EconomySampleInterval);
            FogSampleInterval = Math.Max(0.20f, FogSampleInterval);
            MaxFrames = Math.Max(50, MaxFrames);
            MaxTimelineEvents = Math.Max(100, MaxTimelineEvents);
            MaxDisplayedEvents = Math.Max(10, MaxDisplayedEvents);
            MovementTrailSeconds = Math.Max(1f, MovementTrailSeconds);
            ActionMarkerSeconds = Math.Max(0.5f, ActionMarkerSeconds);
        }
    }
}
