using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private MenuSimulationSettings _simulationSettings;
        private MenuGameplaySimulation _simulation;
        private float _simulationTime;
        private float _nextPreviewFrame;
        private Vector3 _cinematicFocus;
        private MenuWorldPreviewData _simulationWorld;
        private IGridProjection _simulationProjection;
        private Dictionary<string, LivePreviewPrefabMesh> _simulationTiles;
        private Dictionary<string, LivePreviewPrefabMesh> _simulationBuildings;
        private readonly Dictionary<int, float> _simulationSurfaces = new Dictionary<int, float>();

        private void ResolveSimulationSettings()
        {
            _simulationSettings = MoyvaJsonRuntime.Get<HomeMenuConfigSO>("homemenuconfig")?.menuSimulation
                ?? new MenuSimulationSettings();
            _buildingRegistry ??= MoyvaJsonRuntime.GetAll<BuildingRegistrySO>().FirstOrDefault();
        }

        private void StartSimulation(MenuWorldPreviewData world, TileRegistrySO tiles, MoyvaProjectSettingsSO settings)
        {
            try
            {
                _simulationWorld = world;
                _simulationProjection = GridProjectionFactory.Create(settings);
                _simulationTiles = BuildTileLiveMeshCache(tiles, _graphAsset);
                _simulationBuildings = BuildBuildingLiveMeshCache(_buildingRegistry);
                _simulationSurfaces.Clear();
                _simulationTime = 0;
                _nextPreviewFrame = 0;
                _simulation = new MenuGameplaySimulation(_livePreviewRoot, world, tiles, _buildingRegistry,
                    settings, _simulationSettings, _livePreviewRoot.layer, ResolveSimulationPosition, ShowSimulationBuilding);
                _cinematicFocus = _simulation.Focus;
                _livePreviewCamera.orthographic = false;
                _livePreviewCamera.fieldOfView = 42f;
                _livePreviewCamera.backgroundColor = new Color(0.12f, 0.19f, 0.24f);
                _livePreviewCamera.allowMSAA = false;
                _livePreviewLight.color = new Color(1f, 0.91f, 0.76f);
                _livePreviewLight.intensity = 1.35f;
                ClearClouds();
            }
            catch (Exception exception)
            {
                Debug.LogError("[MenuSimulation] Could not start the gameplay demonstration: " + exception);
            }
        }

        private Vector3 ResolveSimulationPosition(Vector2Int cell)
        {
            float elevation = ResolvePreviewHeight(_simulationWorld, cell.x, cell.y, _livePreviewProjectSettings);
            Vector3 position = _simulationProjection.GridToWorld(cell, elevation, 0f);
            position.y = ResolveLiveTerrainSurfaceY(_simulationWorld, _simulationTiles, _simulationProjection,
                _livePreviewProjectSettings, _simulationSurfaces, cell.x, cell.y, elevation) + 0.02f;
            return position;
        }

        private void ShowSimulationBuilding(BuildingPlacedSignal signal)
        {
            if (!_simulationBuildings.TryGetValue(NormalizePreviewId(signal.BuildingId), out var mesh)) return;
            var visual = new GameObject("Preview " + signal.BuildingId);
            visual.transform.SetParent(_livePreviewRoot.transform, false);
            var builder = new LivePreviewMeshBuilder(visual.transform, _livePreviewRoot.layer,
                _livePreviewProjectSettings, _livePreviewMeshes);
            Vector3 position = ResolveSimulationPosition(signal.Position);
            position.y -= mesh.Bounds.min.y;
            builder.AddPrefab(mesh, Matrix4x4.TRS(position, Quaternion.Euler(0, signal.RotationQuarterTurns * 90, 0), Vector3.one));
            builder.Flush();
        }

        private void TickSimulation(float deltaTime)
        {
            if (_simulation == null || _livePreviewCamera == null) return;
            bool visible = Application.isFocused && _targetImage != null && _targetImage.isActiveAndEnabled;
            if (!visible) { _livePreviewCamera.enabled = false; return; }
            _simulationTime += Mathf.Min(deltaTime, 0.1f);
            _simulation.Tick(_simulationTime);
            if (_simulationTime > Mathf.Clamp(_simulationSettings.cycleSeconds, 45f, 180f))
            {
                RegeneratePreview();
                return;
            }
            int fps = Mathf.Clamp(Application.isMobilePlatform
                ? _simulationSettings.mobileFramesPerSecond : _simulationSettings.framesPerSecond, 10, 30);
            _livePreviewCamera.enabled = _simulationTime >= _nextPreviewFrame;
            if (!_livePreviewCamera.enabled) return;
            _nextPreviewFrame = _simulationTime + 1f / fps;
            float shotLength = Mathf.Clamp(_simulationSettings.shotSeconds, 4f, 15f);
            int shot = Mathf.FloorToInt(_simulationTime / shotLength);
            float t = (_simulationTime % shotLength) / shotLength;
            // Wide establishing shot, village dolly, low battle view, reverse
            // angle. Cuts reveal actual actions; small arcs keep each shot alive.
            bool wide = shot % 4 == 0;
            Vector3 target = wide ? _livePreviewWorldBounds.center : _simulation.Focus;
            _cinematicFocus = Vector3.Lerp(_cinematicFocus, target, 0.12f);
            float yaw = (shot % 4 switch { 0 => -35f, 1 => 75f, 2 => 155f, _ => 245f }) + t * 12f;
            float pitch = wide ? 48f : shot % 4 == 2 ? 28f : 38f;
            float distance = wide ? Mathf.Max(18f, _livePreviewWorldBounds.size.x * 0.85f) : 11f + 2f * t;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            _livePreviewCamera.transform.SetPositionAndRotation(
                _cinematicFocus + Vector3.up - rotation * Vector3.forward * distance, rotation);
        }
    }
}
