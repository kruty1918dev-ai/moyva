using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    // Reads the existing environment; owns no policy or command dependency.
    public sealed class TrainingPresentationController : MonoBehaviour
    {
        private readonly Dictionary<UnityEngine.Camera, bool> _cameras = new Dictionary<UnityEngine.Camera, bool>();
        private TrainingBootstrap _bootstrap;
        private int _observed;
        private bool _paused;
        private TrainingWorldPresentation _worldView;
        private GameplayTrainingEpisode _observedEpisode;
        public TrainingPresentationMode Mode => _bootstrap.Config.presentationMode;
        public bool OverlayEnabled { get; set; }
        public bool VisualizationPaused
        {
            get => _paused;
            set { _paused = value; ApplyCameras(); }
        }
        public int ObservedEnvironment
        {
            get => _observed;
            set { _observed = Mathf.Clamp(value, 0, Mathf.Max(0, (_bootstrap.Environments?.Environments.Count ?? 1) - 1)); ApplyCameras(); }
        }
        public TrainingEnvironment Selected
        {
            get
            {
                var environments = _bootstrap.Environments?.Environments;
                return environments != null && _observed < environments.Count ? environments[_observed] : null;
            }
        }
        public void Initialize(TrainingBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            OverlayEnabled = bootstrap.Config.enableSceneOverlay;
            if (Mode == TrainingPresentationMode.Visual)
                gameObject.AddComponent<TrainingSceneOverlay>().Configure(this, bootstrap);
            RefreshCameras();
        }
        // Call after a scope is created, never search the scene per frame.
        public void RefreshCameras()
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var camera in root.GetComponentsInChildren<UnityEngine.Camera>(true))
                    if (!_cameras.ContainsKey(camera)) _cameras.Add(camera, camera.enabled);
            ApplyCameras();
        }
        private void ApplyCameras()
        {
            if (_bootstrap == null) return;
            foreach (var entry in _cameras)
            {
                if (entry.Key == null) continue;
                int owner = 0;
                for (var parent = entry.Key.transform; parent != null; parent = parent.parent)
                    if (parent.name.StartsWith("Environment_") && int.TryParse(parent.name.Substring(12), out var id)) { owner = id; break; }
                entry.Key.enabled = entry.Value && Mode == TrainingPresentationMode.Visual && !_paused && owner == _observed;
            }
        }
        public void RestoreCameras()
        {
            foreach (var entry in _cameras) if (entry.Key != null) entry.Key.enabled = entry.Value;
            _cameras.Clear();
        }
        private void Update()
        {
            if (_bootstrap == null || Mode != TrainingPresentationMode.Visual) return;
            var episode = Selected?.GameplayEpisode;
            if (episode == _observedEpisode) return;
            _observedEpisode = episode;
            if (_worldView != null) Destroy(_worldView.gameObject);
            if (episode == null) return;
            var view = new GameObject("ObservedTrainingWorld");
            view.transform.SetParent(transform, false);
            _worldView = view.AddComponent<TrainingWorldPresentation>();
            _worldView.Build(episode.Grid, episode.Projection);
            _worldView.Observe(episode);
            RefreshCameras();
        }
        public void Zoom(float factor) { if (_worldView != null) _worldView.Zoom(factor); }
        private void OnDestroy() => RestoreCameras();
    }
}
