using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public static class OnnxImportProbe
    {
        private const string ModelPath =
            "Assets/Moyva/Presets/AI/Resources/AI/Models/MoyvaStrategy_Normal.onnx";

        public static void Run()
        {
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceUpdate);
            var asset = AssetDatabase.LoadAssetAtPath<ModelAsset>(ModelPath);
            Debug.Log($"[OnnxImportProbe] ModelAsset={(asset == null ? "null" : "loaded")}");
            if (asset == null) return;
            var model = ModelLoader.Load(asset);
            Debug.Log(model == null
                ? "[OnnxImportProbe] ModelLoader=null"
                : $"[OnnxImportProbe] ModelLoader inputs={model.inputs.Count} outputs={model.outputs.Count}");
        }
    }
}
