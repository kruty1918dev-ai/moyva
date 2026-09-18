using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.Tests.Performance.PlayMode
{
    /// <summary>
    /// End-to-end startup timings measured in a real PlayMode session:
    /// scene load + activation, installer bootstrap, menu idle frame cost.
    /// Writes Temp/ai/perf-playmode.json.
    /// </summary>
    public class MoyvaStartupPerformanceTests
    {
        private sealed class Row
        {
            public string Name;
            public int Samples;
            public double MinMs, MedianMs, MeanMs, MaxMs, P95Ms;
            public string Note = "", Error = "";
        }

        private static readonly List<Row> Rows = new List<Row>();

        private static Row RowFromTimes(string name, List<double> ms, string note = "")
        {
            var r = new Row { Name = name, Samples = ms.Count, Note = note };
            if (ms.Count == 0) return r;
            ms.Sort();
            r.MinMs = ms[0];
            r.MedianMs = ms[ms.Count / 2];
            r.MeanMs = ms.Average();
            r.MaxMs = ms[ms.Count - 1];
            r.P95Ms = ms[Mathf.Clamp((int)(ms.Count * 0.95f), 0, ms.Count - 1)];
            Rows.Add(r);
            return r;
        }

        private static IEnumerator TimedSceneLoad(string scenePath, string label)
        {
            var sw = Stopwatch.StartNew();
            var op = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            while (op != null && !op.isDone) yield return null;
            sw.Stop();
            RowFromTimes(label + " load+activate", new List<double> { sw.Elapsed.TotalMilliseconds });

            // warmup settle: startup installers fire in first frames
            var settle = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++) yield return null;
            settle.Stop();
            RowFromTimes(label + " first 10 frames (bootstrap settle)", new List<double> { settle.Elapsed.TotalMilliseconds });
        }

        private static IEnumerator SampleFrames(string label, int count)
        {
            var times = new List<double>(count);
            var sw = new Stopwatch();
            for (int i = 0; i < count; i++)
            {
                sw.Restart();
                yield return null;
                sw.Stop();
                times.Add(sw.Elapsed.TotalMilliseconds);
            }
            RowFromTimes(label, times);
        }

        [OneTimeTearDown]
        public void FlushAll() => Flush();

        [UnityTest]
        public IEnumerator HomeMenuScene_StartupAndIdleFrames()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return TimedSceneLoad("Assets/Moyva/Scenes/HomeMenu.unity", "HomeMenu scene");
            yield return SampleFrames("HomeMenu idle frame (menu+preview tick)", 120);
            Flush();
        }

        [UnityTest]
        public IEnumerator GameplayScene_StartupAndIdleFrames()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return TimedSceneLoad("Assets/Moyva/Scenes/Gamplay_Scene.unity", "Gameplay scene");
            yield return SampleFrames("Gameplay idle frame", 120);
            Flush();
        }

        private static void Flush()
        {
            var sb = new StringBuilder();
            sb.Append("{\"results\":[");
            for (int i = 0; i < Rows.Count; i++)
            {
                var r = Rows[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"name\":\"").Append(r.Name.Replace("\"", "'"))
                  .Append("\",\"samples\":").Append(r.Samples)
                  .Append(",\"medianMs\":").Append(F(r.MedianMs))
                  .Append(",\"meanMs\":").Append(F(r.MeanMs))
                  .Append(",\"minMs\":").Append(F(r.MinMs))
                  .Append(",\"maxMs\":").Append(F(r.MaxMs))
                  .Append(",\"p95Ms\":").Append(F(r.P95Ms))
                  .Append(",\"error\":\"").Append(r.Error.Replace("\"", "'")).Append("\"}");
            }
            sb.Append("]}");
            var path = Path.GetFullPath("Temp/ai/perf-playmode.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, sb.ToString());
            Debug.Log("[PerfPlayMode] " + sb.ToString());
        }

        private static string F(double v) => v.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
    }
}
