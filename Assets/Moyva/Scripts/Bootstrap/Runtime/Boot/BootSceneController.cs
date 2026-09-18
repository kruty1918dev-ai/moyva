using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Контролер першої boot-сцени: показує логотип, слайдшоу й підказки,
    /// ініціалізує project-сервіси та асинхронно завантажує HomeMenu-сцену.
    ///
    /// Стадії: ініціалізація сервісів -> прелоад ресурсів -> завантаження сцени меню ->
    /// фінальна підготовка -> активація сцени.
    /// Залежності: Zenject <see cref="ProjectContext"/> (створюється тут уперше),
    /// опційний <see cref="ISceneTransitionService"/> для плавного переходу.
    /// </summary>
    public sealed class BootSceneController : MonoBehaviour
    {
        private const string Prefix = "[BootSceneController]";

        /// <summary>Підказки за замовчуванням (source keys, локалізуються перед показом).</summary>
        private static readonly string[] DefaultHints =
        {
            "Scout the map: fog of war hides resources and enemies.",
            "The castle controls nearby lands and unlocks construction.",
            "Economy is the backbone of an army. Balance gold and provisions.",
            "Rivers slow movement — plan crossings in advance.",
            "Walls can be joined with gates to let your own units through.",
            "Protect settlements: they fill the treasury and provide recruits.",
        };

        [Header("Перехід")]
        [Tooltip("Сцена, що завантажується після boot-екрана.")]
        [SerializeField] private string _nextSceneName = "HomeMenu";

        [Tooltip("Мінімальний час показу boot-екрана, секунд.")]
        [SerializeField] private float _minBootSeconds = 4f;

        [Header("Показ")]
        [Tooltip("Інтервал між автоматичними змінами слайдів, секунд.")]
        [SerializeField] private float _slideIntervalSeconds = 5f;

        [Tooltip("Інтервал між автоматичними змінами підказок, секунд.")]
        [SerializeField] private float _hintIntervalSeconds = 5.5f;

        [Header("Арт (опційно)")]
        [Tooltip("Шрифт TMP для всіх написів. Якщо порожньо — береться TMP default font.")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        [Tooltip("Якщо задано — використовується замість процедурної емблеми.")]
        [SerializeField] private Sprite _logoOverride;

        [Tooltip("Якщо не порожньо — використовуються замість процедурних слайдів.")]
        [SerializeField] private Sprite[] _slideOverrides = Array.Empty<Sprite>();

        [Tooltip("Якщо не порожньо — використовуються замість підказок за замовчуванням.")]
        [SerializeField] private string[] _hints = Array.Empty<string>();

        private BootScreenArtSet _art;
        private BootScreenView _view;
        private CancellationTokenSource _cts;
        private Kruty1918.Moyva.Shared.Localization.ILocalizationService _loca;

        private void Start()
        {
            // 1: Генеруємо процедурну графіку (або беремо override-спрайти з інспектора)
            //    та будуємо весь UI-шар у коді.
            _art = BootScreenArt.Create(_logoOverride, _slideOverrides);

            // Локалізація доступна до ProjectContext: сервіс читає каталоги з Resources
            // і persisted вибір з диска, тож boot-екран одразу говорить мовою гравця.
            _loca = new Kruty1918.Moyva.Shared.Localization.LocalizationService();
            var rawHints = _hints != null && _hints.Length > 0 ? _hints : DefaultHints;
            var hints = new string[rawHints.Length];
            for (int i = 0; i < rawHints.Length; i++)
                hints[i] = _loca?.T(rawHints[i]) ?? rawHints[i];
            var font = _fontAsset != null ? _fontAsset : TMP_Settings.defaultFontAsset;
            _view = new BootScreenView(_art.Slides, hints, _slideIntervalSeconds, _hintIntervalSeconds);
            _view.Build(transform as RectTransform, _art, font);

            // 2: Запускаємо послідовність завантаження паралельно з показом слайдшоу.
            _cts = new CancellationTokenSource();
            _ = RunBootAsync(_cts.Token);
        }

        private void Update()
        {
            _view?.Tick(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _art?.Dispose();
            _art = null;
        }

        /// <summary>Виконати всі стадії завантаження у фіксованій послідовності.</summary>
        private async Task RunBootAsync(CancellationToken ct)
        {
            float startTime = Time.realtimeSinceStartup;
            try
            {
                // 1: Даємо першому кадру намалюватись перед важкою синхронною роботою.
                SetStage("Preparation");
                await Task.Yield();
                Canvas.ForceUpdateCanvases();

                // 2: Одразу починаємо фонове завантаження сцени меню без активації.
                var loadOp = SceneManager.LoadSceneAsync(_nextSceneName, LoadSceneMode.Single);
                if (loadOp == null)
                    throw new InvalidOperationException(
                        $"{Prefix} SceneManager returned null for '{_nextSceneName}'. Ensure the scene is in Build Settings.");
                loadOp.allowSceneActivation = false;

                // 3: Ініціалізуємо project-контейнер: усі міжсценові сервіси створюються тут.
                SetStage("Initializing services");
                _view.SetProgressTarget(0.10f);
                ISceneTransitionService transition = InitializeProjectContext(ct);
                _view.SetProgressTarget(0.16f);

                // 4: Прогріваємо стартові ресурси, які знадобляться меню та грі.
                SetStage("Loading resources");
                await PreloadResourcesAsync(ct);
                _view.SetProgressTarget(0.24f);

                // 5: Чекаємо завантаження сцени до межі pre-activation (0.9 у Unity).
                SetStage("Loading the main menu");
                while (loadOp.progress < 0.9f)
                {
                    ct.ThrowIfCancellationRequested();
                    _view.SetProgressTarget(0.24f + (loadOp.progress / 0.9f) * 0.66f);
                    await Task.Yield();
                }
                _view.SetProgressTarget(0.9f);

                // 6: Дотримуємо мінімальну тривалість boot-екрана, щоб UI не блимнув.
                SetStage("Final preparations");
                _view.SetProgressTarget(0.97f);
                float elapsed = Time.realtimeSinceStartup - startTime;
                if (elapsed < _minBootSeconds)
                    await Task.Delay(Mathf.RoundToInt((_minBootSeconds - elapsed) * 1000f), ct);

                // 7: Чекаємо, доки бар візуально дійде до ~100%, і закриваємо екран переходом.
                _view.SetProgressTarget(1f);
                float guard = 0f;
                while (_view.Progress < 0.98f && guard < 1f)
                {
                    guard += Time.unscaledDeltaTime;
                    await Task.Yield();
                }
                if (transition != null)
                    await transition.CoverAsync(ct);

                // 8: Активуємо сцену й чекаємо повного завершення операції.
                loadOp.allowSceneActivation = true;
                while (!loadOp.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                // 9: Відкриваємо вже активну сцену меню.
                if (transition != null)
                    await transition.RevealAsync(CancellationToken.None);

                Debug.Log($"{Prefix} Boot completed. ActiveScene='{SceneManager.GetActiveScene().name}'.");
            }
            catch (OperationCanceledException)
            {
                // Сцена завантажилась раніше або об'єкт знищено — нормальне завершення.
            }
            catch (Exception exception)
            {
                Debug.LogError($"{Prefix} Boot failed: {exception}");
                SetStage("Loading failed");
            }
        }

        /// <summary>Створити ProjectContext і дістати опційний transition-сервіс.</summary>
        private ISceneTransitionService InitializeProjectContext(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                // 1: Перше звернення до Instance створює ProjectContext і всі project-сервіси.
                var context = ProjectContext.Instance;
                if (context == null || context.Container == null)
                    return null;

                // 2: Transition-сервіс опційний — boot має працювати й без нього.
                var sharedLoca = context.Container.TryResolve<Kruty1918.Moyva.Shared.Localization.ILocalizationService>();
                if (sharedLoca != null)
                    _loca = sharedLoca;
                return context.Container.TryResolve<ISceneTransitionService>();
            }
            catch (Exception exception)
            {
                // 3: Не блокуємо перехід у меню: SceneContext меню повторить ініціалізацію
                //    і покаже первинну помилку в консолі.
                Debug.LogError($"{Prefix} ProjectContext init failed: {exception.Message}");
                return null;
            }
        }

        /// <summary>Прогріти ресурси, які використовуються меню та раннім gameplay-стартом.</summary>
        private static async Task PreloadResourcesAsync(CancellationToken ct)
        {
            // 1: Аудіо-реєстр і стартові графічні налаштування — ті самі ресурси,
            //    що прогріває GameplayStartupPipeline перед gameplay-сценою.
            ResourceRequest audioRequest = Resources.LoadAsync<TextAsset>("MoyvaAudioRegistry");
            ResourceRequest graphicsRequest = Resources.LoadAsync<TextAsset>(GraphicsStartupSettingsSO.DefaultResourcePath);

            while (!audioRequest.isDone || !graphicsRequest.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            _ = audioRequest.asset;
            _ = graphicsRequest.asset;
        }

        private void SetStage(string stage)
        {
            _view?.SetStage(_loca?.T(stage) ?? stage);
            Debug.Log($"{Prefix} Stage: {stage}.");
        }
    }
}
