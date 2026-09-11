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
        private enum CinematicShotKind
        {
            Establishing,
            Settlement,
            March,
            Battle,
            Reverse
        }

        private MenuSimulationSettings _simulationSettings;
        private MenuGameplaySimulation _simulation;
        private float _simulationTime;
        private float _nextPreviewFrame;
        private Vector3 _cinematicFocus;
        private int _lastCinematicShot = -1;
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

        private void StartSimulation(
            MenuWorldPreviewData world,
            TileRegistrySO tiles,
            MoyvaProjectSettingsSO settings)
        {
            try
            {
                _simulationWorld = world;
                _simulationProjection = GridProjectionFactory.Create(settings);
                _simulationTiles = BuildTileLiveMeshCache(tiles, _graphAsset);
                _simulationBuildings = BuildBuildingLiveMeshCache(_buildingRegistry);
                _simulationSurfaces.Clear();
                _simulationTime = 0f;
                _nextPreviewFrame = 0f;
                _lastCinematicShot = -1;

                _simulation = new MenuGameplaySimulation(
                    _livePreviewRoot,
                    world,
                    tiles,
                    _buildingRegistry,
                    settings,
                    _simulationSettings,
                    _livePreviewRoot.layer,
                    ResolveSimulationPosition,
                    ShowSimulationBuilding);

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
            float elevation = ResolvePreviewHeight(
                _simulationWorld,
                cell.x,
                cell.y,
                _livePreviewProjectSettings);

            Vector3 position = _simulationProjection.GridToWorld(cell, elevation, 0f);
            position.y = ResolveLiveTerrainSurfaceY(
                _simulationWorld,
                _simulationTiles,
                _simulationProjection,
                _livePreviewProjectSettings,
                _simulationSurfaces,
                cell.x,
                cell.y,
                elevation) + 0.02f;

            return position;
        }

        private void ShowSimulationBuilding(BuildingPlacedSignal signal)
        {
            if (!_simulationBuildings.TryGetValue(
                    NormalizePreviewId(signal.BuildingId),
                    out var mesh))
            {
                return;
            }

            var visual = new GameObject("Preview " + signal.BuildingId);
            visual.transform.SetParent(_livePreviewRoot.transform, false);

            var builder = new LivePreviewMeshBuilder(
                visual.transform,
                _livePreviewRoot.layer,
                _livePreviewProjectSettings,
                _livePreviewMeshes);

            Vector3 position = ResolveSimulationPosition(signal.Position);
            position.y -= mesh.Bounds.min.y;

            builder.AddPrefab(
                mesh,
                Matrix4x4.TRS(
                    position,
                    Quaternion.Euler(0f, signal.RotationQuarterTurns * 90f, 0f),
                    Vector3.one));

            builder.Flush();
        }

        private void TickSimulation(float deltaTime)
        {
            if (_simulation == null || _livePreviewCamera == null)
                return;

            bool visible =
                Application.isFocused &&
                _targetImage != null &&
                _targetImage.isActiveAndEnabled;

            if (!visible)
            {
                _livePreviewCamera.enabled = false;
                return;
            }

            _simulationTime += Mathf.Min(deltaTime, 0.1f);
            _simulation.Tick(_simulationTime);

            if (_simulationTime > Mathf.Clamp(_simulationSettings.cycleSeconds, 45f, 180f))
            {
                RegeneratePreview();
                return;
            }

            int fps = Mathf.Clamp(
                Application.isMobilePlatform
                    ? _simulationSettings.mobileFramesPerSecond
                    : _simulationSettings.framesPerSecond,
                10,
                30);

            _livePreviewCamera.enabled = _simulationTime >= _nextPreviewFrame;
            if (!_livePreviewCamera.enabled)
                return;

            _nextPreviewFrame = _simulationTime + 1f / fps;

            float shotLength = Mathf.Clamp(_simulationSettings.shotSeconds, 4f, 15f);
            int shot = Mathf.FloorToInt(_simulationTime / shotLength);
            float shotT = (_simulationTime % shotLength) / shotLength;

            UpdateCinematicCamera(shot, shotT);
        }

        private void UpdateCinematicCamera(int shot, float shotT)
        {
            MenuSimulationStage stage = _simulation.Stage;
            CinematicShotKind kind = ResolveShotKind(stage, shot);

            bool cut = shot != _lastCinematicShot;
            _lastCinematicShot = shot;

            Vector3 actionFocus = _simulation.Focus;
            Vector3 mapFocus = _livePreviewWorldBounds.center;
            Vector3 target = kind == CinematicShotKind.Establishing
                ? mapFocus
                : actionFocus;

            if (kind == CinematicShotKind.Settlement)
                target = Vector3.Lerp(mapFocus, actionFocus, 0.72f);

            if (cut)
                _cinematicFocus = target;
            else
                _cinematicFocus = Vector3.Lerp(_cinematicFocus, target, 0.09f);

            float worldExtent = Mathf.Max(
                _livePreviewWorldBounds.size.x,
                _livePreviewWorldBounds.size.z);

            float yaw;
            float height;
            float distance;
            float fov;

            switch (kind)
            {
                case CinematicShotKind.Establishing:
                    yaw = -38f + (shot % 3) * 72f;
                    height = Mathf.Clamp(worldExtent * 0.72f, 15f, 34f);
                    distance = Mathf.Clamp(worldExtent * 0.9f, 20f, 48f);
                    fov = 43f;
                    break;

                case CinematicShotKind.Settlement:
                    yaw = 48f + (shot % 4) * 63f;
                    height = 9.5f;
                    distance = 14.5f;
                    fov = 39f;
                    break;

                case CinematicShotKind.March:
                    yaw = 120f + (shot % 4) * 52f;
                    height = 7.2f;
                    distance = 12.5f;
                    fov = 37f;
                    break;

                case CinematicShotKind.Battle:
                    yaw = 205f + (shot % 5) * 41f;
                    height = 5.4f;
                    distance = 9.6f;
                    fov = 35f;
                    break;

                default:
                    yaw = 286f - (shot % 4) * 57f;
                    height = 8.2f;
                    distance = 13.2f;
                    fov = 38f;
                    break;
            }

            // The angle changes immediately on a cut, then each shot has a restrained
            // dolly/orbit so the background feels edited rather than like a spinning camera.
            float eased = Mathf.SmoothStep(0f, 1f, shotT);
            yaw += Mathf.Lerp(-4f, 7f, eased);

            if (kind == CinematicShotKind.Battle)
            {
                distance -= Mathf.Sin(shotT * Mathf.PI) * 1.2f;
                height -= Mathf.Sin(shotT * Mathf.PI) * 0.45f;
            }
            else if (kind != CinematicShotKind.Establishing)
            {
                distance += Mathf.Lerp(0.8f, -0.6f, eased);
            }

            float yawRadians = yaw * Mathf.Deg2Rad;
            Vector3 planarOffset = new Vector3(
                Mathf.Sin(yawRadians),
                0f,
                -Mathf.Cos(yawRadians)) * distance;

            Vector3 cameraPosition =
                _cinematicFocus +
                planarOffset +
                Vector3.up * height;

            Vector3 lookTarget =
                _cinematicFocus +
                Vector3.up * (kind == CinematicShotKind.Battle ? 0.55f : 0.8f);

            Vector3 lookDirection = lookTarget - cameraPosition;
            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            Quaternion cameraRotation = Quaternion.LookRotation(
                lookDirection.normalized,
                Vector3.up);

            _livePreviewCamera.transform.SetPositionAndRotation(
                cameraPosition,
                cameraRotation);

            _livePreviewCamera.fieldOfView = cut
                ? fov
                : Mathf.Lerp(_livePreviewCamera.fieldOfView, fov, 0.12f);
        }

        private static CinematicShotKind ResolveShotKind(MenuSimulationStage stage, int shot)
        {
            int sequence = Mathf.Abs(shot) % 4;

            return stage switch
            {
                MenuSimulationStage.Founding => sequence switch
                {
                    0 => CinematicShotKind.Establishing,
                    1 => CinematicShotKind.Settlement,
                    2 => CinematicShotKind.Reverse,
                    _ => CinematicShotKind.Settlement
                },

                MenuSimulationStage.Growth => sequence switch
                {
                    0 => CinematicShotKind.Settlement,
                    1 => CinematicShotKind.Establishing,
                    2 => CinematicShotKind.Reverse,
                    _ => CinematicShotKind.Settlement
                },

                MenuSimulationStage.Expansion => sequence switch
                {
                    0 => CinematicShotKind.Establishing,
                    1 => CinematicShotKind.Settlement,
                    2 => CinematicShotKind.March,
                    _ => CinematicShotKind.Reverse
                },

                MenuSimulationStage.Conflict => sequence switch
                {
                    0 => CinematicShotKind.March,
                    1 => CinematicShotKind.Reverse,
                    2 => CinematicShotKind.Establishing,
                    _ => CinematicShotKind.Battle
                },

                _ => sequence switch
                {
                    0 => CinematicShotKind.Battle,
                    1 => CinematicShotKind.Reverse,
                    2 => CinematicShotKind.March,
                    _ => CinematicShotKind.Establishing
                }
            };
        }
    }
}
