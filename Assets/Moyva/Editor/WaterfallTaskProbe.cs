using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;

[InitializeOnLoad]
public static class WaterfallTaskProbe
{
    static WaterfallTaskProbe() { EditorApplication.update += Tick; }
    static void Tick()
    {
        const string path = "Temp/ai/waterfall-probe.request";
        if (!File.Exists(path) || EditorApplication.isCompiling) return;
        string command = File.ReadAllText(path).Trim();
        if (command == "capture" && !EditorApplication.isPlaying) return;
        if (command == "tests" && EditorApplication.isPlaying) return;
        File.Delete(path);
        if (command == "capture") { Capture(); return; }
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new Results());
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode,
            testNames = new[] { "Kruty1918.Moyva.Generator.Tests.Runtime.WaterfallFieldPlannerTests",
                "Kruty1918.Moyva.Generator.Tests.Runtime.G18AcceptanceTests",
                "Kruty1918.Moyva.Generator.Tests.Runtime.G19AcceptanceTests" } }));
    }
    static void Capture()
    {
        var emitters = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        var lip = emitters.FirstOrDefault(p => p.name.StartsWith("wfall_edge_"));
        if (lip == null) return;
        var target = lip.transform.position + Vector3.down * 0.3f;
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(r => r.bounds.SqrDistance(target) < 400f).ToArray();
        var states = renderers.Select(r => (r.enabled, r.forceRenderingOff)).ToArray();
        var inactive = renderers.SelectMany(r => r.GetComponentsInParent<Transform>(true))
            .Where(t => !t.gameObject.activeSelf).Distinct().ToArray();
        var go = new GameObject("Waterfall task camera");
        var camera = go.AddComponent<Camera>();
        camera.CopyFrom(Camera.main);
        camera.enabled = false;
        camera.cameraType = CameraType.Preview;
        var rt = new RenderTexture(1200, 900, 24);
        var texture = new Texture2D(1200, 900, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            foreach (var t in inactive) t.gameObject.SetActive(true);
            foreach (var r in renderers) { r.enabled = true; r.forceRenderingOff = false; }
            go.transform.position = target + lip.transform.forward * 4f + lip.transform.right * 3f + Vector3.up * 2.5f;
            go.transform.LookAt(target);
            camera.orthographic = false;
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.05f;
            camera.cullingMask = -1;
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, 1200, 900), 0, 0);
            texture.Apply();
            File.WriteAllBytes("Temp/ai/waterfall-blending.png", texture.EncodeToPNG());
            File.WriteAllText("Temp/ai/waterfall-vfx.txt", string.Join("\n", emitters.Where(p => p.name.StartsWith("wfall_"))
                .Select(p => p.name + " count=" + p.particleCount + " worldSize=" + p.main.startSize.constantMax * p.transform.lossyScale.x)));
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(texture); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
            for (int i = 0; i < renderers.Length; i++) { renderers[i].enabled = states[i].enabled; renderers[i].forceRenderingOff = states[i].forceRenderingOff; }
            foreach (var t in inactive) t.gameObject.SetActive(false);
        }
    }
    class Results : ICallbacks
    {
        public void RunStarted(ITestAdaptor t) { }
        public void TestStarted(ITestAdaptor t) { }
        public void TestFinished(ITestResultAdaptor t) { }
        public void RunFinished(ITestResultAdaptor t) { TestRunnerApi.SaveResultToFile(t, "Temp/ai/waterfall-tests.xml"); }
    }
}
