using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>Bakes procedural undergrowth into one opaque-cutout card.
    /// Palette colours and directional shading preserve the source volume.</summary>
    internal static class MoyvaVegetationBillboardBaker
    {
        public static Material Bake(Mesh mesh, Material source, string textures, string materials)
        {
            var shader = Shader.Find("Hidden/Moyva/Vegetation Billboard Bake");
            if (shader == null)
                throw new System.InvalidOperationException("Vegetation billboard bake shader is missing.");
            var bake = new Material(shader);
            bake.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
            var rotation = Quaternion.Euler(15f, 0f, 0f);
            var verts = mesh.vertices;
            var bounds = new Bounds(rotation * verts[0], Vector3.zero);
            foreach (var v in verts) bounds.Encapsulate(rotation * v);
            float width = Mathf.Max(0.05f, bounds.size.x) * 1.04f;
            float height = Mathf.Max(0.05f, bounds.size.y) * 1.04f;
            var target = RenderTexture.GetTemporary(512, 512, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            string texturePath = textures + "/" + mesh.name + "_billboard.png";
            try
            {
                using (var commands = new CommandBuffer())
                {
                    commands.SetRenderTarget(target);
                    commands.ClearRenderTarget(true, true, Color.clear);
                    var eye = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z - 3f);
                    var view = Matrix4x4.Scale(new Vector3(1, 1, -1))
                        * Matrix4x4.TRS(eye, Quaternion.identity, Vector3.one).inverse;
                    var projection = Matrix4x4.Ortho(-width / 2, width / 2, -height / 2, height / 2, 0.01f, 10f);
                    // ReadPixels uses a bottom-left origin. No camera target
                    // flip here: the saved PNG must keep the roots at v=0.
                    commands.SetViewProjectionMatrices(view, GL.GetGPUProjectionMatrix(projection, false));
                    commands.DrawMesh(mesh, Matrix4x4.Rotate(rotation), bake, 0, 0);
                    Graphics.ExecuteCommandBuffer(commands);
                }
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
                texture.Apply();
                File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(bake);
            }
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.mipMapsPreserveCoverage = true;
            importer.alphaTestReferenceValue = 0.35f;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            string materialPath = materials + "/" + mesh.name + "_billboard.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, materialPath); }
            else material.CopyPropertiesFromMaterial(source);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_AlphaClipEnabled", 1f);
            material.SetFloat("_BillboardEnabled", 1f);
            material.SetFloat("_Cull", 0f);
            EditorUtility.SetDirty(material);

            // Keep the existing mesh asset and GUID referenced by registries.
            mesh.Clear();
            mesh.vertices = new[] { new Vector3(-width / 2, 0, 0), new Vector3(-width / 2, height, 0),
                new Vector3(width / 2, height, 0), new Vector3(width / 2, 0, 0) };
            mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return material;
        }
    }
}
