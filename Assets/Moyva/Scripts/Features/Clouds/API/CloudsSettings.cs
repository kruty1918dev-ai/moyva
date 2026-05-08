using UnityEngine;

namespace Kruty1918.Moyva.Clouds.API
{
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

        [Header("Варіанти спрайтів")]
        [Tooltip("Список спрайтів хмаринок із вагами вибору.")]
        public CloudSpriteVariant[] CloudSprites;

        [Header("Рух")]
        [Tooltip("Діапазон горизонтальної швидкості у world units за секунду.")]
        public Vector2 SpeedRange = new Vector2(0.35f, 0.9f);

        [Tooltip("Діапазон масштабу хмаринок.")]
        public Vector2 ScaleRange = new Vector2(0.85f, 1.35f);

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

        [Header("Вигляд")]
        [Tooltip("Колір хмаринки. Білий колір зберігає оригінальні кольори спрайта.")]
        public Color CloudColor = Color.white;

        [Tooltip("Загальна прозорість хмаринок.")]
        [Range(0f, 1f)] public float CloudAlpha = 0.85f;

        [Tooltip("Назва sorting layer для хмаринок. Якщо порожньо, використовується Default.")]
        public string SortingLayerName = "Default";

        [Tooltip("Sorting order хмаринок.")]
        public int SortingOrder = 50;

        [Header("Тіні")]
        [Tooltip("Створювати темнішу копію хмаринки як тінь.")]
        public bool ShadowsEnabled = true;

        [Tooltip("Зміщення тіні відносно хмаринки у world units. Для top-down 2D зазвичай Y від'ємний, щоб тінь була нижче.")]
        public Vector2 ShadowOffset = new Vector2(0f, -0.45f);

        [Tooltip("Колір тіні хмаринки.")]
        public Color ShadowColor = new Color(0f, 0f, 0f, 1f);

        [Tooltip("Множник прозорості тіні відносно прозорості хмаринки.")]
        [Range(0f, 1f)] public float ShadowAlphaMultiplier = 0.35f;

        [Tooltip("Множник масштабу тіні відносно хмаринки.")]
        [Min(0.01f)] public float ShadowScaleMultiplier = 1.03f;

        [Tooltip("Зсув sorting order тіні відносно хмаринки.")]
        public int ShadowSortingOrderOffset = -1;

        private void OnValidate()
        {
            MaxActiveClouds = Mathf.Max(0, MaxActiveClouds);
            InitialClouds = Mathf.Clamp(InitialClouds, 0, MaxActiveClouds);
            SpawnIntervalRange = ClampRange(SpawnIntervalRange, 0.01f);
            SpeedRange = ClampRange(SpeedRange, 0.001f);
            ScaleRange = ClampRange(ScaleRange, 0.01f);
            SpawnHorizontalPadding = Mathf.Max(0f, SpawnHorizontalPadding);
            SpawnVerticalPadding = Mathf.Max(0f, SpawnVerticalPadding);
            DespawnHorizontalPadding = Mathf.Max(0f, DespawnHorizontalPadding);
            FadeDuration = Mathf.Max(0f, FadeDuration);
            CloudAlpha = Mathf.Clamp01(CloudAlpha);
            ShadowAlphaMultiplier = Mathf.Clamp01(ShadowAlphaMultiplier);
            ShadowScaleMultiplier = Mathf.Max(0.01f, ShadowScaleMultiplier);

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