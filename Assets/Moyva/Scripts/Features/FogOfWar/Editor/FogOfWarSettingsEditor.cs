using Kruty1918.Moyva.FogOfWar.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    [CustomEditor(typeof(FogOfWarSettings))]
    public sealed class FogOfWarSettingsEditor : UnityEditor.Editor
    {
        private const float LargePreviewSize = 64f;

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
        private SerializedProperty _fogIconSpritesProp;
        private SerializedProperty _fogIconGridSizeProp;
        private SerializedProperty _fogIconScaleProp;
        private SerializedProperty _fogIconSeedProp;
        private SerializedProperty _fogIconDensityProp;

        private bool _visionFoldout = true;
        private bool _heightFoldout;
        private bool _appearanceFoldout = true;
        private bool _iconsFoldout = true;

        private GUIStyle _panelStyle;
        private GUIStyle _filledSlotStyle;

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
            int icons = CountAssignedSprites(_fogIconSpritesProp, _fogIconSpritesProp.arraySize);

            EditorGUILayout.BeginVertical(_panelStyle);
            EditorGUILayout.LabelField("Стан туману", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Розмір тайлу", $"{FogOfWarSettings.FogTilePixelSize}x{FogOfWarSettings.FogTilePixelSize} px");
            EditorGUILayout.LabelField("Базовий тайл", _fogTileSpriteProp.objectReferenceValue != null ? "призначено" : "немає");
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
    }
}
