using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallMaterialService : ITileWorldCreatorTerrainSideWallMaterialService
    {
        private const string LogTag = "[MoyvaTWCHeight:SideWalls]";
        private const string ArtifactLogTag = "[MoyvaTWCHeight:SideWallArtifact]";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public Material Resolve(TileWorldCreatorTerrainSideWallState state, Material materialOverride, Color wallColor)
        {
            if (materialOverride != null)
                return materialOverride;

            if (state.RuntimeMaterial == null)
                state.RuntimeMaterial = CreateRuntimeMaterial();

            ApplyColor(state.RuntimeMaterial, wallColor);
            if (state.RuntimeMaterial != null && state.RuntimeMaterial.HasProperty("_Cull"))
            {
                state.RuntimeMaterial.SetFloat("_Cull", (float)CullMode.Back);
                Debug.Log($"{ArtifactLogTag} Runtime side-wall material uses back-face culling for one-sided wall quads. material='{state.RuntimeMaterial.name}', materialCull={TileWorldCreatorTerrainSideWallFormat.FormatMaterialCull(state.RuntimeMaterial)}.");
            }

            return state.RuntimeMaterial;
        }

        private static Material CreateRuntimeMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Hidden/InternalErrorShader");

            if (shader == null)
            {
                Debug.LogWarning($"{LogTag} Could not find a shader for generated side-wall material.");
                return null;
            }

            return new Material(shader)
            {
                name = "Moyva TWC Terrain Side Wall Runtime Material",
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
            };
        }

        private static void ApplyColor(Material material, Color color)
        {
            if (material == null)
                return;

            if (color.a <= 0.001f)
                color.a = 1f;
            if (material.HasProperty(BaseColorId))
                material.SetColor(BaseColorId, color);
            if (material.HasProperty(ColorId))
                material.SetColor(ColorId, color);
        }
    }
}
