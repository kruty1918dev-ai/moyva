#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
[InitializeOnLoad]
public static class StartupBarrierVerification
{
    static TestRunnerApi api;
    static StartupBarrierVerification() { EditorApplication.update += Update; }
    static void Update()
    {
        const string request = "Temp/ai/barrier-tests.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(request);
        api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new Results());
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "Kruty1918.Moyva.Startup.Tests" } }));
    }
    sealed class Results : ICallbacks
    {
        public void RunStarted(ITestAdaptor t) { }
        public void TestStarted(ITestAdaptor t) { }
        public void TestFinished(ITestResultAdaptor r) { }
        public void RunFinished(ITestResultAdaptor r)
        {
            TestRunnerApi.SaveResultToFile(r, "Temp/ai/barrier-tests.xml");
            File.WriteAllText("Temp/ai/barrier-tests.summary", $"{r.ResultState}: passed={r.PassCount}, failed={r.FailCount}, skipped={r.SkipCount}");
        }
    }
}
#endif