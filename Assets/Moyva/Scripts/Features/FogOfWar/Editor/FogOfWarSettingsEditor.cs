using Kruty1918.Moyva.FogOfWar.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    [CustomEditor(typeof(FogOfWarSettings))]
    public sealed class FogOfWarSettingsEditor : UnityEditor.Editor
    {
        private const int BitmaskCount = 16;
        private const float SmallPreviewSize = 50f;
        private const float LargePreviewSize = 64f;

        private static readonly string[] BitmaskSlotNames =
        {
            "Без сусідів", "Північ", "Схід", "Пн + Сх",
            "Південь", "Пн + Пд", "Сх + Пд", "Пн + Сх + Пд",
            "Захід", "Пн + Зх", "Сх + Зх", "Пн + Сх + Зх",
            "Пд + Зх", "Пн + Пд + Зх", "Сх + Пд + Зх", "Усі сусіди",
        };

        private SerializedProperty _defaultVisionRangeProp;
        private SerializedProperty _minVisionRangeProp;
        private SerializedProperty _maxVisionRangeProp;
        private SerializedProperty _elevationStepProp;
        private SerializedProperty _observerHeightBonusProp;
        private SerializedProperty _downhillBonusProp;
        private SerializedProperty _uphillPenaltyProp;
        private SerializedProperty _maxObserverHeightBonusProp;
        private SerializedProperty _maxDownhillBonusProp;
        private SerializedProperty _maxUphillPenaltyProp;
        private SerializedProperty _occlusionSlopeBiasProp;
        private SerializedProperty _unexploredColorProp;
        private SerializedProperty _exploredColorProp;
        private SerializedProperty _unexploredAlphaProp;
        private SerializedProperty _exploredAlphaProp;
        private SerializedProperty _fogTileSpriteProp;
        private SerializedProperty _fogTileTilingProp;
        private SerializedProperty _useBitmaskAutotilingProp;
        private SerializedProperty _fogBitmaskSpritesProp;
        private SerializedProperty _fogIconSpritesProp;
        private SerializedProperty _fogIconGridSizeProp;
        private SerializedProperty _fogIconScaleProp;
        private SerializedProperty _fogIconSeedProp;
        private SerializedProperty _fogIconDensityProp;

        private bool _visionFoldout = true;
        private bool _heightFoldout;
        private bool _appearanceFoldout = true;
        private bool _bitmaskFoldout = true;
        private bool _iconsFoldout = true;

        private GUIStyle _panelStyle;
        private GUIStyle _filledSlotStyle;
        private GUIStyle _emptySlotStyle;

        private void OnEnable()
        {
            _defaultVisionRangeProp     = serializedObject.FindProperty("DefaultVisionRange");
            _minVisionRangeProp         = serializedObject.FindProperty("MinVisionRange");
            _maxVisionRangeProp         = serializedObject.FindProperty("MaxVisionRange");
            _elevationStepProp          = serializedObject.FindProperty("ElevationStep");
            _observerHeightBonusProp    = serializedObject.FindProperty("ObserverHeightBonusPerStep");
            _downhillBonusProp          = serializedObject.FindProperty("DownhillVisionBonusPerStep");
            _uphillPenaltyProp          = serializedObject.FindProperty("UphillVisionPenaltyPerStep");
            _maxObserverHeightBonusProp = serializedObject.FindProperty("MaxObserverHeightBonus");
            _maxDownhillBonusProp       = serializedObject.FindProperty("MaxDownhillVisionBonus");
            _maxUphillPenaltyProp       = serializedObject.FindProperty("MaxUphillVisionPenalty");
            _occlusionSlopeBiasProp     = serializedObject.FindProperty("OcclusionSlopeBias");
            _unexploredColorProp        = serializedObject.FindProperty("UnexploredColor");
            _exploredColorProp          = serializedObject.FindProperty("ExploredColor");
            _unexploredAlphaProp        = serializedObject.FindProperty("UnexploredAlpha");
            _exploredAlphaProp          = serializedObject.FindProperty("ExploredAlpha");
            _fogTileSpriteProp          = serializedObject.FindProperty("FogTileSprite");
            _fogTileTilingProp          = serializedObject.FindProperty("FogTileTiling");
            _useBitmaskAutotilingProp   = serializedObject.FindProperty("UseBitmaskAutotiling");
            _fogBitmaskSpritesProp      = serializedObject.FindProperty("FogBitmaskSprites");
            _fogIconSpritesProp         = serializedObject.FindProperty("FogIconSprites");
            _fogIconGridSizeProp        = serializedObject.FindProperty("FogIconGridSize");
            _fogIconScaleProp           = serializedObject.FindProperty("FogIconScale");
            _fogIconSeedProp            = serializedObject.FindProperty("FogIconSeed");
            _fogIconDensityProp         = serializedObject.FindProperty("FogIconDensity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureStyles();

            DrawScriptReference();
            DrawSummaryPanel();
            DrawVisionSection();
            DrawHeightVisionSection();
            DrawAppearanceSection();
            DrawBitmaskSection();
            DrawIconsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void EnsureStyles()
        {
            _panelStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 4, 8),
            };

            _filledSlotStyle ??= new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(2, 2, 2, 2),
            };

            _emptySlotStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(2, 2, 2, 2),
            };
        }

        private void DrawScriptReference()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var script = MonoScript.FromScriptableObject((FogOfWarSettings)target);
                EditorGUILayout.ObjectField("Скрипт", script, typeof(MonoScript), false);
            }
        }

        private void DrawSummaryPanel()
        {
            int masks = CountAssignedSprites(_fogBitmaskSpritesProp, BitmaskCount);
            int icons = CountAssignedSprites(_fogIconSpritesProp, _fogIconSpritesProp.arraySize);

            EditorGUILayout.BeginVertical(_panelStyle);
            EditorGUILayout.LabelField("Стан туману", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Розмір тайлу", $"{FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px");
            EditorGUILayout.LabelField("Базовий тайл", _fogTileSpriteProp.objectReferenceValue != null ? "призначено" : "немає");
            EditorGUILayout.LabelField("Бітмаска", _useBitmaskAutotilingProp.boolValue ? $"увімкнено, {masks}/{BitmaskCount} слотів" : "вимкнено");
            EditorGUILayout.LabelField("Іконки", icons > 0 ? $"{icons} активних" : "не використовуються");
            EditorGUILayout.EndVertical();
        }

        private void DrawVisionSection()
        {
            _visionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visionFoldout,
                new GUIContent("Дальність огляду", "Базові межі видимості юнітів"));

            if (_visionFoldout)
            {
                EditorGUILayout.BeginVertical(_panelStyle);
                EditorGUILayout.PropertyField(_defaultVisionRangeProp,
                    new GUIContent("За замовчуванням", "Радіус видимості, якщо юніт не задає власного значення"));
                EditorGUILayout.PropertyField(_minVisionRangeProp,
                    new GUIContent("Мінімум", "Мінімальна допустима дальність огляду"));
                EditorGUILayout.PropertyField(_maxVisionRangeProp,
                    new GUIContent("Максимум", "Максимальна допустима дальність огляду"));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawHeightVisionSection()
        {
            _heightFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_heightFoldout,
                new GUIContent("Висота та рельєф", "Як перепад висот змінює видимість"));

            if (_heightFoldout)
            {
                EditorGUILayout.BeginVertical(_panelStyle);
                EditorGUILayout.LabelField("Крок висоти", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_elevationStepProp,
                    new GUIContent("Розмір кроку", "Різниця висоти між одним логічним ступенем підйому або спуску"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Зміна дальності за крок", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_observerHeightBonusProp,
                    new GUIContent("Бонус спостерігача", "Скільки дальності додає висота самого спостерігача"));
                EditorGUILayout.PropertyField(_downhillBonusProp,
                    new GUIContent("Бонус донизу", "Бонус за огляд цілей нижче спостерігача"));
                EditorGUILayout.PropertyField(_uphillPenaltyProp,
                    new GUIContent("Штраф угору", "Штраф за огляд цілей вище спостерігача"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Обмеження", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_maxObserverHeightBonusProp,
                    new GUIContent("Макс. бонус спостерігача", "Максимальний бонус від висоти спостерігача"));
                EditorGUILayout.PropertyField(_maxDownhillBonusProp,
                    new GUIContent("Макс. бонус донизу", "Максимальний бонус за погляд донизу"));
                EditorGUILayout.PropertyField(_maxUphillPenaltyProp,
                    new GUIContent("Макс. штраф угору", "Максимальний штраф за погляд угору"));
                EditorGUILayout.PropertyField(_occlusionSlopeBiasProp,
                    new GUIContent("Зсув нахилу", "Невеликий допуск для стабільнішої оклюзії на схилах"));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAppearanceSection()
        {
            _appearanceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_appearanceFoldout,
                new GUIContent("Вигляд туману", "Колір, прозорість і базовий спрайт туману"));

            if (_appearanceFoldout)
            {
                EditorGUILayout.BeginVertical(_panelStyle);
                EditorGUILayout.LabelField("Кольори", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_unexploredColorProp,
                    new GUIContent("Не досліджено", "Tint для невідвіданих клітинок. Білий колір залишає оригінальний колір спрайту"));
                EditorGUILayout.PropertyField(_exploredColorProp,
                    new GUIContent("Досліджено", "Tint для відвіданих клітинок поза оглядом. Білий колір залишає оригінальний колір спрайту"));
                EditorGUILayout.PropertyField(_unexploredAlphaProp,
                    new GUIContent("Альфа не досліджено", "Непрозорість для невідвіданих клітинок"));
                EditorGUILayout.PropertyField(_exploredAlphaProp,
                    new GUIContent("Альфа досліджено", "Непрозорість для відвіданих, але невидимих клітинок"));

                EditorGUILayout.Space(5);
                DrawColorPreview();

                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Базовий тайл", EditorStyles.miniBoldLabel);
                DrawSpriteField(_fogTileSpriteProp,
                    new GUIContent("Спрайт тайлу", $"Спрайт із атласу, який повторюється по клітинках туману. Очікуваний розмір: {FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px"),
                    LargePreviewSize);
                EditorGUILayout.PropertyField(_fogTileTilingProp,
                    new GUIContent("Тайлінг", "Кількість повторень спрайту на одну клітинку туману"));

                if (_fogTileSpriteProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "Базовий тайл не встановлено. Рендер використає суцільний колір туману як фолбек.",
                        MessageType.Warning);
                }
                else if (!HasExpectedTileSize((Sprite)_fogTileSpriteProp.objectReferenceValue))
                {
                    EditorGUILayout.HelpBox(
                        $"Базовий тайл має бути {FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px. Інший розмір може дати нечіткий або нерівний туман.",
                        MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawColorPreview()
        {
            EditorGUILayout.BeginHorizontal();
            DrawColorSwatch("Не досліджено", _unexploredColorProp.colorValue, _unexploredAlphaProp.floatValue);
            GUILayout.Space(10);
            DrawColorSwatch("Досліджено", _exploredColorProp.colorValue, _exploredAlphaProp.floatValue);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawColorSwatch(string label, Color color, float alpha)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
            Rect rect = GUILayoutUtility.GetRect(120, 24, GUILayout.Width(120));
            EditorGUI.DrawTextureTransparent(rect, Texture2D.whiteTexture);
            color.a = alpha;
            EditorGUI.DrawRect(rect, color);
            EditorGUILayout.EndVertical();
        }

        private void DrawBitmaskSection()
        {
            _bitmaskFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_bitmaskFoldout,
                new GUIContent("Бітова маска країв", $"16 спрайтів {FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px для автотайлінгу за сусідами: Пн=1, Сх=2, Пд=4, Зх=8"));

            if (_bitmaskFoldout)
            {
                EnsureBitmaskArraySize();
                EditorGUILayout.BeginVertical(_panelStyle);

                EditorGUILayout.PropertyField(_useBitmaskAutotilingProp,
                    new GUIContent("Увімкнути автотайлінг", "Клітинка туману обирає спрайт за кодом сусідніх туманних клітинок"));

                EditorGUILayout.HelpBox(
                    $"Один тайл туману = {FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px. Код слоту: Пн=1, Сх=2, Пд=4, Зх=8. Наприклад #3 = Пн + Сх, #15 = усі сусіди.",
                    MessageType.None);

                DrawBitmaskToolbar();
                DrawBitmaskGrid();
                DrawBitmaskValidation();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBitmaskToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(_fogTileSpriteProp.objectReferenceValue == null))
            {
                if (GUILayout.Button(new GUIContent("Заповнити порожні тайлом", "Скопіювати базовий тайл в усі порожні слоти маски")))
                    FillEmptyBitmaskSlotsWithTile();
            }

            if (GUILayout.Button(new GUIContent("Очистити маски", "Прибрати всі 16 спрайтів бітової маски")))
                ClearBitmaskSlots();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        private void DrawBitmaskGrid()
        {
            int assigned = CountAssignedSprites(_fogBitmaskSpritesProp, BitmaskCount);
            EditorGUILayout.LabelField($"Налаштовано {assigned}/{BitmaskCount} слотів", EditorStyles.miniBoldLabel);

            for (int row = 0; row < 4; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    SerializedProperty slot = _fogBitmaskSpritesProp.GetArrayElementAtIndex(index);
                    bool empty = slot.objectReferenceValue == null;

                    EditorGUILayout.BeginVertical(empty ? _emptySlotStyle : _filledSlotStyle, GUILayout.Width(72));
                    EditorGUILayout.LabelField(new GUIContent($"#{index}", GetBitmaskTooltip(index)), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(64));
                    DrawCompass(index);

                    slot.objectReferenceValue = EditorGUILayout.ObjectField(
                        slot.objectReferenceValue,
                        typeof(Sprite),
                        false,
                        GUILayout.Width(SmallPreviewSize),
                        GUILayout.Height(SmallPreviewSize));

                    EditorGUILayout.LabelField(BitmaskSlotNames[index], EditorStyles.centeredGreyMiniLabel, GUILayout.Width(64));
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawCompass(int mask)
        {
            string north = (mask & 1) != 0 ? "Пн" : "--";
            string east = (mask & 2) != 0 ? "Сх" : "--";
            string south = (mask & 4) != 0 ? "Пд" : "--";
            string west = (mask & 8) != 0 ? "Зх" : "--";

            EditorGUILayout.LabelField(north, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(64));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(west, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(32));
            GUILayout.Label(east, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(32));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(south, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(64));
        }

        private static string GetBitmaskTooltip(int mask)
        {
            return $"Сусіди туману: {BitmaskSlotNames[mask]}. Бінарний код: {System.Convert.ToString(mask, 2).PadLeft(4, '0')}";
        }

        private void DrawBitmaskValidation()
        {
            int assigned = CountAssignedSprites(_fogBitmaskSpritesProp, BitmaskCount);
            bool hasAtlasMismatch = HasAtlasMismatch(_fogBitmaskSpritesProp, BitmaskCount, out Texture firstMaskTexture);
            int wrongSizeCount = CountSpritesWithUnexpectedTileSize(_fogBitmaskSpritesProp, BitmaskCount);
            Texture tileTexture = _fogTileSpriteProp.objectReferenceValue is Sprite tileSprite ? tileSprite.texture : null;

            if (_useBitmaskAutotilingProp.boolValue && assigned == 0)
            {
                EditorGUILayout.HelpBox(
                    "Бітова маска увімкнена, але спрайти не призначено. Буде використано базовий тайл як фолбек.",
                    MessageType.Warning);
            }
            else if (assigned > 0 && assigned < BitmaskCount)
            {
                EditorGUILayout.HelpBox(
                    $"Призначено {assigned}/{BitmaskCount} масок. Порожні слоти використають базовий тайл як фолбек.",
                    MessageType.Info);
            }

            if (hasAtlasMismatch)
            {
                EditorGUILayout.HelpBox(
                    "Маски взято з різних атласів. Шейдер читає один атлас, тому частина спрайтів може не відобразитись.",
                    MessageType.Warning);
            }

            if (wrongSizeCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{wrongSizeCount} слот(ів) бітової маски мають розмір не {FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px. Для рівного тайлування краще використовувати саме 16x16 px.",
                    MessageType.Warning);
            }

            if (tileTexture != null && firstMaskTexture != null && tileTexture != firstMaskTexture)
            {
                EditorGUILayout.HelpBox(
                    "Базовий тайл і маски з різних атласів. Це допустимо як фолбек, але для стабільного рендера краще тримати їх разом.",
                    MessageType.Warning);
            }
        }

        private void FillEmptyBitmaskSlotsWithTile()
        {
            Object tileSprite = _fogTileSpriteProp.objectReferenceValue;
            if (tileSprite == null)
                return;

            for (int i = 0; i < BitmaskCount; i++)
            {
                SerializedProperty slot = _fogBitmaskSpritesProp.GetArrayElementAtIndex(i);
                if (slot.objectReferenceValue == null)
                    slot.objectReferenceValue = tileSprite;
            }
        }

        private void ClearBitmaskSlots()
        {
            for (int i = 0; i < BitmaskCount; i++)
                _fogBitmaskSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
        }

        private void DrawIconsSection()
        {
            _iconsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_iconsFoldout,
                new GUIContent("Іконки на тумані", "Список спрайтів, які циклічно розміщуються поверх туману"));

            if (_iconsFoldout)
            {
                EditorGUILayout.BeginVertical(_panelStyle);
                DrawIconList();

                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(_fogIconGridSizeProp,
                    new GUIContent("Сітка іконок", "Незалежна сітка розміщення іконок по всій мапі. Іконки залишаються вирівняними по цій сітці"));
                EditorGUILayout.PropertyField(_fogIconScaleProp,
                    new GUIContent("Масштаб", "Розмір іконки відносно клітинки туману"));
                EditorGUILayout.PropertyField(_fogIconSeedProp,
                    new GUIContent("Seed розкладання", "Число, яке задає стабільний алгоритмічний візерунок показу і вибору іконок. Однаковий seed = однакова розкладка"));
                EditorGUILayout.PropertyField(_fogIconDensityProp,
                    new GUIContent("Щільність", "Скільки комірок сітки показують іконку. 1 = кожна комірка, 0 = іконки вимкнені"));

                DrawIconValidation();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawIconList()
        {
            EditorGUILayout.LabelField("Спрайти іконок", EditorStyles.miniBoldLabel);

            for (int i = 0; i < _fogIconSpritesProp.arraySize; i++)
            {
                SerializedProperty slot = _fogIconSpritesProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal(_filledSlotStyle);
                EditorGUILayout.LabelField($"#{i + 1}", GUILayout.Width(28));
                slot.objectReferenceValue = EditorGUILayout.ObjectField(
                    slot.objectReferenceValue,
                    typeof(Sprite),
                    false,
                    GUILayout.Width(LargePreviewSize),
                    GUILayout.Height(LargePreviewSize));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField(GetSpriteDescription(slot.objectReferenceValue as Sprite), EditorStyles.wordWrappedMiniLabel);

                if (GUILayout.Button(new GUIContent("Видалити", "Прибрати цю іконку зі списку"), GUILayout.Width(90)))
                {
                    DeleteArrayElement(_fogIconSpritesProp, i);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Додати іконку", "Додати порожній слот для нового спрайта")))
            {
                int index = _fogIconSpritesProp.arraySize;
                _fogIconSpritesProp.InsertArrayElementAtIndex(index);
                _fogIconSpritesProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
            }

            using (new EditorGUI.DisabledScope(_fogIconSpritesProp.arraySize == 0))
            {
                if (GUILayout.Button(new GUIContent("Очистити", "Прибрати всі іконки")))
                    _fogIconSpritesProp.ClearArray();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIconValidation()
        {
            int assigned = CountAssignedSprites(_fogIconSpritesProp, _fogIconSpritesProp.arraySize);
            bool mismatch = HasAtlasMismatch(_fogIconSpritesProp, _fogIconSpritesProp.arraySize, out _);
            bool hasEmpty = assigned < _fogIconSpritesProp.arraySize;

            if (_fogIconSpritesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Іконки не додані — туман рендериться без декоративних іконок.", MessageType.Info);
            }
            else if (hasEmpty)
            {
                EditorGUILayout.HelpBox("У списку є порожні слоти. Під час рендера вони пропускаються.", MessageType.Warning);
            }

            if (mismatch)
            {
                EditorGUILayout.HelpBox(
                    "Іконки взято з різних атласів. Шейдер використовує один атлас, тому частина іконок може не відобразитись.",
                    MessageType.Warning);
            }
        }

        private static void DrawSpriteField(SerializedProperty spriteProp, GUIContent label, float size)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            spriteProp.objectReferenceValue = EditorGUILayout.ObjectField(
                spriteProp.objectReferenceValue,
                typeof(Sprite),
                false,
                GUILayout.Width(size),
                GUILayout.Height(size));
            EditorGUILayout.EndHorizontal();
        }

        private static string GetSpriteDescription(Sprite sprite)
        {
            if (sprite == null)
                return "Порожній слот";

            Rect rect = sprite.textureRect;
            string texture = sprite.texture != null ? sprite.texture.name : "без текстури";
            return $"{sprite.name}\nАтлас: {texture}\nRect: {Mathf.RoundToInt(rect.width)}x{Mathf.RoundToInt(rect.height)}";
        }

        private static int CountAssignedSprites(SerializedProperty arrayProp, int maxCount)
        {
            if (arrayProp == null || !arrayProp.isArray)
                return 0;

            int count = 0;
            int limit = Mathf.Min(arrayProp.arraySize, maxCount);
            for (int i = 0; i < limit; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
        }

        private static int CountSpritesWithUnexpectedTileSize(SerializedProperty arrayProp, int maxCount)
        {
            if (arrayProp == null || !arrayProp.isArray)
                return 0;

            int count = 0;
            int limit = Mathf.Min(arrayProp.arraySize, maxCount);
            for (int i = 0; i < limit; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue is Sprite sprite && !HasExpectedTileSize(sprite))
                    count++;
            }

            return count;
        }

        private static bool HasExpectedTileSize(Sprite sprite)
        {
            if (sprite == null)
                return true;

            Rect rect = sprite.textureRect;
            return Mathf.RoundToInt(rect.width) == FogOfWarSettings.FogTilePixelSize &&
                   Mathf.RoundToInt(rect.height) == FogOfWarSettings.FogTilePixelSize;
        }

        private static bool HasAtlasMismatch(SerializedProperty arrayProp, int maxCount, out Texture firstTexture)
        {
            firstTexture = null;
            bool mismatch = false;
            int limit = Mathf.Min(arrayProp.arraySize, maxCount);

            for (int i = 0; i < limit; i++)
            {
                if (!(arrayProp.GetArrayElementAtIndex(i).objectReferenceValue is Sprite sprite) || sprite.texture == null)
                    continue;

                if (firstTexture == null)
                    firstTexture = sprite.texture;
                else if (firstTexture != sprite.texture)
                    mismatch = true;
            }

            return mismatch;
        }

        private static void DeleteArrayElement(SerializedProperty arrayProp, int index)
        {
            if (arrayProp.GetArrayElementAtIndex(index).objectReferenceValue != null)
                arrayProp.GetArrayElementAtIndex(index).objectReferenceValue = null;

            arrayProp.DeleteArrayElementAtIndex(index);
        }

        private void EnsureBitmaskArraySize()
        {
            if (_fogBitmaskSpritesProp == null || _fogBitmaskSpritesProp.arraySize == BitmaskCount)
                return;

            while (_fogBitmaskSpritesProp.arraySize < BitmaskCount)
                _fogBitmaskSpritesProp.InsertArrayElementAtIndex(_fogBitmaskSpritesProp.arraySize);

            while (_fogBitmaskSpritesProp.arraySize > BitmaskCount)
                _fogBitmaskSpritesProp.DeleteArrayElementAtIndex(_fogBitmaskSpritesProp.arraySize - 1);
        }
    }
}
