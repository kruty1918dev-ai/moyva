using UnityEngine;

namespace Kruty1918.Moyva.Clouds.API
{
    public enum CloudSpawnAreaMode
    {
        CameraViewport = 0,
        MapBounds = 1
    }

    [CreateAssetMenu(menuName = "Moyva/Clouds/Clouds Settings", fileName = "CloudsSettings")]
    public sealed class CloudsSettings : ScriptableObject
    {
        [Header("Загальне")]
        [Tooltip("Вмикає або вимикає систему хмаринок.")]
        public bool Enabled = true;

        [Tooltip("Максимальна кількість хмаринок, які одночасно існують у сцені.")]
        [Min(0)] public int MaxActiveClouds = 10;

        [Tooltip("Скільки хмаринок створити одразу після запуску системи.")]
        [Min(0)] public int InitialClouds = 3;

        [Tooltip("Якщо увімкнено, стартові хмаринки одразу розкладаються у видимій зоні камери. Якщо вимкнено, вони також стартують за екраном.")]
        public bool InitialCloudsStartInView = true;

        [Tooltip("Діапазон часу між спавнами у секундах.")]
        public Vector2 SpawnIntervalRange = new Vector2(1.5f, 4f);

        [Tooltip("Зона, відносно якої створюються хмаринки. MapBounds розкладає їх по всій мапі, а не тільки біля поточного кадру камери.")]
        public CloudSpawnAreaMode SpawnAreaMode = CloudSpawnAreaMode.MapBounds;

        [Tooltip("Мінімальна бажана відстань між новою та вже активними хмаринками у world units.")]
        [Min(0f)] public float MinimumSpawnDistance = 5f;

        [Tooltip("Скільки випадкових позицій спробувати перед тим, як погодитись на останню знайдену.")]
        [Min(1)] public int SpawnPlacementAttempts = 10;

        [Header("Варіанти спрайтів")]
        [Tooltip("Список спрайтів хмаринок із вагами вибору.")]
        public CloudSpriteVariant[] CloudSprites;

        [Header("Рух")]
        [Tooltip("Діапазон горизонтальної швидкості у world units за секунду.")]
        public Vector2 SpeedRange = new Vector2(0.35f, 0.9f);

        [Tooltip("Діапазон масштабу хмаринок.")]
        public Vector2 ScaleRange = new Vector2(0.85f, 1.35f);

        [Header("Altitude Variation")]
        [Tooltip("Наскільки випадково можна відхилити масштаб після прив'язки до вертикальної висоти. 0 = масштаб повністю визначається висотою.")]
        [Range(0f, 0.5f)] public float AltitudeScaleJitter = 0.12f;

        [Tooltip("Додатковий базовий mip bias для найнижчих хмаринок. Нижчі/менші хмари виглядають сильніше деформованими.")]
        [Range(0f, 4f)] public float LowAltitudeMipBias = 1.2f;

        [Tooltip("Додатковий базовий mip bias для найвищих хмаринок.")]
        [Range(0f, 4f)] public float HighAltitudeMipBias = 0.15f;

        [Tooltip("Множник CloudHeight для нижніх хмаринок.")]
        [Min(0f)] public float LowAltitudeHeightMultiplier = 0.7f;

        [Tooltip("Множник CloudHeight для верхніх хмаринок.")]
        [Min(0f)] public float HighAltitudeHeightMultiplier = 1.35f;

        [Tooltip("Шанс руху зліва направо. 0 = тільки справа наліво, 1 = тільки зліва направо.")]
        [Range(0f, 1f)] public float LeftToRightChance = 0.5f;

        [Tooltip("Відстань за межами екрана, де хмаринка створюється по X.")]
        [Min(0f)] public float SpawnHorizontalPadding = 3f;

        [Tooltip("Додатковий вертикальний запас навколо екрана, де може пройти хмаринка.")]
        [Min(0f)] public float SpawnVerticalPadding = 1.5f;

        [Tooltip("Відстань за протилежним краєм екрана, після якої хмаринка знищується.")]
        [Min(0f)] public float DespawnHorizontalPadding = 4f;

        [Tooltip("Тривалість плавної появи та зникнення у секундах.")]
        [Min(0f)] public float FadeDuration = 1.2f;

        [Header("Маска мапи")]
        [Tooltip("Показувати хмаринки тільки всередині меж мапи. Якщо GridService доступний, межі беруться автоматично з розміру мапи.")]
        public bool MapMaskEnabled = true;

