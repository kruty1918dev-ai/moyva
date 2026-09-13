using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    // Meshes observe the actual grid. They contain no colliders or gameplay state.
    public sealed class TrainingWorldPresentation : MonoBehaviour
    {
        private readonly List<Object> _assets = new List<Object>();
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
            var lightObject = new GameObject("TrainingLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = camera.transform.rotation;
            lightObject.AddComponent<Light>().type = LightType.Directional;
        }
        private void OnDestroy() { foreach (var asset in _assets) if (asset != null) Destroy(asset); }
    }
}
