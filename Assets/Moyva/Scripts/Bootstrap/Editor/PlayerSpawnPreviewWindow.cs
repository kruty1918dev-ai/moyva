using Kruty1918.Moyva.Bootstrap.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    public sealed class PlayerSpawnPreviewWindow : EditorWindow
    {
        private const string DefaultBootstrapAssetPath = "Assets/Moyva/SO/Bootstrap/BootstrapInstallerConfig.asset";
        private const float PreviewPadding = 20f;
        private const int DefaultPreviewSeed = 1918;

        private readonly struct PreviewSpawnPoint
        {
            public readonly Vector2Int Tile;
            public readonly bool IsBot;
            public readonly int SlotIndex;

            public PreviewSpawnPoint(Vector2Int tile, bool isBot, int slotIndex)
            {
                Tile = tile;
                IsBot = isBot;
                SlotIndex = slotIndex;
            }
        }

        private BootstrapInstallerConfigSO _config;
        private SerializedObject _serializedConfig;
        private int _previewWidth = 64;
        private int _previewHeight = 64;
        private int _previewHumans = 2;
        private int _previewBots = 2;
        private int _previewSeed = DefaultPreviewSeed;
        private bool _showGrid = true;
        private bool _showDistanceLinks = true;
        private bool _showRevealRadius = true;
        private Vector2 _scroll;

        public static void Open()
        {
            var window = GetWindow<PlayerSpawnPreviewWindow>("Стартовий спавн");
            window.minSize = new Vector2(560f, 640f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadDefaultConfig();
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUI.BeginChangeCheck();
            _config = (BootstrapInstallerConfigSO)EditorGUILayout.ObjectField("Bootstrap config", _config, typeof(BootstrapInstallerConfigSO), false);
            if (EditorGUI.EndChangeCheck())
                _serializedConfig = _config != null ? new SerializedObject(_config) : null;

            if (_config == null)
            {
                EditorGUILayout.HelpBox("Признач BootstrapInstallerConfigSO. За замовчуванням інструмент шукає Assets/Moyva/SO/Bootstrap/BootstrapInstallerConfig.asset.", MessageType.Info);
                if (GUILayout.Button("Завантажити стандартний config"))
                    LoadDefaultConfig();
                return;
            }

            _serializedConfig ??= new SerializedObject(_config);
            _serializedConfig.Update();

            SerializedProperty settings = _serializedConfig.FindProperty("_startingPositionSettings");
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Property '_startingPositionSettings' не знайдено.", MessageType.Error);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSpawnSettings(settings);
            DrawPreviewControls(settings);
            DrawPreview(settings);
            EditorGUILayout.EndScrollView();

            _serializedConfig.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            Rect header = GUILayoutUtility.GetRect(10f, 58f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(header, new Color(0.12f, 0.15f, 0.17f));

            Rect accent = new Rect(header.x, header.y, 5f, header.height);
            EditorGUI.DrawRect(accent, new Color(0.27f, 0.64f, 0.86f));

            GUI.Label(new Rect(header.x + 16f, header.y + 8f, header.width - 32f, 20f), "Дизайнер стартового спавну", EditorStyles.boldLabel);
            GUI.Label(
                new Rect(header.x + 16f, header.y + 30f, header.width - 32f, 18f),
                "Налаштовує ті самі поля, які використовуються при старті нового світу.",
                EditorStyles.miniLabel);
        }

        private void DrawSpawnSettings(SerializedProperty settings)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Runtime налаштування", EditorStyles.boldLabel);
                DrawProperty(settings, "multiplayerStartSlots", "Резерв слотів", "Скільки стартових позицій хост резервує для гравців і ботів.");
                DrawProperty(settings, "minAStarDistanceBetweenPlayers", "Мін. дистанція", "Мінімальна відстань між стартами у тайлах. У runtime використовується A*, якщо pathfinder доступний.");
                DrawProperty(settings, "startCandidateAttempts", "Спроби пошуку", "Скільки випадкових кандидатів перевіряти перед fallback-скануванням.");
                DrawProperty(settings, "minMarginFromBorder", "Відступ від краю", "Мінімальний відступ стартів від меж мапи.");
                DrawProperty(settings, "relativeMarginFactor", "Відносний відступ", "Додатковий відступ як частка від меншої сторони мапи.");
                DrawProperty(settings, "startMinHeight", "Мін. висота", "Нижня межа висоти тайла для старту.");
                DrawProperty(settings, "startMaxHeight", "Макс. висота", "Верхня межа висоти тайла для старту.");
                DrawProperty(settings, "requireHeightMapForStart", "Вимагати HeightMap", "Якщо увімкнено, старт не обирається без HeightMap.");
            }
        }

        private void DrawPreviewControls(SerializedProperty settings)
        {
            int slots = Mathf.Max(1, settings.FindPropertyRelative("multiplayerStartSlots").intValue);
            _previewHumans = Mathf.Clamp(_previewHumans, 0, slots);
            _previewBots = Mathf.Clamp(_previewBots, 0, slots - _previewHumans);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Превʼю", EditorStyles.boldLabel);
                _previewWidth = EditorGUILayout.IntSlider("Ширина мапи", _previewWidth, 16, 256);
                _previewHeight = EditorGUILayout.IntSlider("Висота мапи", _previewHeight, 16, 256);
                _previewHumans = EditorGUILayout.IntSlider("Гравці", _previewHumans, 0, slots);
                _previewBots = EditorGUILayout.IntSlider("Боти", _previewBots, 0, slots - _previewHumans);
                _previewSeed = EditorGUILayout.IntField("Seed превʼю", _previewSeed);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Новий seed"))
                        _previewSeed = Random.Range(1, int.MaxValue);

                    if (GUILayout.Button("Синхронізувати слоти"))
                    {
                        int half = Mathf.CeilToInt(slots * 0.5f);
                        _previewHumans = Mathf.Clamp(half, 0, slots);
                        _previewBots = slots - _previewHumans;
                    }
                }

                _showGrid = EditorGUILayout.Toggle("Сітка", _showGrid);
                _showDistanceLinks = EditorGUILayout.Toggle("Лінії дистанції", _showDistanceLinks);
                _showRevealRadius = EditorGUILayout.Toggle("Радіус стартового туману", _showRevealRadius);
            }
        }

        private void DrawPreview(SerializedProperty settings)
        {
            int slots = Mathf.Max(1, settings.FindPropertyRelative("multiplayerStartSlots").intValue);
            int minDistance = Mathf.Max(1, settings.FindPropertyRelative("minAStarDistanceBetweenPlayers").intValue);
            int minMargin = Mathf.Max(0, settings.FindPropertyRelative("minMarginFromBorder").intValue);
            float relativeMargin = Mathf.Clamp01(settings.FindPropertyRelative("relativeMarginFactor").floatValue);
            int revealRadius = ResolvePreviewRevealRadius(settings, _previewWidth, _previewHeight);
            int totalParticipants = Mathf.Clamp(_previewHumans + _previewBots, 1, slots);

            EditorGUILayout.Space(8f);
            Rect previewRect = GUILayoutUtility.GetRect(10f, 380f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.09f, 0.11f, 0.12f));

            Rect mapRect = FitMapRect(previewRect, _previewWidth, _previewHeight);
            DrawMapBackground(mapRect);

            int side = Mathf.Min(_previewWidth, _previewHeight);
            int marginTiles = Mathf.Max(minMargin, Mathf.FloorToInt(side * relativeMargin));
            Rect safeRect = TileRectToGui(mapRect, marginTiles, marginTiles, _previewWidth - marginTiles * 2, _previewHeight - marginTiles * 2);
            Handles.DrawSolidRectangleWithOutline(safeRect, new Color(0.12f, 0.28f, 0.18f, 0.22f), new Color(0.42f, 0.88f, 0.56f, 0.75f));

            if (_showGrid)
                DrawGrid(mapRect);

            List<PreviewSpawnPoint> points = BuildPreviewPoints(totalParticipants, _previewBots, minDistance, marginTiles);
            if (_showDistanceLinks)
                DrawDistanceLinks(mapRect, points, minDistance);

            for (int i = 0; i < points.Count; i++)
            {
                DrawSpawnPoint(mapRect, points[i], minDistance, revealRadius);
            }

            Handles.DrawSolidRectangleWithOutline(mapRect, Color.clear, new Color(1f, 1f, 1f, 0.35f));
            DrawLegend(previewRect, points, minDistance, marginTiles);
            DrawValidation(points, minDistance, marginTiles);
        }

        private static void DrawProperty(SerializedProperty settings, string propertyName, string label, string tooltip)
        {
            SerializedProperty property = settings.FindPropertyRelative(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox($"Property '{propertyName}' не знайдено.", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), includeChildren: true);
        }

        private List<PreviewSpawnPoint> BuildPreviewPoints(int slots, int botCount, int minDistance, int marginTiles)
        {
            var points = new List<PreviewSpawnPoint>(slots);
            int attempts = Mathf.Max(64, slots * 128);
            var random = new System.Random(_previewSeed);

            for (int slot = 0; slot < slots; slot++)
            {
                bool isBot = slot >= slots - botCount;
                if (TryPickPreviewPoint(points, minDistance, marginTiles, attempts, random, out Vector2Int tile))
                {
                    points.Add(new PreviewSpawnPoint(tile, isBot, slot));
                    continue;
                }

                points.Add(new PreviewSpawnPoint(PickFallbackPreviewPoint(slot, slots, marginTiles), isBot, slot));
            }

            return points;
        }

        private bool TryPickPreviewPoint(
            IReadOnlyList<PreviewSpawnPoint> existingPoints,
            int minDistance,
            int marginTiles,
            int attempts,
            System.Random random,
            out Vector2Int tile)
        {
            int xMin = Mathf.Clamp(marginTiles, 0, _previewWidth - 1);
            int xMax = Mathf.Clamp(_previewWidth - marginTiles - 1, xMin, _previewWidth - 1);
            int yMin = Mathf.Clamp(marginTiles, 0, _previewHeight - 1);
            int yMax = Mathf.Clamp(_previewHeight - marginTiles - 1, yMin, _previewHeight - 1);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                var candidate = new Vector2Int(random.Next(xMin, xMax + 1), random.Next(yMin, yMax + 1));
                if (HasPreviewDistance(candidate, existingPoints, minDistance))
                {
                    tile = candidate;
                    return true;
                }
            }

            tile = default;
            return false;
        }

        private Vector2Int PickFallbackPreviewPoint(int slot, int slots, int marginTiles)
        {
            Vector2 center = new Vector2((_previewWidth - 1) * 0.5f, (_previewHeight - 1) * 0.5f);
            float maxRadius = Mathf.Max(1f, Mathf.Min(_previewWidth, _previewHeight) * 0.5f - marginTiles - 1f);
            float angle = slots == 1 ? 0f : (Mathf.PI * 2f * slot / slots) - Mathf.PI * 0.5f;
            Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * maxRadius;

            return new Vector2Int(
                Mathf.RoundToInt(Mathf.Clamp(point.x, marginTiles, Mathf.Max(marginTiles, _previewWidth - marginTiles - 1))),
                Mathf.RoundToInt(Mathf.Clamp(point.y, marginTiles, Mathf.Max(marginTiles, _previewHeight - marginTiles - 1))));
        }

        private static bool HasPreviewDistance(Vector2Int candidate, IReadOnlyList<PreviewSpawnPoint> existingPoints, int minDistance)
        {
            for (int i = 0; i < existingPoints.Count; i++)
            {
                if (Vector2Int.Distance(candidate, existingPoints[i].Tile) < minDistance)
                    return false;
            }

            return true;
        }

        private void DrawMapBackground(Rect mapRect)
        {
            EditorGUI.DrawRect(mapRect, new Color(0.16f, 0.2f, 0.2f));

            for (int y = 0; y < 8; y++)
            {
                float t = y / 7f;
                Rect band = new Rect(mapRect.x, Mathf.Lerp(mapRect.yMax, mapRect.y, t + 0.125f), mapRect.width, mapRect.height / 8f + 1f);
                EditorGUI.DrawRect(band, Color.Lerp(new Color(0.12f, 0.18f, 0.18f, 0.55f), new Color(0.24f, 0.29f, 0.24f, 0.55f), t));
            }
        }

        private void DrawGrid(Rect mapRect)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.055f);
            int xStep = Mathf.Max(4, Mathf.CeilToInt(_previewWidth / 16f));
            int yStep = Mathf.Max(4, Mathf.CeilToInt(_previewHeight / 16f));

            for (int x = xStep; x < _previewWidth; x += xStep)
            {
                Vector2 a = TileToGui(mapRect, new Vector2(x, 0));
                Vector2 b = TileToGui(mapRect, new Vector2(x, _previewHeight));
                Handles.DrawLine(a, b);
            }

            for (int y = yStep; y < _previewHeight; y += yStep)
            {
                Vector2 a = TileToGui(mapRect, new Vector2(0, y));
                Vector2 b = TileToGui(mapRect, new Vector2(_previewWidth, y));
                Handles.DrawLine(a, b);
            }
        }

        private void DrawDistanceLinks(Rect mapRect, IReadOnlyList<PreviewSpawnPoint> points, int minDistance)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    float distance = Vector2Int.Distance(points[i].Tile, points[j].Tile);
                    Handles.color = distance >= minDistance
                        ? new Color(0.42f, 0.9f, 0.58f, 0.42f)
                        : new Color(1f, 0.34f, 0.27f, 0.75f);

                    Vector2 a = TileToGui(mapRect, points[i].Tile);
                    Vector2 b = TileToGui(mapRect, points[j].Tile);
                    Handles.DrawAAPolyLine(2.5f, a, b);

                    Vector2 mid = (a + b) * 0.5f;
                    GUI.Label(new Rect(mid.x - 18f, mid.y - 9f, 36f, 18f), Mathf.RoundToInt(distance).ToString(), EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawSpawnPoint(Rect mapRect, PreviewSpawnPoint point, int minDistance, int revealRadius)
        {
            Vector2 gui = TileToGui(mapRect, point.Tile);
            float pixelsPerTile = Mathf.Min(mapRect.width / _previewWidth, mapRect.height / _previewHeight);
            float minRadiusPixels = Mathf.Max(5f, minDistance * pixelsPerTile);
            float revealRadiusPixels = Mathf.Max(5f, revealRadius * pixelsPerTile);
            Color pointColor = point.IsBot ? new Color(0.95f, 0.48f, 0.28f) : new Color(0.28f, 0.7f, 1f);

            Handles.color = new Color(pointColor.r, pointColor.g, pointColor.b, 0.12f);
            Handles.DrawSolidDisc(gui, Vector3.forward, minRadiusPixels);
            Handles.color = new Color(pointColor.r, pointColor.g, pointColor.b, 0.68f);
            Handles.DrawWireDisc(gui, Vector3.forward, minRadiusPixels);

            if (_showRevealRadius)
            {
                Handles.color = new Color(1f, 0.86f, 0.38f, 0.2f);
                Handles.DrawSolidDisc(gui, Vector3.forward, revealRadiusPixels);
                Handles.color = new Color(1f, 0.86f, 0.38f, 0.62f);
                Handles.DrawWireDisc(gui, Vector3.forward, revealRadiusPixels);
            }

            Rect pointRect = new Rect(gui.x - 8f, gui.y - 8f, 16f, 16f);
            EditorGUI.DrawRect(pointRect, pointColor);
            GUI.Label(new Rect(gui.x - 18f, gui.y - 27f, 36f, 18f), point.IsBot ? $"B{point.SlotIndex + 1}" : $"P{point.SlotIndex + 1}", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawLegend(Rect previewRect, IReadOnlyList<PreviewSpawnPoint> points, int minDistance, int marginTiles)
        {
            int bots = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IsBot)
                    bots++;
            }

            Rect legend = new Rect(previewRect.x + 14f, previewRect.yMax - 54f, previewRect.width - 28f, 38f);
            EditorGUI.DrawRect(legend, new Color(0.05f, 0.06f, 0.07f, 0.78f));
            GUI.Label(
                new Rect(legend.x + 10f, legend.y + 5f, legend.width - 20f, 16f),
                $"Слоти: {points.Count} | Гравці: {points.Count - bots} | Боти: {bots} | Мін. дистанція: {minDistance} | Відступ: {marginTiles}",
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(legend.x + 10f, legend.y + 20f, legend.width - 20f, 16f),
                "Сині точки - гравці, помаранчеві - боти, жовте кільце - стартове розкриття туману.",
                EditorStyles.miniLabel);
        }

        private void DrawValidation(IReadOnlyList<PreviewSpawnPoint> points, int minDistance, int marginTiles)
        {
            float actualMinDistance = float.PositiveInfinity;
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                    actualMinDistance = Mathf.Min(actualMinDistance, Vector2Int.Distance(points[i].Tile, points[j].Tile));
            }

            bool enoughSpace = float.IsPositiveInfinity(actualMinDistance) || actualMinDistance >= minDistance;
            MessageType type = enoughSpace ? MessageType.Info : MessageType.Warning;
            string distanceText = float.IsPositiveInfinity(actualMinDistance) ? "один слот" : Mathf.RoundToInt(actualMinDistance).ToString();
            string message = enoughSpace
                ? $"Превʼю валідне. Найменша дистанція між стартами: {distanceText}."
                : $"У превʼю є конфлікт дистанції: найменша дистанція {distanceText}, потрібно {minDistance}. Зменш дистанцію або збільш мапу/відступи.";

            if (marginTiles * 2 >= Mathf.Min(_previewWidth, _previewHeight))
            {
                type = MessageType.Warning;
                message += " Безпечна зона майже зникла через великий відступ від краю.";
            }

            EditorGUILayout.HelpBox(message, type);
        }

        private static int ResolvePreviewRevealRadius(SerializedProperty settings, int width, int height)
        {
            SerializedProperty useScaled = settings.FindPropertyRelative("useMapSizeScaledFog");
            SerializedProperty fallbackRadius = settings.FindPropertyRelative("revealedCircleRadius");
            SerializedProperty scalePoints = settings.FindPropertyRelative("fogScaleByMapSize");

            if (useScaled == null || !useScaled.boolValue || scalePoints == null || scalePoints.arraySize == 0)
                return Mathf.Max(1, fallbackRadius?.intValue ?? 1);

            int side = Mathf.Max(1, Mathf.Min(width, height));
            SerializedProperty lower = null;
            SerializedProperty upper = null;

            for (int i = 0; i < scalePoints.arraySize; i++)
            {
                SerializedProperty point = scalePoints.GetArrayElementAtIndex(i);
                int mapSide = point.FindPropertyRelative("MapSideTiles").intValue;
                if (mapSide <= 0)
                    continue;

                if (mapSide <= side && (lower == null || mapSide > lower.FindPropertyRelative("MapSideTiles").intValue))
                    lower = point;

                if (mapSide >= side && (upper == null || mapSide < upper.FindPropertyRelative("MapSideTiles").intValue))
                    upper = point;
            }

            lower ??= upper;
            upper ??= lower;

            if (lower == null)
                return Mathf.Max(1, fallbackRadius?.intValue ?? 1);

            int lowerSide = lower.FindPropertyRelative("MapSideTiles").intValue;
            int upperSide = upper.FindPropertyRelative("MapSideTiles").intValue;
            int lowerRadius = Mathf.Max(1, lower.FindPropertyRelative("RevealedRadius").intValue);
            int upperRadius = Mathf.Max(1, upper.FindPropertyRelative("RevealedRadius").intValue);

            if (lowerSide == upperSide)
                return lowerRadius;

            float t = Mathf.InverseLerp(lowerSide, upperSide, side);
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(lowerRadius, upperRadius, t)));
        }

        private static Rect FitMapRect(Rect outer, int width, int height)
        {
            Rect padded = new Rect(outer.x + PreviewPadding, outer.y + PreviewPadding, outer.width - PreviewPadding * 2f, outer.height - PreviewPadding * 2f);
            float aspect = width / (float)Mathf.Max(1, height);
            float targetWidth = padded.width;
            float targetHeight = targetWidth / aspect;
            if (targetHeight > padded.height)
            {
                targetHeight = padded.height;
                targetWidth = targetHeight * aspect;
            }

            return new Rect(
                padded.x + (padded.width - targetWidth) * 0.5f,
                padded.y + (padded.height - targetHeight) * 0.5f,
                targetWidth,
                targetHeight);
        }

        private Rect TileRectToGui(Rect mapRect, int x, int y, int width, int height)
        {
            Vector2 min = TileCornerToGui(mapRect, new Vector2(x, y));
            Vector2 max = TileCornerToGui(mapRect, new Vector2(x + width, y + height));
            return Rect.MinMaxRect(min.x, max.y, max.x, min.y);
        }

        private Vector2 TileCornerToGui(Rect mapRect, Vector2 tile)
        {
            float x = mapRect.x + tile.x / Mathf.Max(1, _previewWidth) * mapRect.width;
            float y = mapRect.yMax - tile.y / Mathf.Max(1, _previewHeight) * mapRect.height;
            return new Vector2(x, y);
        }

        private Vector2 TileToGui(Rect mapRect, Vector2 tile)
        {
            float x = mapRect.x + (tile.x + 0.5f) / Mathf.Max(1, _previewWidth) * mapRect.width;
            float y = mapRect.yMax - (tile.y + 0.5f) / Mathf.Max(1, _previewHeight) * mapRect.height;
            return new Vector2(x, y);
        }

        private void LoadDefaultConfig()
        {
            _config = AssetDatabase.LoadAssetAtPath<BootstrapInstallerConfigSO>(DefaultBootstrapAssetPath);
            _serializedConfig = _config != null ? new SerializedObject(_config) : null;
        }
    }
}
