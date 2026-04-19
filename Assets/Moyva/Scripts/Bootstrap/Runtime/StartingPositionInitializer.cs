using System;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// На старті нової гри обирає випадкову точку на мапі,
    /// хаотично розкриває туман навколо неї (імітація стартової позиції)
    /// та миттєво переміщує камеру туди.
    ///
    /// При завантаженні збереження — не втручається: туман відновлює FogOfWarSaveModule.
    /// </summary>
    internal sealed class StartingPositionInitializer : IInitializable, IDisposable
    {
        private const string StartVisionAnchorId = "bootstrap-start-vision-anchor";

        private readonly IFogOfWarService _fogOfWarService;
        private readonly ISaveService     _saveService;
        private readonly SignalBus        _signalBus;
        private readonly StartingPositionInitializerSettings _settings;
        private readonly BootstrapStartingPositionState _startingPositionState;
        private readonly ICameraMovement _cameraMovement;

        private bool _startAnchorRegistered;

        public StartingPositionInitializer(
            IFogOfWarService fogOfWarService,
            ISaveService     saveService,
            SignalBus        signalBus,
            StartingPositionInitializerSettings settings,
            BootstrapStartingPositionState startingPositionState,
            ICameraMovement cameraMovement)
        {
            _fogOfWarService       = fogOfWarService;
            _saveService           = saveService;
            _signalBus             = signalBus;
            _settings              = settings;
            _startingPositionState = startingPositionState;
            _cameraMovement        = cameraMovement;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        // ─── Основна логіка ───────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            // Якщо є збереження і автозавантаження ввімкнено —
            // туман відновить FogOfWarSaveModule, не перезаписуємо.
            if (SavePlayModeOptions.AutoLoadEnabled && _saveService.HasSave(0))
                return;

            var startPos = PickStartingPosition(signal.Width, signal.Height);

            // Зберігаємо позицію, щоб BootstrapGameInitializer міг розмістити замок тут.
            _startingPositionState.Set(startPos);

            RevealStartingArea(signal.Width, signal.Height, startPos);

            if (_settings.keepCoreFullyVisible)
            {
                // Уникаємо попередження FogOfWar «UnregisterUnit before Initialize»:
                // на першому виклику якорь ще не зареєстровано — пропускаємо unregister.
                if (_startAnchorRegistered)
                    _fogOfWarService.UnregisterUnit(StartVisionAnchorId);

                int visibleRange = _settings.coreVisibleRadiusOverride > 0
                    ? _settings.coreVisibleRadiusOverride
                    : Mathf.Max(1, _settings.innerRadius);
                _fogOfWarService.RegisterUnit(StartVisionAnchorId, startPos, visibleRange);
                _startAnchorRegistered = true;
            }

            TeleportMainCamera(startPos);

            Debug.Log($"[Bootstrap] Стартова позиція: {startPos}. Туман розкрито, камеру переміщено.");
        }

        private void TeleportMainCamera(Vector2Int startPos)
        {
            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
        }

        // ─── Вибір стартової точки ────────────────────────────────────────────

        private Vector2Int PickStartingPosition(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return Vector2Int.zero;

            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(_settings.relativeMarginFactor));
            int margin = Mathf.Max(_settings.minMarginFromBorder, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(width - margin - 1, xMin, width - 1);
            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(height - margin - 1, yMin, height - 1);

            int x = UnityEngine.Random.Range(xMin, xMax + 1);
            int y = UnityEngine.Random.Range(yMin, yMax + 1);
            return new Vector2Int(x, y);
        }

        // ─── Хаотичне розкриття туману ────────────────────────────────────────

        private void RevealStartingArea(int width, int height, Vector2Int center)
        {
            int innerRadius = Mathf.Max(1, _settings.innerRadius);
            int outerRadius = Mathf.Max(innerRadius, _settings.outerRadius);
            float outerPadding = Mathf.Max(0f, _settings.outerPadding);

            // Зміщення шуму, щоб кожна гра виглядала унікально
            float noiseOffX = UnityEngine.Random.Range(0f, _settings.noiseOffsetRange);
            float noiseOffY = UnityEngine.Random.Range(0f, _settings.noiseOffsetRange);

            var snapshot = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center.x, center.y));
                    if (dist > outerRadius + outerPadding) continue;

                    if (dist <= innerRadius)
                    {
                        // Ядро завжди повністю розвідане (а за потреби ще й повністю видиме через StartVisionAnchor).
                        snapshot[x, y] = true;
                        continue;
                    }

                    // Perlin-шум для органічних «дірок» і виступів
                    float noise = Mathf.PerlinNoise(
                        (x + noiseOffX) * Mathf.Max(0.01f, _settings.noiseScale),
                        (y + noiseOffY) * Mathf.Max(0.01f, _settings.noiseScale)); // 0..1

                    float probability;
                    // Периферія: імовірність спадає від центру + сильно залежить від шуму.
                    float t = Mathf.InverseLerp(innerRadius, outerRadius, dist);
                    probability = Mathf.Lerp(_settings.outerStartReveal, _settings.outerEndReveal, t) *
                                  (_settings.outerNoiseMinFactor + noise * _settings.outerNoiseFactor);

                    snapshot[x, y] = UnityEngine.Random.value < Mathf.Clamp01(probability);
                }
            }

            _fogOfWarService.LoadFromSnapshot(snapshot);
        }
    }
}
