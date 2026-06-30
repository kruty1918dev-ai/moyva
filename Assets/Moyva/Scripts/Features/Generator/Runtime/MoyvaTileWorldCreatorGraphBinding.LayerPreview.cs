using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Тестова генерація окремих шарів графа та очищення згенерованої мапи.
    /// </summary>
    public sealed partial class MoyvaTileWorldCreatorGraphBinding
    {
        /// <summary>
        /// Назви шарів графа у порядку оголошення. Використовується редактором
        /// для вибору окремого шару під тестову генерацію.
        /// </summary>
        public IReadOnlyList<string> GetGraphLayerNames()
        {
            if (_graphAsset?.Layers == null)
                return Array.Empty<string>();

            var names = new List<string>();
            foreach (var layer in _graphAsset.Layers)
            {
                if (layer != null && !string.IsNullOrWhiteSpace(layer.Name))
                    names.Add(layer.Name);
            }

            return names;
        }

        /// <summary>
        /// Тестова генерація лише одного шару графа: компілює граф, лишає увімкненим
        /// тільки blueprint-шар з відповідною назвою, будує мапу й відновлює стани шарів.
        /// </summary>
        public void GenerateLayerPreview(string layerName)
        {
            GenerateLayerPreview(layerName, ResolveGenerationSeed());
        }

        public void GenerateLayerPreview(string layerName, int seed)
        {
            if (_isGenerating)
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Генерація вже виконується, preview шару пропущено.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(layerName))
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Назву шару не задано.", this);
                return;
            }

            _isGenerating = true;
            try
            {
                if (_compileBeforeGenerate)
                    CompileGraphToConfiguration(seed);

                var config = Manager?.configuration;
                if (config?.blueprintLayerFolders == null)
                    return;

                var previousStates = new List<(BlueprintLayer layer, bool enabled)>();
                bool matched = false;

                foreach (var folder in config.blueprintLayerFolders)
                {
                    if (folder?.blueprintLayers == null)
                        continue;

                    foreach (var layer in folder.blueprintLayers)
                    {
                        if (layer == null)
                            continue;

                        previousStates.Add((layer, layer.isEnabled));
                        bool isTarget = string.Equals(layer.layerName, layerName, StringComparison.Ordinal);
                        layer.isEnabled = isTarget;
                        matched |= isTarget;
                    }
                }

                if (!matched)
                {
                    Debug.LogWarning($"[Moyva TWC Graph Binding] Шар '{layerName}' не знайдено серед blueprint-шарів.", this);
                    RestoreLayerStates(previousStates);
                    return;
                }

                try
                {
                    TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(Manager);
                }
                finally
                {
                    RestoreLayerStates(previousStates);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка preview шару '{layerName}': {ex}", this);
            }
            finally
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
                _isGenerating = false;
            }
        }

        /// <summary>
        /// Очищає згенеровану мапу зі сцени: скидає blueprint/build-шари й перебудовує порожній світ.
        /// </summary>
        public void ClearGeneratedMap()
        {
            var config = Manager?.configuration;
            if (config == null)
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Сховище шарів не задано — нема чого очищати.", this);
                return;
            }

            Manager.ResetConfiguration();
            Manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
        }

        private static void RestoreLayerStates(List<(BlueprintLayer layer, bool enabled)> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].layer != null)
                    states[i].layer.isEnabled = states[i].enabled;
            }
        }
    }
}
