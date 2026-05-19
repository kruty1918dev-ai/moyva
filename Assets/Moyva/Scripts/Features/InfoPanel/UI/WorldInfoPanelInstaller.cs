using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    /// <summary>
    /// Окремий інсталер системи world info panel.
    /// Не є частиною Construction UI.
    /// </summary>
    public class WorldInfoPanelInstaller : MonoInstaller
    {
        [Header("World Info Panel")]
        [SerializeField] private GameObject panelPrefab;
        [SerializeField] private Transform panelParent;

        public override void InstallBindings()
        {
            if (panelPrefab == null)
            {
                Debug.LogWarning("[WorldInfoPanelInstaller] panelPrefab не присвоєно. Панель інформації вимкнена.");
                return;
            }

            var parent = panelParent != null ? panelParent : transform;

            GameObject panelInstance;
            try
            {
                var instantiated = UnityEngine.Object.Instantiate((UnityEngine.Object)panelPrefab);
                panelInstance = instantiated as GameObject;
                if (panelInstance == null)
                {
                    Debug.LogError("[WorldInfoPanelInstaller] Instantiate не повернув GameObject.");
                    if (instantiated != null)
                        UnityEngine.Object.Destroy(instantiated);
                    return;
                }

                panelInstance.transform.SetParent(parent, false);
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

            // Контейнер для відображення вартості будівництва з іконками ресурсів.
            // Шукаємо "ConstructionCostContainer" в ієрархії панелі; якщо відсутній — створюємо програмно.
            var costContainerTransform = panelInstance.transform.Find("ConstructionCostContainer");
            if (costContainerTransform == null)
            {
                var containerGO = new GameObject("ConstructionCostContainer", typeof(RectTransform));
                containerGO.transform.SetParent(panelInstance.transform, false);

                var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 2f;
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.padding = new RectOffset(0, 0, 4, 0);

                var csf = containerGO.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                costContainerTransform = containerGO.transform;
            }

            Container.Bind<Transform>().WithId("ConstructionCostContainer").FromInstance(costContainerTransform).AsTransient();

            var controllerType = Type.GetType("Kruty1918.Moyva.InfoPanel.UI.WorldInfoPanelController, Kruty1918.Moyva.InfoPanel");
            if (controllerType == null)
            {
                Debug.LogError("[WorldInfoPanelInstaller] Не знайдено тип Kruty1918.Moyva.InfoPanel.UI.WorldInfoPanelController у збірці Kruty1918.Moyva.InfoPanel.");
                return;
            }

            Container.BindInterfacesAndSelfTo(controllerType)
                .AsSingle()
                .NonLazy();
        }
    }
}
