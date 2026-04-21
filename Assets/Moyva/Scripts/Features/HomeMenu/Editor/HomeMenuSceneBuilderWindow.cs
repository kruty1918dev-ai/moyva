using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.WorldCreation.Runtime;
using Kruty1918.Moyva.WorldCreation.UI;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    /// <summary>
    /// Редакторський інструмент, який будує повну сцену HomeMenu у ВІДКРИТІЙ сцені
    /// (не створює нову). Меню: <c>Moyva/HomeMenu/Побудувати сцену</c>.
    ///
    /// Структура:
    /// • Боковик: 3 центровані по висоті кнопки (Грати, Налаштування, Вийти).
    /// • Права частина порожня, поки не натиснеш якусь кнопку.
    /// • Toggle-логіка: повторний клік по активній кнопці закриває її меню.
    /// • Play розгортає панель з 4 вибір-картками: Соло / З Ботом / Мультиплеєр / Продовжити.
    /// • Кожна картка має власне підменю з Back-навігацією та переходом у налаштування світу.
    /// • Exit показує діалог підтвердження.
    /// • Додається SceneContext з усіма потрібними інсталерами (SignalBus, SaveSystem,
    ///   WorldCreation, HomeMenu).
    /// </summary>
    public sealed class HomeMenuSceneBuilderWindow : EditorWindow
    {
        // ─────────────────────────────────────────────────────────────────
        // Палітра
        // ─────────────────────────────────────────────────────────────────
        static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
        static readonly Color C_BG        = C("0E0F15");
        static readonly Color C_SIDEBAR   = C("13141E");
        static readonly Color C_SURFACE   = C("1E1F2C");
        static readonly Color C_SURFACE2  = C("272838");
        static readonly Color C_GOLD      = C("C8A84B");
        static readonly Color C_GOLD_DIM  = C("7A6630");
        static readonly Color C_RED       = C("D64045");
        static readonly Color C_GREEN     = C("3FB950");
        static readonly Color C_TEXT      = C("E8E8F0");
        static readonly Color C_TEXT_DIM  = C("8A8AA0");
        static readonly Color C_BTN       = C("22232F");
        static readonly Color C_BTN_H     = C("2E2F42");
        static readonly Color C_CLEAR     = new Color(0, 0, 0, 0);
        static readonly Color C_OVERLAY   = new Color(0, 0, 0, 0.86f);
        static readonly Color C_DIALOGBG  = new Color(0, 0, 0, 0.72f);

        // ─────────────────────────────────────────────────────────────────
        // Розміри
        // ─────────────────────────────────────────────────────────────────
        const float SIDEBAR_W = 280f;
        const float REF_W     = 1920f;
        const float REF_H     = 1080f;
        const float BTN_H     = 64f;   // великі боковикові кнопки
        const float BTN_GAP   = 18f;
        const float PAD       = 40f;
        const float ROW_H     = 48f;
        const float ITEM_SP   = 10f;

        // ─────────────────────────────────────────────────────────────────
        // Вікно
        // ─────────────────────────────────────────────────────────────────
        bool    _clearFirst = true;
        string  _gameplayScene = "Gamplay_Scene";
        bool    _addSceneContext = true;
        Vector2 _scroll;
        string  _log = "";

        [MenuItem("Moyva/HomeMenu/Побудувати сцену %#&H")]
        static void Open()
        {
            var w = GetWindow<HomeMenuSceneBuilderWindow>("HomeMenu Builder");
            w.minSize = new Vector2(420, 580);
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            GUILayout.Space(6);
            var title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.LabelField("HomeMenu Scene Builder", title, GUILayout.Height(26));
            EditorGUILayout.LabelField("Будує повну структуру в активній сцені.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(12);

            EditorGUILayout.LabelField("Параметри", EditorStyles.boldLabel);
            _clearFirst       = EditorGUILayout.Toggle("Очистити поточну сцену", _clearFirst);
            _addSceneContext  = EditorGUILayout.Toggle("Додати SceneContext + інсталери", _addSceneContext);
            _gameplayScene    = EditorGUILayout.TextField("Назва ігрової сцени", _gameplayScene);
            GUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "БОКОВИК (центр):\n" +
                "  Play\n" +
                "  Settings\n" +
                "  Exit (з діалогом підтвердження)\n\n" +
                "TOGGLE-ЛОГІКА:\n" +
                "  • Повторний клік = закрити меню.\n" +
                "  • Клік по іншій кнопці = замінити меню.\n\n" +
                "ГРАТИ → Соло / З Ботом / Мультиплеєр / Продовжити.\n" +
                "Соло, Бот, Create Room -> панель налаштування світу -> Запуск сцени.",
                MessageType.Info);
            GUILayout.Space(14);

            GUI.backgroundColor = new Color(0.3f, 0.72f, 0.3f);
            if (GUILayout.Button("Build сцени", GUILayout.Height(46)))
                DoBuild();
            GUI.backgroundColor = Color.white;

            if (!string.IsNullOrEmpty(_log))
            {
                GUILayout.Space(12);
                EditorGUILayout.LabelField("Лог", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(_log, EditorStyles.helpBox, GUILayout.MinHeight(120));
            }

            EditorGUILayout.EndScrollView();
        }

        // ─────────────────────────────────────────────────────────────────
        // Build
        // ─────────────────────────────────────────────────────────────────
        void DoBuild()
        {
            _log = "";
            try
            {
                var scene = SceneManager.GetActiveScene();
                Log($"Активна сцена: {(string.IsNullOrEmpty(scene.path) ? "(untitled)" : scene.path)}");

                if (_clearFirst)
                {
                    Log("Очищення сцени…");
                    foreach (var go in scene.GetRootGameObjects().ToArray())
                        Undo.DestroyObjectImmediate(go);
                }

                CreateEventSystem();
                var canvas = CreateCanvas();

                // Overlays на верхі
                var load = BuildLoadingOverlay(canvas.transform);
                var exitDlg = BuildConfirmDialog(canvas.transform, "ExitConfirmDialog",
                    "Вийти з гри", "Ви впевнені, що хочете вийти?");

                BuildBackground(canvas.transform);

                // Боковик + порожня область контенту
                var nav = canvas.AddComponent<HomeMenuNavigationController>();
                var side = BuildSidePanel(canvas.transform);
                var content = BuildContentArea(canvas.transform);

                // Корені панелей
                var playRoot     = BuildPlayModePanel(content, nav);
                var settingsRoot = BuildSettingsPanel(content, nav);

                // Підпанелі Play
                var soloPanel       = BuildSoloPanel(content, nav);
                var botPanel        = BuildBotPanel(content, nav);
                var multiPanel      = BuildMultiplayerPanel(content, nav);
                var createRoomPanel = BuildCreateRoomPanel(content, nav);
                var joinRoomPanel   = BuildJoinRoomPanel(content, nav);
                var continuePanel   = BuildContinuePanel(content, nav);

                // Спільна панель налаштування світу (універсальна — використовується Solo/Bot/CreateRoom).
                var worldSetupPanel = BuildWorldSetupPanel(content, nav);

                // Wire картки PlayMode → підпанелі
                WireCard(playRoot, "Card_Solo",        nav, soloPanel);
                WireCard(playRoot, "Card_Bot",         nav, botPanel);
                WireCard(playRoot, "Card_Multiplayer", nav, multiPanel);
                WireCard(playRoot, "Card_Continue",    nav, continuePanel);

                // Мультиплеєр → Create / Join
                WireCard(multiPanel, "Card_CreateRoom", nav, createRoomPanel);
                WireCard(multiPanel, "Card_JoinRoom",   nav, joinRoomPanel);

                // Solo / Bot / CreateRoom → WorldSetup (replace, щоб Back повертав у попередню панель)
                WireReplace(soloPanel,       "NextBtn", nav, worldSetupPanel);
                WireReplace(botPanel,        "NextBtn", nav, worldSetupPanel);
                WireReplace(createRoomPanel, "NextBtn", nav, worldSetupPanel);

                // Animator панелей (fade-переходи)
                var panelAnimator = canvas.AddComponent<HomeMenuPanelAnimator>();

                // Конфігурація контролера навігації: rootButtons + exit dialog + animator + gameplayScene
                WireNavigation(nav, side, playRoot, settingsRoot, exitDlg, panelAnimator, _gameplayScene);

                // SceneContext та інсталери
                if (_addSceneContext)
                    BuildSceneContext();

                // Гарантує, що згенеровані UI-елементи мають базові візуальні ресурси.
                EnsureVisualDefaults(canvas);

                EditorSceneManager.MarkSceneDirty(scene);
                Log("✔ Готово. Не забудь зберегти сцену (Ctrl+S).");
            }
            catch (Exception e)
            {
                Log("✘ " + e.GetType().Name + ": " + e.Message);
                Debug.LogException(e);
            }
        }

        void Log(string m) { _log += m + "\n"; Debug.Log("[HomeMenuBuilder] " + m); Repaint(); }

        // ─────────────────────────────────────────────────────────────────
        // EventSystem + Canvas
        // ─────────────────────────────────────────────────────────────────
        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(go, "HomeMenu EventSystem");
        }

        static GameObject CreateCanvas()
        {
            var go = new GameObject("UICanvas");
            Undo.RegisterCreatedObjectUndo(go, "HomeMenu Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(REF_W, REF_H);
            sc.matchWidthOrHeight  = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static void BuildBackground(Transform parent)
        {
            var rt = MakeRT("Background", parent);
            Stretch(rt);
            rt.SetSiblingIndex(0);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = C_BG;
            img.raycastTarget = false;
        }

        // ─────────────────────────────────────────────────────────────────
        // Side Panel (3 центровані кнопки)
        // ─────────────────────────────────────────────────────────────────
        struct SideRefs
        {
            public Button Play, Settings, Exit;
            public GameObject PlayIndicator, SettingsIndicator;
        }

        static SideRefs BuildSidePanel(Transform parent)
        {
            var rt = MakeRT("SidePanel", parent);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(SIDEBAR_W, 0);
            rt.anchoredPosition = Vector2.zero;

            rt.gameObject.AddComponent<Image>().color = C_SIDEBAR;

            // Права золота лінія
            var div = MakeRT("RightBorder", rt);
            div.anchorMin = new Vector2(1, 0);
            div.anchorMax = new Vector2(1, 1);
            div.pivot     = new Vector2(1, 0.5f);
            div.sizeDelta = new Vector2(2, 0);
            div.gameObject.AddComponent<Image>().color = C_GOLD_DIM;

            // Логотип згори
            var logo = MakeTMP("Logo", rt);
            logo.rectTransform.anchorMin = new Vector2(0, 1);
            logo.rectTransform.anchorMax = new Vector2(1, 1);
            logo.rectTransform.pivot     = new Vector2(0.5f, 1);
            logo.rectTransform.sizeDelta = new Vector2(0, 100);
            logo.rectTransform.anchoredPosition = new Vector2(0, -40);
            logo.text      = "MOYVA";
            logo.fontSize  = 44f;
            logo.fontStyle = FontStyles.Bold;
            logo.color     = C_GOLD;
            logo.alignment = TextAlignmentOptions.Center;
            logo.raycastTarget = false;

            // Контейнер центрованих кнопок
            var center = MakeRT("CenterButtons", rt);
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot     = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(SIDEBAR_W - 40, BTN_H * 3 + BTN_GAP * 2);
            center.anchoredPosition = Vector2.zero;

            var vl = center.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing                = BTN_GAP;
            vl.childAlignment         = TextAnchor.MiddleCenter;
            vl.childControlWidth      = true;
            vl.childControlHeight     = false;
            vl.childForceExpandWidth  = true;
            vl.childForceExpandHeight = false;

            var (playBtn, playInd) = BuildSideButton("SideBtn_Play",     "Play",        center, C_GOLD);
            var (setBtn,  setInd)  = BuildSideButton("SideBtn_Settings", "Settings",    center, C_GOLD);
            var (exitBtn, _)       = BuildSideButton("SideBtn_Exit",     "Exit",        center, C_RED);

            // Версія внизу
            var ver = MakeTMP("Version", rt);
            ver.rectTransform.anchorMin = new Vector2(0, 0);
            ver.rectTransform.anchorMax = new Vector2(1, 0);
            ver.rectTransform.pivot     = new Vector2(0.5f, 0);
            ver.rectTransform.sizeDelta = new Vector2(0, 30);
            ver.rectTransform.anchoredPosition = new Vector2(0, 12);
            ver.text      = "v0.1.0-alpha";
            ver.fontSize  = 11f;
            ver.color     = C_TEXT_DIM;
            ver.alignment = TextAlignmentOptions.Center;
            ver.raycastTarget = false;

            return new SideRefs { Play = playBtn, Settings = setBtn, Exit = exitBtn, PlayIndicator = playInd, SettingsIndicator = setInd };
        }

        static (Button btn, GameObject indicator) BuildSideButton(string name, string label, RectTransform parent, Color accent)
        {
            var rt = MakeRT(name, parent);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = BTN_H; le.preferredHeight = BTN_H;

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = C_BTN;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.colors = MakeColors(C_BTN, C_BTN_H, accent.WithAlpha(0.28f));

            // Active indicator (жовта смуга зліва)
            var ind = MakeRT("Indicator", rt);
            ind.anchorMin = new Vector2(0, 0.18f);
            ind.anchorMax = new Vector2(0, 0.82f);
            ind.pivot     = new Vector2(0, 0.5f);
            ind.sizeDelta = new Vector2(5, 0);
            ind.anchoredPosition = Vector2.zero;
            var indImg = ind.gameObject.AddComponent<Image>();
            indImg.color = accent;
            indImg.raycastTarget = false;
            ind.gameObject.SetActive(false);

            var txt = MakeTMP("Label", rt);
            txt.rectTransform.anchorMin = new Vector2(0, 0);
            txt.rectTransform.anchorMax = new Vector2(1, 1);
            txt.rectTransform.offsetMin = new Vector2(16, 0);
            txt.rectTransform.offsetMax = new Vector2(-12, 0);
            txt.text      = label;
            txt.fontSize  = 20f;
            txt.fontStyle = FontStyles.Bold;
            txt.color     = C_TEXT;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.raycastTarget = false;

            return (btn, ind.gameObject);
        }

        // ─────────────────────────────────────────────────────────────────
        // Content Area
        // ─────────────────────────────────────────────────────────────────
        static RectTransform BuildContentArea(Transform parent)
        {
            var rt = MakeRT("ContentArea", parent);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(SIDEBAR_W, 0);
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        // ─────────────────────────────────────────────────────────────────
        // PlayMode root (4 картки)
        // ─────────────────────────────────────────────────────────────────
        static GameObject BuildPlayModePanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var rt = MakeRT("PlayModePanel", content);
            Stretch(rt);
            rt.gameObject.AddComponent<Image>().color = C_BG;

            MakeTitle(rt, "Почати гру");

            var grid = MakeRT("CardGrid", rt);
            grid.anchorMin = new Vector2(0, 0);
            grid.anchorMax = new Vector2(1, 1);
            grid.offsetMin = new Vector2(PAD, PAD);
            grid.offsetMax = new Vector2(-PAD, -(PAD + 100));
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize        = new Vector2(340, 240);
            g.spacing         = new Vector2(24, 24);
            g.childAlignment  = TextAnchor.UpperCenter;
            g.startAxis       = GridLayoutGroup.Axis.Horizontal;
            g.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 2;

            BuildCard(grid, "Card_Solo",        "Соло",        "Зіграти одному — без ботів і мережі.", C_GOLD);
            BuildCard(grid, "Card_Bot",         "З ботом",     "Налаштуй бота і зіграй локально.",       C_GOLD);
            BuildCard(grid, "Card_Multiplayer", "Мультиплеєр", "Створи кімнату або приєднайся.",         C_GOLD);
            BuildCard(grid, "Card_Continue",    "Продовжити",  "Відкрити один із збережених світів.",    C_GREEN);

            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildCard(RectTransform parent, string name, string title, string subtitle, Color accent)
        {
            var rt = MakeRT(name, parent);
            rt.sizeDelta = new Vector2(340, 240);
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = C_SURFACE;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.colors = MakeColors(C_SURFACE, C_SURFACE2, accent.WithAlpha(0.3f));

            // Верхня акцент-смуга
            var accentBar = MakeRT("AccentBar", rt);
            accentBar.anchorMin = new Vector2(0, 1);
            accentBar.anchorMax = new Vector2(1, 1);
            accentBar.pivot     = new Vector2(0.5f, 1);
            accentBar.sizeDelta = new Vector2(0, 4);
            accentBar.gameObject.AddComponent<Image>().color = accent;

            var tt = MakeTMP("Title", rt);
            tt.rectTransform.anchorMin = new Vector2(0, 0.5f);
            tt.rectTransform.anchorMax = new Vector2(1, 1);
            tt.rectTransform.offsetMin = new Vector2(20, 10);
            tt.rectTransform.offsetMax = new Vector2(-20, -20);
            tt.text = title;
            tt.fontSize = 26f;
            tt.fontStyle = FontStyles.Bold;
            tt.color = C_TEXT;
            tt.alignment = TextAlignmentOptions.BottomLeft;
            tt.raycastTarget = false;

            var sub = MakeTMP("Subtitle", rt);
            sub.rectTransform.anchorMin = new Vector2(0, 0);
            sub.rectTransform.anchorMax = new Vector2(1, 0.5f);
            sub.rectTransform.offsetMin = new Vector2(20, 20);
            sub.rectTransform.offsetMax = new Vector2(-20, -6);
            sub.text = subtitle;
            sub.fontSize = 14f;
            sub.color = C_TEXT_DIM;
            sub.alignment = TextAlignmentOptions.TopLeft;
            sub.textWrappingMode = TMPro.TextWrappingModes.Normal;
            sub.raycastTarget = false;

            return rt.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────
        // Sub-panels
        // ─────────────────────────────────────────────────────────────────

        static GameObject BuildSoloPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "SoloPanel", "Соло гра", nav);
            MakeSection(body, "Опис");
            MakeDesc(body, "Гра без інших гравців та ботів. Зручно для дослідження світу та відпрацювання механік.");
            MakeSpacer(body, 20);
            AddNextBackFooter(rt, nav, nextLabel: "Налаштувати світ");
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildBotPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "BotPanel", "Гра з ботом", nav);

            MakeSection(body, "Налаштування бота");
            CreateLabeledDropdown(body, "Bot_Difficulty", "Складність бота",
                new[] { "Легкий", "Середній", "Важкий", "Кошмар" });
            CreateSliderRow(body, "Bot_Count", "Кількість ботів (1–4)", 1, 4, 1, wholeNumbers: true);
            CreateLabeledDropdown(body, "Bot_Strategy", "Стратегія",
                new[] { "Оборонна", "Агресивна", "Збалансована", "Експансіонистська" });
            CreateLabeledToggle(body, "Bot_Cheats", "Дозволити читерство боту", false);

            AddNextBackFooter(rt, nav, nextLabel: "Налаштувати світ");
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildMultiplayerPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "MultiplayerPanel", "Мультиплеєр", nav);

            MakeSection(body, "Оберіть варіант");

            var grid = MakeRT("ChoiceGrid", body);
            grid.sizeDelta = new Vector2(0, 200);
            var grLE = grid.gameObject.AddComponent<LayoutElement>();
            grLE.minHeight = 200;
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(320, 180);
            g.spacing = new Vector2(20, 20);
            g.childAlignment = TextAnchor.UpperLeft;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 2;

            BuildCard(grid, "Card_CreateRoom", "Створити кімнату",
                "Створи свою гру та запроси друзів.", C_GREEN);
            BuildCard(grid, "Card_JoinRoom",   "Приєднатись",
                "Введи код або обери з публічного списку.", C_GOLD);

            AddBackFooter(rt, nav);
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildCreateRoomPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "CreateRoomPanel", "Створити кімнату", nav);

            MakeSection(body, "Параметри кімнати");
            CreateLabeledInput(body, "Room_Name",     "Назва кімнати",        "Моя кімната");
            CreateLabeledInput(body, "Room_Password", "Пароль (опційно)",     "");
            CreateSliderRow(body, "Room_MaxPlayers", "Макс. гравців (2–8)", 2, 8, 4, wholeNumbers: true);
            CreateLabeledToggle(body, "Room_Public", "Публічна кімната (у списку)", true);

            AddNextBackFooter(rt, nav, nextLabel: "Налаштувати світ");
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildJoinRoomPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "JoinRoomPanel", "Приєднатись", nav);

            MakeSection(body, "Приєднання за кодом");
            CreateLabeledInput(body, "Join_Code", "Код кімнати", "ABCD-1234");
            var joinBtn = CreateButton(body, "Join_ByCodeBtn", "Приєднатись", C_BTN, C_GREEN.WithAlpha(0.22f));
            AttachNavAction(joinBtn, nav, MenuNavButton.ActionKind.LaunchGameplay);
            MakeSpacer(body, 16);
            MakeSection(body, "Або з публічного списку");

            // Placeholder список
            var (scrollGO, scrollContent) = BuildScrollView(body, "RoomsScroll");
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.sizeDelta = new Vector2(0, 220);
            scrollGO.AddComponent<LayoutElement>().minHeight = 220;

            var vl = scrollContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 6; vl.childControlWidth = true; vl.childControlHeight = false;
            vl.childForceExpandHeight = false;
            scrollContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 4; i++)
            {
                var row = MakeRT("Room_Item_" + i, scrollContent);
                row.sizeDelta = new Vector2(0, 48);
                row.gameObject.AddComponent<LayoutElement>().minHeight = 48;
                row.gameObject.AddComponent<Image>().color = C_SURFACE2;
                var l = MakeTMP("Name", row);
                l.rectTransform.anchorMin = new Vector2(0, 0); l.rectTransform.anchorMax = new Vector2(0.7f, 1);
                l.rectTransform.offsetMin = new Vector2(16, 0); l.rectTransform.offsetMax = Vector2.zero;
                l.text = $"Кімната #{i + 1} ({2 + i}/8)";
                l.fontSize = 14f; l.color = C_TEXT; l.alignment = TextAlignmentOptions.MidlineLeft;
                l.raycastTarget = false;
            }

            AddBackFooter(rt, nav);
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildContinuePanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "ContinuePanel", "Продовжити", nav);

            MakeSection(body, "Збережені світи");

            var (scrollGO, scrollContent) = BuildScrollView(body, "SavesScroll");
            scrollGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 400);
            scrollGO.AddComponent<LayoutElement>().minHeight = 400;

            var vl = scrollContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 8; vl.childControlWidth = true; vl.childControlHeight = false;
            vl.childForceExpandHeight = false;
            scrollContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 3; i++)
            {
                var row = MakeRT("Save_Item_" + i, scrollContent);
                row.sizeDelta = new Vector2(0, 84);
                row.gameObject.AddComponent<LayoutElement>().minHeight = 84;
                row.gameObject.AddComponent<Image>().color = C_SURFACE2;

                var name = MakeTMP("Name", row);
                name.rectTransform.anchorMin = new Vector2(0, 0.5f);
                name.rectTransform.anchorMax = new Vector2(0.7f, 1);
                name.rectTransform.offsetMin = new Vector2(20, 0);
                name.rectTransform.offsetMax = new Vector2(0, -8);
                name.text = $"Світ #{i + 1}";
                name.fontSize = 18f; name.color = C_TEXT; name.alignment = TextAlignmentOptions.MidlineLeft;
                name.raycastTarget = false;

                var info = MakeTMP("Info", row);
                info.rectTransform.anchorMin = new Vector2(0, 0);
                info.rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
                info.rectTransform.offsetMin = new Vector2(20, 8);
                info.rectTransform.offsetMax = Vector2.zero;
                info.text = "21 квітня 2026 • Середній • Нормально";
                info.fontSize = 12f; info.color = C_TEXT_DIM; info.alignment = TextAlignmentOptions.MidlineLeft;
                info.raycastTarget = false;

                var contBtn = CreateButton(row, "PlayBtn_" + i, "Продовжити", C_TEXT, C_GREEN.WithAlpha(0.22f));
                contBtn.GetComponent<RectTransform>().Apply(r =>
                {
                    r.anchorMin = new Vector2(0.7f, 0.2f);
                    r.anchorMax = new Vector2(1, 0.8f);
                    r.offsetMin = new Vector2(8, 0);
                    r.offsetMax = new Vector2(-16, 0);
                });
                AttachNavAction(contBtn, nav, MenuNavButton.ActionKind.LaunchGameplay);
            }

            AddBackFooter(rt, nav);
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static GameObject BuildWorldSetupPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var (rt, body) = BuildSubPanel(content, "WorldSetupPanel", "Налаштування світу", nav);

            MakeSection(body, "Основні");
            CreateLabeledInput(body, "W_Name", "Назва світу", "Новий світ");
            CreateLabeledInput(body, "W_Seed", "Seed генератора", "0");
            CreateButton(body, "W_RandomSeed", "Випадковий seed", C_TEXT, C_BTN).AddComponent<LayoutElement>().minHeight = BTN_H;
            CreateLabeledDropdown(body, "W_Size",    "Розмір карти", new[] { "Малий", "Середній", "Великий", "Custom" });
            CreateLabeledDropdown(body, "W_MapType", "Тип карти",    new[] { "Рівнина", "Острів", "Архіпелаг", "Пустеля", "Тундра" });

            MakeSection(body, "Правила");
            CreateLabeledDropdown(body, "W_Difficulty", "Складність", new[] { "Легко", "Нормально", "Важко", "Кошмар" });
            CreateLabeledInput(body, "W_Gold", "Стартове золото", "500");
            CreateLabeledInput(body, "W_Food", "Стартова їжа",    "200");

            MakeSection(body, "Генерація");
            CreateSliderRow(body, "W_Forest",   "Ліси [0..1]",       0, 1, 0.35f);
            CreateSliderRow(body, "W_Mountain", "Гори [0..1]",       0, 1, 0.2f);
            CreateSliderRow(body, "W_Water",    "Вода [0..1]",       0, 1, 0.25f);
            CreateSliderRow(body, "W_Village",  "Поселення [0..1]",  0, 1, 0.1f);
            CreateLabeledToggle(body, "W_Rivers", "Генерувати річки",  true);
            CreateLabeledToggle(body, "W_Biomes", "Генерувати біоми",  true);
            CreateLabeledToggle(body, "W_WFC",    "WFC полірування",    true);

            // Footer: Back / Start
            var footer = BuildFooterRow(rt);
            var backBtn  = CreateFooterButton(footer, "BackBtn",  "Назад",           C_TEXT_DIM, C_BTN);
            var startBtn = CreateFooterButton(footer, "StartBtn", "Запустити світ",  C_TEXT,     C_GREEN.WithAlpha(0.22f));
            AttachNavAction(backBtn,  nav, MenuNavButton.ActionKind.Back);
            AttachNavAction(startBtn, nav, MenuNavButton.ActionKind.LaunchGameplay);

            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────
        // Settings
        // ─────────────────────────────────────────────────────────────────
        static GameObject BuildSettingsPanel(RectTransform content, HomeMenuNavigationController nav)
        {
            var rt = MakeRT("SettingsPanel", content);
            Stretch(rt);
            rt.gameObject.AddComponent<Image>().color = C_BG;

            MakeTitle(rt, "Налаштування");

            var (scrollGO, body) = BuildScrollView(rt, "SettingsScroll");
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(PAD, PAD + BTN_H);
            scrollRT.offsetMax = new Vector2(-PAD, -(PAD + 90));

            var vl = body.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = ITEM_SP; vl.childControlWidth = true; vl.childControlHeight = false;
            vl.childForceExpandHeight = false; vl.childForceExpandWidth = true;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeSection(body, "Аудіо");
            CreateSliderRow(body, "Audio_Master", "Загальна гучність");
            CreateSliderRow(body, "Audio_Music",  "Музика");
            CreateSliderRow(body, "Audio_Sfx",    "Звукові ефекти");
            CreateSliderRow(body, "Audio_Ui",     "Інтерфейс");
            CreateLabeledToggle(body, "Audio_Mute", "Вимкнути звук", false);

            MakeSection(body, "Гра");
            CreateLabeledDropdown(body, "Game_Lang", "Мова", new[] { "Українська", "English" });
            CreateLabeledDropdown(body, "Game_Res",  "Роздільна здатність", new[] { "1920×1080", "1600×900", "1280×720" });
            CreateLabeledToggle(body, "Game_Fullscreen", "Повноекранний режим", true);
            CreateLabeledToggle(body, "Game_VSync",      "VSync",                true);

            MakeSection(body, "Соціальне");
            var socialRoot = MakeRT("SocialGrid", body);
            socialRoot.sizeDelta = new Vector2(0, 120);
            socialRoot.gameObject.AddComponent<LayoutElement>().minHeight = 120;
            var sg = socialRoot.gameObject.AddComponent<GridLayoutGroup>();
            sg.cellSize = new Vector2(220, 52); sg.spacing = new Vector2(10, 10);
            sg.childAlignment = TextAnchor.UpperLeft;
            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton(socialRoot, $"Social_{i}", "Посилання", C_TEXT, C_SURFACE2);
            }

            MakeSection(body, "Дані");
            var resetBtn  = CreateButton(body, "ResetDefaultsBtn", "Скинути налаштування", C_TEXT, C_BTN);
            resetBtn.AddComponent<LayoutElement>().minHeight = BTN_H;
            var delBtn    = CreateButton(body, "DeleteDataBtn", "Видалити всі дані", C_TEXT, C_RED.WithAlpha(0.2f));
            delBtn.AddComponent<LayoutElement>().minHeight = BTN_H;

            // Footer: Back
            var footer = BuildFooterRow(rt);
            var back = CreateFooterButton(footer, "BackBtn", "Назад", C_TEXT_DIM, C_BTN);
            AttachNavAction(back, nav, MenuNavButton.ActionKind.CloseAll);

            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────
        // Sub-panel шаблон
        // ─────────────────────────────────────────────────────────────────
        static (RectTransform root, RectTransform body) BuildSubPanel(
            RectTransform content, string name, string title, HomeMenuNavigationController nav)
        {
            var rt = MakeRT(name, content);
            Stretch(rt);
            rt.gameObject.AddComponent<Image>().color = C_BG;

            MakeTitle(rt, title);

            var (scrollGO, body) = BuildScrollView(rt, "Scroll");
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(PAD, PAD + BTN_H);
            scrollRT.offsetMax = new Vector2(-PAD, -(PAD + 90));

            var vl = body.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = ITEM_SP; vl.childControlWidth = true; vl.childControlHeight = false;
            vl.childForceExpandHeight = false; vl.childForceExpandWidth = true;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return (rt, body);
        }

        static RectTransform BuildFooterRow(RectTransform panel)
        {
            var row = MakeRT("FooterRow", panel);
            row.anchorMin = new Vector2(0, 0);
            row.anchorMax = new Vector2(1, 0);
            row.pivot     = new Vector2(0.5f, 0);
            row.sizeDelta = new Vector2(-PAD * 2, BTN_H);
            row.anchoredPosition = new Vector2(0, PAD * 0.5f);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 16; hl.childControlWidth = true; hl.childControlHeight = true;
            hl.childForceExpandWidth = true;
            return row;
        }

        static void AddBackFooter(RectTransform panel, HomeMenuNavigationController nav)
        {
            var footer = BuildFooterRow(panel);
            var back = CreateFooterButton(footer, "BackBtn", "Назад", C_TEXT_DIM, C_BTN);
            AttachNavAction(back, nav, MenuNavButton.ActionKind.Back);
        }

        static void AddNextBackFooter(RectTransform panel, HomeMenuNavigationController nav, string nextLabel)
        {
            var footer = BuildFooterRow(panel);
            var back = CreateFooterButton(footer, "BackBtn", "Назад", C_TEXT_DIM, C_BTN);
            var next = CreateFooterButton(footer, "NextBtn", nextLabel, C_TEXT, C_GREEN.WithAlpha(0.22f));
            AttachNavAction(back, nav, MenuNavButton.ActionKind.Back);
            // NextBtn's target буде призначено пізніше у WireReplace
        }

        static GameObject CreateFooterButton(RectTransform parent, string name, string label, Color textColor, Color bgColor)
        {
            return CreateButton(parent, name, label, textColor, bgColor);
        }

        // ─────────────────────────────────────────────────────────────────
        // Loading + Confirm Dialog
        // ─────────────────────────────────────────────────────────────────
        static GameObject BuildLoadingOverlay(Transform parent)
        {
            var rt = MakeRT("LoadingOverlay", parent);
            Stretch(rt);
            rt.gameObject.AddComponent<Image>().color = C_OVERLAY;

            var box = MakeRT("Box", rt);
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
            box.pivot = new Vector2(0.5f, 0.5f);
            box.sizeDelta = new Vector2(480, 180);
            box.gameObject.AddComponent<Image>().color = C_SURFACE;

            var status = MakeTMP("Status", box);
            status.rectTransform.anchorMin = new Vector2(0, 0.55f);
            status.rectTransform.anchorMax = new Vector2(1, 1);
            status.rectTransform.offsetMin = new Vector2(16, 0); status.rectTransform.offsetMax = new Vector2(-16, -16);
            status.text = "Завантаження…";
            status.fontSize = 18f; status.color = C_TEXT; status.alignment = TextAlignmentOptions.Center;
            status.raycastTarget = false;

            var bar = MakeRT("ProgressBar", box);
            bar.anchorMin = new Vector2(0, 0); bar.anchorMax = new Vector2(1, 0);
            bar.pivot = new Vector2(0.5f, 0); bar.sizeDelta = new Vector2(-32, 24);
            bar.anchoredPosition = new Vector2(0, 32);
            bar.gameObject.AddComponent<Image>().color = C_SURFACE2;
            var fill = MakeRT("Fill", bar);
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0.3f, 1);
            fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            fill.gameObject.AddComponent<Image>().color = C_GOLD;

            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        static Kruty1918.Moyva.HomeMenu.UI.ConfirmDialogView BuildConfirmDialog(Transform parent, string name, string title, string message)
        {
            var rt = MakeRT(name, parent);
            Stretch(rt);
            rt.gameObject.AddComponent<Image>().color = C_DIALOGBG;

            var card = MakeRT("Card", rt);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(520, 260);
            card.gameObject.AddComponent<Image>().color = C_SURFACE;

            var t = MakeTMP("Title", card);
            t.rectTransform.anchorMin = new Vector2(0, 1); t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.pivot = new Vector2(0.5f, 1);
            t.rectTransform.sizeDelta = new Vector2(-40, 60);
            t.rectTransform.anchoredPosition = new Vector2(0, -16);
            t.text = title; t.fontSize = 22f; t.fontStyle = FontStyles.Bold;
            t.color = C_TEXT; t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;

            var m = MakeTMP("Message", card);
            m.rectTransform.anchorMin = new Vector2(0, 0.35f);
            m.rectTransform.anchorMax = new Vector2(1, 0.8f);
            m.rectTransform.offsetMin = new Vector2(28, 0); m.rectTransform.offsetMax = new Vector2(-28, 0);
            m.text = message; m.fontSize = 15f; m.color = C_TEXT_DIM;
            m.alignment = TextAlignmentOptions.Center;
            m.textWrappingMode = TMPro.TextWrappingModes.Normal;
            m.raycastTarget = false;

            var btnRow = MakeRT("BtnRow", card);
            btnRow.anchorMin = new Vector2(0, 0); btnRow.anchorMax = new Vector2(1, 0);
            btnRow.pivot = new Vector2(0.5f, 0);
            btnRow.sizeDelta = new Vector2(-40, BTN_H);
            btnRow.anchoredPosition = new Vector2(0, 20);
            var hl = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 16; hl.childControlWidth = true; hl.childControlHeight = true;
            hl.childForceExpandWidth = true;

            var confirmBtn = CreateButton(btnRow, "ConfirmBtn", "Підтвердити",  C_TEXT,     C_RED.WithAlpha(0.22f));
            var cancelBtn  = CreateButton(btnRow, "CancelBtn",  "Скасувати",    C_TEXT_DIM, C_BTN);

            var view = rt.gameObject.AddComponent<Kruty1918.Moyva.HomeMenu.UI.ConfirmDialogView>();
            Ref(view, "titleLabel",    t);
            Ref(view, "messageLabel",  m);
            Ref(view, "confirmButton", confirmBtn.GetComponent<Button>());
            Ref(view, "cancelButton",  cancelBtn.GetComponent<Button>());

            rt.gameObject.SetActive(false);
            return view;
        }

        // ─────────────────────────────────────────────────────────────────
        // Wiring helpers (кнопки у SceneContext / nav)
        // ─────────────────────────────────────────────────────────────────

        static void WireNavigation(
            HomeMenuNavigationController nav, SideRefs side,
            GameObject playRoot, GameObject settingsRoot,
            Kruty1918.Moyva.HomeMenu.UI.ConfirmDialogView exitDialog,
            HomeMenuPanelAnimator panelAnimator,
            string gameplaySceneName)
        {
            var so = new SerializedObject(nav);

            // rootButtons: Play / Settings
            var list = so.FindProperty("rootButtons");
            list.arraySize = 2;

            void SetRoot(int idx, Button btn, GameObject panel, GameObject indicator)
            {
                var el = list.GetArrayElementAtIndex(idx);
                el.FindPropertyRelative("Button").objectReferenceValue     = btn;
                el.FindPropertyRelative("RootPanel").objectReferenceValue  = panel;
                el.FindPropertyRelative("Indicator").objectReferenceValue  = indicator;
            }
            SetRoot(0, side.Play,     playRoot,     side.PlayIndicator);
            SetRoot(1, side.Settings, settingsRoot, side.SettingsIndicator);

            // Exit button + dialog
            so.FindProperty("exitButton").objectReferenceValue        = side.Exit;
            so.FindProperty("exitConfirmDialog").objectReferenceValue = exitDialog;

            // Animator
            so.FindProperty("animator").objectReferenceValue          = panelAnimator;

            // Gameplay scene name
            so.FindProperty("gameplaySceneName").stringValue           = gameplaySceneName;

            so.ApplyModifiedPropertiesWithoutUndo();

            // Verify wiring via logs
            Debug.Log($"[Builder] Nav wired: rootButtons={list.arraySize}, exitButton={side.Exit != null}, " +
                      $"exitDlg={exitDialog != null}, animator={panelAnimator != null}, scene='{gameplaySceneName}'");
        }

        static void WireCard(GameObject parentPanel, string cardName, HomeMenuNavigationController nav, GameObject target)
        {
            var t = FindInChildren(parentPanel.transform, cardName);
            if (t == null) { Debug.LogWarning($"[Builder] Card '{cardName}' not found under {parentPanel.name}"); return; }
            AttachNavAction(t.gameObject, nav, MenuNavButton.ActionKind.Push, target);
        }

        static void WireReplace(GameObject parentPanel, string btnName, HomeMenuNavigationController nav, GameObject target)
        {
            var t = FindInChildren(parentPanel.transform, btnName);
            if (t == null) { Debug.LogWarning($"[Builder] Button '{btnName}' not found under {parentPanel.name}"); return; }
            AttachNavAction(t.gameObject, nav, MenuNavButton.ActionKind.Replace, target);
        }

        static void AttachNavAction(GameObject host, HomeMenuNavigationController nav, MenuNavButton.ActionKind kind, GameObject target = null)
        {
            if (host == null) return;
            var nb = host.GetComponent<MenuNavButton>() ?? host.AddComponent<MenuNavButton>();
            Ref(nb, "navigation", nav);
            var so = new SerializedObject(nb);
            so.FindProperty("action").enumValueIndex = (int)kind;
            so.FindProperty("target").objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Transform FindInChildren(Transform root, string name)
        {
            if (root == null) return null;
            foreach (Transform c in root)
            {
                if (c.name == name) return c;
                var sub = FindInChildren(c, name);
                if (sub != null) return sub;
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────
        // SceneContext
        // ─────────────────────────────────────────────────────────────────
        static void BuildSceneContext()
        {
            var ctxGO = new GameObject("SceneContext");
            Undo.RegisterCreatedObjectUndo(ctxGO, "SceneContext");
            var ctx = ctxGO.AddComponent<SceneContext>();

            // Додаємо інсталери як компоненти на окремих дочірніх об'єктах для читабельності
            var installersList = new List<MonoInstaller>();

            var sigGO = new GameObject("SignalBusInstaller");
            sigGO.transform.SetParent(ctxGO.transform);
            installersList.Add(sigGO.AddComponent<Kruty1918.Moyva.Signals.SignalBusInstaller>());

            var saveGO = new GameObject("SaveSystemInstaller");
            saveGO.transform.SetParent(ctxGO.transform);
            installersList.Add(saveGO.AddComponent<SaveSystemInstaller>());

            var wcGO = new GameObject("WorldCreationInstaller");
            wcGO.transform.SetParent(ctxGO.transform);
            installersList.Add(wcGO.AddComponent<WorldCreationInstaller>());

            var wcUIGO = new GameObject("WorldCreationUIInstaller");
            wcUIGO.transform.SetParent(ctxGO.transform);
            installersList.Add(wcUIGO.AddComponent<WorldCreationUIInstaller>());

            var hmGO = new GameObject("HomeMenuInstaller");
            hmGO.transform.SetParent(ctxGO.transform);
            installersList.Add(hmGO.AddComponent<HomeMenuInstaller>());

            // Зв'язати з SceneContext._installers
            var so = new SerializedObject(ctx);
            var list = so.FindProperty("_installers");
            if (list != null)
            {
                list.arraySize = installersList.Count;
                for (int i = 0; i < installersList.Count; i++)
                    list.GetArrayElementAtIndex(i).objectReferenceValue = installersList[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void EnsureVisualDefaults(GameObject canvasRoot)
        {
            if (canvasRoot == null) return;

            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var defaultFont = TMP_Settings.defaultFontAsset;
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer < 0) uiLayer = 5;

            int fixedImageSprite = 0;
            int fixedButtonTarget = 0;
            int fixedLabelFont = 0;
            int fixedLabelMaterial = 0;
            int fixedLabelRect = 0;
            int fixedLabelFaceAlpha = 0;
            int fixedSubMeshAlpha = 0;
            int contentLabelsChecked = 0;

            SetLayerRecursively(canvasRoot.transform, uiLayer);

            var images = canvasRoot.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image == null) continue;
                if (image.sprite == null)
                {
                    image.sprite = uiSprite;
                    fixedImageSprite++;
                }
            }

            var buttons = canvasRoot.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                if (button == null) continue;
                if (button.targetGraphic == null)
                {
                    var img = button.GetComponent<Image>();
                    if (img != null)
                    {
                        button.targetGraphic = img;
                        fixedButtonTarget++;
                    }
                }
            }

            var labels = canvasRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label == null) continue;

                if (label.font == null && defaultFont != null)
                {
                    label.font = defaultFont;
                    fixedLabelFont++;
                }
                if (label.fontSharedMaterial == null && label.font != null)
                {
                    label.fontSharedMaterial = label.font.material;
                    fixedLabelMaterial++;
                }

                // У частини TMP текстів material face alpha може стати 0.
                // Примусово піднімаємо видимість для основного матеріалу тексту.
                var sharedMat = label.fontSharedMaterial;
                if (sharedMat != null && sharedMat.HasProperty("_FaceColor"))
                {
                    var fc = sharedMat.GetColor("_FaceColor");
                    if (fc.a < 0.99f)
                    {
                        fc.a = 1f;
                        sharedMat.SetColor("_FaceColor", fc);
                        fixedLabelFaceAlpha++;
                    }
                }

                // Якщо текстовий Rect не розтягнуто по ширині й має майже нульову ширину,
                // він візуально "зникає". Розтягуємо по батьку.
                var rt = label.rectTransform;
                if (rt != null && Mathf.Abs(rt.anchorMin.x - rt.anchorMax.x) < 0.0001f && Mathf.Abs(rt.sizeDelta.x) < 1f)
                {
                    rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                    rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
                    rt.offsetMin = new Vector2(8f, rt.offsetMin.y);
                    rt.offsetMax = new Vector2(-8f, rt.offsetMax.y);
                    fixedLabelRect++;
                }

                var c = label.color;
                if (c.a < 0.98f) label.color = new Color(c.r, c.g, c.b, 1f);

                // Деякі тексти у Content мають дочірні TMP_SubMeshUI (Atlas/Material),
                // де матеріал може бути прозорим — робимо їх непрозорими і видимими.
                var subMeshes = label.GetComponentsInChildren<TMP_SubMeshUI>(true);
                foreach (var sm in subMeshes)
                {
                    if (sm == null) continue;
                    var smColor = sm.color;
                    if (smColor.a < 0.98f)
                        sm.color = new Color(smColor.r, smColor.g, smColor.b, 1f);

                    var smMat = sm.sharedMaterial;
                    if (smMat != null && smMat.HasProperty("_FaceColor"))
                    {
                        var smFace = smMat.GetColor("_FaceColor");
                        if (smFace.a < 0.99f)
                        {
                            smFace.a = 1f;
                            smMat.SetColor("_FaceColor", smFace);
                            fixedSubMeshAlpha++;
                        }
                    }
                }

                // Підтягуємо geometry після масових змін.
                label.ForceMeshUpdate();
            }

            // Діагностика саме для всіх гілок з назвою Content (як просив користувач).
            var allTransforms = canvasRoot.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t == null || t.name != "Content") continue;

                var branchLabels = t.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var lbl in branchLabels)
                {
                    if (lbl == null) continue;
                    contentLabelsChecked++;
                    string fontName = lbl.font != null ? lbl.font.name : "<null>";
                    string matName = lbl.fontSharedMaterial != null ? lbl.fontSharedMaterial.name : "<null>";
                    int subCount = lbl.GetComponentsInChildren<TMP_SubMeshUI>(true).Length;
                    Debug.Log($"[Builder][ContentText] {GetTransformPath(lbl.transform)} | font={fontName} | mat={matName} | subMeshes={subCount} | alpha={lbl.color.a:F2}");
                }
            }

            Debug.Log($"[Builder] Visual defaults: images={images.Length}, buttons={buttons.Length}, labels={labels.Length}; " +
                      $"fixedSprite={fixedImageSprite}, fixedButtonTarget={fixedButtonTarget}, fixedFont={fixedLabelFont}, " +
                      $"fixedMaterial={fixedLabelMaterial}, fixedLabelRect={fixedLabelRect}, fixedFaceAlpha={fixedLabelFaceAlpha}, " +
                      $"fixedSubMeshAlpha={fixedSubMeshAlpha}, contentLabelsChecked={contentLabelsChecked}, uiLayer={uiLayer}");
        }

        static void SetLayerRecursively(Transform root, int layer)
        {
            if (root == null) return;
            root.gameObject.layer = layer;
            foreach (Transform child in root)
                SetLayerRecursively(child, layer);
        }

        static string GetTransformPath(Transform target)
        {
            if (target == null) return "<null>";
            var stack = new Stack<string>();
            var t = target;
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }

            return string.Join("/", stack);
        }

        // ─────────────────────────────────────────────────────────────────
        // UI Factory
        // ─────────────────────────────────────────────────────────────────
        static RectTransform MakeRT(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static TextMeshProUGUI MakeTMP(string name, Transform parent)
        {
            var rt = MakeRT(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.color = C_TEXT; t.fontSize = 16f;
            return t;
        }

        static void MakeTitle(RectTransform panel, string text)
        {
            var t = MakeTMP("Title", panel);
            t.rectTransform.anchorMin = new Vector2(0, 1); t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.pivot = new Vector2(0.5f, 1);
            t.rectTransform.sizeDelta = new Vector2(-PAD * 2, 72);
            t.rectTransform.anchoredPosition = new Vector2(0, -PAD);
            t.text = text; t.fontSize = 30f; t.fontStyle = FontStyles.Bold;
            t.color = C_TEXT; t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;

            var div = MakeRT("TitleDivider", panel);
            div.anchorMin = new Vector2(0, 1); div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.sizeDelta = new Vector2(-PAD * 2, 1);
            div.anchoredPosition = new Vector2(0, -(PAD + 72));
            div.gameObject.AddComponent<Image>().color = C_GOLD_DIM;
        }

        static void MakeSection(RectTransform parent, string text)
        {
            var t = MakeTMP("Section_" + text, parent);
            t.rectTransform.sizeDelta = new Vector2(0, 28);
            t.gameObject.AddComponent<LayoutElement>().minHeight = 28;
            t.text = text; t.fontSize = 12f; t.fontStyle = FontStyles.Bold;
            t.color = C_GOLD_DIM; t.characterSpacing = 1.5f;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
        }

        static void MakeDesc(RectTransform parent, string text)
        {
            var t = MakeTMP("Desc", parent);
            t.rectTransform.sizeDelta = new Vector2(0, 80);
            t.gameObject.AddComponent<LayoutElement>().minHeight = 80;
            t.text = text; t.fontSize = 14f; t.color = C_TEXT_DIM;
            t.alignment = TextAlignmentOptions.TopLeft;
            t.textWrappingMode = TMPro.TextWrappingModes.Normal;
            t.raycastTarget = false;
        }

        static void MakeSpacer(RectTransform parent, float height)
        {
            var rt = MakeRT("Spacer", parent);
            rt.sizeDelta = new Vector2(0, height);
            rt.gameObject.AddComponent<LayoutElement>().minHeight = height;
        }

        static (Slider sl, TMP_Text lbl) CreateSliderRow(RectTransform parent, string id, string label,
            float min = 0f, float max = 1f, float val = 1f, bool wholeNumbers = false)
        {
            var row = MakeRT("Row_" + id, parent);
            row.sizeDelta = new Vector2(0, ROW_H);
            row.gameObject.AddComponent<LayoutElement>().minHeight = ROW_H;

            var name = MakeTMP("Label", row);
            name.rectTransform.anchorMin = new Vector2(0, 0); name.rectTransform.anchorMax = new Vector2(0.4f, 1);
            name.rectTransform.offsetMin = Vector2.zero; name.rectTransform.offsetMax = Vector2.zero;
            name.text = label; name.fontSize = 14f; name.color = C_TEXT;
            name.alignment = TextAlignmentOptions.MidlineLeft;
            name.raycastTarget = false;

            var slider = CreateSliderRaw(row, id);
            var slRT = slider.GetComponent<RectTransform>();
            slRT.anchorMin = new Vector2(0.41f, 0.2f); slRT.anchorMax = new Vector2(0.85f, 0.8f);
            slRT.offsetMin = Vector2.zero; slRT.offsetMax = Vector2.zero;
            slider.minValue = min; slider.maxValue = max; slider.value = val; slider.wholeNumbers = wholeNumbers;

            var valTxt = MakeTMP("Val", row);
            valTxt.rectTransform.anchorMin = new Vector2(0.86f, 0); valTxt.rectTransform.anchorMax = new Vector2(1, 1);
            valTxt.text = wholeNumbers ? $"{(int)val}" : $"{val:F2}";
            valTxt.fontSize = 14f; valTxt.color = C_GOLD;
            valTxt.alignment = TextAlignmentOptions.Midline;
            valTxt.raycastTarget = false;

            return (slider, valTxt);
        }

        static Slider CreateSliderRaw(RectTransform parent, string id)
        {
            var go = MakeRT("Slider_" + id, parent);
            var slider = go.gameObject.AddComponent<Slider>();
            go.gameObject.AddComponent<Image>().color = C_SURFACE2;

            var fillArea = MakeRT("Fill Area", go);
            fillArea.anchorMin = new Vector2(0, 0.25f); fillArea.anchorMax = new Vector2(1, 0.75f);
            fillArea.offsetMin = new Vector2(5, 0); fillArea.offsetMax = new Vector2(-15, 0);
            var fill = MakeRT("Fill", fillArea);
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1);
            fill.gameObject.AddComponent<Image>().color = C_GOLD.WithAlpha(0.8f);

            var slideArea = MakeRT("Handle Slide Area", go);
            slideArea.anchorMin = Vector2.zero; slideArea.anchorMax = Vector2.one;
            slideArea.offsetMin = new Vector2(10, 0); slideArea.offsetMax = new Vector2(-10, 0);
            var handle = MakeRT("Handle", slideArea);
            handle.sizeDelta = new Vector2(20, 0);
            handle.anchorMin = new Vector2(0, 0); handle.anchorMax = new Vector2(0, 1);
            handle.gameObject.AddComponent<Image>().color = C_GOLD;

            var so = new SerializedObject(slider);
            so.FindProperty("m_FillRect").objectReferenceValue   = fill;
            so.FindProperty("m_HandleRect").objectReferenceValue = handle;
            so.FindProperty("m_Direction").intValue = 0;
            so.FindProperty("m_MinValue").floatValue = 0f;
            so.FindProperty("m_MaxValue").floatValue = 1f;
            so.FindProperty("m_Value").floatValue    = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            return slider;
        }

        static TMP_InputField CreateLabeledInput(RectTransform parent, string id, string label, string placeholder)
        {
            var row = MakeRT("Row_" + id, parent);
            row.sizeDelta = new Vector2(0, ROW_H);
            row.gameObject.AddComponent<LayoutElement>().minHeight = ROW_H;

            var lbl = MakeTMP("Label", row);
            lbl.rectTransform.anchorMin = new Vector2(0, 0); lbl.rectTransform.anchorMax = new Vector2(0.4f, 1);
            lbl.text = label; lbl.fontSize = 14f; lbl.color = C_TEXT;
            lbl.alignment = TextAlignmentOptions.MidlineLeft; lbl.raycastTarget = false;

            var field = CreateInputFieldRaw(row, "Input_" + id, placeholder);
            var fRT = field.GetComponent<RectTransform>();
            fRT.anchorMin = new Vector2(0.41f, 0.1f); fRT.anchorMax = new Vector2(1, 0.9f);
            fRT.offsetMin = Vector2.zero; fRT.offsetMax = Vector2.zero;
            return field;
        }

        static TMP_InputField CreateInputFieldRaw(RectTransform parent, string name, string placeholder)
        {
            var go = MakeRT(name, parent);
            go.gameObject.AddComponent<Image>().color = C_SURFACE2;
            var field = go.gameObject.AddComponent<TMP_InputField>();

            var textArea = MakeRT("Text Area", go);
            Stretch(textArea);
            textArea.offsetMin = new Vector2(10, 6); textArea.offsetMax = new Vector2(-10, -6);
            textArea.gameObject.AddComponent<RectMask2D>();

            var ph = MakeTMP("Placeholder", textArea); Stretch(ph.rectTransform);
            ph.text = string.IsNullOrEmpty(placeholder) ? "Введіть значення…" : placeholder;
            ph.color = C_TEXT_DIM; ph.fontSize = 14f; ph.fontStyle = FontStyles.Italic;
            ph.raycastTarget = false;

            var txt = MakeTMP("Text", textArea); Stretch(txt.rectTransform);
            txt.text = ""; txt.color = C_TEXT; txt.fontSize = 14f;

            var so = new SerializedObject(field);
            so.FindProperty("m_TextViewport").objectReferenceValue  = textArea;
            so.FindProperty("m_TextComponent").objectReferenceValue = txt;
            so.FindProperty("m_Placeholder").objectReferenceValue   = ph;
            so.ApplyModifiedPropertiesWithoutUndo();

            return field;
        }

        static TMP_Dropdown CreateLabeledDropdown(RectTransform parent, string id, string label, string[] options)
        {
            var row = MakeRT("Row_" + id, parent);
            row.sizeDelta = new Vector2(0, ROW_H);
            row.gameObject.AddComponent<LayoutElement>().minHeight = ROW_H;

            var lbl = MakeTMP("Label", row);
            lbl.rectTransform.anchorMin = new Vector2(0, 0); lbl.rectTransform.anchorMax = new Vector2(0.4f, 1);
            lbl.text = label; lbl.fontSize = 14f; lbl.color = C_TEXT;
            lbl.alignment = TextAlignmentOptions.MidlineLeft; lbl.raycastTarget = false;

            var dd = CreateDropdownRaw(row, "DD_" + id, options);
            var rt = dd.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.41f, 0.1f); rt.anchorMax = new Vector2(1, 0.9f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return dd;
        }

        static TMP_Dropdown CreateDropdownRaw(RectTransform parent, string name, string[] options)
        {
            var go = MakeRT(name, parent);
            go.gameObject.AddComponent<Image>().color = C_SURFACE2;
            var dd = go.gameObject.AddComponent<TMP_Dropdown>();
            dd.colors = MakeColors(C_SURFACE2, C_SURFACE, C_GOLD.WithAlpha(0.3f));

            var caption = MakeTMP("Label", go);
            caption.rectTransform.anchorMin = new Vector2(0, 0);
            caption.rectTransform.anchorMax = new Vector2(1, 1);
            caption.rectTransform.offsetMin = new Vector2(12, 4);
            caption.rectTransform.offsetMax = new Vector2(-30, -4);
            caption.text = options.Length > 0 ? options[0] : "";
            caption.fontSize = 14f; caption.color = C_TEXT;
            caption.alignment = TextAlignmentOptions.MidlineLeft;
            caption.raycastTarget = false;

            var arrow = MakeTMP("Arrow", go);
            arrow.rectTransform.anchorMin = new Vector2(1, 0.5f);
            arrow.rectTransform.anchorMax = new Vector2(1, 0.5f);
            arrow.rectTransform.pivot = new Vector2(1, 0.5f);
            arrow.rectTransform.sizeDelta = new Vector2(24, 24);
            arrow.rectTransform.anchoredPosition = new Vector2(-8, 0);
            arrow.text = "v"; arrow.fontSize = 14f; arrow.color = C_TEXT_DIM;
            arrow.alignment = TextAlignmentOptions.Center;
            arrow.raycastTarget = false;

            // Template
            var tpl = MakeRT("Template", go);
            tpl.anchorMin = new Vector2(0, 0); tpl.anchorMax = new Vector2(1, 0);
            tpl.pivot = new Vector2(0.5f, 1); tpl.sizeDelta = new Vector2(0, 160);
            tpl.gameObject.AddComponent<Image>().color = C_SURFACE;
            var tplScroll = tpl.gameObject.AddComponent<ScrollRect>(); tplScroll.horizontal = false;

            var vp = MakeRT("Viewport", tpl); Stretch(vp);
            vp.gameObject.AddComponent<Image>().color = C_SURFACE;
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var cnt = MakeRT("Content", vp);
            cnt.anchorMin = new Vector2(0, 1); cnt.anchorMax = new Vector2(1, 1);
            cnt.pivot = new Vector2(0.5f, 1); cnt.sizeDelta = new Vector2(0, 28);
            var cntVL = cnt.gameObject.AddComponent<VerticalLayoutGroup>();
            cntVL.childControlHeight = true; cntVL.childControlWidth = true;

            var item = MakeRT("Item", cnt); item.sizeDelta = new Vector2(0, 32);
            var itemTgl = item.gameObject.AddComponent<Toggle>();
            item.gameObject.AddComponent<Image>().color = C_CLEAR;
            var itemBg = MakeRT("Item Background", item); Stretch(itemBg);
            itemBg.gameObject.AddComponent<Image>().color = C_SURFACE2;
            var itemLbl = MakeTMP("Item Label", item);
            itemLbl.rectTransform.anchorMin = new Vector2(0, 0); itemLbl.rectTransform.anchorMax = new Vector2(1, 1);
            itemLbl.rectTransform.offsetMin = new Vector2(12, 2); itemLbl.rectTransform.offsetMax = new Vector2(-4, -2);
            itemLbl.text = "Option"; itemLbl.fontSize = 14f; itemLbl.color = C_TEXT;
            itemLbl.alignment = TextAlignmentOptions.MidlineLeft;

            var itemBgImage = itemBg.gameObject.GetComponent<Image>();
            itemTgl.targetGraphic = itemBgImage;
            itemTgl.graphic = itemBgImage;

            tplScroll.viewport = vp;
            tplScroll.content = cnt;

            tpl.gameObject.SetActive(false);

            dd.template = tpl;
            dd.captionText = caption;
            dd.itemText = itemLbl;
            dd.ClearOptions();
            dd.AddOptions(options != null ? new List<string>(options) : new List<string>());
            if (dd.options != null && dd.options.Count > 0)
                dd.value = 0;

            return dd;
        }

        static (Toggle toggle, TMP_Text lbl) CreateLabeledToggle(RectTransform parent, string id, string label, bool defaultValue)
        {
            var row = MakeRT("Row_" + id, parent);
            row.sizeDelta = new Vector2(0, ROW_H);
            row.gameObject.AddComponent<LayoutElement>().minHeight = ROW_H;

            var lbl = MakeTMP("Label", row);
            lbl.rectTransform.anchorMin = new Vector2(0, 0); lbl.rectTransform.anchorMax = new Vector2(0.82f, 1);
            lbl.text = label; lbl.fontSize = 14f; lbl.color = C_TEXT;
            lbl.alignment = TextAlignmentOptions.MidlineLeft; lbl.raycastTarget = false;

            var toggleRT = MakeRT("Toggle", row);
            toggleRT.anchorMin = new Vector2(1, 0.2f); toggleRT.anchorMax = new Vector2(1, 0.8f);
            toggleRT.pivot = new Vector2(1, 0.5f);
            toggleRT.sizeDelta = new Vector2(52, 0);
            toggleRT.anchoredPosition = new Vector2(-8, 0);
            var tg = toggleRT.gameObject.AddComponent<Toggle>();
            var bg = toggleRT.gameObject.AddComponent<Image>(); bg.color = C_SURFACE2;
            tg.colors = MakeColors(C_SURFACE2, C_SURFACE, C_GOLD.WithAlpha(0.4f));
            var check = MakeRT("Checkmark", toggleRT); Stretch(check);
            check.gameObject.AddComponent<Image>().color = C_GOLD;

            var checkImage = check.gameObject.GetComponent<Image>();
            tg.targetGraphic = bg;
            if (checkImage != null)
                tg.graphic = checkImage;
            tg.isOn = defaultValue;
            return (tg, lbl);
        }

        static (GameObject go, RectTransform content) BuildScrollView(RectTransform parent, string name)
        {
            var go = MakeRT(name, parent);
            go.gameObject.AddComponent<Image>().color = C_CLEAR;
            var sr = go.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            var vp = MakeRT("Viewport", go); Stretch(vp);
            vp.gameObject.AddComponent<Image>().color = C_CLEAR;
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var cnt = MakeRT("Content", vp);
            cnt.anchorMin = new Vector2(0, 1); cnt.anchorMax = new Vector2(1, 1);
            cnt.pivot = new Vector2(0.5f, 1);
            cnt.sizeDelta = new Vector2(0, 0);

            var srSO = new SerializedObject(sr);
            srSO.FindProperty("m_Viewport").objectReferenceValue = vp;
            srSO.FindProperty("m_Content").objectReferenceValue  = cnt;
            srSO.ApplyModifiedPropertiesWithoutUndo();

            return (go.gameObject, cnt);
        }

        static GameObject CreateButton(Transform parent, string name, string label, Color textColor, Color bgColor)
        {
            var rt = MakeRT(name, parent);
            rt.sizeDelta = new Vector2(0, BTN_H);
            rt.gameObject.AddComponent<Image>().color = bgColor;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.colors = MakeColors(bgColor, bgColor * 1.15f, C_GOLD.WithAlpha(0.3f));
            var t = MakeTMP("Label", rt); Stretch(t.rectTransform);
            t.text = label; t.fontSize = 15f; t.fontStyle = FontStyles.Bold;
            t.color = textColor; t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return rt.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────
        // Reflective SerializedProperty helpers
        // ─────────────────────────────────────────────────────────────────
        static void Ref(Component comp, string field, UnityEngine.Object value)
        {
            if (comp == null) return;
            var so = new SerializedObject(comp);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[Builder] property '{field}' not found on {comp.GetType().Name}"); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static ColorBlock MakeColors(Color normal, Color highlight, Color pressed) => new ColorBlock
        {
            normalColor      = normal,
            highlightedColor = highlight,
            pressedColor     = pressed,
            selectedColor    = pressed.WithAlpha(0.7f),
            disabledColor    = normal.WithAlpha(0.4f),
            colorMultiplier  = 1f,
            fadeDuration     = 0.1f,
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers (file-scope)
    // ─────────────────────────────────────────────────────────────────────
    internal static class BuilderExt
    {
        internal static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
        internal static void Apply(this RectTransform rt, Action<RectTransform> a) => a(rt);
    }
}
