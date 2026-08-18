using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Thin runtime pause view. It is created in its own canvas so a gameplay
    /// scene migration is not required for pause safety.
    /// </summary>
    internal sealed class GameplayPauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IGameStateService _gameState;
        private readonly IExitMatchCoordinator _exitCoordinator;
        private readonly IGamePauseModePolicy _pauseModePolicy;

        private GameObject _canvasObject;
        private GameObject _panelRoot;
        private Button _resumeButton;
        private Button _exitButton;
        private TMP_Text _exitLabel;
        private TMP_Text _statusLabel;
        private bool _exitConfirmationArmed;

        public GameplayPauseMenuPresenter(
            SignalBus signalBus,
            IGameStateService gameState,
            IExitMatchCoordinator exitCoordinator,
            [InjectOptional] IGamePauseModePolicy pauseModePolicy = null)
        {
            _signalBus = signalBus;
            _gameState = gameState;
            _exitCoordinator = exitCoordinator;
            _pauseModePolicy = pauseModePolicy;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GamePausedSignal>(OnPauseChanged);
            CreateView();
            SetVisible(_gameState.CurrentState == GameStateType.Paused);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GamePausedSignal>(OnPauseChanged);
            if (_canvasObject != null)
                UnityEngine.Object.Destroy(_canvasObject);
        }

        private void OnPauseChanged(GamePausedSignal signal)
            => SetVisible(signal.IsPaused);

        private void SetVisible(bool visible)
        {
            if (_panelRoot == null)
                return;

            _panelRoot.SetActive(visible);
            if (visible && EventSystem.current != null && _resumeButton != null)
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
            if (!visible)
                ResetExitConfirmation();
        }

        private void CreateView()
        {
            TMP_FontAsset font = ResolveFont();
            if (font == null)
            {
                Debug.LogError(
                    "[PauseMenu] TMP default font is missing; pause overlay was not created.");
                return;
            }

            _canvasObject = new GameObject(
                "Modal/PauseCanvas (Runtime)",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            _panelRoot = CreateImage(
                _canvasObject.transform,
                "PauseModal",
                new Color(0.02f, 0.018f, 0.016f, 0.72f));
            Stretch(_panelRoot.GetComponent<RectTransform>());

            GameObject dialog = CreateImage(
                _panelRoot.transform,
                "Dialog",
                new Color(0.115f, 0.095f, 0.072f, 0.99f));
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(420f, 330f);

            CreateText(
                dialog.transform,
                "Title",
                "Пауза",
                font,
                28f,
                FontStyles.Bold,
                new Vector2(28f, -28f),
                new Vector2(-28f, -72f));

            string modeHint = _pauseModePolicy?.IsMultiplayerSessionActive == true
                ? "Мережева гра продовжується. Локальне керування заблоковано."
                : "Симуляцію та ігровий час зупинено.";
            CreateText(
                dialog.transform,
                "ModeHint",
                modeHint,
                font,
                15f,
                FontStyles.Normal,
                new Vector2(28f, -78f),
                new Vector2(-28f, -130f));

            _resumeButton = CreateButton(
                dialog.transform,
                "ResumeButton",
                "Продовжити",
                font,
                new Vector2(28f, -150f),
                new Vector2(-28f, -194f),
                out _);
            _resumeButton.onClick.AddListener(_gameState.ResumeGame);

            _exitButton = CreateButton(
                dialog.transform,
                "ExitButton",
                "Вийти до меню",
                font,
                new Vector2(28f, -206f),
                new Vector2(-28f, -250f),
                out _exitLabel);
            _exitButton.onClick.AddListener(OnExitClicked);

            _statusLabel = CreateText(
                dialog.transform,
                "Status",
                string.Empty,
                font,
                13f,
                FontStyles.Normal,
                new Vector2(28f, -262f),
                new Vector2(-28f, -308f));
            _statusLabel.color = new Color(0.94f, 0.72f, 0.42f, 1f);

            _panelRoot.SetActive(false);
        }

        private async void OnExitClicked()
        {
            if (_exitCoordinator.IsExiting)
                return;

            if (!_exitConfirmationArmed)
            {
                _exitConfirmationArmed = true;
                _exitLabel.text = "Підтвердити вихід";
                _statusLabel.text = _pauseModePolicy?.IsMultiplayerSessionActive == true
                    ? "Сесію буде залишено без збереження локального матчу."
                    : "Поточний матч буде збережено.";
                return;
            }

            _resumeButton.interactable = false;
            _exitButton.interactable = false;
            _statusLabel.text = "Вихід…";

            ExitMatchResult result = await _exitCoordinator.ExitToMenuAsync();
            if (result.Succeeded)
                return;

            _resumeButton.interactable = true;
            _exitButton.interactable = true;
            _exitConfirmationArmed = false;
            _exitLabel.text = "Вийти до меню";
            _statusLabel.text = result.Cancelled
                ? "Вихід скасовано."
                : $"Не вдалося вийти: {result.Error}";
        }

        private void ResetExitConfirmation()
        {
            _exitConfirmationArmed = false;
            if (_exitLabel != null)
                _exitLabel.text = "Вийти до меню";
            if (_statusLabel != null)
                _statusLabel.text = string.Empty;
            if (_resumeButton != null)
                _resumeButton.interactable = true;
            if (_exitButton != null)
                _exitButton.interactable = true;
        }

        private static GameObject CreateImage(
            Transform parent,
            string name,
            Color color)
        {
            var result = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            result.transform.SetParent(parent, false);
            result.GetComponent<Image>().color = color;
            return result;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            TMP_FontAsset font,
            float size,
            FontStyles style,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
            rect.offsetMax = new Vector2(offsetMax.x, offsetMin.y);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = new Color(0.96f, 0.91f, 0.82f, 1f);
            text.text = value;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            TMP_FontAsset font,
            Vector2 offsetMin,
            Vector2 offsetMax,
            out TMP_Text labelText)
        {
            GameObject buttonObject = CreateImage(
                parent,
                name,
                new Color(0.29f, 0.22f, 0.14f, 1f));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
            rect.offsetMax = new Vector2(offsetMax.x, offsetMin.y);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.91f, 0.72f, 1f);
            colors.pressedColor = new Color(0.82f, 0.72f, 0.54f, 1f);
            colors.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.7f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;

            labelText = CreateText(
                buttonObject.transform,
                "Label",
                label,
                font,
                16f,
                FontStyles.Normal,
                new Vector2(14f, 0f),
                new Vector2(-14f, 0f));
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = new Vector2(14f, 0f);
            labelRect.offsetMax = new Vector2(-14f, 0f);
            labelText.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_FontAsset ResolveFont()
        {
            if (TMP_Settings.defaultFontAsset != null)
                return TMP_Settings.defaultFontAsset;

            TMP_FontAsset[] loaded =
                Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            return loaded.Length > 0 ? loaded[0] : null;
        }
    }
}
