using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер навігації HomeMenu: керує показом «кореневих» панелей (Play, Settings),
    /// вкладеним стеком підпанелей (наприклад PlayMode → Solo → WorldCreation),
    /// та підсвічуванням активної кнопки у боковику.
    ///
    /// Логіка кнопок боковика:
    ///   • Клік по кореневій кнопці, коли її дерево вже відкрите → закрити все (повернутись у порожній стан).
    ///   • Клік по іншій кореневій кнопці → замінити активне дерево на нове.
    ///   • Кнопка Exit не відкриває панель, а показує <see cref="exitConfirmDialog"/>.
    /// </summary>
    public sealed class HomeMenuNavigationController : MonoBehaviour
    {
        [Serializable]
        public struct RootButtonBinding
        {
            [Tooltip("Кнопка боковика (Play / Settings).")]
            public Button Button;

            [Tooltip("Кореневий GameObject панелі, яку ця кнопка відкриває.")]
            public GameObject RootPanel;

            [Tooltip("Smarter-індикатор активності (вертикальна смуга зліва на кнопці).")]
            public GameObject Indicator;
        }

        [Header("Кореневі кнопки (toggle)")]
        [SerializeField] private List<RootButtonBinding> rootButtons = new();

        [Header("Exit")]
        [SerializeField] private Button exitButton;
        [SerializeField] private ConfirmDialogView exitConfirmDialog;
        [SerializeField] private string exitDialogTitle    = "Вийти з гри";
        [SerializeField] private string exitDialogMessage  = "Ви впевнені, що хочете вийти?";

        [Header("Анімація")]
        [SerializeField] private HomeMenuPanelAnimator animator;

        [Header("Запуск гри")]
        [Tooltip("Назва ігрової сцени (має бути в Build Settings). Запускається через LaunchGameplay().")]
        [SerializeField] private string gameplaySceneName = "Gamplay_Scene";

        // Стек навігації серед усіх підпанелей
        private readonly Stack<GameObject> _stack = new();
        private GameObject _activeRoot;

        private void Awake()
        {
            // Підписати кореневі кнопки
            foreach (var b in rootButtons)
            {
                if (b.Button == null) continue;
                var captured = b;
                b.Button.onClick.AddListener(() => ToggleRoot(captured.RootPanel));
            }

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            // Початковий стан — все приховано
            foreach (var b in rootButtons)
            {
                if (b.RootPanel != null)  b.RootPanel.SetActive(false);
                if (b.Indicator != null)  b.Indicator.SetActive(false);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Public navigation API (викликається кнопками з Inspector через UnityEvent)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Відкриває підпанель як новий рівень у стеку навігації.</summary>
        public void Push(GameObject subPanel)
        {
            if (subPanel == null) return;

            // Приховати попередню підпанель (верхівку стеку) без очищення стеку
            if (_stack.Count > 0)
            {
                var top = _stack.Peek();
                if (top != null) top.SetActive(false);
            }

            subPanel.SetActive(true);
            _stack.Push(subPanel);
        }

        /// <summary>Повертається на попередній рівень навігації.</summary>
        public void Back()
        {
            if (_stack.Count == 0) return;

            var top = _stack.Pop();
            if (top != null) top.SetActive(false);

            if (_stack.Count > 0)
            {
                var prev = _stack.Peek();
                if (prev != null) prev.SetActive(true);
            }
        }

        /// <summary>Замінює поточну підпанель (без додавання в стек): корисно для свопу «Create Room» → «World Setup».</summary>
        public void Replace(GameObject subPanel)
        {
            if (_stack.Count > 0)
            {
                var top = _stack.Pop();
                if (top != null) top.SetActive(false);
            }
            Push(subPanel);
        }

        /// <summary>Повністю закриває активне дерево панелей (повертає стан «правий бік порожній»).</summary>
        public void CloseAll()
        {
            while (_stack.Count > 0)
            {
                var p = _stack.Pop();
                if (p != null) p.SetActive(false);
            }

            if (_activeRoot != null) _activeRoot.SetActive(false);
            _activeRoot = null;

            foreach (var b in rootButtons)
                if (b.Indicator != null) b.Indicator.SetActive(false);
        }

        /// <summary>Запускає ігрову сцену за назвою з <c>gameplaySceneName</c>.</summary>
        public void LaunchGameplay()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                Debug.LogError("[HomeMenuNavigationController] gameplaySceneName не задано.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        /// <summary>Змінює назву сцени для запуску в рантаймі.</summary>
        public void SetGameplayScene(string sceneName) => gameplaySceneName = sceneName;

        // ─────────────────────────────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────────────────────────────

        private void ToggleRoot(GameObject rootPanel)
        {
            if (rootPanel == null) return;

            // Клік по активній кореневій → закрити все
            if (_activeRoot == rootPanel)
            {
                CloseAll();
                return;
            }

            // Інакше — закриваємо старе дерево та відкриваємо нове
            CloseAll();
            _activeRoot = rootPanel;
            rootPanel.SetActive(true);

            // Оновити індикатори
            foreach (var b in rootButtons)
                if (b.Indicator != null)
                    b.Indicator.SetActive(b.RootPanel == rootPanel);
        }

        private void OnExitClicked()
        {
            if (exitConfirmDialog != null)
            {
                exitConfirmDialog.Show(
                    exitDialogTitle,
                    exitDialogMessage,
                    onConfirm: QuitApplication,
                    onCancel: () => exitConfirmDialog.Hide());
            }
            else
            {
                QuitApplication();
            }
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