        [Tooltip("Розмір мапи для сцен без GridService. У звичайній ігровій сцені використовується автоматичний розмір з GridService.")]
        public Vector2 ManualMapSize = new Vector2(32f, 32f);

        [Tooltip("Центр ручних меж мапи для сцен без GridService.")]
        public Vector2 ManualMapCenter = new Vector2(15.5f, 15.5f);

        [Tooltip("Ширина піксельного входу/виходу хмаринки біля краю маски у world units.")]
        [Min(0f)] public float MaskEdgeFadeWidth = 1.5f;

        [Tooltip("Кількість ступенів прозорості для піксельного стилю біля краю маски.")]
        [Min(1)] public int MaskEdgeFadeSteps = 5;

        [Header("Розчинення")]
        [Tooltip("Якщо увімкнено, хмаринка після випадкового часу життя починає плавно розчинятися, навіть якщо ще не дійшла до краю екрана.")]
        public bool LifetimeDissolveEnabled = false;

        [Tooltip("Діапазон часу життя хмаринки до початку розчинення у секундах.")]
        public Vector2 LifetimeRange = new Vector2(12f, 24f);

        [Tooltip("Скільки секунд хмаринка плавно розчиняється після завершення часу життя.")]
        [Min(0f)] public float DissolveDuration = 3f;

        [Header("Вигляд")]
        [Tooltip("Колір хмаринки. Білий колір зберігає оригінальні кольори спрайта.")]
        public Color CloudColor = Color.white;

        [Tooltip("Загальна прозорість хмаринок.")]
        [Range(0f, 1f)] public float CloudAlpha = 0.85f;

        [Tooltip("Матеріал для SpriteRenderer хмаринок. Якщо порожньо, система створить runtime-матеріал на Sprites/Default, щоб спрайт не підмінявся текстурою матеріалу.")]
        public Material SpriteMaterial;

        [Tooltip("Коли камера сильно наближена, хмаринки стають прозорішими, щоб не перекривати геймплей під ними.")]
        public bool CameraProximityFadeEnabled = true;

        [Tooltip("Orthographic size: X = дуже близько і мінімальна прозорість, Y = достатньо далеко і повна прозорість.")]
        public Vector2 CameraFadeOrthographicRange = new Vector2(5f, 14f);

        [Tooltip("Множник прозорості хмаринок при максимально близькому zoom.")]
        [Range(0f, 1f)] public float CloseCameraAlphaMultiplier = 0.28f;

        [Tooltip("Кількість ступенів прозорості для close-zoom fade. 1 = плавно без піксельних сходинок.")]
        [Min(1)] public int CameraFadeSteps = 4;

        [Tooltip("Назва sorting layer для хмаринок. Якщо порожньо, використовується Default.")]
        public string SortingLayerName = "Default";

        [Tooltip("Sorting order хмаринок.")]
        public int SortingOrder = 50;

        [Header("Тіні")]
        [Tooltip("Створювати темнішу копію хмаринки як тінь.")]
        public bool ShadowsEnabled = true;

        [Tooltip("Висота хмаринки над землею. Впливає на автоматичне зміщення, масштаб і прозорість тіні.")]
        [Min(0f)] public float CloudHeight = 2f;

        [Tooltip("Базове зміщення тіні відносно хмаринки у world units.")]
        public Vector2 ShadowOffset = new Vector2(0f, -0.45f);

        [Tooltip("Додаткове зміщення тіні на одну одиницю висоти хмаринки.")]
        public Vector2 ShadowOffsetPerHeight = new Vector2(0.08f, -0.18f);

        [Tooltip("Колір тіні хмаринки.")]
        public Color ShadowColor = new Color(0f, 0f, 0f, 1f);

        [Tooltip("Множник прозорості тіні відносно прозорості хмаринки.")]
        [Range(0f, 1f)] public float ShadowAlphaMultiplier = 0.35f;

        [Tooltip("Множник масштабу тіні відносно хмаринки.")]
        [Min(0.01f)] public float ShadowScaleMultiplier = 1.03f;

        [Tooltip("Додатковий масштаб тіні на одну одиницю висоти хмаринки.")]
        [Min(0f)] public float ShadowScalePerHeight = 0.04f;

        [Tooltip("Наскільки висота послаблює прозорість тіні. 0 = висота не впливає на прозорість.")]
        [Min(0f)] public float ShadowAlphaHeightFade = 0.08f;

        [Tooltip("Зсув sorting order тіні відносно хмаринки.")]
        public int ShadowSortingOrderOffset = -1;

