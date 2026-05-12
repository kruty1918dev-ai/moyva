using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.InfoPanel.UI.Editor
{
    /// <summary>
    /// Утиліта для створення WorldInfoPanel префаба з потрібною структурою.
    /// 
    /// Як використовувати:
    /// 1. Відкрий меню Assets > Moyva > UI > Create WorldInfoPanel Prefab
    /// 2. Вибери папку, куди зберегти префаб
    /// 3. Префаб буде створений з готовою структурою
    /// </summary>
    #if UNITY_EDITOR
    public static class WorldInfoPanelPrefabBuilder
    {
        [MenuItem("Assets/Moyva/UI/Create WorldInfoPanel Prefab")]
        public static void CreateWorldInfoPanelPrefab()
        {
            // Створюємо контейнер панелі
            var panelGO = new GameObject("WorldInfoPanel");
            var panelRectTransform = panelGO.AddComponent<RectTransform>();
            var panelImage = panelGO.AddComponent<Image>();
            var panelLayoutGroup = panelGO.AddComponent<VerticalLayoutGroup>();
            
            // Налаштування контейнера
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            panelLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
            panelLayoutGroup.spacing = 5;
            panelLayoutGroup.childForceExpandHeight = false;
            panelLayoutGroup.childForceExpandWidth = true;
            
            // Встановлюємо розміри
            panelRectTransform.sizeDelta = new Vector2(300, 400);
            panelRectTransform.anchoredPosition = new Vector2(-150, 0);
            panelRectTransform.anchorMin = new Vector2(0, 0.5f);
            panelRectTransform.anchorMax = new Vector2(0, 0.5f);
            panelRectTransform.pivot = new Vector2(0, 0.5f);

            // Заголовок
            var titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(panelGO.transform);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleText.text = "Title";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleRect.sizeDelta = new Vector2(300, 50);
            
            // Підзаголовок
            var subtitleGO = new GameObject("SubtitleText");
            subtitleGO.transform.SetParent(panelGO.transform);
            var subtitleText = subtitleGO.AddComponent<TextMeshProUGUI>();
            var subtitleRect = subtitleGO.GetComponent<RectTransform>();
            subtitleText.text = "Subtitle";
            subtitleText.fontSize = 24;
            subtitleText.fontStyle = FontStyles.Italic;
            subtitleRect.sizeDelta = new Vector2(300, 40);
            
            // Ресурси
            var resourcesGO = new GameObject("ResourcesText");
            resourcesGO.transform.SetParent(panelGO.transform);
            var resourcesText = resourcesGO.AddComponent<TextMeshProUGUI>();
            var resourcesRect = resourcesGO.GetComponent<RectTransform>();
            resourcesText.text = "Resources: 0";
            resourcesText.fontSize = 20;
            resourcesRect.sizeDelta = new Vector2(300, 60);
            
            // Кнопка закриття
            var closeButtonGO = new GameObject("CloseButton");
            closeButtonGO.transform.SetParent(panelGO.transform);
            var closeButtonRect = closeButtonGO.GetComponent<RectTransform>();
            var closeImage = closeButtonGO.AddComponent<Image>();
            var closeButton = closeButtonGO.AddComponent<Button>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            closeButtonRect.sizeDelta = new Vector2(280, 40);
            
            var closeTextGO = new GameObject("Text");
            closeTextGO.transform.SetParent(closeButtonGO.transform);
            var closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
            var closeTextRect = closeTextGO.GetComponent<RectTransform>();
            closeText.text = "Close";
            closeText.fontSize = 20;
            closeText.alignment = TextAlignmentOptions.Center;
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            // Зберігаємо префаб
            string path = EditorUtility.SaveFilePanelInProject(
                "Save WorldInfoPanel Prefab",
                "WorldInfoPanel",
                "prefab",
                "Select folder to save prefab"
            );

            if (!string.IsNullOrEmpty(path))
            {
                PrefabUtility.SaveAsPrefabAsset(panelGO, path);
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                Debug.Log($"[WorldInfoPanelPrefabBuilder] ✓ Префаб створено: {path}");
            }

            DestroyImmediate(panelGO);
        }
    }
    #endif
}
