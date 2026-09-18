using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.HomeMenu.PlayMode
{
    /// <summary>Minimal smoke probes used to isolate PlayMode harness issues.</summary>
    public sealed class HomeMenuMiniSmokeTests : HomeMenuSmokeFixture
    {
        [UnityTest]
        public IEnumerator MiniCaptureMainMenu()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);
            yield return Capture("mini-main");
            AssertNoFatalLogs();
        }
    }
}
