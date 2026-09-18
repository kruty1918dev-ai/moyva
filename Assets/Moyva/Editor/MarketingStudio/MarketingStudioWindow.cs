using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Marketing.Content;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using Kruty1918.Moyva.Marketing.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Moyva → Marketing Content Studio. Production authoring UI: pick a
    /// recipe/platform/output, generate, review the contact sheet.
    /// </summary>
    public sealed class MarketingStudioWindow : EditorWindow
    {
        private MarketingPresetStore _presets = new MarketingPresetStore();
        private Vector2 _scroll;
        private int _recipeIndex;
        private string _outputRoot = string.Empty;
        private bool _autoStart = true;
        private string _language = "en";
        private Texture2D _contactSheet;
        private string _lastRunFolder;
        private double _indexCheckAt;

        private static readonly (string label, string recipe)[] OneClick =
        {
            ("GENERATE STEAM SCREENSHOTS", "steam-screenshots"),
            ("GENERATE TRAILER (60s)", "trailer-60"),
            ("GENERATE TEASER (15s)", "teaser-15"),
            ("GENERATE SHORTS (30s, 9:16)", "shorts-30"),
            ("GENERATE SOCIAL PACK", "social-pack"),
            ("GENERATE CAMPAIGN PACK", "campaign-pack"),
        };

        [MenuItem("Moyva/Marketing Content Studio")]
        public static void Open()
        {
            var w = GetWindow<MarketingStudioWindow>(false, "Marketing Content Studio", true);
            w.minSize = new Vector2(420, 560);
            w.Show();
        }

        private void OnEnable()
        {
            _presets.LoadAll();
            EditorApplication.update += PollStatus;
        }

        private void OnDisable()
        {
            EditorApplication.update -= PollStatus;
        }

        private void PollStatus()
        {
            // Refresh index freshness hint every few seconds (cheap file check).
            if (EditorApplication.timeSinceStartup - _indexCheckAt > 5)
            {
                _indexCheckAt = EditorApplication.timeSinceStartup;
                Repaint();
            }
            var status = MarketingRunMonitor.LastStatus;
            if (status != null && status.done && status.runFolder != _lastRunFolder)
            {
                _lastRunFolder = status.runFolder;
                LoadContactSheet(status.runFolder);
                Repaint();
            }
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.Space(6);
            EditorGUILayout.LabelField("MOYVA MARKETING CONTENT STUDIO", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            DrawIndexSection();
            EditorGUILayout.Separator();
            DrawRecipeSection();
            EditorGUILayout.Separator();
            DrawRunSection();
            EditorGUILayout.Separator();
            DrawOneClick();
            EditorGUILayout.Separator();
            DrawReviewSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawIndexSection()
        {
            EditorGUILayout.LabelField("CONTENT INDEX", EditorStyles.boldLabel);
            var snap = _presets.LoadIndexSnapshot();
            if (snap == null)
            {
                EditorGUILayout.HelpBox("No content index yet — build it before capture.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"Entries: {snap.entries.Count}  |  Music: {snap.musicKeys.Count}  |  Sfx: {snap.sfxKeys.Count}");
                EditorGUILayout.LabelField($"Built: {snap.builtAtUtc}  FP: {snap.fingerprint}");
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild Content Index"))
            {
                var built = MarketingContentIndexBuilder.Build(saveSnapshot: true);
                _presets.LoadAll();
                Debug.Log($"[MarketingStudio] Index rebuilt: {built.entries.Count} entries");
            }
            if (GUILayout.Button("Check Freshness"))
            {
                bool stale = MarketingContentIndexBuilder.NeedsRebuild();
                Debug.Log(stale
                    ? "[MarketingStudio] Content index is OUTDATED → rebuild recommended."
                    : "[MarketingStudio] Content index is up to date.");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRecipeSection()
        {
            EditorGUILayout.LabelField("RECIPE", EditorStyles.boldLabel);
            if (_presets.Recipes.Count == 0)
            {
                EditorGUILayout.HelpBox("No recipes found under Presets/Marketing/recipes.", MessageType.Error);
                return;
            }
            var names = _presets.Recipes.ConvertAll(r =>
                $"{r.id}  ({r.contentType}, {r.platformProfileId}, {r.outputKind})");
            _recipeIndex = Mathf.Clamp(_recipeIndex, 0, _presets.Recipes.Count - 1);
            _recipeIndex = EditorGUILayout.Popup("Capture Recipe", _recipeIndex, names.ToArray());
            var recipe = _presets.Recipes[_recipeIndex];
            var platform = _presets.FindPlatform(recipe.platformProfileId);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Platform", platform?.id ?? "generic");
                EditorGUILayout.TextField("Resolution",
                    $"{(recipe.resolutionWidth > 0 ? recipe.resolutionWidth : platform?.resolutionWidth ?? 0)}×" +
                    $"{(recipe.resolutionHeight > 0 ? recipe.resolutionHeight : platform?.resolutionHeight ?? 0)}");
                if (recipe.outputKind >= MarketingOutputKind.Video)
                    EditorGUILayout.FloatField("Duration (s)", recipe.durationSec);
                else
                    EditorGUILayout.IntField("Shot count", recipe.shotCount);
                EditorGUILayout.TextField("UI", recipe.uiVisibility.ToString());
                EditorGUILayout.Toggle("Marketing text", recipe.includeMarketingText);
                EditorGUILayout.Toggle("Logo", recipe.includeLogo);
                EditorGUILayout.Toggle("Music", recipe.includeMusic);
            }
            _language = EditorGUILayout.TextField("Language", _language);
            _autoStart = EditorGUILayout.Toggle("Auto Start Capture", _autoStart);
            EditorGUILayout.BeginHorizontal();
            _outputRoot = EditorGUILayout.TextField("Output Root", _outputRoot);
            if (string.IsNullOrEmpty(_outputRoot))
                EditorGUILayout.LabelField("(default: <project>/MarketingOutput)", EditorStyles.miniLabel, GUILayout.Width(190));
            if (GUILayout.Button("…", GUILayout.Width(28)))
            {
                string picked = EditorUtility.OpenFolderPanel("Marketing output root", "", "");
                if (!string.IsNullOrEmpty(picked)) _outputRoot = picked;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRunSection()
        {
            var status = MarketingRunMonitor.LastStatus;
            bool busy = MarketingRunMonitor.RunPending || (status != null && !status.done && EditorApplication.isPlaying);
            using (new EditorGUI.DisabledScope(busy))
            {
                if (GUILayout.Button("GENERATE", GUILayout.Height(34)))
                    StartRun(_presets.Recipes[_recipeIndex]);
                if (GUILayout.Button("Open Studio Scene"))
                {
                    MarketingSceneBuilder.EnsureSceneExists();
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(MarketingSceneBuilder.ScenePath);
                }
            }
            using (new EditorGUI.DisabledScope(!busy))
            {
                var c = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.6f, 0.55f);
                if (GUILayout.Button("CANCEL GENERATION"))
                    MarketingRunMonitor.CancelRun();
                GUI.backgroundColor = c;
            }
            if (status != null)
            {
                EditorGUILayout.LabelField($"Phase: {status.phase}  {status.progress01:P0}  {status.message}");
                if (!string.IsNullOrEmpty(status.error))
                    EditorGUILayout.HelpBox(status.error, MessageType.Error);
            }
        }

        private void DrawOneClick()
        {
            EditorGUILayout.LabelField("ONE-CLICK FLOWS", EditorStyles.boldLabel);
            foreach (var (label, recipeId) in OneClick)
            {
                if (recipeId == "social-pack")
                {
                    if (GUILayout.Button(label))
                        StartCampaign(new[] { "shorts-30", "instagram-post", "hero-stills" });
                    continue;
                }
                if (recipeId == "campaign-pack")
                {
                    if (GUILayout.Button(label))
                        StartCampaign(new[]
                        {
                            "steam-screenshots", "gameplay-screenshots", "hero-stills",
                            "trailer-60", "trailer-30", "teaser-15", "shorts-30",
                            "youtube-thumbnail",
                        });
                    continue;
                }
                var recipe = _presets.FindRecipe(recipeId);
                using (new EditorGUI.DisabledScope(recipe == null))
                {
                    if (GUILayout.Button(label))
                        StartRun(recipe);
                }
            }
        }

        private void StartCampaign(IReadOnlyList<string> recipeIds)
        {
            MarketingContentIndexBuilder.Build(saveSnapshot: true);
            MarketingRunMonitor.StartCampaign(recipeIds, new MarketingStudioSession.RunRequest
            {
                outputRoot = _outputRoot,
                language = _language,
                autoStart = _autoStart,
                exitPlayModeOnFinish = true,
                batch = false,
            }, batch: false);
        }

        private void DrawReviewSection()
        {
            EditorGUILayout.LabelField("REVIEW", EditorStyles.boldLabel);
            if (_contactSheet != null)
            {
                float w = position.width - 20;
                float h = w * _contactSheet.height / (float)_contactSheet.width;
                GUILayout.Label(_contactSheet, GUILayout.Width(w), GUILayout.Height(h));
            }
            if (!string.IsNullOrEmpty(_lastRunFolder))
            {
                EditorGUILayout.LabelField("Last run:", _lastRunFolder, EditorStyles.miniLabel);
                if (GUILayout.Button("Open Output Folder"))
                    EditorUtility.RevealInFinder(_lastRunFolder);
            }
        }

        private void StartRun(MarketingCaptureRecipe recipe)
        {
            if (recipe == null) return;
            MarketingContentIndexBuilder.Build(saveSnapshot: true);
            MarketingRunMonitor.StartRun(new MarketingStudioSession.RunRequest
            {
                recipeId = recipe.id,
                outputRoot = _outputRoot,
                language = _language,
                autoStart = _autoStart,
                exitPlayModeOnFinish = true,
                batch = false,
            }, batch: false);
        }

        private void LoadContactSheet(string runFolder)
        {
            string path = Path.Combine(runFolder, "ContactSheet.png");
            if (!File.Exists(path)) return;
            if (_contactSheet != null) DestroyImmediate(_contactSheet);
            _contactSheet = new Texture2D(2, 2);
            _contactSheet.LoadImage(File.ReadAllBytes(path));
        }
    }
}
