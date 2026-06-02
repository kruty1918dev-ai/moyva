using System;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.UI;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Панель "Build-шари" графового генератора. Відтворює налаштування вкладки
    /// "Build Layer" TileWorldCreator, але керується графом: один build-шар на кожен
    /// шар графа, розташовані вертикальним стеком (порядок змінюється стрілками ↑/↓).
    ///
    /// Викликається з GraphEditorWindow через рефлексію (GraphSystem.Editor не посилається
    /// на Generator.Editor напряму).
    /// </summary>
    public static class GraphBuildLayersPanel
    {
        /// <summary>
        /// Будує панель build-шарів для заданого графа. Сигнатура з <c>object</c>
        /// дружня до виклику через рефлексію з інших збірок.
        /// </summary>
        public static VisualElement Build(object graphAsset)
        {
            var root = new VisualElement();

            if (graphAsset is not GraphAsset graph)
            {
                root.Add(new HelpBox("Граф-асет не задано.", HelpBoxMessageType.Info));
                return root;
            }

            Configuration config;
            try
            {
                config = GraphBuildLayerStore.Sync(graph);
            }
            catch (Exception e)
            {
                root.Add(new HelpBox(
                    "Не вдалося синхронізувати build-шари: " + e.Message,
                    HelpBoxMessageType.Error));
                return root;
            }

            if (config == null)
            {
                root.Add(new HelpBox(
                    "Граф-асет має бути збережений у проєкті, щоб налаштовувати build-шари.",
                    HelpBoxMessageType.Warning));
                return root;
            }

            root.Add(new HelpBox(
                "Кожен шар графа має власний build-шар TileWorldCreator — тут налаштовуються " +
                "тайли та візуал. Порядок виконання змінюється стрілками ↑/↓.",
                HelpBoxMessageType.Info));

            var editor = UnityEditor.Editor.CreateEditor(config, typeof(ConfigurationEditor)) as ConfigurationEditor;
            if (editor == null)
            {
                root.Add(new HelpBox(
                    "Не вдалося створити редактор конфігурації TileWorldCreator.",
                    HelpBoxMessageType.Error));
                return root;
            }

            if (config.buildLayerFolders != null)
            {
                foreach (var folder in config.buildLayerFolders)
                {
                    if (folder == null)
                        continue;

                    var folderView = new BuildLayerFolderListViewElement(config, editor);
                    folderView.Bind(folder);
                    root.Add(folderView);
                }
            }

            return root;
        }
    }
}
