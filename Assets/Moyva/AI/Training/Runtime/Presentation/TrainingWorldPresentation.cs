using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    // Meshes observe the actual grid. They contain no colliders or gameplay state.
    public sealed class TrainingWorldPresentation : MonoBehaviour
    {
        private readonly List<Object> _assets = new List<Object>();
        private GameplayTrainingEpisode _episode;
        private UnityEngine.Camera _camera;
        private float _fitSize;
        private Vector3 _fitPosition;
        private float _nextRefresh;
        private readonly Dictionary<Vector2Int, GameObject> _buildings = new Dictionary<Vector2Int, GameObject>();
        internal void Observe(GameplayTrainingEpisode episode) { _episode = episode; RefreshBuildings(); }
        public void Zoom(float factor)
        {
            if (_camera == null) return;
            _camera.orthographicSize = factor == 0 ? _fitSize : Mathf.Clamp(_camera.orthographicSize * factor, 2, _fitSize * 2);
            if (factor == 0) _camera.transform.position = _fitPosition;
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
                    + Vector3.up * (definition?.ResolveVisualYOffset() ?? 0);
                building.transform.rotation = Quaternion.Euler(0, (int)placement.Rotation * 90, 0);
                if (definition?.Prefab != null) CopyRenderTree(definition.Prefab.transform, building.transform);
                _buildings.Add(placement.Position, building);
            }
        }
        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.25f;
            RefreshBuildings();
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
            var input = Event.current;
            if (input.type == EventType.ScrollWheel && input.mousePosition.y > 320)
            { Zoom(Mathf.Pow(1.1f, input.delta.y)); input.Use(); }
            if (input.type == EventType.KeyDown)
            {
                Vector3 direction = input.keyCode == KeyCode.LeftArrow ? -_camera.transform.right
                    : input.keyCode == KeyCode.RightArrow ? _camera.transform.right
                    : input.keyCode == KeyCode.UpArrow ? _camera.transform.up
                    : input.keyCode == KeyCode.DownArrow ? -_camera.transform.up : Vector3.zero;
                _camera.transform.position += direction * _camera.orthographicSize * 0.1f;
            }
            foreach (var id in _episode.Units.GetAllUnitIds())
                if (_episode.Units.TryGetUnitPosition(id, out var cell))
                    Label(_episode.Projection.GridToWorld(cell), _episode.Units.GetUnitTypeId(id),
                        _episode.UnitOwners.GetUnitOwnerId(id) == TrainingGameplayScope.LearnerId);
            if (_episode.EconomyInstalled)
                foreach (var placement in _episode.Placements.GetSavedPlacements())
                    Label(_episode.Projection.GridToWorld(placement.Position),
                        _episode.Buildings.GetById(placement.BuildingId)?.DisplayName ?? placement.BuildingId,
                        placement.OwnerId == TrainingGameplayScope.LearnerId);
        }
        public void Build(IGridService grid, IGridProjection projection)
        {
            Vector3 origin = projection.GridToWorld(Vector2Int.zero);
            Vector3 u = projection.GridToWorld(Vector2Int.right) - origin;
            Vector3 v = projection.GridToWorld(Vector2Int.up) - origin;
            Vector3 normal = Vector3.Cross(v, u).normalized;
            var groups = new Dictionary<string, List<Vector3>>();
            for (int y = 0; y < grid.GridHeight; y++)
                for (int x = 0; x < grid.GridWidth; x++)
                {
                    var cell = new Vector2Int(x, y);
                    string tile = grid.GetTileData(cell) ?? "empty";
                    if (!groups.TryGetValue(tile, out var vertices)) groups[tile] = vertices = new List<Vector3>();
                    var center = projection.GridToWorld(cell) - normal * 0.02f;
                    vertices.Add(center - u * 0.49f - v * 0.49f);
                    vertices.Add(center + u * 0.49f - v * 0.49f);
                    vertices.Add(center + u * 0.49f + v * 0.49f);
                    vertices.Add(center - u * 0.49f + v * 0.49f);
                }
            foreach (var group in groups)
            {
                var go = new GameObject(group.Key);
                go.transform.SetParent(transform, false);
                var mesh = new Mesh { name = "Training grid " + group.Key, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.SetVertices(group.Value);
                var triangles = new int[group.Value.Count / 4 * 6];
                for (int i = 0, t = 0; i < group.Value.Count; i += 4)
                { triangles[t++] = i; triangles[t++] = i + 2; triangles[t++] = i + 1; triangles[t++] = i; triangles[t++] = i + 3; triangles[t++] = i + 2; }
                mesh.triangles = triangles;
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                if (shader == null) throw new System.InvalidOperationException("Training presentation shader is unavailable.");
                var material = new Material(shader);
                string tile = group.Key.ToLowerInvariant();
                var color = tile.Contains("water") || tile.Contains("ocean") ? new Color(0.15f, 0.35f, 0.65f)
                    : tile.Contains("mountain") || tile.Contains("rock") ? Color.gray : new Color(0.3f, 0.5f, 0.25f);
                material.SetColor("_BaseColor", color); material.color = color;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
                _assets.Add(mesh); _assets.Add(material);
            }
            var cameraObject = new GameObject("TrainingCamera");
            cameraObject.transform.SetParent(transform, false);
            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(grid.GridHeight * v.magnitude, grid.GridWidth * u.magnitude) * 0.6f;
            var centerPoint = projection.GridToWorld(new Vector2Int(grid.GridWidth / 2, grid.GridHeight / 2));
            camera.transform.position = centerPoint + normal * 100;
            camera.transform.rotation = Quaternion.LookRotation(-normal, v.normalized);
            camera.farClipPlane = 300;
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.16f);
            _camera = camera; _fitSize = camera.orthographicSize; _fitPosition = camera.transform.position;
            var lightObject = new GameObject("TrainingLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = camera.transform.rotation;
            lightObject.AddComponent<Light>().type = LightType.Directional;
        }
        private void OnDestroy() { foreach (var asset in _assets) if (asset != null) Destroy(asset); }
    }
}
