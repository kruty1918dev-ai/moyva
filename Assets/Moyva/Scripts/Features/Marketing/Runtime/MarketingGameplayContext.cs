using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Read-only access point into the live gameplay scene. The studio never
    /// re-binds gameplay services; it resolves canonical APIs from the
    /// gameplay SceneContext container that already exists.
    /// </summary>
    public sealed class MarketingGameplayContext
    {
        public Scene GameplayScene { get; private set; }
        public DiContainer Container { get; private set; }

        public bool IsReady => Container != null;

        public static MarketingGameplayContext Attach(Scene gameplayScene)
        {
            var ctx = new MarketingGameplayContext { GameplayScene = gameplayScene };
            foreach (var root in gameplayScene.GetRootGameObjects())
            {
                var sc = root.GetComponentInChildren<SceneContext>(true);
                if (sc != null)
                {
                    ctx.Container = sc.Container;
                    break;
                }
            }
            if (ctx.Container == null)
            {
                // SceneContext may live on a non-root object.
                foreach (var sc in Object.FindObjectsByType<SceneContext>())
                {
                    if (sc.gameObject.scene == gameplayScene)
                    {
                        ctx.Container = sc.Container;
                        break;
                    }
                }
            }
            return ctx;
        }

        public T TryResolve<T>() where T : class
            => Container != null && Container.HasBinding(typeof(T)) ? Container.Resolve<T>() : null;

        public IUnitService Units => TryResolve<IUnitService>();
        public IUnitFactory UnitFactory => TryResolve<IUnitFactory>();
        public IGridService Grid => TryResolve<IGridService>();
        public IGridProjection GridProjection => TryResolve<IGridProjection>();
        public Construction.API.IConstructionService Construction => TryResolve<Construction.API.IConstructionService>();
        public IAudioService Audio => TryResolve<IAudioService>();
        public IMusicService Music => TryResolve<IMusicService>();
        public FogOfWar.API.IFogOfWarService Fog => TryResolve<FogOfWar.API.IFogOfWarService>();

        /// <summary>Reveals the whole map through the canonical fog vision
        /// registry so marketing captures are not obscured by unexplored fog,
        /// then forces a full fog-visual rebuild so the change is visible on
        /// the next rendered frame. Returns false when no fog service is bound
        /// (fog disabled).</summary>
        public bool RevealMapForCapture()
        {
            var fog = Fog;
            var grid = Grid;
            if (fog == null || grid == null || grid.GridWidth <= 0) return false;
            int radius = Mathf.CeilToInt(Mathf.Max(grid.GridWidth, grid.GridHeight) * 0.5f) + 2;
            var center = new Vector2Int(grid.GridWidth / 2, grid.GridHeight / 2);
            fog.RevealArea(center, radius, FogOfWar.API.FogRevealShape.Square,
                keepVisible: true, visibleAreaId: "marketing-capture");
            TryResolve<FogOfWar.API.IFogVisualUpdater>()?.RebuildFullVisual(fog);
            Debug.Log($"[MarketingStudio] Fog reveal: center={center} radius={radius} " +
                $"centerVisible={fog.IsVisible(center)} " +
                $"cornerVisible={fog.IsVisible(new Vector2Int(1, 1))}");
            return true;
        }
    }

    /// <summary>
    /// Discovers live subjects in the built world: units via IUnitService,
    /// buildings via scene-instance name matching against the content index,
    /// landmarks via a coarse grid tile scan.
    /// </summary>
    public static class MarketingWorldScanner
    {
        public static Contracts.WorldSubjects Scan(
            MarketingGameplayContext ctx,
            Contracts.ContentIndexSnapshot index,
            int landmarkSamples = 400)
        {
            var world = new Contracts.WorldSubjects();
            if (ctx == null || !ctx.IsReady) return world;

            var projection = ctx.GridProjection;
            var grid = ctx.Grid;

            // World bounds
            if (projection != null && grid != null && grid.GridWidth > 0)
            {
                Bounds b = projection.GetWorldBounds(grid.GridWidth, grid.GridHeight);
                world.worldCenterX = b.center.x;
                world.worldCenterY = b.center.y;
                world.worldCenterZ = b.center.z;
                world.worldRadius = Mathf.Max(4f, Mathf.Max(b.extents.x, b.extents.z));
            }

            // Units (canonical service)
            var units = ctx.Units;
            if (units != null)
            {
                foreach (var id in units.GetAllUnitIds())
                {
                    var go = units.GetUnitObject(id);
                    if (go == null) continue;
                    var p = go.transform.position;
                    world.units.Add(new Contracts.ShotSubject
                    {
                        contentId = SafeTypeId(units, id),
                        instanceId = id,
                        worldX = p.x, worldY = p.y, worldZ = p.z,
                        approximateRadius = BoundsRadius(go, 1.2f),
                    });
                }
            }

            // Buildings: match instantiated scene objects to index entries.
            if (index != null && ctx.GameplayScene.IsValid())
            {
                var wanted = new Dictionary<string, Contracts.ContentIndexEntry>();
                foreach (var e in index.entries)
                {
                    if (e.category != Contracts.MarketingContentCategory.Building) continue;
                    string prefab = System.IO.Path.GetFileNameWithoutExtension(e.editorPath);
                    if (!string.IsNullOrEmpty(prefab) && !wanted.ContainsKey(prefab))
                        wanted[prefab] = e;
                }
                var seen = new HashSet<string>();
                foreach (var root in ctx.GameplayScene.GetRootGameObjects())
                    CollectBuildings(root.transform, wanted, world, seen, projection);
            }

            // Landmarks: coarse tile scan for interesting terrain.
            if (grid != null && projection != null && grid.GridWidth > 0 && grid.GridHeight > 0)
            {
                ScanLandmarks(grid, projection, world, landmarkSamples);
            }

            return world;
        }

        private static void CollectBuildings(
            Transform t,
            Dictionary<string, Contracts.ContentIndexEntry> wanted,
            Contracts.WorldSubjects world,
            HashSet<string> seen,
            IGridProjection projection)
        {
            string name = t.name;
            const string cloneSuffix = "(Clone)";
            if (name.EndsWith(cloneSuffix))
                name = name.Substring(0, name.Length - cloneSuffix.Length).TrimEnd();

            if (wanted.TryGetValue(name, out var entry))
            {
                string key = entry.id + "@" + t.position.x.ToString("F1") + "," + t.position.z.ToString("F1");
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    world.buildings.Add(new Contracts.ShotSubject
                    {
                        contentId = entry.id,
                        instanceId = t.GetEntityId().ToString(),
                        worldX = t.position.x, worldY = t.position.y, worldZ = t.position.z,
                        approximateRadius = BoundsRadius(t.gameObject, 2.5f),
                    });
                }
            }
            foreach (Transform child in t)
                CollectBuildings(child, wanted, world, seen, projection);
        }

        private static void ScanLandmarks(
            IGridService grid, IGridProjection projection,
            Contracts.WorldSubjects world, int samples)
        {
            int w = grid.GridWidth, h = grid.GridHeight;
            int step = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(w * h / (float)samples)));
            var interesting = new List<Vector2Int>();
            for (int y = step / 2; y < h; y += step)
            for (int x = step / 2; x < w; x += step)
            {
                var cell = new Vector2Int(x, y);
                if (!grid.TryGetTileData(cell, out string tileId) || string.IsNullOrEmpty(tileId))
                    continue;
                string tl = tileId.ToLowerInvariant();
                if (tl.Contains("water") || tl.Contains("forest") || tl.Contains("hill")
                    || tl.Contains("mount") || tl.Contains("crest"))
                    interesting.Add(cell);
            }
            // Spread picks: greedy min-distance selection
            var picked = new List<Vector2Int>();
            foreach (var cell in interesting)
            {
                bool far = true;
                foreach (var p in picked)
                    if ((p - cell).sqrMagnitude < (step * 4) * (step * 4)) { far = false; break; }
                if (far) picked.Add(cell);
                if (picked.Count >= 12) break;
            }
            foreach (var cell in picked)
            {
                Vector3 pos = projection.GridToWorld(cell);
                world.landmarks.Add(new Contracts.ShotSubject
                {
                    contentId = "terrain",
                    instanceId = $"tile-{cell.x}-{cell.y}",
                    worldX = pos.x, worldY = pos.y, worldZ = pos.z,
                    approximateRadius = 4f,
                });
            }
        }

        private static string SafeTypeId(IUnitService units, string id)
        {
            try { return units.GetUnitTypeId(id) ?? "unit"; }
            catch { return "unit"; }
        }

        private static float BoundsRadius(GameObject go, float fallback)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return fallback;
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return Mathf.Max(0.5f, b.extents.magnitude);
        }
    }
}
