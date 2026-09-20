using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Clouds.API
{
    /// <summary>CloudSpawnAreaMode — enum: хмари спавну Area режим.</summary>
    public enum CloudSpawnAreaMode
    {
        /// <summary>Варіант CameraViewport.</summary>
        CameraViewport = 0,
        /// <summary>Варіант MapBounds.</summary>
        MapBounds = 1
    }
/// <summary>CloudsSettings — class: Clouds налаштування.</summary>
[System.Serializable]
public sealed class CloudsSettings : MoyvaJsonConfigObject
    {
        /// <summary>увімкненої — bool.</summary>
        [Header("Загальне")]
        [Tooltip("Вмикає або вимикає систему хмаринок.")]
        public bool Enabled = true;

        [Tooltip("Максимальна кількість хмаринок, які одночасно існують у сцені.")]
        [Min(0)] public int MaxActiveClouds = 10;

        [Tooltip("Скільки хмаринок створити одразу після запуску системи.")]
        [Min(0)] public int InitialClouds = 3;

        /// <summary>Initial Clouds початку у виду — bool.</summary>
        [Tooltip("Якщо увімкнено, стартові хмаринки одразу розкладаються у видимій зоні. Якщо вимкнено, вони також стартують за краєм мапи.")]
        public bool InitialCloudsStartInView = true;

        /// <summary>спавну інтервалу дальності — Vector2.</summary>
        [Tooltip("Діапазон часу між спавнами у секундах.")]
        public Vector2 SpawnIntervalRange = new Vector2(1.5f, 4f);

        /// <summary>спавну Area режим — CloudSpawnAreaMode.</summary>
        [Tooltip("Зона, відносно якої створюються хмаринки. MapBounds розкладає їх по всій мапі, а не тільки біля поточного кадру камери.")]
        public CloudSpawnAreaMode SpawnAreaMode = CloudSpawnAreaMode.MapBounds;

        [Tooltip("Мінімальна бажана відстань між новою та вже активними хмаринками у world units (площина XZ).")]
        [Min(0f)] public float MinimumSpawnDistance = 5f;

        [Tooltip("Скільки випадкових позицій спробувати перед тим, як погодитись на останню знайдену.")]
        [Min(1)] public int SpawnPlacementAttempts = 10;

        /// <summary>хмари Prefabs — CloudPrefabVariant[].</summary>
        [Header("Варіанти префабів")]
        [Tooltip("Список 3D prefab-варіантів хмаринок із вагами вибору.")]
        public CloudPrefabVariant[] CloudPrefabs;

        /// <summary>висоти дальності — Vector2.</summary>
        [Header("Висота")]
        [Tooltip("Діапазон висоти хмаринок над базовою площиною мапи у world units.")]
        public Vector2 AltitudeRange = new Vector2(7f, 14f);

        /// <summary>швидкість дальності — Vector2.</summary>
        [Header("Рух")]
        [Tooltip("Діапазон горизонтальної швидкості у world units за секунду.")]
        public Vector2 SpeedRange = new Vector2(0.35f, 0.9f);

        /// <summary>масштаб дальності — Vector2.</summary>
        [Tooltip("Діапазон масштабу хмаринок.")]
        public Vector2 ScaleRange = new Vector2(0.85f, 1.35f);

        [Tooltip("Наскільки масштаб може випадково відхилятися від базового. 0 = без відхилення.")]
        [Range(0f, 0.5f)] public float ScaleJitter = 0.12f;

        [Tooltip("Наскільки непропорційно по X/Z може стискатися хмаринка. 0 = рівномірний масштаб.")]
        [Range(0f, 0.4f)] public float NonUniformScaleJitter = 0.15f;

        [Tooltip("Шанс руху зліва направо. 0 = тільки справа наліво, 1 = тільки зліва направо.")]
        [Range(0f, 1f)] public float LeftToRightChance = 0.5f;

        [Tooltip("Максимальне відхилення напрямку руху від осі X у градусах. Додає легкий дрейф по Z.")]
        [Range(0f, 45f)] public float DirectionAngleJitterDegrees = 12f;

        [Tooltip("Максимальна швидкість повільного обертання навколо вертикальної осі у градусах за секунду.")]
        [Range(0f, 10f)] public float MaxYawDegreesPerSecond = 1.5f;

        [Tooltip("Амплітуда легкого вертикального гойдання у world units. 0 = вимкнено.")]
        [Min(0f)] public float VerticalBobAmplitude = 0.12f;

        [Tooltip("Швидкість вертикального гойдання (циклів за секунду).")]
        [Min(0f)] public float VerticalBobSpeed = 0.05f;

        [Tooltip("Відстань за краєм зони спавну, де хмаринка створюється.")]
        [Min(0f)] public float SpawnPadding = 4f;

        [Tooltip("Відстань за протилежним краєм, після якої хмаринка знищується.")]
        [Min(0f)] public float DespawnPadding = 6f;

        [Tooltip("Тривалість плавної появи та зникнення у секундах.")]
        [Min(0f)] public float FadeDuration = 1.2f;

        /// <summary>карти краю затухання увімкненої — bool.</summary>
        [Header("Межі мапи")]
        [Tooltip("Плавно гасити хмаринки біля меж мапи замість різкого обрізання.")]
        public bool MapEdgeFadeEnabled = true;

        [Tooltip("Ширина смуги затухання біля краю мапи у world units.")]
        [Min(0f)] public float MapEdgeFadeWidth = 2f;

        /// <summary>ручного карти розміру — Vector2.</summary>
        [Tooltip("Розмір мапи для сцен без IGridService. У звичайній ігровій сцені використовується автоматичний розмір з IGridService.")]
        public Vector2 ManualMapSize = new Vector2(32f, 32f);

        /// <summary>ручного карти центру — Vector2.</summary>
        [Tooltip("Центр ручних меж мапи для сцен без IGridService.")]
        public Vector2 ManualMapCenter = new Vector2(15.5f, 15.5f);

        /// <summary>життєвого циклу Dissolve увімкненої — bool.</summary>
        [Header("Розчинення")]
        [Tooltip("Якщо увімкнено, хмаринка після випадкового часу життя починає плавно розчинятися, навіть якщо ще не дійшла до краю.")]
        public bool LifetimeDissolveEnabled = false;

        /// <summary>життєвого циклу дальності — Vector2.</summary>
        [Tooltip("Діапазон часу життя хмаринки до початку розчинення у секундах.")]
        public Vector2 LifetimeRange = new Vector2(12f, 24f);

        [Tooltip("Скільки секунд хмаринка плавно розчиняється після завершення часу життя.")]
        [Min(0f)] public float DissolveDuration = 3f;

        /// <summary>хмари кольору — Color.</summary>
        [Header("Вигляд")]
        [Tooltip("Базовий колірний тон хмаринок. Множиться на колір матеріалу.")]
        public Color CloudColor = Color.white;

        [Tooltip("Загальна прозорість хмаринок (реалізована dither-fade, без sorting-артефактів).")]
        [Range(0f, 1f)] public float CloudAlpha = 0.92f;

        [Tooltip("Наскільки сильно може варіюватися яскравість тону окремих хмаринок. 0 = однакові.")]
        [Range(0f, 0.3f)] public float CloudTintJitter = 0.08f;

        /// <summary>камери Proximity затухання увімкненої — bool.</summary>
        [Tooltip("Коли камера сильно наближена, хмаринки стають прозорішими, щоб не перекривати геймплей під ними.")]
        public bool CameraProximityFadeEnabled = true;

        /// <summary>камери затухання зум дальності — Vector2.</summary>
        [Tooltip("Zoom камери (FOV для perspective або orthographic size): X = дуже близько і мінімальна прозорість, Y = достатньо далеко і повна прозорість.")]
        public Vector2 CameraFadeZoomRange = new Vector2(5f, 14f);

        [Tooltip("Множник прозорості хмаринок при максимально близькому zoom.")]
        [Range(0f, 1f)] public float CloseCameraAlphaMultiplier = 0.28f;

        /// <summary>тіней увімкненої — bool.</summary>
        [Header("Тіні")]
        [Tooltip("Чи відкидають хмаринки реальні 3D тіні на мапу.")]
        public bool ShadowsEnabled = true;

        /// <summary>Обчислює хмари масштаб.</summary>
        public float EvaluateCloudScale(float altitude01, float random01)
        {
            altitude01 = Mathf.Clamp01(altitude01);
            random01 = Mathf.Clamp01(random01);

            float baseScale = Mathf.Lerp(ScaleRange.x, ScaleRange.y, altitude01);
            float jitter = Mathf.Lerp(1f - ScaleJitter, 1f + ScaleJitter, random01);
            return Mathf.Max(0.01f, baseScale * jitter);
        }

        private void OnValidate()
        {
            MaxActiveClouds = Mathf.Max(0, MaxActiveClouds);
            InitialClouds = Mathf.Clamp(InitialClouds, 0, MaxActiveClouds);
            SpawnIntervalRange = ClampRange(SpawnIntervalRange, 0.01f);
            MinimumSpawnDistance = Mathf.Max(0f, MinimumSpawnDistance);
            SpawnPlacementAttempts = Mathf.Max(1, SpawnPlacementAttempts);
            AltitudeRange = ClampRange(AltitudeRange, 0f);
            SpeedRange = ClampRange(SpeedRange, 0.001f);
            ScaleRange = ClampRange(ScaleRange, 0.01f);
            ScaleJitter = Mathf.Clamp(ScaleJitter, 0f, 0.5f);
            NonUniformScaleJitter = Mathf.Clamp(NonUniformScaleJitter, 0f, 0.4f);
            DirectionAngleJitterDegrees = Mathf.Clamp(DirectionAngleJitterDegrees, 0f, 45f);
            MaxYawDegreesPerSecond = Mathf.Clamp(MaxYawDegreesPerSecond, 0f, 10f);
            VerticalBobAmplitude = Mathf.Max(0f, VerticalBobAmplitude);
            VerticalBobSpeed = Mathf.Max(0f, VerticalBobSpeed);
            SpawnPadding = Mathf.Max(0f, SpawnPadding);
            DespawnPadding = Mathf.Max(0f, DespawnPadding);
            FadeDuration = Mathf.Max(0f, FadeDuration);
            MapEdgeFadeWidth = Mathf.Max(0f, MapEdgeFadeWidth);
            ManualMapSize = new Vector2(Mathf.Max(0.01f, ManualMapSize.x), Mathf.Max(0.01f, ManualMapSize.y));
            LifetimeRange = ClampRange(LifetimeRange, 0.01f);
            DissolveDuration = Mathf.Max(0f, DissolveDuration);
            CloudAlpha = Mathf.Clamp01(CloudAlpha);
            CloudTintJitter = Mathf.Clamp(CloudTintJitter, 0f, 0.3f);
            CameraFadeZoomRange = ClampRange(CameraFadeZoomRange, 0.01f);
            CloseCameraAlphaMultiplier = Mathf.Clamp01(CloseCameraAlphaMultiplier);

            if (CloudPrefabs == null)
                return;

            for (int i = 0; i < CloudPrefabs.Length; i++)
            {
                if (CloudPrefabs[i] != null)
                {
                    CloudPrefabs[i].Chance = Mathf.Max(0f, CloudPrefabs[i].Chance);
                    CloudPrefabs[i].ScaleMultiplier = Mathf.Max(0.01f, CloudPrefabs[i].ScaleMultiplier);
                }
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
