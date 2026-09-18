using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Візуальний шар boot-екрана: будує ієрархію UI під заданим RectTransform
    /// та анімує слайдшоу, ротацію підказок і згладжений прогрес-бар.
    /// Залежності: TMP-шрифт і набір спрайтів <see cref="BootScreenArtSet"/>.
    /// </summary>
    internal sealed class BootScreenView
    {
        /// <summary>Тривалість кросфейду між слайдами, секунд.</summary>
        private const float SlideFadeSeconds = 1.2f;

        /// <summary>Тривалість зникнення/появи підказки, секунд.</summary>
        private const float HintFadeSeconds = 0.45f;

        /// <summary>Швидкість експоненційного доганяння прогрес-бару.</summary>
        private const float ProgressSmoothing = 5f;

        private static readonly Color GoldText = new(0.93f, 0.8f, 0.52f);
        private static readonly Color DimText = new(0.72f, 0.74f, 0.8f);
        private static readonly Color HintTextColor = new(0.8f, 0.82f, 0.88f);

        private readonly Sprite[] _slides;
        private readonly string[] _hints;
        private readonly float _slideInterval;
        private readonly float _hintInterval;

        private CanvasGroup _slideGroupA;
        private CanvasGroup _slideGroupB;
        private Image _slideImageA;
        private Image _slideImageB;
        private AspectRatioFitter _slideFitA;
        private AspectRatioFitter _slideFitB;
        private TMP_Text _stageText;
        private TMP_Text _hintText;
        private CanvasGroup _hintGroup;
        private Image _progressFill;
        private TMP_Text _percentText;

        private float _progressShown;
        private float _progressTarget;

        private int _slideIndex;
        private float _slideTimer;
        private bool _slideOnB;
        private float _slideFadeT = -1f;
        private CanvasGroup _slideFadeIn;
        private CanvasGroup _slideFadeOut;

        private int _hintIndex = -1;
        private float _hintTimer;
        private float _hintFadeT = -1f;
        private string _pendingHint;

        public BootScreenView(Sprite[] slides, string[] hints, float slideInterval, float hintInterval)
        {
            _slides = slides ?? Array.Empty<Sprite>();
            _hints = hints ?? Array.Empty<string>();
            _slideInterval = Mathf.Max(1f, slideInterval);
            _hintInterval = Mathf.Max(1f, hintInterval);
        }

        /// <summary>Поточний відображуваний прогрес 0..1 (згладжений).</summary>
        public float Progress => _progressShown;

        /// <summary>Побудувати весь UI-шар під батьківським RectTransform.</summary>
        public void Build(RectTransform parent, BootScreenArtSet art, TMP_FontAsset font)
        {
            // 1: Темний градієнтний фон — основа під усіма шарами.
            CreateImage(parent, "Background", art.Background, stretch: true);

            // 2: Дві повноекранні картинки слайдів для кросфейду.
            _slideGroupA = SlideLayer(parent, "Slide_A", out _slideImageA, out _slideFitA);
            _slideGroupB = SlideLayer(parent, "Slide_B", out _slideImageB, out _slideFitB);
            _slideGroupB.alpha = 0f;

            // 3: Завіса для читабельності та віньєтка по краях.
            CreateImage(parent, "Veil", art.Veil, stretch: true);
            CreateImage(parent, "Vignette", art.Vignette, stretch: true);

            // 4: Логотип, назва й слоган у центрі.
            var logo = CreateImage(parent, "Logo", art.Logo, stretch: false);
            Anchor(logo.rectTransform, 0.5f, 0.66f, 0f, 0f, 230f, 230f);

            var title = CreateText(parent, "Title", font, "MOYVA", 66f, GoldText);
            title.fontStyle = FontStyles.Bold;
            title.characterSpacing = 18f;
            Anchor(title.rectTransform, 0.5f, 0.66f, 0f, -150f, 900f, 90f);

            var tagline = CreateText(parent, "Tagline", font, "TURN-BASED STRATEGY", 16f, DimText);
            tagline.characterSpacing = 10f;
            Anchor(tagline.rectTransform, 0.5f, 0.66f, 0f, -196f, 900f, 30f);

            // 5: Текст поточної стадії над прогрес-баром.
            _stageText = CreateText(parent, "StageText", font, string.Empty, 14f, DimText);
            _stageText.characterSpacing = 6f;
            Anchor(_stageText.rectTransform, 0.5f, 0f, 0f, 118f, 900f, 26f);

            // 6: Прогрес-бар: основа, заливка й рамка.
            var barTrack = CreateImage(parent, "BarTrack", art.BarTrack, stretch: false);
            Anchor(barTrack.rectTransform, 0.5f, 0f, 0f, 82f, 560f, 18f);
            _progressFill = CreateImage(parent, "BarFill", art.BarFill, stretch: false);
            Anchor(_progressFill.rectTransform, 0.5f, 0f, 0f, 82f, 560f, 18f);
            _progressFill.type = Image.Type.Filled;
            _progressFill.fillMethod = Image.FillMethod.Horizontal;
            _progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _progressFill.fillAmount = 0f;
            var barFrame = CreateImage(parent, "BarFrame", art.BarFrame, stretch: false);
            Anchor(barFrame.rectTransform, 0.5f, 0f, 0f, 82f, 560f, 18f);
            _percentText = CreateText(parent, "PercentText", font, "0%", 13f, Color.white);
            Anchor(_percentText.rectTransform, 0.5f, 0f, 0f, 82f, 560f, 18f);

            // 7: Ротаційна підказка внизу екрана.
            _hintGroup = CreateGroup(parent, "HintGroup");
            _hintText = CreateText(
                _hintGroup.transform as RectTransform, "HintText", font, string.Empty, 15f, HintTextColor);
            _hintText.fontStyle = FontStyles.Italic;
            Anchor(_hintText.rectTransform, 0.5f, 0f, 0f, 40f, 880f, 44f);

            // 8: Одразу показуємо перший слайд і першу підказку без затримки.
            if (_slides.Length > 0)
                SetSlide(_slideImageA, _slideFitA, _slides[0]);
            _slideIndex = 0;
            if (_slides.Length > 1)
                SetSlide(_slideImageB, _slideFitB, _slides[1]);
            SetNextHint();
            _hintGroup.alpha = 1f;
        }

        /// <summary>Встановити цільовий прогрес 0..1; бар плавно дожене його в Tick.</summary>
        public void SetProgressTarget(float value01)
        {
            _progressTarget = Mathf.Clamp01(value01);
        }

        /// <summary>Показати назву поточної стадії завантаження.</summary>
        public void SetStage(string stage)
        {
            if (_stageText != null)
                _stageText.text = (stage ?? string.Empty).ToUpperInvariant();
        }

        /// <summary>Просунути слайдшоу, підказки й згладжений прогрес на dt секунд.</summary>
        public void Tick(float dt)
        {
            TickProgress(dt);
            TickSlides(dt);
            TickHint(dt);
        }

        private void TickProgress(float dt)
        {
            // 1: Експоненційне згладжування до цілі, щоб бар не рухався ривками.
            _progressShown = Mathf.Lerp(_progressShown, _progressTarget, 1f - Mathf.Exp(-ProgressSmoothing * dt));
            if (Mathf.Abs(_progressTarget - _progressShown) < 0.0005f)
                _progressShown = _progressTarget;

            // 2: Відображаємо заливку й відсотки.
            if (_progressFill != null)
                _progressFill.fillAmount = _progressShown;
            if (_percentText != null)
                _percentText.text = Mathf.RoundToInt(_progressShown * 100f) + "%";
        }

        private void TickSlides(float dt)
        {
            if (_slides.Length < 2)
                return;

            // 1: Під час кросфейду інтерполюємо альфу обох шарів.
            if (_slideFadeT >= 0f)
            {
                _slideFadeT += dt / SlideFadeSeconds;
                float t = Mathf.Clamp01(_slideFadeT);
                float eased = t * t * (3f - 2f * t);
                if (_slideFadeIn != null)
                    _slideFadeIn.alpha = eased;
                if (_slideFadeOut != null)
                    _slideFadeOut.alpha = 1f - eased;
                if (t >= 1f)
                {
                    _slideFadeT = -1f;
                    if (_slideFadeOut != null)
                        _slideFadeOut.alpha = 0f;
                    _slideFadeIn = null;
                    _slideFadeOut = null;
                }
                return;
            }

            // 2: Між кросфейдами рахуємо інтервал до наступної зміни слайда.
            _slideTimer += dt;
            if (_slideTimer < _slideInterval)
                return;
            _slideTimer = 0f;

            // 3: Готуємо наступний слайд на прихованому шарі й запускаємо кросфейд.
            _slideIndex = (_slideIndex + 1) % _slides.Length;
            _slideOnB = !_slideOnB;
            var incoming = _slideOnB ? _slideGroupB : _slideGroupA;
            var outgoing = _slideOnB ? _slideGroupA : _slideGroupB;
            var incomingImage = _slideOnB ? _slideImageB : _slideImageA;
            var incomingFit = _slideOnB ? _slideFitB : _slideFitA;
            SetSlide(incomingImage, incomingFit, _slides[_slideIndex]);
            _slideFadeIn = incoming;
            _slideFadeOut = outgoing;
            _slideFadeT = 0f;
        }

        private void TickHint(float dt)
        {
            if (_hints.Length < 2 || _hintGroup == null)
                return;

            // 1: Фаза фейду: спочатку ховаємо стару підказку, потім показуємо нову.
            if (_hintFadeT >= 0f)
            {
                _hintFadeT += dt / HintFadeSeconds;
                float t = Mathf.Clamp01(_hintFadeT);
                if (_pendingHint != null)
                {
                    // 2: Перша половина — зникнення; у середині підміняємо текст.
                    if (t < 0.5f)
                    {
                        _hintGroup.alpha = 1f - t * 2f;
                    }
                    else
                    {
                        if (_hintText != null && _hintText.text != _pendingHint)
                            _hintText.text = _pendingHint;
                        _pendingHint = null;
                    }
                }
                else
                {
                    _hintGroup.alpha = (t - 0.5f) * 2f;
                }
                if (t >= 1f)
                    _hintFadeT = -1f;
                return;
            }

            // 2: Рахуємо інтервал до наступної підказки.
            _hintTimer += dt;
            if (_hintTimer < _hintInterval)
                return;
            _hintTimer = 0f;
            _pendingHint = _hints[(_hintIndex + 1) % _hints.Length];
            _hintIndex = (_hintIndex + 1) % _hints.Length;
            _hintFadeT = 0f;
        }

        private void SetNextHint()
        {
            if (_hints.Length == 0 || _hintText == null)
                return;
            _hintIndex = 0;
            _hintText.text = _hints[0];
        }

        private CanvasGroup SlideLayer(
            RectTransform parent, string name, out Image image, out AspectRatioFitter fit)
        {
            var group = CreateGroup(parent, name);
            var rect = group.transform as RectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            image = rect.gameObject.AddComponent<Image>();
            image.sprite = _slides.Length > 0 ? _slides[0] : null;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;
            fit = rect.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = image.sprite != null
                ? image.sprite.rect.width / image.sprite.rect.height
                : 16f / 9f;
            return group;
        }

        /// <summary>Призначити спрайт шару й оновити cover-співвідношення сторін.</summary>
        private static void SetSlide(Image image, AspectRatioFitter fit, Sprite sprite)
        {
            if (image != null)
                image.sprite = sprite;
            if (fit != null && sprite != null)
                fit.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        private static CanvasGroup CreateGroup(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            Stretch(go.transform as RectTransform);
            return go.GetComponent<CanvasGroup>();
        }

        private static Image CreateImage(RectTransform parent, string name, Sprite sprite, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            if (stretch)
                Stretch(go.transform as RectTransform);
            return image;
        }

        private static TMP_Text CreateText(
            RectTransform parent, string name, TMP_FontAsset font, string content,
            float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (font != null)
                text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void Anchor(
            RectTransform rect, float ax, float ay, float x, float y, float w, float h)
        {
            rect.anchorMin = new Vector2(ax, ay);
            rect.anchorMax = new Vector2(ax, ay);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }
    }
}
