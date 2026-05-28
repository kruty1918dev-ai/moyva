using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class ConstructionUISetupWindow : EditorWindow
    {
        private const string ControllerTypeName = "Kruty1918.Moyva.Construction.UI.ConstructionUIController";
        private const string InputSystemUiInputModuleTypeName = "UnityEngine.InputSystem.UI.InputSystemUIInputModule";

        private MonoBehaviour _controller;
        private BuildingRegistrySO _registry;
        private Vector2 _scroll;
        private readonly List<string> _messages = new List<string>();

        public static void Open()
        {
            var window = GetWindow<ConstructionUISetupWindow>("Construction UI Setup");
            window.minSize = new Vector2(560f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            _registry ??= MoyvaProjectEditorContext.Get<BuildingRegistrySO>();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Construction UI: сетап і валідація", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Інструмент допомагає знайти посилання, перевірити реєстр і валідність префабів для меню будівництва.", MessageType.Info);

            var controllerType = FindControllerType();
            var objectFieldType = controllerType ?? typeof(MonoBehaviour);

            _controller = (MonoBehaviour)EditorGUILayout.ObjectField("Construction UI Controller", _controller, objectFieldType, true);

            EditorGUI.BeginChangeCheck();
            _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField("Building Registry SO", _registry, typeof(BuildingRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
                MoyvaProjectEditorContext.Set(_registry);

            if (controllerType == null)
                EditorGUILayout.HelpBox("Не знайдено тип ConstructionUIController. Перевірте asmdef посилання Editor -> Construction.UI.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Автопошук у сцені/проєкті"))
                AutoFind();
            if (GUILayout.Button("Валідувати"))
                ValidateAll();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Автоматичне створення", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Створить повну ієрархію ConstructionUI у відкритій сцені:\n" +
                "• Canvas + EventSystem (якщо відсутні)\n" +
                "• ConstructionUI root (Controller + Installer)\n" +
                "• Верхній StatusBar на всю ширину\n" +
                "• Нижній BuildingSelectionPanel зі скролом по горизонталі + CategoryTabs\n" +
                "• ActionBar (5 кнопок) + автогенерація prefab-шаблонів\n" +
                "• Всі серіалізовані посилання призначаються автоматично.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(controllerType == null))
            {
                if (GUILayout.Button("Створити усю UI-ієрархію у сцені", GUILayout.Height(34f)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Створення Construction UI",
                        "Буде створено ConstructionUI з усіма дочірніми панелями та компонентами у відкритій сцені.\n\nПродовжити?",
                        "Створити", "Відміна"))
                    {
                        CreateAll();
                    }
                }
            }

            EditorGUILayout.Space(4f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_messages.Count == 0)
            {
                EditorGUILayout.HelpBox("Результати з'являться після виконання операції.", MessageType.None);
            }
            else
            {
                for (int i = 0; i < _messages.Count; i++)
                    EditorGUILayout.LabelField($"{i + 1}. {_messages[i]}", EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();
        }

        private void CreateAll()
        {
            const string undoName = "Створити Construction UI";
            _messages.Clear();

            var controllerType = FindControllerType();
            if (controllerType == null)
            {
                _messages.Add("Помилка: тип ConstructionUIController не знайдено.");
                Repaint();
                return;
            }

            var asm = controllerType.Assembly;
            var selectionPanelType = asm.GetType("Kruty1918.Moyva.Construction.UI.BuildingSelectionPanelUI");
            var categoryTabsType = asm.GetType("Kruty1918.Moyva.Construction.UI.BuildingCategoryTabsUI");
            var buildingButtonType = asm.GetType("Kruty1918.Moyva.Construction.UI.BuildingButtonUI");
            var actionBarType = asm.GetType("Kruty1918.Moyva.Construction.UI.ConstructionActionBarUI");
            var statusUIType = asm.GetType("Kruty1918.Moyva.Construction.UI.ConstructionStatusUI");
            var installerType = asm.GetType("Kruty1918.Moyva.Construction.UI.ConstructionUIInstaller");

            Undo.SetCurrentGroupName(undoName);
            int group = Undo.GetCurrentGroup();

#if UNITY_2023_1_OR_NEWER
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
#else
            Canvas canvas = Object.FindObjectOfType<Canvas>();
#endif
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                Undo.RegisterCreatedObjectUndo(canvasGO, undoName);
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                ApplyAuthoringCanvasScaler(scaler);
                canvasGO.AddComponent<GraphicRaycaster>();
                _messages.Add("Створено: Canvas.");
            }
            else
            {
                var scaler = canvas.GetComponent<CanvasScaler>() ?? Undo.AddComponent<CanvasScaler>(canvas.gameObject);
                ApplyAuthoringCanvasScaler(scaler);
                _messages.Add($"OK: використано існуючий Canvas '{canvas.name}'.");
            }

#if UNITY_2023_1_OR_NEWER
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
            {
                var evGO = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(evGO, undoName);
                evGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                if (TryAddInputSystemUiInputModule(evGO))
                    _messages.Add("Створено: EventSystem з InputSystemUIInputModule.");
                else
                    _messages.Add("Помилка: InputSystemUIInputModule не знайдено. Перевірте, що пакет Input System встановлений і активний.");
            }

            var root = new GameObject("ConstructionUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(root, undoName);
            root.transform.SetParent(canvas.transform, false);
            Stretch(root, 0f);
            root.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.10f, 0.85f);

            var controllerComp = (MonoBehaviour)root.AddComponent(controllerType);
            MonoBehaviour installerComp = installerType != null ? (MonoBehaviour)root.AddComponent(installerType) : null;
            _messages.Add("Створено: ConstructionUI (ConstructionUIController + ConstructionUIInstaller).");

            var templatesGO = new GameObject("Templates", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(templatesGO, undoName);
            templatesGO.transform.SetParent(root.transform, false);
            templatesGO.SetActive(false);

            var tabButtonTemplate = MakeTabButtonTemplate(templatesGO.transform, "CategoryTabButtonTemplate");
            var buildingButtonTemplate = MakeBuildingButtonTemplate(templatesGO.transform, "BuildingButtonTemplate", buildingButtonType);
            _messages.Add("Створено: Templates (кнопка категорії + кнопка будівлі).");

            var statusPanelGO = MakePanel("StatusPanel", root.transform, new Color(0.08f, 0.08f, 0.12f, 0.94f));
            TopBar(statusPanelGO, 72f, 8f);
            var hgStatus = statusPanelGO.AddComponent<HorizontalLayoutGroup>();
            hgStatus.childAlignment = TextAnchor.MiddleLeft;
            hgStatus.spacing = 16f;
            hgStatus.padding = new RectOffset(14, 14, 8, 8);
            MonoBehaviour statusComp = statusUIType != null
                ? (MonoBehaviour)statusPanelGO.AddComponent(statusUIType) : null;

            var stateTMP = MakeTMPLabel("StateLabel", statusPanelGO.transform, "Стан: Idle");
            var buildingTMP = MakeTMPLabel("BuildingLabel", statusPanelGO.transform, "Будівля: —");
            var previewTMP = MakeTMPLabel("PreviewLabel", statusPanelGO.transform, "Preview: —");

            if (statusComp != null)
            {
                var soStatus = new SerializedObject(statusComp);
                if (stateTMP != null) soStatus.FindProperty("placementStateLabel").objectReferenceValue = stateTMP;
                if (buildingTMP != null) soStatus.FindProperty("selectedBuildingLabel").objectReferenceValue = buildingTMP;
                if (previewTMP != null) soStatus.FindProperty("previewStateLabel").objectReferenceValue = previewTMP;
                soStatus.ApplyModifiedPropertiesWithoutUndo();
            }
            _messages.Add("Створено: StatusPanel зверху на всю ширину.");

            var selectionGO = MakePanel("BuildingSelectionPanel", root.transform, new Color(0.12f, 0.12f, 0.15f, 0.92f));
            BottomBar(selectionGO, 220f, 8f);
            MonoBehaviour selectionComp = selectionPanelType != null
                ? (MonoBehaviour)selectionGO.AddComponent(selectionPanelType) : null;

            var categoryTabsGO = MakePanel("CategoryTabs", selectionGO.transform, new Color(0.15f, 0.15f, 0.20f, 0.95f));
            TopBar(categoryTabsGO, 64f, 8f);
            MonoBehaviour categoryTabsComp = categoryTabsType != null
                ? (MonoBehaviour)categoryTabsGO.AddComponent(categoryTabsType) : null;

            var tabContainerGO = new GameObject("TabContainer", typeof(RectTransform));
            tabContainerGO.transform.SetParent(categoryTabsGO.transform, false);
            var hTab = tabContainerGO.AddComponent<HorizontalLayoutGroup>();
            hTab.childAlignment = TextAnchor.MiddleLeft;
            hTab.spacing = 6f;
            hTab.padding = new RectOffset(8, 8, 4, 4);
            tabContainerGO.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            Stretch(tabContainerGO, 0f);

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(selectionGO.transform, false);
            StretchBetween(viewportGO, 8f, 8f, 8f, 72f);
            var viewportImage = viewportGO.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportGO.GetComponent<Mask>().showMaskGraphic = false;

            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var hContent = contentGO.AddComponent<HorizontalLayoutGroup>();
            hContent.childAlignment = TextAnchor.MiddleLeft;
            hContent.spacing = 8f;
            hContent.padding = new RectOffset(6, 6, 4, 4);
            var fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 0.5f);
            contentRT.anchorMax = new Vector2(0f, 0.5f);
            contentRT.pivot = new Vector2(0f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;

            var scrollRect = selectionGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.viewport = viewportGO.GetComponent<RectTransform>();
            scrollRect.content = contentRT;

            if (selectionComp != null)
            {
                var soSel = new SerializedObject(selectionComp);
                soSel.FindProperty("itemContainer").objectReferenceValue = contentGO.transform;
                soSel.FindProperty("buttonPrefab").objectReferenceValue = buildingButtonTemplate;
                if (categoryTabsComp != null)
                    soSel.FindProperty("categoryTabs").objectReferenceValue = categoryTabsComp;
                soSel.ApplyModifiedPropertiesWithoutUndo();
            }

            if (categoryTabsComp != null)
            {
                var soTabs = new SerializedObject(categoryTabsComp);
                soTabs.FindProperty("tabContainer").objectReferenceValue = tabContainerGO.transform;
                soTabs.FindProperty("tabButtonPrefab").objectReferenceValue = tabButtonTemplate;
                soTabs.ApplyModifiedPropertiesWithoutUndo();
            }
            _messages.Add("Створено: BuildingSelectionPanel з горизонтальним скролом + CategoryTabs для класів.");

            var actionBarGO = MakePanel("ActionBar", root.transform, new Color(0.10f, 0.12f, 0.10f, 0.95f));
            BottomBar(actionBarGO, 76f, 236f);
            var hg = actionBarGO.AddComponent<HorizontalLayoutGroup>();
            hg.childAlignment = TextAnchor.MiddleCenter;
            hg.spacing = 8f;
            hg.padding = new RectOffset(8, 8, 8, 8);
            MonoBehaviour actionBarComp = actionBarType != null
                ? (MonoBehaviour)actionBarGO.AddComponent(actionBarType) : null;

            var (_, confirmBtn) = MakeActionButton("ConfirmButton", "Підтвердити", actionBarGO.transform);
            var (_, cancelBtn) = MakeActionButton("CancelButton", "Скасувати", actionBarGO.transform);
            var (_, undoBtn) = MakeActionButton("UndoButton", "Відмінити", actionBarGO.transform);
            var (_, redoBtn) = MakeActionButton("RedoButton", "Повторити", actionBarGO.transform);
            var (_, demolishBtn) = MakeActionButton("DemolishButton", "Знести", actionBarGO.transform);

            if (actionBarComp != null)
            {
                var soBar = new SerializedObject(actionBarComp);
                soBar.FindProperty("confirmButton").objectReferenceValue = confirmBtn;
                soBar.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
                soBar.FindProperty("undoButton").objectReferenceValue = undoBtn;
                soBar.FindProperty("redoButton").objectReferenceValue = redoBtn;
                soBar.FindProperty("demolishButton").objectReferenceValue = demolishBtn;
                soBar.ApplyModifiedPropertiesWithoutUndo();
            }
            _messages.Add("Створено: ActionBar (Confirm / Cancel / Undo / Redo / Знести).");

            var soCtrl = new SerializedObject(controllerComp);
            if (selectionComp != null) soCtrl.FindProperty("selectionPanel").objectReferenceValue = selectionComp;
            if (actionBarComp != null) soCtrl.FindProperty("actionBar").objectReferenceValue = actionBarComp;
            if (statusComp != null) soCtrl.FindProperty("statusDisplay").objectReferenceValue = statusComp;
            soCtrl.FindProperty("constructionUIRoot").objectReferenceValue = root;
            soCtrl.ApplyModifiedPropertiesWithoutUndo();
            _messages.Add("OK: ConstructionUIController — посилання призначено.");

            if (installerComp != null)
            {
                WireRef(installerComp, "uiController", controllerComp);
                _messages.Add("OK: ConstructionUIInstaller.uiController призначено.");
            }

            Undo.CollapseUndoOperations(group);

            _controller = controllerComp;
            Selection.activeGameObject = root;
            _messages.Add("ГОТОВО. UI адаптований під різні екрани; залишилось призначити 'constructionUIRoot' (опціонально) у Inspector контролера.");
            Repaint();
        }

        private static GameObject MakePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static (GameObject go, Button btn) MakeActionButton(string name, string label, Transform parent)
        {
            var btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            btnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 60f);
            btnGO.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.25f, 1f);

            var tmpType = FindTMPTextType();
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            if (tmpType != null)
            {
                var tmpComp = labelGO.AddComponent(tmpType);
                tmpType.GetProperty("text")?.SetValue(tmpComp, label);
                tmpType.GetProperty("fontSize")?.SetValue(tmpComp, 16f);
                tmpType.GetProperty("color")?.SetValue(tmpComp, Color.white);
                var alignProp = tmpType.GetProperty("alignment");
                if (alignProp != null)
                    alignProp.SetValue(tmpComp, System.Enum.ToObject(alignProp.PropertyType, 514));
            }

            return (btnGO, btnGO.GetComponent<Button>());
        }

        private static GameObject MakeTabButtonTemplate(Transform parent, string name)
        {
            var tabGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            tabGO.transform.SetParent(parent, false);
            var tabRT = tabGO.GetComponent<RectTransform>();
            tabRT.sizeDelta = new Vector2(180f, 56f);
            tabGO.GetComponent<Image>().color = new Color(0.20f, 0.20f, 0.24f, 1f);

            var tmpType = FindTMPTextType();
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGO.transform.SetParent(tabGO.transform, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            if (tmpType != null)
            {
                var tmpComp = labelGO.AddComponent(tmpType);
                tmpType.GetProperty("text")?.SetValue(tmpComp, "Всі");
                tmpType.GetProperty("fontSize")?.SetValue(tmpComp, 15f);
                tmpType.GetProperty("color")?.SetValue(tmpComp, Color.white);
                var alignProp = tmpType.GetProperty("alignment");
                if (alignProp != null)
                    alignProp.SetValue(tmpComp, System.Enum.ToObject(alignProp.PropertyType, 514));
            }

            return tabGO;
        }

        private static GameObject MakeBuildingButtonTemplate(Transform parent, string name, System.Type buildingButtonType)
        {
            var buttonGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);
            var buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(170f, 170f);
            buttonGO.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.18f, 1f);

            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGO.transform.SetParent(buttonGO.transform, false);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.62f);
            iconRT.anchorMax = new Vector2(0.5f, 0.62f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(88f, 88f);
            iconRT.anchoredPosition = Vector2.zero;
            iconGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.85f);

            var tmpType = FindTMPTextType();
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGO.transform.SetParent(buttonGO.transform, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0f);
            labelRT.anchorMax = new Vector2(1f, 0f);
            labelRT.pivot = new Vector2(0.5f, 0f);
            labelRT.offsetMin = new Vector2(8f, 8f);
            labelRT.offsetMax = new Vector2(-8f, 44f);

            Component labelComp = null;
            if (tmpType != null)
            {
                labelComp = labelGO.AddComponent(tmpType);
                tmpType.GetProperty("text")?.SetValue(labelComp, "Назва");
                tmpType.GetProperty("fontSize")?.SetValue(labelComp, 15f);
                tmpType.GetProperty("color")?.SetValue(labelComp, Color.white);
                var alignProp = tmpType.GetProperty("alignment");
                if (alignProp != null)
                    alignProp.SetValue(labelComp, System.Enum.ToObject(alignProp.PropertyType, 514));
            }

            if (buildingButtonType != null)
            {
                var buildingButtonComp = buttonGO.AddComponent(buildingButtonType);
                var so = new SerializedObject(buildingButtonComp);
                var labelProp = so.FindProperty("label");
                if (labelProp != null && labelComp != null)
                    labelProp.objectReferenceValue = labelComp;
                var iconProp = so.FindProperty("iconImage");
                if (iconProp != null)
                    iconProp.objectReferenceValue = iconGO.GetComponent<Image>();
                var buttonProp = so.FindProperty("button");
                if (buttonProp != null)
                    buttonProp.objectReferenceValue = buttonGO.GetComponent<Button>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return buttonGO;
        }

        private static Component MakeTMPLabel(string name, Transform parent, string text)
        {
            var tmpType = FindTMPTextType();
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(240f, 28f);
            if (tmpType == null) return null;
            var comp = go.AddComponent(tmpType);
            tmpType.GetProperty("text")?.SetValue(comp, text);
            tmpType.GetProperty("fontSize")?.SetValue(comp, 15f);
            tmpType.GetProperty("color")?.SetValue(comp, Color.white);
            return comp;
        }

        private static System.Type FindTMPTextType()
        {
            return System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("TMPro.TextMeshProUGUI", false))
                .FirstOrDefault(t => t != null);
        }

        private static void WireRef(MonoBehaviour target, string fieldName, Object reference)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = reference;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Stretch(GameObject go, float margin)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        private static void TopBar(GameObject go, float height, float margin)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(margin, -height - margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        private static void BottomBar(GameObject go, float height, float bottomOffset)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(8f, bottomOffset);
            rt.offsetMax = new Vector2(-8f, bottomOffset + height);
        }

        private static void StretchBetween(GameObject go, float left, float right, float bottom, float top)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private void AutoFind()
        {
            _messages.Clear();

            var controllerType = FindControllerType();
            if (controllerType == null)
            {
                _messages.Add("Помилка: не знайдено тип ConstructionUIController. Перевірте збірки та asmdef-посилання.");
                Repaint();
                return;
            }

#if UNITY_2023_1_OR_NEWER
            _controller = Object.FindFirstObjectByType(controllerType) as MonoBehaviour;
#else
            _controller = Object.FindObjectOfType(controllerType) as MonoBehaviour;
#endif
            if (_controller == null)
                _messages.Add("Помилка: не знайдено ConstructionUIController у відкритій сцені.");
            else
                _messages.Add($"OK: знайдено контролер '{_controller.name}'.");

            if (_registry == null)
            {
                _registry = MoyvaProjectEditorContext.GetOrFindFirst<BuildingRegistrySO>();
            }

            if (_registry == null)
                _messages.Add("Попередження: BuildingRegistrySO не знайдено автоматично. Призначте вручну.");
            else
                _messages.Add($"OK: знайдено реєстр '{_registry.name}'.");

            Repaint();
        }

        private void ValidateAll()
        {
            _messages.Clear();

            ValidateController();
            ValidateRegistry();

            if (_messages.Count == 0)
                _messages.Add("OK: критичних проблем не знайдено.");

            Repaint();
        }

        private void ValidateController()
        {
            if (_controller == null)
            {
                _messages.Add("Помилка: не призначено ConstructionUIController.");
                return;
            }

            var so = new SerializedObject(_controller);
            CheckRequiredReference(so, "selectionPanel", "Помилка: не призначено Selection Panel.");
            CheckRequiredReference(so, "actionBar", "Помилка: не призначено Action Bar.");
            CheckRequiredReference(so, "statusDisplay", "Попередження: не призначено Status Display.");
            CheckRequiredReference(so, "constructionUIRoot", "Попередження: не призначено Construction UI Root. Буде використано gameObject контролера.");

            if (_controller.gameObject.scene.IsValid())
                _messages.Add("OK: контролер знаходиться у валідній сцені.");
            else
                _messages.Add("Попередження: контролер не належить відкритій сцені.");
        }

        private void ValidateRegistry()
        {
            if (_registry == null)
            {
                _messages.Add("Помилка: не призначено BuildingRegistrySO.");
                return;
            }

            var buildings = _registry.GetAll();
            if (buildings.Length == 0)
            {
                _messages.Add("Попередження: реєстр будівель порожній.");
                return;
            }

            int missingPrefab = 0;
            int missingSprite = 0;

            foreach (var building in buildings)
            {
                if (building == null)
                    continue;

                if (building.Prefab == null)
                {
                    missingPrefab++;
                    _messages.Add($"Попередження: '{building.Id}' не має prefab.");
                    continue;
                }

                bool hasSprite = AdaptivePrefabPreviewUtility.TryGetPrimarySprite(building.Prefab, out _, out _);

                if (!hasSprite && building.Icon == null)
                {
                    missingSprite++;
                    _messages.Add($"Помилка: '{building.Id}' не має sprite ні в prefab, ні в полі Icon.");
                }
            }

            _messages.Add($"Підсумок реєстру: будівель {buildings.Length}, без prefab {missingPrefab}, без sprite {missingSprite}.");
        }

        private void CheckRequiredReference(SerializedObject so, string fieldName, string failMessage)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                _messages.Add($"Попередження: поле '{fieldName}' не знайдено (можливо, перейменовано).");
                return;
            }

            if (prop.objectReferenceValue == null)
                _messages.Add(failMessage);
            else
                _messages.Add($"OK: '{fieldName}' призначено.");
        }

        private static System.Type FindControllerType()
        {
            return System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(ControllerTypeName, false))
                .FirstOrDefault(type => type != null);
        }

        private static bool TryAddInputSystemUiInputModule(GameObject eventSystemObject)
        {
            var inputModuleType = System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(InputSystemUiInputModuleTypeName, false))
                .FirstOrDefault(type => type != null);

            if (inputModuleType == null)
                return false;

            eventSystemObject.AddComponent(inputModuleType);
            return true;
        }

        private static void ApplyAuthoringCanvasScaler(CanvasScaler scaler)
        {
            if (scaler == null)
                return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            scaler.referencePixelsPerUnit = 100f;
        }
    }
}
