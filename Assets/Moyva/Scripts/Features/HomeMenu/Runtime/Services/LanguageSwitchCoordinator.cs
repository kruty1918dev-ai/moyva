using System;
using System.Threading.Tasks;
using Kruty1918.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Sequences a language switch requested from Settings:
    /// cover the menu -> apply the new language -> show log steps already in the
    /// new language -> warm up glyphs -> let the presenter re-render under cover ->
    /// smooth reveal of the fully translated menu.
    /// Залежності: <see cref="HomeMenuMoyvaUiViewController.OnLanguageSelected"/>,
    /// <see cref="ILocalizationService"/>, <see cref="LocalizationFontService"/>,
    /// <see cref="LanguageSwitchOverlay"/>.
    /// </summary>
    internal sealed class LanguageSwitchCoordinator : IInitializable, IDisposable
    {
        private readonly HomeMenuMoyvaUiViewController _view;
        private readonly ILocalizationService _localization;
        private readonly LocalizationFontService _fonts;
        private LanguageSwitchOverlay _overlay;
        private bool _switching;

        public LanguageSwitchCoordinator(
            HomeMenuMoyvaUiViewController view,
            [InjectOptional] ILocalizationService localization = null,
            [InjectOptional] LocalizationFontService fonts = null)
        {
            _view = view;
            _localization = localization;
            _fonts = fonts;
        }

        public void Initialize()
        {
            if (_view != null)
                _view.OnLanguageSelected += HandleLanguageSelected;
        }

        public void Dispose()
        {
            if (_view != null)
                _view.OnLanguageSelected -= HandleLanguageSelected;
            if (_overlay != null)
                UnityEngine.Object.Destroy(_overlay.gameObject);
            _overlay = null;
        }

        private void HandleLanguageSelected(int index) => _ = SwitchAsync(index);

        private async Task SwitchAsync(int index)
        {
            if (_switching || _localization == null) return;
            var languages = _localization.SupportedLanguages;
            if (index < 0 || index >= languages.Count) return;
            var target = languages[index];
            if (string.Equals(target.Id, _localization.CurrentLanguageId, StringComparison.OrdinalIgnoreCase))
                return;

            _switching = true;
            try
            {
                EnsureOverlay();
                // 1: Накриваємо меню. Далі всі статуси вже новою мовою.
                _overlay.SetProgress(0f);
                _overlay.SetStatus(string.Empty);
                await _overlay.FadeInAsync();

                // 2: Єдиний mutation path — сервіс зберігає вибір і сповіщає UI.
                _localization.TrySetLanguage(target.Id);
                _fonts?.Warmup(_localization.CurrentLanguage);

                // 3: Лог-кроки новою мовою, як вимагає сценарій перемикання.
                _overlay.SetStatus(_localization.TF("Switching language to {0}…", target.DisplayName));
                _overlay.SetProgress(0.25f);
                await Task.Delay(180);

                _overlay.SetStatus(_localization.T("Preparing fonts…"));
                _overlay.SetProgress(0.5f);
                await Task.Delay(160);

                _overlay.SetStatus(_localization.T("Translating interface…"));
                _overlay.SetProgress(0.75f);
                // Чекаємо два кадри: presenter робить dirty-render новою мовою під покривом.
                await Task.Delay(120);

                _overlay.SetStatus(_localization.T("Done"));
                _overlay.SetProgress(1f);
                await Task.Delay(140);

                // 4: Плавне проявлення повністю перекладеного меню.
                await _overlay.FadeOutAsync();
            }
            finally
            {
                _switching = false;
            }
        }

        private void EnsureOverlay()
        {
            if (_overlay != null) return;
            var go = new GameObject("LanguageSwitchOverlay", typeof(RectTransform));
            _overlay = go.AddComponent<LanguageSwitchOverlay>();
        }
    }

    /// <summary>
    /// Fullscreen uGUI cover with a progress bar and a localized status log line.
    /// Thin MonoBehaviour: самоанімує fade у Update, показується лише під час
    /// перемикання мови (sorting order вище scene transition 32000).
    /// </summary>
    internal sealed class LanguageSwitchOverlay : MonoBehaviour
    {
        private const int SortingOrder = 32500;
        private const float FadeSpeed = 5f;

        private CanvasGroup _group;
        private TMP_Text _statusText;
        private Image _fill;
        private float _targetAlpha;
        private TaskCompletionSource<bool> _fadeCompletion;

        private void Awake()
        {
            BuildVisuals();
            _group.alpha = 0f;
            _targetAlpha = 0f;
        }

        private void Update()
        {
            float alpha = Mathf.MoveTowards(_group.alpha, _targetAlpha,
                FadeSpeed * Time.unscaledDeltaTime);
            _group.alpha = alpha;
            if (Mathf.Approximately(alpha, _targetAlpha))
            {
                var completion = _fadeCompletion;
                _fadeCompletion = null;
                completion?.TrySetResult(true);
            }
            _group.blocksRaycasts = alpha > 0.02f;
        }

        public void SetStatus(string status)
        {
            if (_statusText != null)
                _statusText.text = status ?? string.Empty;
        }

        public void SetProgress(float value)
        {
            if (_fill != null)
                _fill.fillAmount = Mathf.Clamp01(value);
        }

        public Task FadeInAsync() => FadeToAsync(1f);

        public Task FadeOutAsync() => FadeToAsync(0f);

        private Task FadeToAsync(float target)
        {
            _targetAlpha = target;
            _fadeCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return _fadeCompletion.Task;
        }

        private void BuildVisuals()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            gameObject.AddComponent<GraphicRaycaster>();
            _group = gameObject.AddComponent<CanvasGroup>();

            var dim = CreateRect("Dim", transform, Color.black).rectTransform;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = Vector2.zero;
            dim.offsetMax = Vector2.zero;

            var center = CreateRect("Center", transform, Color.clear).rectTransform;
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(560f, 160f);
            center.anchoredPosition = Vector2.zero;

            _statusText = CreateText("Status", center, 22f, TextAlignmentOptions.Center);
            var statusRect = _statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 0.5f);
            statusRect.anchorMax = new Vector2(1f, 0.5f);
            statusRect.sizeDelta = new Vector2(0f, 60f);
            statusRect.anchoredPosition = new Vector2(0f, 30f);

            var barBack = CreateRect("BarBack", center, new Color(1f, 1f, 1f, 0.12f)).rectTransform;
            barBack.anchorMin = new Vector2(0f, 0.5f);
            barBack.anchorMax = new Vector2(1f, 0.5f);
            barBack.sizeDelta = new Vector2(0f, 6f);
            barBack.anchoredPosition = new Vector2(0f, -24f);

            _fill = CreateRect("Fill", barBack, new Color(1f, 1f, 1f, 0.9f));
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillAmount = 0f;
            var fillRect = _fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private static Image CreateRect(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, float size, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TMP_Text>();
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(1f, 1f, 1f, 0.92f);
            // TMP default font + global fallback list (LocalizationFontService) covers
            // кирилицю та інші письма — гліфи не пропадають під час перемикання.
            return text;
        }
    }
}
