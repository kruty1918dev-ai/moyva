using Kruty1918.Moyva.FogOfWar.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    [CustomEditor(typeof(FogOfWarSettings))]
    public sealed class FogOfWarSettingsEditor : UnityEditor.Editor
    {
        // ─── Bitmask labels: N/E/S/W neighbors encoded as bits 0-3 ──────────
        // Bit pattern displayed per slot (index 0..15):
        //   N=bit0(1), E=bit1(2), S=bit2(4), W=bit3(8)
        private static readonly string[] BitmaskSlotHints = new string[16]
        {
            "Немає сусідів",        // 0000
            "Північ",               // 0001
            "Схід",                 // 0010
            "Північ + Схід",        // 0011
            "Південь",              // 0100
            "Північ + Південь",     // 0101
            "Схід + Південь",       // 0110
            "Пн + Сх + Пд",        // 0111
            "Захід",               // 1000
            "Північ + Захід",       // 1001
            "Схід + Захід",        // 1010
            "Пн + Сх + Зх",        // 1011
            "Південь + Захід",     // 1100
            "Пд + Пн + Зх",        // 1101
            "Пд + Сх + Зх",        // 1110
            "Всі сусіди",          // 1111
        };

        private const int BitmaskCount = 16;
        private const float SpritePreviewSize = 52f;

        // ─── Serialized properties ────────────────────────────────────────────
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

        // ─── Foldout state ────────────────────────────────────────────────────
        private bool _visionFoldout = true;
        private bool _heightFoldout = false;
        private bool _colorsFoldout = true;
        private bool _tileFoldout = true;
        private bool _bitmaskFoldout = true;
        private bool _iconsFoldout = true;

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptReference();
            DrawVisionSection();
            DrawHeightVisionSection();
            DrawColorSection();
            DrawTileSection();
            DrawBitmaskSection();
            DrawIconsSection();

            serializedObject.ApplyModifiedProperties();
        }

        // ─── Script reference ─────────────────────────────────────────────────

        private void DrawScriptReference()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var script = MonoScript.FromScriptableObject((FogOfWarSettings)target);
                EditorGUILayout.ObjectField("Скрипт", script, typeof(MonoScript), false);
            }
            EditorGUILayout.Space(4);
        }

        // ─── Vision Range ─────────────────────────────────────────────────────

        private void DrawVisionSection()
        {
            _visionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visionFoldout,
                new GUIContent("Дальність огляду", "Базові параметри дальності видимості юнітів"));

            if (_visionFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_defaultVisionRangeProp,
                    new GUIContent("За замовчуванням", "Радіус видимості, якщо юніт не задає власного значення"));
                EditorGUILayout.PropertyField(_minVisionRangeProp,
                    new GUIContent("Мінімум", "Мінімально допустима дальність огляду"));
                EditorGUILayout.PropertyField(_maxVisionRangeProp,
                    new GUIContent("Максимум", "Максимально допустима дальність огляду"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        // ─── Height Vision ────────────────────────────────────────────────────

        private void DrawHeightVisionSection()
        {
            _heightFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_heightFoldout,
                new GUIContent("Висота та рельєф", "Як висота клітинок впливає на дальність огляду"));

            if (_heightFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Параметри кроку висоти", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_elevationStepProp,
                    new GUIContent("Крок висоти", "Різниця висоти (у world units) між ступенями підйому/спуску"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Бонуси і штрафи за крок", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_observerHeightBonusProp,
                    new GUIContent("Бонус спостерігача / крок", "Скільки клітинок дальності додає кожен крок висоти спостерігача"));
                EditorGUILayout.PropertyField(_downhillBonusProp,
                    new GUIContent("Бонус донизу / крок", "Бонус дальності за погляд під ухил (ціль нижча)"));
                EditorGUILayout.PropertyField(_uphillPenaltyProp,
                    new GUIContent("Штраф вгору / крок", "Зменшення дальності за погляд вгору (ціль вища)"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Ліміти", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(_maxObserverHeightBonusProp,
                    new GUIContent("Макс. бонус спостерігача", "Стеля бонусу від висоти спостерігача"));
                EditorGUILayout.PropertyField(_maxDownhillBonusProp,
                    new GUIContent("Макс. бонус донизу", "Стеля бонусу за погляд донизу"));
                EditorGUILayout.PropertyField(_maxUphillPenaltyProp,
                    new GUIContent("Макс. штраф вгору", "Стеля штрафу за погляд угору"));
                EditorGUILayout.PropertyField(_occlusionSlopeBiasProp,
                    new GUIContent("Зсув нахилу оклюзії", "Невеликий зсув, що зменшує мерехтіння ліній видимості на схилах"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        // ─── Colors & transparency ────────────────────────────────────────────

        private void DrawColorSection()
        {
            _colorsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_colorsFoldout,
                new GUIContent("Кольори та прозорість", "Кольори та рівні альфа для різних станів туману"));

            if (_colorsFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_unexploredColorProp,
                    new GUIContent("Колір «Не досліджено»", "Колір повністю закритих клітинок (ще не відвіданих)"));
                EditorGUILayout.PropertyField(_exploredColorProp,
                    new GUIContent("Колір «Досліджено»", "Колір клітинок, що були видимі, але зараз поза зоною огляду"));

                EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(_unexploredAlphaProp,
                    new GUIContent("Альфа «Не досліджено»", "Непрозорість туману для невідвіданих клітинок (0 = повністю прозоро)"));
                EditorGUILayout.PropertyField(_exploredAlphaProp,
                    new GUIContent("Альфа «Досліджено»", "Непрозорість туману для відвіданих, але наразі невидимих клітинок"));

                DrawColorPreview();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawColorPreview()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            DrawColorSwatch("Не досліджено",
                _unexploredColorProp.colorValue,
                _unexploredAlphaProp.floatValue);

            GUILayout.FlexibleSpace();

            DrawColorSwatch("Досліджено",
                _exploredColorProp.colorValue,
                _exploredAlphaProp.floatValue);

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawColorSwatch(string label, Color baseColor, float alpha)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(90));

            var swatchRect = GUILayoutUtility.GetRect(90, 20, GUILayout.Width(90));
            // Checkerboard to show transparency
            EditorGUI.DrawTextureTransparent(swatchRect, Texture2D.whiteTexture);
            var swatchColor = baseColor;
            swatchColor.a = alpha;
            EditorGUI.DrawRect(swatchRect, swatchColor);

            EditorGUILayout.EndVertical();
        }

        // ─── Fog Tile ─────────────────────────────────────────────────────────

        private void DrawTileSection()
        {
            _tileFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_tileFoldout,
                new GUIContent("Базовий тайл туману", "Спрайт і параметри тайлу, що вкриває невидимі клітинки"));

            if (_tileFoldout)
            {
                EditorGUI.indentLevel++;

                DrawSpriteFieldWithPreview(_fogTileSpriteProp,
                    new GUIContent("Спрайт тайлу", "Спрайт, що повторюється по туману. Береться з атласу — вказуйте окремий спрайт, не текстуру цілком"));

                if (_fogTileSpriteProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "Базовий тайл не встановлено. Туман буде відображатись суцільним кольором без текстури.",
                        MessageType.Warning);
                }

                EditorGUILayout.PropertyField(_fogTileTilingProp,
                    new GUIContent("Тайлінг", "Кількість повторень спрайту на одну клітинку туману. Більше значення = дрібніший візерунок"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        // ─── Bitmask ──────────────────────────────────────────────────────────

        private void DrawBitmaskSection()
        {
            _bitmaskFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_bitmaskFoldout,
                new GUIContent("Бітмаска країв туману", "Автотайлінг країв за 4-сусідами (Пн/Сх/Пд/Зх). 16 варіантів спрайтів"));

            if (_bitmaskFoldout)
            {
                EditorGUI.indentLevel++;
                EnsureBitmaskArraySize();

                EditorGUILayout.PropertyField(_useBitmaskAutotilingProp,
                    new GUIContent("Увімкнути бітмаску", "Якщо увімкнено — кожна клітинка туману отримує спрайт-маску згідно сусідів"));

                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Індекс слоту = код сусідів (біти): Пн=1, Сх=2, Пд=4, Зх=8.\n" +
                    "Наприклад, слот 3 (Пн+Сх) — клітинка має туманних сусідів на Північ і Схід.",
                    MessageType.None);
                EditorGUILayout.Space(4);

                DrawBitmaskGrid();

                EditorGUILayout.Space(4);
                DrawBitmaskValidation();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawBitmaskGrid()
        {
            for (int row = 0; row < 4; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < 4; col++)
                {
                    int idx = row * 4 + col;
                    var elem = _fogBitmaskSpritesProp.GetArrayElementAtIndex(idx);
                    bool isEmpty = elem.objectReferenceValue == null;

                    // Slot background to visually indicate empty/filled
                    var bgStyle = isEmpty
                        ? EditorStyles.helpBox
                        : GUI.skin.box;

                    EditorGUILayout.BeginVertical(bgStyle, GUILayout.Width(SpritePreviewSize + 8));

                    // Slot label: index + neighbor hint
                    string slotLabel = $"[{idx}] {BitmaskSlotHints[idx]}";
                    EditorGUILayout.LabelField(
                        new GUIContent(slotLabel, $"Бітний код сусідів: {idx:D2} ({System.Convert.ToString(idx, 2).PadLeft(4, '0')} бінарно)"),
                        EditorStyles.centeredGreyMiniLabel,
                        GUILayout.Width(SpritePreviewSize + 4));

                    // Sprite picker with thumbnail
                    elem.objectReferenceValue = EditorGUILayout.ObjectField(
                        elem.objectReferenceValue,
                        typeof(Sprite),
                        false,
                        GUILayout.Width(SpritePreviewSize), GUILayout.Height(SpritePreviewSize));

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        private void DrawBitmaskValidation()
        {
            bool autotilingOn = _useBitmaskAutotilingProp.boolValue;
            Texture baseTex = _fogTileSpriteProp.objectReferenceValue is Sprite tile ? tile.texture : null;
            bool hasAtlasMismatch = false;
            int assigned = 0;
            Texture firstMaskTex = null;

            for (int i = 0; i < BitmaskCount; i++)
            {
                var prop = _fogBitmaskSpritesProp.GetArrayElementAtIndex(i);
                if (!(prop.objectReferenceValue is Sprite s))
                    continue;

                assigned++;
                if (firstMaskTex == null)
                    firstMaskTex = s.texture;
                else if (s.texture != firstMaskTex)
                    hasAtlasMismatch = true;
            }

            if (autotilingOn && assigned == 0)
            {
                EditorGUILayout.HelpBox(
                    "Бітмаска увімкнена, але жоден спрайт не призначено. " +
                    "Буде використано базовий тайл як фолбек.",
                    MessageType.Warning);
            }
            else if (assigned > 0 && assigned < BitmaskCount)
            {
                EditorGUILayout.HelpBox(
                    $"Призначено {assigned}/{BitmaskCount} масок. " +
                    "Порожні слоти автоматично замінять базовим тайлом.",
                    MessageType.Info);
            }

            if (hasAtlasMismatch)
            {
                EditorGUILayout.HelpBox(
                    "Спрайти бітмаски з різних текстурних атласів. " +
                    "Шейдер читає лише один атлас — деякі маски не відображатимуться правильно. " +
                    "Використовуйте спрайти з одного атласу.",
                    MessageType.Warning);
            }

            if (baseTex != null && firstMaskTex != null && firstMaskTex != baseTex)
            {
                EditorGUILayout.HelpBox(
                    "Спрайти бітмаски та базовий тайл з різних текстур. " +
                    "Рекомендується тримати всі в одному атласі.",
                    MessageType.Warning);
            }
        }

        // ─── Icons ────────────────────────────────────────────────────────────

        private void DrawIconsSection()
        {
            _iconsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_iconsFoldout,
                new GUIContent("Іконки на тумані", "Повторювані іконки (наприклад, черепи / запитання), розміщені поверх туману"));

            if (_iconsFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_fogIconSpritesProp,
                    new GUIContent("Іконки (масив)", "Масив спрайтів іконок. Іконки циклічно розставляються по сітці туману"),
                    true);

                EditorGUILayout.Space(4);

                var iconArray = _fogIconSpritesProp;
                bool iconsEmpty = iconArray.arraySize == 0;
                bool hasNullIcon = false;
                bool hasAtlasMismatch = false;
                Texture firstIconTex = null;

                for (int i = 0; i < iconArray.arraySize; i++)
                {
                    var elem = iconArray.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue == null)
                    {
                        hasNullIcon = true;
                        continue;
                    }
                    var s = (Sprite)elem.objectReferenceValue;
                    if (firstIconTex == null)
                        firstIconTex = s.texture;
                    else if (s.texture != firstIconTex)
                        hasAtlasMismatch = true;
                }

                if (iconsEmpty)
                {
                    EditorGUILayout.HelpBox(
                        "Масив іконок порожній — іконки на тумані не відображатимуться.",
                        MessageType.Info);
                }
                else
                {
                    if (hasNullIcon)
                        EditorGUILayout.HelpBox(
                            "Деякі слоти іконок порожні (null). Порожні слоти пропускаються при рендері.",
                            MessageType.Warning);

                    if (hasAtlasMismatch)
                        EditorGUILayout.HelpBox(
                            "Іконки з різних текстурних атласів. " +
                            "Шейдер читає лише один атлас — використовуйте іконки з одного атласу.",
                            MessageType.Warning);
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(_fogIconGridSizeProp,
                    new GUIContent("Розмір сітки іконок", "Незалежна сітка (X стовпців × Y рядків), по якій розставляються іконки по карті"));
                EditorGUILayout.PropertyField(_fogIconScaleProp,
                    new GUIContent("Масштаб іконки", "Розмір іконки відносно клітинки туману (0.1 = дуже дрібно, 1.0 = повна клітинка)"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void DrawSpriteFieldWithPreview(SerializedProperty spriteProp, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(label);

            var prevSprite = spriteProp.objectReferenceValue as Sprite;

            // Sprite object field
            spriteProp.objectReferenceValue = EditorGUILayout.ObjectField(
                prevSprite, typeof(Sprite), false,
                GUILayout.Height(SpritePreviewSize), GUILayout.Width(SpritePreviewSize));

            EditorGUILayout.EndHorizontal();
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
