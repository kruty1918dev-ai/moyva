using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Editor
{
    public static class TrainingChecks
    {
        [MenuItem("Moyva/Training/Run pure tests %&t")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                Debug.LogWarning("Run training checks in idle Edit Mode.");
                return;
            }
            Directory.CreateDirectory("Temp/ai");
            var runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results(runner));
            runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "Kruty1918.Moyva.AI.Training.Tests" }
            }));
        }

        private sealed class Results : ICallbacks
        {
            private readonly TestRunnerApi _runner;
            public Results(TestRunnerApi runner) { _runner = runner; }
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, "Temp/ai/training-editmode.xml");
                Debug.Log($"Training checks: {result.PassCount} passed, {result.FailCount} failed.");
                Object.DestroyImmediate(_runner);
            }
        }
    }
}
