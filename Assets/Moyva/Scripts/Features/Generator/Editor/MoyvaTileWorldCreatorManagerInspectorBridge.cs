using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Панель Moyva у інспекторі TileWorldCreatorManager.
    /// Менеджер виступає виконавцем: приймає мапу-інструкції від графового генератора
    /// й будує її на сцені. Уся робота керується вибраним GraphAsset, а не ручним
    /// налаштуванням TWC blueprint-шарів.
    /// </summary>
    public static class MoyvaTileWorldCreatorManagerInspectorBridge
    {
        public static bool AppendPanel(SerializedObject managerSerializedObject, TileWorldCreatorManager manager, VisualElement root)
        {
            if (manager == null || root == null)
                return false;

            var panel = new VisualElement
            {
                style =
                {
                    marginBottom = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8,
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new Color(0.32f, 0.42f, 0.48f),
                    borderTopColor = new Color(0.32f, 0.42f, 0.48f),
                    borderLeftColor = new Color(0.32f, 0.42f, 0.48f),
                    borderRightColor = new Color(0.32f, 0.42f, 0.48f),
                    backgroundColor = new Color(0.15f, 0.18f, 0.19f)
                }
            };

            panel.Add(new Label("Графовий генератор Moyva")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            });

            var binding = manager.GetComponent<MoyvaTileWorldCreatorGraphBinding>();
            if (binding == null)
            {
                DrawMissingBinding(manager, panel);
                root.Add(panel);
                return true;
            }

            DrawGraphField(binding, panel);
            DrawActions(manager, binding, panel);
            DrawLayerActions(manager, binding, panel);
            DrawQuickSettings(binding, panel);
            DrawConfigurationField(manager, panel);
            DrawStatus(binding, panel);

            root.Add(panel);
            return true;
        }

        private static void DrawMissingBinding(TileWorldCreatorManager manager, VisualElement panel)
        {
            panel.Add(new HelpBox(
                "Цей менеджер ще не підключений до графового генератора Moyva. " +
                "Додайте binding, щоб GraphAsset став джерелом мапи для сцени.",
                HelpBoxMessageType.Info));

            panel.Add(new Button(() =>
            {
                var addedBinding = Undo.AddComponent<MoyvaTileWorldCreatorGraphBinding>(manager.gameObject);
                EditorUtility.SetDirty(addedBinding);
                EditorUtility.SetDirty(manager.gameObject);
                Selection.activeGameObject = manager.gameObject;
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            })
            {
                text = "Підключити графовий генератор"
            });
        }

        private static void DrawGraphField(MoyvaTileWorldCreatorGraphBinding binding, VisualElement panel)
        {
            var graphField = new ObjectField("Граф-асет")
            {
                objectType = typeof(GraphAsset),
                allowSceneObjects = false,
                value = binding.GraphAsset
            };
            graphField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(binding, "Assign Moyva graph asset");
                binding.SetGraphAsset(evt.newValue as GraphAsset);
                EditorUtility.SetDirty(binding);
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            });
            panel.Add(graphField);
        }

        private static void DrawActions(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding, VisualElement panel)
        {
            panel.Add(new Button(() => OpenGraph(binding.GraphAsset))
            {
                text = "Відкрити редактор графів",
                style = { marginTop = 6, height = 24 }
            });

            var buttonRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 4,
                    marginBottom = 2
                }
            };

            buttonRow.Add(new Button(() => GenerateFull(manager, binding))
            {
                text = "Превʼю: вся мапа",
                style = { flexGrow = 1, marginRight = 3, height = 26 }
            });

            buttonRow.Add(new Button(() => ClearMap(manager, binding))
            {
                text = "Очистити мапу",
                style = { flexGrow = 1, marginLeft = 3, height = 26 }
            });

            panel.Add(buttonRow);
        }

        private static void DrawLayerActions(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding, VisualElement panel)
        {
            var names = binding.GetGraphLayerNames();
            if (names.Count == 0)
            {
                panel.Add(new HelpBox("У графі немає шарів для окремої генерації.", HelpBoxMessageType.None));
                return;
            }

            var choices = new List<string>(names);
            var layerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 2,
                    marginBottom = 4,
                    alignItems = Align.Center
                }
            };

            var popup = new PopupField<string>("Шар", choices, 0)
            {
                style = { flexGrow = 1, marginRight = 4 }
            };
            layerRow.Add(popup);

            layerRow.Add(new Button(() => GenerateLayer(manager, binding, popup.value))
            {
                text = "Превʼю шару",
                style = { width = 120, height = 24 }
            });

            panel.Add(layerRow);
        }

        private static void DrawQuickSettings(MoyvaTileWorldCreatorGraphBinding binding, VisualElement panel)
        {
            var foldout = new Foldout
            {
                text = "Швидкі налаштування",
                value = false,
                style = { marginTop = 4 }
            };

            var seedField = new IntegerField("Seed (редактор)")
            {
                value = binding.EditorSeed
            };
            seedField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(binding, "Change Moyva graph seed");
                binding.SetEditorSeed(evt.newValue);
                EditorUtility.SetDirty(binding);
            });
            foldout.Add(seedField);

            var compileToggle = new Toggle("Компілювати граф перед генерацією")
            {
                value = binding.CompileBeforeGenerate
            };
            compileToggle.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(binding, "Change Moyva graph compile mode");
                binding.SetCompileBeforeGenerate(evt.newValue);
                EditorUtility.SetDirty(binding);
            });
            foldout.Add(compileToggle);

            var buildToggle = new Toggle("Будувати візуальні шари")
            {
                value = binding.GenerateBuildLayersAfterCompile
            };
            buildToggle.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(binding, "Change Moyva graph build mode");
                binding.SetGenerateBuildLayersAfterCompile(evt.newValue);
                EditorUtility.SetDirty(binding);
            });
            foldout.Add(buildToggle);

            panel.Add(foldout);
        }

        private static void DrawConfigurationField(TileWorldCreatorManager manager, VisualElement panel)
        {
            var foldout = new Foldout
            {
                text = "Сховище TWC (конфігурація)",
                value = false,
                style = { marginTop = 4 }
            };

            var info = new HelpBox(
                "Конфігурація TWC береться з графа автоматично (компаньйон-конфіг). " +
                "Тайли налаштовуються у вкладці \"Build-шари\" редактора графа.",
                HelpBoxMessageType.Info);

            var configField = new ObjectField("Конфігурація TWC")
            {
                objectType = typeof(Configuration),
                allowSceneObjects = false,
                value = manager.configuration
            };
            configField.SetEnabled(false);

            foldout.Add(info);
            foldout.Add(configField);
            panel.Add(foldout);
        }

        private static void DrawStatus(MoyvaTileWorldCreatorGraphBinding binding, VisualElement panel)
        {
            if (binding.GraphAsset == null)
            {
                panel.Add(new HelpBox("Призначте граф-асет, щоб керувати генерацією мапи.", HelpBoxMessageType.Warning));
                return;
            }

            int layerCount = binding.GraphAsset.Layers?.Count ?? 0;
            int nodeCount = binding.GraphAsset.Nodes?.Count ?? 0;

            panel.Add(new HelpBox(
                $"Готово до генерації. Шарів: {layerCount}, вузлів: {nodeCount}. " +
                "Сховище TWC створюється автоматично. Результат повної генерації з'являється як chunk-first меші у scene root 'MapVisualChunks', а не як класичні TWC layer objects.",
                HelpBoxMessageType.Info));
        }

        private static void GenerateFull(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding)
        {
            if (binding == null)
                return;

            if (!EnsureConfiguration(manager, binding))
                return;

            if (!HasEnabledBuildLayers(manager.configuration))
            {
                EditorUtility.DisplayDialog(
                    "Графовий генератор Moyva",
                    "Немає увімкнених Build-шарів у конфігурації графа. " +
                    "Відкрийте граф і увімкніть хоча б один Build-шар у вкладці \"Build-шари\".",
                    "OK");
                return;
            }

            RecordUndo(manager, binding, "Generate map from Moyva graph");
            binding.GenerateFromGraph();
            MarkDirty(manager, binding);
            PingGeneratedChunkRoot();
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void GenerateLayer(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding, string layerName)
        {
            if (binding == null || string.IsNullOrWhiteSpace(layerName))
                return;

            if (!EnsureConfiguration(manager, binding))
                return;

            if (!HasEnabledBuildLayers(manager.configuration))
            {
                EditorUtility.DisplayDialog(
                    "Графовий генератор Moyva",
                    "Немає увімкнених Build-шарів у конфігурації графа. " +
                    "Відкрийте граф і увімкніть потрібний Build-шар у вкладці \"Build-шари\".",
                    "OK");
                return;
            }

            RecordUndo(manager, binding, "Generate Moyva graph layer");
            binding.GenerateLayerPreview(layerName);
            MarkDirty(manager, binding);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void ClearMap(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding)
        {
            if (binding == null)
                return;

            if (manager == null || manager.configuration == null)
                return;

            RecordUndo(manager, binding, "Clear Moyva generated map");
            binding.ClearGeneratedMap();
            MarkDirty(manager, binding);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void RecordUndo(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding, string label)
        {
            Undo.RecordObject(binding, label);
            if (manager != null && manager.configuration != null)
                Undo.RecordObject(manager.configuration, label);
        }

        private static void MarkDirty(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding)
        {
            if (manager != null)
                EditorUtility.SetDirty(manager);
            if (manager != null && manager.configuration != null)
                EditorUtility.SetDirty(manager.configuration);
            EditorUtility.SetDirty(binding);
        }

        private static void PingGeneratedChunkRoot()
        {
            var root = GameObject.Find("MapVisualChunks");
            if (root == null)
                return;

            EditorGUIUtility.PingObject(root);
            Selection.activeGameObject = root;
            Debug.Log(
                "[Moyva Graph Generator] Generation completed. Chunk-first meshes were built under scene root 'MapVisualChunks'.");
        }

        /// <summary>
        /// Гарантує наявність TWC <see cref="Configuration"/>: це внутрішнє сховище інструкцій,
        /// яке графовий генератор створює автоматично. Користувач не керує ним вручну.
        /// </summary>
        private static bool EnsureConfiguration(TileWorldCreatorManager manager, MoyvaTileWorldCreatorGraphBinding binding)
        {
            if (manager == null)
                return false;

            // Граф — джерело правди: конфігурація TWC береться з компаньйон-конфігу графа.
            // У ньому зберігаються build-шари з тайлами (налаштовуються у вкладці "Build-шари"
            // редактора графа) та blueprint-шари (форма), скомпільовані з графа.
            if (binding != null && binding.GraphAsset != null)
            {
                var companion = GraphBuildLayerStore.Sync(binding.GraphAsset);
                if (companion != null)
                {
                    if (manager.configuration != companion)
                    {
                        Undo.RecordObject(manager, "Assign Moyva graph configuration");
                        manager.configuration = companion;
                        EditorUtility.SetDirty(manager);
                    }
                    return true;
                }
            }

            // Резерв: графа немає — створюємо/лишаємо власну конфігурацію менеджера.
            if (manager.configuration != null)
                return true;

            const string folder = "Assets/Moyva/SO/Generation/TileWorldCreator";
            EnsureFolder(folder);

            string baseName = binding != null && binding.GraphAsset != null
                ? binding.GraphAsset.name
                : manager.gameObject.name;
            string fileName = SanitizeFileName($"{baseName}_TWC");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}.asset");

            var configuration = ScriptableObject.CreateInstance<Configuration>();
            configuration.name = fileName;
            AssetDatabase.CreateAsset(configuration, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(manager, "Assign Moyva configuration");
            manager.configuration = configuration;
            EditorUtility.SetDirty(manager);
            return true;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "MoyvaTileWorldCreatorConfiguration";

            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return name;
        }

        private static bool HasEnabledBuildLayers(Configuration configuration)
        {
            if (configuration?.buildLayerFolders == null)
                return false;

            foreach (var folder in configuration.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;

                foreach (var layer in folder.buildLayers)
                {
                    if (layer != null && layer.isEnabled)
                        return true;
                }
            }

            return false;
        }

        private static void OpenGraph(GraphAsset graphAsset)
        {
            if (graphAsset == null)
            {
                EditorUtility.DisplayDialog("Графовий генератор Moyva", "Граф-асет не задано.", "OK");
                return;
            }

            var windowType = Type.GetType("Kruty1918.Moyva.GraphSystem.Editor.GraphEditorWindow, Kruty1918.Moyva.GraphSystem.Editor");
            var openMethod = windowType?.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "Open"
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(GraphAsset));

            if (openMethod != null)
                openMethod.Invoke(null, new object[] { graphAsset });
            else
                EditorApplication.ExecuteMenuItem("Moyva/Graph Editor");

            EditorGUIUtility.PingObject(graphAsset);
        }
    }
}
