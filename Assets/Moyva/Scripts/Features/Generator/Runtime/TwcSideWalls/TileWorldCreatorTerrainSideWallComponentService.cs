using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallComponentService : ITileWorldCreatorTerrainSideWallComponentService
    {
        private readonly ITileWorldCreatorTerrainSideWallMaterialService _materials;

        public TileWorldCreatorTerrainSideWallComponentService(ITileWorldCreatorTerrainSideWallMaterialService materials)
        {
            _materials = materials;
        }

        public void Ensure(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            TileWorldCreatorTerrainSideWallConfig config)
        {
            Transform root = config.TargetRoot != null ? config.TargetRoot : owner.transform.parent;
            if (root != null && owner.transform.parent != root)
                owner.transform.SetParent(root, false);

            ResetTransform(owner.transform);
            EnsureMeshComponents(state, owner);
            state.MeshRenderer.sharedMaterial = _materials.Resolve(state, config.MaterialOverride, config.WallColor);
            state.MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
            state.MeshRenderer.receiveShadows = true;
        }

        private static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static void EnsureMeshComponents(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner)
        {
            if (state.MeshFilter == null)
            {
                if (!owner.TryGetComponent(out MeshFilter meshFilter))
                    meshFilter = owner.gameObject.AddComponent<MeshFilter>();
                state.MeshFilter = meshFilter;
            }

            if (state.MeshRenderer == null)
            {
                if (!owner.TryGetComponent(out MeshRenderer meshRenderer))
                    meshRenderer = owner.gameObject.AddComponent<MeshRenderer>();
                state.MeshRenderer = meshRenderer;
            }

            if (state.Mesh != null)
                return;

            state.Mesh = new Mesh
            {
                name = "Moyva TWC Terrain Side Walls",
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
            };
            state.MeshFilter.sharedMesh = state.Mesh;
        }
    }
}
