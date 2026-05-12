using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Zenject;
using Kruty1918.Moyva.InfoPanel.API;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    /// <summary>
    /// Zenject инсталер для World Info Panel.
    /// Читає WorldUIConfigSO і створює панель програмно.
    /// 
    /// Як використовувати:
    /// 1. Додай цей інсталер до SceneContext Installers
    /// 2. Призначте WorldUIConfigSO у полі конфіга
    /// 3. Префаб панелі буде завантажений і розміщений автоматично
    /// </summary>
    public class WorldInfoPanelInstaller : MonoInstaller
    {
        [Header("UI Configuration")]
        [SerializeField] private WorldUIConfigSO _uiConfig;

        [Header("Legacy Fallback (auto-migrated)")]
        [FormerlySerializedAs("panelPrefab")]
        [SerializeField] private GameObject _legacyPanelPrefab;

        [FormerlySerializedAs("panelParent")]
        [SerializeField] private Transform _legacyPanelParent;

        public override void InstallBindings()
        {
            var panelPrefab = _uiConfig != null
                ? _uiConfig.WorldInfoPanelPrefab
                : _legacyPanelPrefab;

            if (panelPrefab == null)
            {
                Debug.LogWarning("[WorldInfoPanelInstaller] Не задано World Info Panel префаб (ані через _uiConfig, ані через legacy поля). Панель вимкнена.");
                return;
            }

            // Знаходимо батьківський об'єкт для панелі
            Transform panelParent = null;
            if (_uiConfig != null && !string.IsNullOrWhiteSpace(_uiConfig.PanelParentName))
            {
                var parentObj = GameObject.Find(_uiConfig.PanelParentName);
                if (parentObj != null)
                    panelParent = parentObj.transform;
            }

            if (panelParent == null && _legacyPanelParent != null)
                panelParent = _legacyPanelParent;

            if (panelParent == null)
                panelParent = transform.parent ?? transform;

            GameObject panelInstance;
            try
            {
                panelInstance = UnityEngine.Object.Instantiate(panelPrefab);
                panelInstance.transform.SetParent(panelParent, false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WorldInfoPanelInstaller] Помилка Instantiate: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            panelInstance.name = "WorldInfoPanel";
            panelInstance.SetActive(false);

            var title = panelInstance.transform.Find("TitleText")?.GetComponent<TMP_Text>();
            var subtitle = panelInstance.transform.Find("SubtitleText")?.GetComponent<TMP_Text>();
            var resources = panelInstance.transform.Find("ResourcesText")?.GetComponent<TMP_Text>();
            var closeButton = panelInstance.transform.Find("CloseButton")?.GetComponent<Button>();

            if (title == null || subtitle == null || resources == null || closeButton == null)
            {
                Debug.LogError("[WorldInfoPanelInstaller] Некоректна структура панелі. Очікуються: TitleText, SubtitleText, ResourcesText, CloseButton.", panelInstance);
                UnityEngine.Object.Destroy(panelInstance);
                return;
            }

            Container.Bind<GameObject>().WithId("BuildingInfoPanelRoot").FromInstance(panelInstance).AsTransient();
            Container.Bind<TMP_Text>().WithId("BuildingInfoTitleText").FromInstance(title).AsTransient();
            Container.Bind<TMP_Text>().WithId("BuildingInfoSubtitleText").FromInstance(subtitle).AsTransient();
            Container.Bind<TMP_Text>().WithId("BuildingInfoResourcesText").FromInstance(resources).AsTransient();
            Container.Bind<Button>().WithId("BuildingInfoCloseButton").FromInstance(closeButton).AsTransient();

            var controllerType = Type.GetType("Kruty1918.Moyva.InfoPanel.UI.WorldInfoPanelController, Kruty1918.Moyva.InfoPanel");
            if (controllerType == null)
            {
                Debug.LogError("[WorldInfoPanelInstaller] Не знайдено тип Kruty1918.Moyva.InfoPanel.UI.WorldInfoPanelController у збірці Kruty1918.Moyva.InfoPanel.");
                return;
            }

            Container.BindInterfacesAndSelfTo(controllerType)
                .AsSingle()
                .NonLazy();

            // ────────────────────────────────────────────────────────────
            // Castle Detailed Info Panel Setup
            // ────────────────────────────────────────────────────────────

            var castlePanelPrefab = _uiConfig != null ? _uiConfig.CastleDetailedPanelPrefab : null;
            if (castlePanelPrefab != null)
            {
                GameObject castlePanelInstance;
                try
                {
                    castlePanelInstance = UnityEngine.Object.Instantiate(castlePanelPrefab);
                    castlePanelInstance.transform.SetParent(panelParent, false);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[WorldInfoPanelInstaller] Помилка Instantiate Castle Panel: {ex.GetType().Name}: {ex.Message}");
                    castlePanelInstance = null;
                }

                if (castlePanelInstance != null)
                {
                    castlePanelInstance.name = "CastleDetailedPanel";
                    castlePanelInstance.SetActive(false);

                    var castleNameText = castlePanelInstance.transform.Find("CastleNameText")?.GetComponent<TMP_Text>();
                    var castleStatsText = castlePanelInstance.transform.Find("StatisticsText")?.GetComponent<TMP_Text>();
                    var buildingsListContainer = castlePanelInstance.transform.Find("BuildingsListContainer");
                    var castleCloseButton = castlePanelInstance.transform.Find("CloseButton")?.GetComponent<Button>();

                    if (castleNameText != null && castleStatsText != null && buildingsListContainer != null && castleCloseButton != null)
                    {
                        Container.Bind<GameObject>().WithId("CastleDetailedPanelRoot").FromInstance(castlePanelInstance).AsTransient();
                        Container.Bind<TMP_Text>().WithId("CastleNameText").FromInstance(castleNameText).AsTransient();
                        Container.Bind<TMP_Text>().WithId("CastleStatisticsText").FromInstance(castleStatsText).AsTransient();
                        Container.Bind<Transform>().WithId("CastleBuildingsListContainer").FromInstance(buildingsListContainer).AsTransient();
                        Container.Bind<Button>().WithId("CastleDetailedCloseButton").FromInstance(castleCloseButton).AsTransient();

                        // Префаб елемента списку буде створений програмно
                        Container.Bind<GameObject>().WithId("CastleBuildingItemPrefab").FromInstance(null).AsTransient();

                        Container.BindInterfacesAndSelfTo<CastleDetailedInfoPanelController>()
                            .AsSingle()
                            .NonLazy();

                        Debug.Log("[WorldInfoPanelInstaller] ✓ Castle Detailed Panel успішно встановлена");
                    }
                    else
                    {
                        Debug.LogError("[WorldInfoPanelInstaller] Некоректна структура Castle Panel. Очікуються: CastleNameText, StatisticsText, BuildingsListContainer, CloseButton.", castlePanelInstance);
                    }
                }
            }

            var statsPanelRoot = CreateStatisticsPanel(panelParent);
            if (statsPanelRoot != null)
            {
                var statsTitle = statsPanelRoot.transform.Find("TitleText")?.GetComponent<TMP_Text>();
                var statsBody = statsPanelRoot.transform.Find("BodyText")?.GetComponent<TMP_Text>();
                var settlementButton = statsPanelRoot.transform.Find("SettlementButton")?.GetComponent<Button>();
                var kingdomButton = statsPanelRoot.transform.Find("KingdomButton")?.GetComponent<Button>();
                var closeStatsButton = statsPanelRoot.transform.Find("CloseButton")?.GetComponent<Button>();

                if (statsTitle != null && statsBody != null && settlementButton != null && kingdomButton != null && closeStatsButton != null)
                {
                    Container.Bind<GameObject>().WithId("StatisticsMenuRoot").FromInstance(statsPanelRoot).AsTransient();
                    Container.Bind<TMP_Text>().WithId("StatisticsMenuTitle").FromInstance(statsTitle).AsTransient();
                    Container.Bind<TMP_Text>().WithId("StatisticsMenuBody").FromInstance(statsBody).AsTransient();
                    Container.Bind<Button>().WithId("StatisticsMenuSettlementButton").FromInstance(settlementButton).AsTransient();
                    Container.Bind<Button>().WithId("StatisticsMenuKingdomButton").FromInstance(kingdomButton).AsTransient();
                    Container.Bind<Button>().WithId("StatisticsMenuCloseButton").FromInstance(closeStatsButton).AsTransient();

                    Container.BindInterfacesAndSelfTo<SettlementKingdomStatisticsPanelController>()
                        .AsSingle()
                        .NonLazy();
                }
                else
                {
                    Debug.LogError("[WorldInfoPanelInstaller] Некоректна структура StatisticsMenuPanel.", statsPanelRoot);
                }
            }

            Debug.Log("[WorldInfoPanelInstaller] ✓ World Info Panel успішно встановлена з конфіга");
        }

        private static GameObject CreateStatisticsPanel(Transform panelParent)
        {
            var panelRoot = new GameObject("StatisticsMenuPanel");
            panelRoot.transform.SetParent(panelParent, false);

            var rect = panelRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, 0f);
            rect.sizeDelta = new Vector2(460f, 480f);

            var bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.11f, 0.15f, 0.96f);

            var titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(panelRoot.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(-24f, 40f);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Статистика";
            titleText.fontSize = 28f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            var bodyGO = new GameObject("BodyText");
            bodyGO.transform.SetParent(panelRoot.transform, false);
            var bodyRect = bodyGO.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(16f, 74f);
            bodyRect.offsetMax = new Vector2(-16f, -76f);
            var bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyText.text = "Дані відсутні";
            bodyText.fontSize = 20f;
            bodyText.color = new Color(0.92f, 0.95f, 0.98f, 1f);
            bodyText.alignment = TextAlignmentOptions.TopLeft;

            var settlementButton = CreateFooterButton(panelRoot.transform, "SettlementButton", "Селище", new Vector2(-140f, 22f));
            var kingdomButton = CreateFooterButton(panelRoot.transform, "KingdomButton", "Королівство", new Vector2(0f, 22f));
            var closeButton = CreateFooterButton(panelRoot.transform, "CloseButton", "Закрити", new Vector2(140f, 22f));

            if (settlementButton == null || kingdomButton == null || closeButton == null)
            {
                UnityEngine.Object.Destroy(panelRoot);
                return null;
            }

            panelRoot.SetActive(false);
            return panelRoot;
        }

        private static Button CreateFooterButton(Transform parent, string name, string caption, Vector2 anchoredPos)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(128f, 36f);

            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.18f, 0.26f, 0.34f, 0.95f);

            var button = buttonGO.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = caption;
            text.fontSize = 18f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return button;
        }
    }
}
