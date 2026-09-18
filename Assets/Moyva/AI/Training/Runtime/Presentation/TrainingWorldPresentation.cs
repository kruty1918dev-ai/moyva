using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kruty1918.Moyva.AI.Training
{
    // Meshes observe the actual grid. They contain no colliders or gameplay state.
    public sealed class TrainingWorldPresentation : MonoBehaviour
    {
        private readonly List<Object> _assets = new List<Object>();
        private GameplayTrainingEpisode _episode;
        private UnityEngine.Camera _camera;
        private Vector3 _lookAt;
        private Vector3 _fitLookAt;
        private float _fitFieldOfView;
        private Vector3 _fitPosition;
        private Quaternion _fitRotation;
        private float _cameraDistance;
        private float _minCameraDistance;
        private float _maxCameraDistance;
        private float _mapSpan = 24f;
        private float _zoomSensitivity = 1f;
        private float _panSpeed = 1f;
        private float _nextRefresh;
        private Vector3 _lastMousePosition;
        private bool _draggingCamera;
        private GameObject _fogOverlayRoot;
        private readonly Dictionary<Vector2Int, GameObject> _buildings = new Dictionary<Vector2Int, GameObject>();
        internal void Observe(GameplayTrainingEpisode episode) { _episode = episode; RefreshBuildings(); }
        public void Zoom(float factor)
        {
            if (_camera == null) return;
            if (factor == 0)
            {
                _lookAt = _fitLookAt;
                _camera.transform.SetPositionAndRotation(_fitPosition, _fitRotation);
                _camera.fieldOfView = _fitFieldOfView;
                _cameraDistance = Vector3.Distance(_camera.transform.position, _lookAt);
                return;
            }

            SetCameraDistance(_cameraDistance * factor);
        }
        private static void CopyRenderTree(Transform source, Transform parent)
        {
            // Copy only render data: prefab behaviours/colliders must never become a second simulation.
            var copy = new GameObject(source.name);
            copy.transform.SetParent(parent, false);
            copy.transform.localPosition = source.localPosition;
            copy.transform.localRotation = source.localRotation;
            copy.transform.localScale = source.localScale;
            var filter = source.GetComponent<MeshFilter>();
            var renderer = source.GetComponent<MeshRenderer>();
            if (filter != null && renderer != null)
            {
                copy.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                copy.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
            }
            foreach (Transform child in source) CopyRenderTree(child, copy.transform);
        }
        private void RefreshBuildings()
        {
            if (_episode == null || _episode.Root == null || !_episode.EconomyInstalled) return;
            var placements = _episode.Placements.GetSavedPlacements();
            var cells = new HashSet<Vector2Int>(placements.Select(p => p.Position));
            foreach (var cell in _buildings.Keys.Where(c => !cells.Contains(c)).ToArray())
            { Destroy(_buildings[cell]); _buildings.Remove(cell); }
            foreach (var placement in placements)
            {
                var definition = _episode.Buildings.GetById(placement.BuildingId);
                string identity = placement.BuildingId + ":" + placement.Rotation;
                if (_buildings.TryGetValue(placement.Position, out var previous))
                {
                    if (previous.name == identity) continue;
                    Destroy(previous); _buildings.Remove(placement.Position);
                }
                var building = new GameObject(identity);
                building.transform.SetParent(transform, false);
                building.transform.position = _episode.Projection.GridToWorld(placement.Position)
                    + Vector3.up * (TileSurfaceHeight(_episode.GeneratedWorld, placement.Position)
                        + (definition?.ResolveVisualYOffset() ?? 0));
                building.transform.rotation = Quaternion.Euler(0, (int)placement.Rotation * 90, 0);
                if (definition?.Prefab != null) CopyRenderTree(definition.Prefab.transform, building.transform);
                _buildings.Add(placement.Position, building);
            }
        }
        private void Update()
        {
            HandleCameraInput();
            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + 0.25f;
                RefreshBuildings();
                RefreshFogOverlay();
            }
        }

        private void HandleCameraInput()
        {
            if (_camera == null || !_camera.enabled) return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            float scroll = mouse?.scroll.ReadValue().y ?? 0f;
            if (Mathf.Abs(scroll) > 0.001f)
                SetCameraDistance(_cameraDistance * Mathf.Pow(0.86f, (scroll / 120f) * _zoomSensitivity));

            Vector3 groundForward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
            if (groundForward.sqrMagnitude < 0.001f) groundForward = Vector3.forward;
            Vector3 groundRight = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
            if (groundRight.sqrMagnitude < 0.001f) groundRight = Vector3.right;

            Vector3 keyboardPan = Vector3.zero;
            if (keyboard?.leftArrowKey.isPressed == true || keyboard?.aKey.isPressed == true) keyboardPan -= groundRight;
            if (keyboard?.rightArrowKey.isPressed == true || keyboard?.dKey.isPressed == true) keyboardPan += groundRight;
            if (keyboard?.upArrowKey.isPressed == true || keyboard?.wKey.isPressed == true) keyboardPan += groundForward;
            if (keyboard?.downArrowKey.isPressed == true || keyboard?.sKey.isPressed == true) keyboardPan -= groundForward;
            if (keyboardPan.sqrMagnitude > 0.001f)
                PanCamera(keyboardPan.normalized * (_mapSpan * 0.65f * _panSpeed * Time.unscaledDeltaTime));

            if (keyboard?.qKey.isPressed == true) OrbitCamera(-80f * Time.unscaledDeltaTime);
            if (keyboard?.eKey.isPressed == true) OrbitCamera(80f * Time.unscaledDeltaTime);

            bool drag = mouse?.rightButton.isPressed == true || mouse?.middleButton.isPressed == true;
            if (drag && !_draggingCamera)
            {
                _draggingCamera = true;
                _lastMousePosition = mouse.position.ReadValue();
            }
            else if (drag)
            {
                Vector3 delta = (Vector3)mouse.position.ReadValue() - _lastMousePosition;
                _lastMousePosition = mouse.position.ReadValue();
                float scale = Mathf.Max(0.01f, _cameraDistance) * 0.0018f;
                PanCamera((-groundRight * delta.x - groundForward * delta.y) * scale * _panSpeed);
            }
            else _draggingCamera = false;
        }

        private void PanCamera(Vector3 delta)
        {
            _lookAt += delta;
            _camera.transform.position += delta;
        }

        private void OrbitCamera(float degrees)
        {
            Vector3 offset = _camera.transform.position - _lookAt;
            offset = Quaternion.AngleAxis(degrees, Vector3.up) * offset;
            _camera.transform.position = _lookAt + offset;
            _camera.transform.LookAt(_lookAt, Vector3.up);
            _cameraDistance = offset.magnitude;
        }

        private void SetCameraDistance(float distance)
        {
            _cameraDistance = Mathf.Clamp(distance, _minCameraDistance, _maxCameraDistance);
            Vector3 direction = (_camera.transform.position - _lookAt).normalized;
            if (direction.sqrMagnitude < 0.001f) direction = new Vector3(0.35f, 0.7f, -0.62f).normalized;
            _camera.transform.position = _lookAt + direction * _cameraDistance;
            _camera.transform.LookAt(_lookAt, Vector3.up);
        }
        private void Label(Vector3 point, string text, bool learner)
        {
            var screen = _camera.WorldToScreenPoint(point);
            if (screen.z <= 0) return;
            var old = GUI.color;
            GUI.color = learner ? new Color(0.45f, 0.8f, 1) : new Color(1, 0.5f, 0.4f);
            GUI.Box(new Rect(screen.x - 65, Screen.height - screen.y - 20, 130, 22), text);
            GUI.color = old;
        }
        private void OnGUI()
        {
            if (_camera == null || !_camera.enabled || _episode?.Root == null) return;
            foreach (var id in _episode.Units.GetAllUnitIds())
                if (_episode.Units.TryGetUnitPosition(id, out var cell))
                    Label(_episode.Projection.GridToWorld(cell) + Vector3.up * (TileSurfaceHeight(_episode.GeneratedWorld, cell) + 0.8f),
                        _episode.Units.GetUnitTypeId(id),
                        _episode.UnitOwners.GetUnitOwnerId(id) == TrainingGameplayScope.LearnerId);
            if (_episode.EconomyInstalled)
                foreach (var placement in _episode.Placements.GetSavedPlacements())
                    Label(_episode.Projection.GridToWorld(placement.Position)
                            + Vector3.up * (TileSurfaceHeight(_episode.GeneratedWorld, placement.Position) + 1.2f),
                        _episode.Buildings.GetById(placement.BuildingId)?.DisplayName ?? placement.BuildingId,
                        placement.OwnerId == TrainingGameplayScope.LearnerId);
        }

        public void Build(IGridService grid, IGridProjection projection, MenuWorldPreviewData worldData,
            float zoomSensitivity = 1f, float panSpeed = 1f)
        {
            _zoomSensitivity = Mathf.Clamp(zoomSensitivity, 0.1f, 5f);
            _panSpeed = Mathf.Clamp(panSpeed, 0.1f, 5f);
            Vector3 origin = projection.GridToWorld(Vector2Int.zero);
            Vector3 u = projection.GridToWorld(Vector2Int.right) - origin;
            Vector3 v = projection.GridToWorld(Vector2Int.up) - origin;
            Vector3 normal = Vector3.Cross(v, u).normalized;
            if (normal.y < 0f) normal = -normal;
            var groups = new Dictionary<string, MeshBuildData>();
            var heightRange = ResolveHeightRange(worldData);
            int waterCells = 0;
            int mountainCells = 0;
            for (int y = 0; y < grid.GridHeight; y++)
                for (int x = 0; x < grid.GridWidth; x++)
                {
                    var cell = new Vector2Int(x, y);
                    string tile = grid.GetTileData(cell) ?? "empty";
                    if (IsWater(tile)) waterCells++;
                    if (IsMountain(tile) || TileHeight01(worldData, cell, heightRange) > 0.78f) mountainCells++;
                    string groupKey = ResolveVisualGroupKey(worldData, heightRange, cell, tile);
                    if (!groups.TryGetValue(groupKey, out var meshData)) groups[groupKey] = meshData = new MeshBuildData();
                    AddTileColumn(meshData, projection, worldData, heightRange, cell, tile, u, v, normal);
                }
            foreach (var group in groups)
            {
                var go = new GameObject(group.Key);
                go.transform.SetParent(transform, false);
                var mesh = new Mesh { name = "Training grid " + group.Key, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.SetVertices(group.Value.Vertices);
                mesh.SetTriangles(group.Value.Triangles, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Unlit/Color");
                if (shader == null) throw new System.InvalidOperationException("Training presentation shader is unavailable.");
                var material = new Material(shader);
                var color = ResolveTileColor(group.Key);
                ApplyMaterialColor(material, color);
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
                _assets.Add(mesh); _assets.Add(material);
            }
            CreateResourceMarkers(projection, worldData, heightRange);
            var cameraObject = new GameObject("TrainingCamera");
            cameraObject.transform.SetParent(transform, false);
            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 34f;
            var centerPoint = projection.GridToWorld(new Vector2Int(grid.GridWidth / 2, grid.GridHeight / 2));
            float span = Mathf.Max(grid.GridHeight * v.magnitude, grid.GridWidth * u.magnitude);
            _mapSpan = Mathf.Max(1f, span);
            centerPoint += normal * Mathf.Max(1f, heightRange.y * 0.5f);
            _lookAt = centerPoint;
            camera.transform.position = centerPoint
                - v.normalized * span * 0.58f
                + u.normalized * span * 0.22f
                + normal * Mathf.Max(12f, span * 0.92f);
            camera.transform.LookAt(centerPoint, normal);
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 600;
            camera.backgroundColor = new Color(0.12f, 0.17f, 0.20f);
            _camera = camera;
            _cameraDistance = Vector3.Distance(camera.transform.position, _lookAt);
            _minCameraDistance = Mathf.Max(4f, span * 0.18f);
            _maxCameraDistance = Mathf.Max(40f, span * 2.5f);
            _fitLookAt = _lookAt;
            _fitFieldOfView = camera.fieldOfView;
            _fitPosition = camera.transform.position;
            _fitRotation = camera.transform.rotation;
            var lightObject = new GameObject("TrainingLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.LookRotation(new Vector3(-0.4f, -1f, -0.25f), Vector3.up);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            RefreshFogOverlay();
            Debug.Log("MOYVA_TRAINING_3D_MAP_READY cells=" + (grid.GridWidth * grid.GridHeight)
                + " tileMeshes=" + groups.Count
                + " waterCells=" + waterCells
                + " mountainCells=" + mountainCells
                + " heightRange=" + heightRange.x.ToString("0.###") + ".." + heightRange.y.ToString("0.###")
                + " camera=PerspectiveThreeQuarter");
        }

        private static void ApplyMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", color.a < 0.999f ? 1f : 0f);
            if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            material.color = color;
            material.renderQueue = color.a < 0.999f
                ? (int)UnityEngine.Rendering.RenderQueue.Transparent
                : (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }

        private void RefreshFogOverlay()
        {
            if (_episode?.Fog == null || _episode.GeneratedWorld == null || _camera == null) return;
            if (_fogOverlayRoot != null) Destroy(_fogOverlayRoot);
            _fogOverlayRoot = new GameObject("TrainingFogOverlay");
            _fogOverlayRoot.transform.SetParent(transform, false);

            var heightRange = ResolveHeightRange(_episode.GeneratedWorld);
            var groups = new Dictionary<string, MeshBuildData>();
            AddFogOwner(groups, TrainingGameplayScope.LearnerId, "learner", _episode.GeneratedWorld, heightRange);
            AddFogOwner(groups, TrainingGameplayScope.OpponentId, "opponent", _episode.GeneratedWorld, heightRange);
            foreach (var group in groups)
            {
                if (group.Value.Vertices.Count == 0) continue;
                var go = new GameObject(group.Key);
                go.transform.SetParent(_fogOverlayRoot.transform, false);
                var mesh = new Mesh { name = "Training fog " + group.Key, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.SetVertices(group.Value.Vertices);
                mesh.SetTriangles(group.Value.Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Unlit/Color");
                if (shader == null) throw new System.InvalidOperationException("Training presentation shader is unavailable.");
                var material = new Material(shader);
                ApplyMaterialColor(material, ResolveFogColor(group.Key));
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
        }

        private void AddFogOwner(Dictionary<string, MeshBuildData> groups, string owner, string key,
            MenuWorldPreviewData worldData, Vector2 heightRange)
        {
            if (_episode?.Fog == null) return;
            for (int y = 0; y < worldData.Height; y++)
                for (int x = 0; x < worldData.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    bool visible = _episode.Fog.IsVisible(owner, cell);
                    bool explored = visible || _episode.Fog.IsExplored(owner, cell);
                    if (!explored) continue;
                    string groupKey = key + (visible ? "#visible" : "#explored");
                    if (!groups.TryGetValue(groupKey, out var meshData)) groups[groupKey] = meshData = new MeshBuildData();
                    AddFogQuad(meshData, _episode.Projection, worldData, heightRange, cell);
                }
        }

        private static void AddFogQuad(MeshBuildData meshData, IGridProjection projection, MenuWorldPreviewData worldData,
            Vector2 heightRange, Vector2Int cell)
        {
            Vector3 origin = projection.GridToWorld(Vector2Int.zero);
            Vector3 u = projection.GridToWorld(Vector2Int.right) - origin;
            Vector3 v = projection.GridToWorld(Vector2Int.up) - origin;
            Vector3 normal = Vector3.Cross(v, u).normalized;
            if (normal.y < 0f) normal = -normal;
            string tile = TryGetTile(worldData, cell) ?? "empty";
            var center = projection.GridToWorld(cell) + normal * (TileSurfaceHeight(worldData, cell, tile, heightRange) + 0.035f);
            Vector3 ux = u * 0.485f;
            Vector3 vz = v * 0.485f;
            meshData.AddQuad(center - ux - vz, center + ux - vz, center + ux + vz, center - ux + vz);
        }

        private static Color ResolveFogColor(string key)
        {
            bool learner = key.StartsWith("learner");
            bool visible = key.Contains("#visible");
            if (learner) return visible ? new Color(0.1f, 0.55f, 1f, 0.26f) : new Color(0.1f, 0.35f, 0.8f, 0.12f);
            return visible ? new Color(1f, 0.18f, 0.12f, 0.22f) : new Color(0.8f, 0.12f, 0.08f, 0.10f);
        }

        private void CreateResourceMarkers(IGridProjection projection, MenuWorldPreviewData worldData, Vector2 heightRange)
        {
            if (worldData?.BiomeMap == null) return;
            var root = new GameObject("TrainingResourceMarkers");
            root.transform.SetParent(transform, false);
            int stride = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(Mathf.Max(1, worldData.Width * worldData.Height)) / 12f));
            for (int y = 0; y < worldData.Height; y += stride)
                for (int x = 0; x < worldData.Width; x += stride)
                {
                    var cell = new Vector2Int(x, y);
                    string tile = worldData.BiomeMap[x, y] ?? string.Empty;
                    string key = tile.ToLowerInvariant();
                    if (IsWater(key)) continue;
                    if (key.Contains("forest") || key.Contains("wood"))
                        AddMarker(root.transform, PrimitiveType.Cylinder, projection, worldData, heightRange, cell,
                            new Vector3(0.22f, 0.65f, 0.22f), new Color(0.08f, 0.28f, 0.08f));
                    else if (key.Contains("mountain") || key.Contains("hill") || key.Contains("rock"))
                        AddMarker(root.transform, PrimitiveType.Cube, projection, worldData, heightRange, cell,
                            new Vector3(0.35f, 0.28f, 0.35f), new Color(0.34f, 0.34f, 0.32f));
                    else if (key.Contains("grass") || key.Contains("lowland"))
                        AddMarker(root.transform, PrimitiveType.Sphere, projection, worldData, heightRange, cell,
                            new Vector3(0.18f, 0.08f, 0.18f), new Color(0.42f, 0.62f, 0.22f));
                    if (IsCoastalLand(worldData, cell))
                        AddMarker(root.transform, PrimitiveType.Cube, projection, worldData, heightRange, cell,
                            new Vector3(0.42f, 0.05f, 0.12f), new Color(0.36f, 0.20f, 0.09f));
                }
        }

        private void AddMarker(Transform parent, PrimitiveType primitive, IGridProjection projection,
            MenuWorldPreviewData worldData, Vector2 heightRange, Vector2Int cell, Vector3 scale, Color color)
        {
            var marker = GameObject.CreatePrimitive(primitive);
            marker.name = "resource:" + cell.x + ":" + cell.y;
            marker.transform.SetParent(parent, false);
            marker.transform.position = projection.GridToWorld(cell)
                + Vector3.up * (TileSurfaceHeight(worldData, cell, TryGetTile(worldData, cell), heightRange) + scale.y * 0.5f + 0.03f);
            marker.transform.localScale = scale;
            var collider = marker.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = marker.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Unlit/Color"));
            ApplyMaterialColor(material, color);
            renderer.sharedMaterial = material;
            _assets.Add(material);
        }

        private static bool IsCoastalLand(MenuWorldPreviewData data, Vector2Int cell)
        {
            if (data?.BiomeMap == null || IsWater(TryGetTile(data, cell))) return false;
            for (int i = 0; i < 4; i++)
            {
                var next = cell + (i == 0 ? Vector2Int.right : i == 1 ? Vector2Int.left : i == 2 ? Vector2Int.up : Vector2Int.down);
                if (next.x >= 0 && next.y >= 0 && next.x < data.Width && next.y < data.Height
                    && IsWater(data.BiomeMap[next.x, next.y]))
                    return true;
            }
            return false;
        }

        private static void AddTileColumn(MeshBuildData meshData, IGridProjection projection, MenuWorldPreviewData worldData,
            Vector2 heightRange, Vector2Int cell, string tile, Vector3 u, Vector3 v, Vector3 normal)
        {
            float top = TileSurfaceHeight(worldData, cell, tile, heightRange);
            bool water = IsWater(tile);
            float baseHeight = water ? -0.16f : -0.08f;
            var center = projection.GridToWorld(cell) + normal * top;
            Vector3 ux = u * 0.49f;
            Vector3 vz = v * 0.49f;
            Vector3 p0 = center - ux - vz;
            Vector3 p1 = center + ux - vz;
            Vector3 p2 = center + ux + vz;
            Vector3 p3 = center - ux + vz;
            Vector3 drop = normal * (top - baseHeight);
            Vector3 b0 = p0 - drop;
            Vector3 b1 = p1 - drop;
            Vector3 b2 = p2 - drop;
            Vector3 b3 = p3 - drop;

            meshData.AddQuad(p0, p1, p2, p3);
            if (!water || top > baseHeight + 0.05f)
            {
                meshData.AddQuad(b1, b0, p0, p1);
                meshData.AddQuad(b2, b1, p1, p2);
                meshData.AddQuad(b3, b2, p2, p3);
                meshData.AddQuad(b0, b3, p3, p0);
            }
        }

        private static Vector2 ResolveHeightRange(MenuWorldPreviewData data)
        {
            if (data?.HeightMap == null) return new Vector2(0f, 1f);
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            for (int y = 0; y < data.HeightMap.GetLength(1); y++)
                for (int x = 0; x < data.HeightMap.GetLength(0); x++)
                {
                    float value = data.HeightMap[x, y];
                    if (float.IsNaN(value) || float.IsInfinity(value)) continue;
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                }
            return !float.IsNaN(min) && !float.IsInfinity(min) && max > min
                ? new Vector2(min, max)
                : new Vector2(0f, 1f);
        }

        private static float TileSurfaceHeight(MenuWorldPreviewData data, Vector2Int cell)
            => TileSurfaceHeight(data, cell, TryGetTile(data, cell), ResolveHeightRange(data));

        private static float TileSurfaceHeight(MenuWorldPreviewData data, Vector2Int cell, string tile, Vector2 range)
        {
            float normalized = TileHeight01(data, cell, range);
            if (IsWater(tile)) return -0.05f;
            float height = 0.08f + normalized * 1.35f;
            if (IsMountain(tile) || normalized > 0.78f) height += 0.8f;
            return height;
        }

        private static float TileHeight01(MenuWorldPreviewData data, Vector2Int cell, Vector2 range)
        {
            if (data?.HeightMap == null
                || cell.x < 0 || cell.y < 0
                || cell.x >= data.HeightMap.GetLength(0)
                || cell.y >= data.HeightMap.GetLength(1))
                return 0.35f;

            float raw = data.HeightMap[cell.x, cell.y];
            return Mathf.Approximately(range.x, range.y)
                ? Mathf.Clamp01(raw)
                : Mathf.Clamp01((raw - range.x) / (range.y - range.x));
        }

        private static string TryGetTile(MenuWorldPreviewData data, Vector2Int cell)
        {
            if (data?.BiomeMap == null
                || cell.x < 0 || cell.y < 0
                || cell.x >= data.BiomeMap.GetLength(0)
                || cell.y >= data.BiomeMap.GetLength(1))
                return null;
            return data.BiomeMap[cell.x, cell.y];
        }

        private static bool IsWater(string tile)
        {
            string key = (tile ?? string.Empty).ToLowerInvariant();
            return key.Contains("water") || key.Contains("ocean") || key.Contains("river") || key.Contains("lake");
        }

        private static bool IsMountain(string tile)
        {
            string key = (tile ?? string.Empty).ToLowerInvariant();
            return key.Contains("mountain") || key.Contains("rock") || key.Contains("cliff");
        }

        private static Color ResolveTileColor(string tile)
        {
            string key = (tile ?? string.Empty).ToLowerInvariant();
            if (IsWater(key)) return new Color(0.08f, 0.32f, 0.66f);
            if (IsMountain(key) || key.Contains("#high")) return new Color(0.48f, 0.48f, 0.46f);
            if (key.Contains("snow")) return new Color(0.86f, 0.88f, 0.84f);
            if (key.Contains("sand") || key.Contains("shore") || key.Contains("beach")) return new Color(0.76f, 0.66f, 0.43f);
            if (key.Contains("forest") || key.Contains("wood")) return new Color(0.16f, 0.38f, 0.18f);
            if (key.Contains("swamp") || key.Contains("marsh")) return new Color(0.22f, 0.34f, 0.22f);
            if (key.Contains("field") || key.Contains("grass") || key.Contains("plain")) return new Color(0.34f, 0.56f, 0.24f);
            return new Color(0.30f, 0.48f, 0.26f);
        }

        private static string ResolveVisualGroupKey(MenuWorldPreviewData data, Vector2 heightRange, Vector2Int cell, string tile)
        {
            if (IsWater(tile)) return tile + "#water";
            float height = TileHeight01(data, cell, heightRange);
            if (IsMountain(tile) || height > 0.78f) return tile + "#high";
            if (height < 0.25f) return tile + "#low";
            return tile + "#mid";
        }

        private sealed class MeshBuildData
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();

            public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int i = Vertices.Count;
                Vertices.Add(a);
                Vertices.Add(b);
                Vertices.Add(c);
                Vertices.Add(d);
                Triangles.Add(i);
                Triangles.Add(i + 2);
                Triangles.Add(i + 1);
                Triangles.Add(i);
                Triangles.Add(i + 3);
                Triangles.Add(i + 2);
            }
        }

        private void OnDestroy() { foreach (var asset in _assets) if (asset != null) Destroy(asset); }
    }
}