        public float EvaluateCloudScale(float altitude01, float random01)
        {
            altitude01 = Mathf.Clamp01(altitude01);
            random01 = Mathf.Clamp01(random01);

            float baseScale = Mathf.Lerp(ScaleRange.x, ScaleRange.y, altitude01);
            float jitter = Mathf.Lerp(1f - AltitudeScaleJitter, 1f + AltitudeScaleJitter, random01);
            return Mathf.Max(0.01f, baseScale * jitter);
        }

        public float EvaluateCloudMipBias(float altitude01)
        {
            altitude01 = Mathf.Clamp01(altitude01);
            return Mathf.Lerp(LowAltitudeMipBias, HighAltitudeMipBias, altitude01);
        }

        public float EvaluateCloudVisualHeight(float altitude01)
        {
            altitude01 = Mathf.Clamp01(altitude01);
            float heightMultiplier = Mathf.Lerp(LowAltitudeHeightMultiplier, HighAltitudeHeightMultiplier, altitude01);
            return Mathf.Max(0f, CloudHeight * heightMultiplier);
        }

        private void OnValidate()
        {
            MaxActiveClouds = Mathf.Max(0, MaxActiveClouds);
            InitialClouds = Mathf.Clamp(InitialClouds, 0, MaxActiveClouds);
            SpawnIntervalRange = ClampRange(SpawnIntervalRange, 0.01f);
            MinimumSpawnDistance = Mathf.Max(0f, MinimumSpawnDistance);
            SpawnPlacementAttempts = Mathf.Max(1, SpawnPlacementAttempts);
            SpeedRange = ClampRange(SpeedRange, 0.001f);
            ScaleRange = ClampRange(ScaleRange, 0.01f);
            AltitudeScaleJitter = Mathf.Clamp(AltitudeScaleJitter, 0f, 0.5f);
            LowAltitudeMipBias = Mathf.Clamp(LowAltitudeMipBias, 0f, 4f);
            HighAltitudeMipBias = Mathf.Clamp(HighAltitudeMipBias, 0f, 4f);
            LowAltitudeHeightMultiplier = Mathf.Max(0f, LowAltitudeHeightMultiplier);
            HighAltitudeHeightMultiplier = Mathf.Max(0f, HighAltitudeHeightMultiplier);
            SpawnHorizontalPadding = Mathf.Max(0f, SpawnHorizontalPadding);
            SpawnVerticalPadding = Mathf.Max(0f, SpawnVerticalPadding);
            DespawnHorizontalPadding = Mathf.Max(0f, DespawnHorizontalPadding);
            FadeDuration = Mathf.Max(0f, FadeDuration);
            ManualMapSize = new Vector2(Mathf.Max(0.01f, ManualMapSize.x), Mathf.Max(0.01f, ManualMapSize.y));
            MaskEdgeFadeWidth = Mathf.Max(0f, MaskEdgeFadeWidth);
            MaskEdgeFadeSteps = Mathf.Max(1, MaskEdgeFadeSteps);
            LifetimeRange = ClampRange(LifetimeRange, 0.01f);
            DissolveDuration = Mathf.Max(0f, DissolveDuration);
            CloudAlpha = Mathf.Clamp01(CloudAlpha);
            CameraFadeOrthographicRange = ClampRange(CameraFadeOrthographicRange, 0.01f);
            CloseCameraAlphaMultiplier = Mathf.Clamp01(CloseCameraAlphaMultiplier);
            CameraFadeSteps = Mathf.Max(1, CameraFadeSteps);
            CloudHeight = Mathf.Max(0f, CloudHeight);
            ShadowAlphaMultiplier = Mathf.Clamp01(ShadowAlphaMultiplier);
            ShadowScaleMultiplier = Mathf.Max(0.01f, ShadowScaleMultiplier);
            ShadowScalePerHeight = Mathf.Max(0f, ShadowScalePerHeight);
            ShadowAlphaHeightFade = Mathf.Max(0f, ShadowAlphaHeightFade);

            if (CloudSprites == null)
                return;

            for (int i = 0; i < CloudSprites.Length; i++)
            {
                if (CloudSprites[i] != null)
                    CloudSprites[i].Chance = Mathf.Max(0f, CloudSprites[i].Chance);
            }
        }

        private static Vector2 ClampRange(Vector2 range, float minValue)
        {
            float min = Mathf.Max(minValue, Mathf.Min(range.x, range.y));
            float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
            return new Vector2(min, max);
        }
    }
}