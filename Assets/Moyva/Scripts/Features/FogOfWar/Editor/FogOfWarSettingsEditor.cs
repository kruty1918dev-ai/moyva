using Kruty1918.Moyva.FogOfWar.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    [CustomEditor(typeof(FogOfWarSettings))]
    public sealed class FogOfWarSettingsEditor : UnityEditor.Editor
    {
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
        private SerializedProperty _fogTileSpritePixelSizeProp;
        private SerializedProperty _fogTileSizeInCellsProp;
        private SerializedProperty _fogTileSeamOverlapPixelsProp;
        private SerializedProperty _fogMapEdgePaddingPixelsProp;
        private SerializedProperty _fogTileTilingProp;
        private SerializedProperty _fogIconSpritesProp;
        private SerializedProperty _fogIconSpritePixelSizeProp;
        private SerializedProperty _fogIconGridSizeProp;
        private SerializedProperty _fogIconScaleProp;
        private SerializedProperty _enable3DVolumeFogProp;
        private SerializedProperty _fog3DTopClearanceProp;
        private SerializedProperty _fog3DVolumeHeightProp;
        private SerializedProperty _enableRendererCullingProp;
        private SerializedProperty _requireOpaqueUnexploredForCullingProp;
        private SerializedProperty _rendererCullingLayerMaskProp;
        private SerializedProperty _enableShaderFogCullingProp;
        private SerializedProperty _rendererCullingMaxRenderersPerFrameProp;
        private SerializedProperty _rendererCullingDiscoveryIntervalProp;
        private SerializedProperty _rendererCullingBoundsPaddingCellsProp;
        private SerializedProperty _shaderFogCullThresholdProp;

        // ─── Foldout state ────────────────────────────────────────────────────
        private bool _visionFoldout = true;
        private bool _heightFoldout = false;
        private bool _colorsFoldout = true;
        private bool _tileFoldout = true;
        private bool _volume3DFoldout = true;
        private bool _cullingFoldout = true;
        private bool _iconsFoldout = true;
        private float _previewFogValue = 0f;

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
            _fogTileSpritePixelSizeProp = serializedObject.FindProperty("FogTileSpritePixelSize");
            _fogTileSizeInCellsProp     = serializedObject.FindProperty("FogTileSizeInCells");
            _fogTileSeamOverlapPixelsProp = serializedObject.FindProperty("FogTileSeamOverlapPixels");
            _fogMapEdgePaddingPixelsProp = serializedObject.FindProperty("FogMapEdgePaddingPixels");
            _fogTileTilingProp          = serializedObject.FindProperty("FogTileTiling");
            _fogIconSpritesProp         = serializedObject.FindProperty("FogIconSprites");
            _fogIconSpritePixelSizeProp = serializedObject.FindProperty("FogIconSpritePixelSize");
            _fogIconGridSizeProp        = serializedObject.FindProperty("FogIconGridSize");
            _fogIconScaleProp           = serializedObject.FindProperty("FogIconScale");
            _enable3DVolumeFogProp      = serializedObject.FindProperty("Enable3DVolumeFog");
            _fog3DTopClearanceProp      = serializedObject.FindProperty("Fog3DTopClearance");
            _fog3DVolumeHeightProp      = serializedObject.FindProperty("Fog3DVolumeHeight");
            _enableRendererCullingProp  = serializedObject.FindProperty("EnableRendererCulling");
            _requireOpaqueUnexploredForCullingProp = serializedObject.FindProperty("RequireOpaqueUnexploredForCulling");
            _rendererCullingLayerMaskProp = serializedObject.FindProperty("RendererCullingLayerMask");
            _enableShaderFogCullingProp = serializedObject.FindProperty("EnableShaderFogCulling");
            _rendererCullingMaxRenderersPerFrameProp = serializedObject.FindProperty("RendererCullingMaxRenderersPerFrame");
            _rendererCullingDiscoveryIntervalProp = serializedObject.FindProperty("RendererCullingDiscoveryInterval");
            _rendererCullingBoundsPaddingCellsProp = serializedObject.FindProperty("RendererCullingBoundsPaddingCells");
            _shaderFogCullThresholdProp = serializedObject.FindProperty("ShaderFogCullThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptReference();
            DrawVisionSection();
            DrawHeightVisionSection();
            DrawColorSection();
            DrawTileSection();
            Draw3DVolumeSection();
            DrawCullingSection();
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
                EditorGUILayout.PropertyField(_fogTileSpritePixelSizeProp,
                    new GUIContent("Розмір спрайта, px", "Скільки пікселів читати з текстури, починаючи від rect переданого спрайта. Наприклад 16x16 для одного тайла"));
                DrawSpritePixelSizeValidation(_fogTileSpriteProp, _fogTileSpritePixelSizeProp, "тайлу");

                EditorGUILayout.PropertyField(_fogTileSizeInCellsProp,
                    new GUIContent("Розмір тайла", "Візуальний footprint одного спрайт-тайла у клітинках мапи. Сітка туману не змінюється: кожна клітинка все одно малює свій спрайт, але значення більше 1 дозволяють йому перекривати сусідів"));
                DrawTileSizeInCellsValidation();

                EditorGUILayout.PropertyField(_fogTileSeamOverlapPixelsProp,
                    new GUIContent("Перекриття швів, px", "Додаткове перекриття країв тайла у пікселях спрайта. 1-2 px зазвичай прибирають просвіти на зумі без накопичення альфи"));
                DrawTileSeamOverlapValidation();

                EditorGUILayout.PropertyField(_fogMapEdgePaddingPixelsProp,
                    new GUIContent("Запас краю мапи, px", "Наскільки розширити геометрію туману за зовнішній край мапи, у пікселях спрайта. Прибирає просвіти саме на краю мапи під час зуму"));
                DrawMapEdgePaddingValidation();

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
                EditorGUILayout.PropertyField(_fogIconSpritePixelSizeProp,
                    new GUIContent("Розмір іконки, px", "Скільки пікселів читати з текстури, починаючи від rect першого переданого спрайта іконки"));
                DrawSpritePixelSizeValidation(GetFirstIconProperty(), _fogIconSpritePixelSizeProp, "іконки");

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

        private void DrawCullingSection()
        {
            _cullingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_cullingFoldout,
                new GUIContent("Маска рендеру за туманом", "Коли туман повністю закриває клітинку, об'єкти під ним не рендеряться"));

            if (_cullingFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_enableRendererCullingProp,
                    new GUIContent("Увімкнути culling рендерерів", "Повністю вимикає рендерери під повністю закритим туманом"));
                EditorGUILayout.PropertyField(_requireOpaqueUnexploredForCullingProp,
                    new GUIContent("Тільки при непрозорому чорному", "Culling активний лише коли UnexploredAlpha >= 0.99"));
                EditorGUILayout.PropertyField(_rendererCullingLayerMaskProp,
                    new GUIContent("Шари для culling", "Які world layers підпадають під відсікання туманом"));
                EditorGUILayout.PropertyField(_rendererCullingMaxRenderersPerFrameProp,
                    new GUIContent("Макс. рендерерів за кадр", "Скільки об'єктів перевіряти за кадр"));
                EditorGUILayout.PropertyField(_rendererCullingDiscoveryIntervalProp,
                    new GUIContent("Інтервал пошуку, сек", "Як часто шукати нові world-renderers"));
                EditorGUILayout.PropertyField(_rendererCullingBoundsPaddingCellsProp,
                    new GUIContent("Padding bounds, клітинки", "Невеликий запас для стабільності на межах"));

                EditorGUILayout.Space(4f);
                EditorGUILayout.PropertyField(_enableShaderFogCullingProp,
                    new GUIContent("Shader fog culling", "Глобальні shader-флаги для піксельного відсікання"));
                EditorGUILayout.PropertyField(_shaderFogCullThresholdProp,
                    new GUIContent("Shader threshold", "Значення fog texture, нижче якого піксель вважається повністю закритим"));

                DrawCullingPreview();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void Draw3DVolumeSection()
        {
            _volume3DFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_volume3DFoldout,
                new GUIContent("3D volume", "Як Fog of War розміщується у XZ/3D світах"));

            if (_volume3DFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_enable3DVolumeFogProp,
                    new GUIContent("Увімкнути 3D volume", "У 3D світі туман стає world-space шаром над рельєфом з вертикальними боками"));
                EditorGUILayout.PropertyField(_fog3DTopClearanceProp,
                    new GUIContent("Відступ над рельєфом", "Наскільки підняти верхню площину туману над найвищою точкою рельєфу"));
                EditorGUILayout.PropertyField(_fog3DVolumeHeightProp,
                    new GUIContent("Висота volume", "Вертикальна товщина бокових площин туману"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawCullingPreview()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);

            _previewFogValue = EditorGUILayout.Slider(
                new GUIContent("Fog Value", "0 = повністю чорний/закритий, 1 = повністю видимий"),
                _previewFogValue,
                0f,
                1f);

            bool isVisible = _previewFogValue >= 0.9f;
            bool isExplored = _previewFogValue >= 0.3f && _previewFogValue < 0.9f;
            bool isUnexplored = _previewFogValue < 0.3f;

            string stateLabel = isVisible ? "Visible" : (isExplored ? "Explored" : "Unexplored (black)");
            bool rendererCullingEnabled = _enableRendererCullingProp != null && _enableRendererCullingProp.boolValue;
            bool requireOpaque = _requireOpaqueUnexploredForCullingProp != null && _requireOpaqueUnexploredForCullingProp.boolValue;
            bool opaqueCondition = !requireOpaque || _unexploredAlphaProp.floatValue >= 0.99f;
            bool rendererWillBeCulled = rendererCullingEnabled && opaqueCondition && isUnexplored;

            bool shaderEnabled = _enableShaderFogCullingProp != null && _enableShaderFogCullingProp.boolValue;
            float threshold = _shaderFogCullThresholdProp != null ? _shaderFogCullThresholdProp.floatValue : 0.01f;
            bool shaderWillClip = shaderEnabled && _previewFogValue <= threshold;

            EditorGUILayout.HelpBox($"Fog State: {stateLabel}", MessageType.None);
            EditorGUILayout.HelpBox(
                rendererWillBeCulled
                    ? "Renderer Culling Preview: об'єкт буде вимкнений (не рендериться)."
                    : "Renderer Culling Preview: об'єкт залишиться увімкненим.",
                rendererWillBeCulled ? MessageType.Info : MessageType.Warning);
            EditorGUILayout.HelpBox(
                shaderWillClip
                    ? "Shader Preview: піксель вважатиметься прихованим за порогом."
                    : "Shader Preview: піксель не перетинає поріг clipping.",
                shaderWillClip ? MessageType.Info : MessageType.None);

            if (requireOpaque && _unexploredAlphaProp.floatValue < 0.99f)
            {
                EditorGUILayout.HelpBox(
                    "Renderer culling зараз заблокований, бо увімкнено 'Тільки при непрозорому чорному', але UnexploredAlpha < 0.99.",
                    MessageType.Warning);
            }
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

        private SerializedProperty GetFirstIconProperty()
        {
            return _fogIconSpritesProp.arraySize > 0 ? _fogIconSpritesProp.GetArrayElementAtIndex(0) : null;
        }

        private static void DrawSpritePixelSizeValidation(SerializedProperty spriteProp, SerializedProperty pixelSizeProp, string spriteName)
        {
            if (spriteProp == null || pixelSizeProp == null || !(spriteProp.objectReferenceValue is Sprite sprite))
                return;

            Vector2Int size = pixelSizeProp.vector2IntValue;
            Rect rect = sprite.textureRect;

            if (size.x <= 0 || size.y <= 0)
            {
                EditorGUILayout.HelpBox($"Розмір {spriteName} має бути більшим за 0 px по X/Y.", MessageType.Warning);
                return;
            }

            if (sprite.texture != null && (rect.x + size.x > sprite.texture.width || rect.y + size.y > sprite.texture.height))
            {
                EditorGUILayout.HelpBox(
                    $"Розмір {spriteName} виходить за межі текстури. Буде використано обрізаний UV rect.",
                    MessageType.Warning);
            }

            int rectWidth = Mathf.RoundToInt(rect.width);
            int rectHeight = Mathf.RoundToInt(rect.height);
            if (size.x != rectWidth || size.y != rectHeight)
            {
                EditorGUILayout.HelpBox(
                    $"Sprite rect має {rectWidth}x{rectHeight} px, але рендер читатиме {size.x}x{size.y} px від його початку.",
                    MessageType.Info);
            }
        }

        private void DrawTileSizeInCellsValidation()
        {
            if (_fogTileSizeInCellsProp == null)
                return;

            Vector2 size = _fogTileSizeInCellsProp.vector2Value;
            if (size.x <= 0f || size.y <= 0f)
            {
                EditorGUILayout.HelpBox(
                    "Розмір тайла у клітинках має бути більшим за 0 по X/Y. Значення буде затиснуто в runtime.",
                    MessageType.Warning);
            }

            if (size.x > 9f || size.y > 9f)
            {
                EditorGUILayout.HelpBox(
                    "Shader композитить перекриття з найближчих клітинок. Значення понад 9 клітинок можуть обрізати далекі краї спрайта.",
                    MessageType.Warning);
            }
        }

        private void DrawTileSeamOverlapValidation()
        {
            if (_fogTileSeamOverlapPixelsProp == null)
                return;

            float overlap = _fogTileSeamOverlapPixelsProp.floatValue;
            if (overlap < 0f)
            {
                EditorGUILayout.HelpBox(
                    "Перекриття швів не може бути від'ємним. Значення буде затиснуто в runtime.",
                    MessageType.Warning);
            }

            if (overlap > 4f)
            {
                EditorGUILayout.HelpBox(
                    "Зазвичай достатньо 1-2 px. Великі значення можуть помітно розтягувати край тайла.",
                    MessageType.Info);
            }
        }

        private void DrawMapEdgePaddingValidation()
        {
            if (_fogMapEdgePaddingPixelsProp == null)
                return;

            float padding = _fogMapEdgePaddingPixelsProp.floatValue;
            if (padding < 0f)
            {
                EditorGUILayout.HelpBox(
                    "Запас краю мапи не може бути від'ємним. Значення буде затиснуто в runtime.",
                    MessageType.Warning);
            }

            if (padding > 8f)
            {
                EditorGUILayout.HelpBox(
                    "Зазвичай достатньо 1-4 px. Великі значення можуть помітно виводити туман за межі мапи.",
                    MessageType.Info);
            }
        }
    }
}
