using Kruty1918.Moyva.Clouds.API;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Clouds.Editor
{
    [CustomEditor(typeof(CloudsSettings))]
    public sealed class CloudsSettingsEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 42f;
        private const float LivePreviewHeight = 250f;
        private const float DropAreaHeight = 48f;
        private const int PreviewCloudsCount = 3;

        private SerializedProperty _enabledProp;
        private SerializedProperty _maxActiveCloudsProp;
        private SerializedProperty _initialCloudsProp;
        private SerializedProperty _initialCloudsStartInViewProp;
        private SerializedProperty _spawnIntervalRangeProp;
        private SerializedProperty _spawnAreaModeProp;
        private SerializedProperty _minimumSpawnDistanceProp;
        private SerializedProperty _spawnPlacementAttemptsProp;
        private SerializedProperty _cloudSpritesProp;
        private SerializedProperty _speedRangeProp;
        private SerializedProperty _scaleRangeProp;
        private SerializedProperty _leftToRightChanceProp;
        private SerializedProperty _spawnHorizontalPaddingProp;
        private SerializedProperty _spawnVerticalPaddingProp;
        private SerializedProperty _despawnHorizontalPaddingProp;
        private SerializedProperty _fadeDurationProp;
        private SerializedProperty _mapMaskEnabledProp;
        private SerializedProperty _manualMapSizeProp;
        private SerializedProperty _manualMapCenterProp;
        private SerializedProperty _maskEdgeFadeWidthProp;
        private SerializedProperty _maskEdgeFadeStepsProp;
        private SerializedProperty _lifetimeDissolveEnabledProp;
        private SerializedProperty _lifetimeRangeProp;
        private SerializedProperty _dissolveDurationProp;
        private SerializedProperty _cloudColorProp;
        private SerializedProperty _cloudAlphaProp;
        private SerializedProperty _spriteMaterialProp;
        private SerializedProperty _cameraProximityFadeEnabledProp;
        private SerializedProperty _cameraFadeOrthographicRangeProp;
        private SerializedProperty _closeCameraAlphaMultiplierProp;
        private SerializedProperty _cameraFadeStepsProp;
        private SerializedProperty _sortingLayerNameProp;
        private SerializedProperty _sortingOrderProp;
        private SerializedProperty _shadowsEnabledProp;
        private SerializedProperty _cloudHeightProp;
        private SerializedProperty _shadowOffsetProp;
        private SerializedProperty _shadowOffsetPerHeightProp;
        private SerializedProperty _shadowColorProp;
        private SerializedProperty _shadowAlphaMultiplierProp;
        private SerializedProperty _shadowScaleMultiplierProp;
        private SerializedProperty _shadowScalePerHeightProp;
        private SerializedProperty _shadowAlphaHeightFadeProp;
        private SerializedProperty _shadowSortingOrderOffsetProp;

        private bool _generalFoldout = true;
        private bool _spritesFoldout = true;
        private bool _movementFoldout = true;
        private bool _mapMaskFoldout = true;
        private bool _dissolveFoldout = true;
        private bool _viewFoldout = true;
        private bool _shadowsFoldout = true;
        private bool _documentationFoldout = true;
        private bool _previewFoldout = true;

        private GUIStyle _dropAreaStyle;

        private void OnEnable()
        {
            EditorApplication.update += Repaint;

            _enabledProp = serializedObject.FindProperty("Enabled");
            _maxActiveCloudsProp = serializedObject.FindProperty("MaxActiveClouds");
            _initialCloudsProp = serializedObject.FindProperty("InitialClouds");
            _initialCloudsStartInViewProp = serializedObject.FindProperty("InitialCloudsStartInView");
            _spawnIntervalRangeProp = serializedObject.FindProperty("SpawnIntervalRange");
            _spawnAreaModeProp = serializedObject.FindProperty("SpawnAreaMode");
            _minimumSpawnDistanceProp = serializedObject.FindProperty("MinimumSpawnDistance");
            _spawnPlacementAttemptsProp = serializedObject.FindProperty("SpawnPlacementAttempts");
            _cloudSpritesProp = serializedObject.FindProperty("CloudSprites");
            _speedRangeProp = serializedObject.FindProperty("SpeedRange");
            _scaleRangeProp = serializedObject.FindProperty("ScaleRange");
            _leftToRightChanceProp = serializedObject.FindProperty("LeftToRightChance");
            _spawnHorizontalPaddingProp = serializedObject.FindProperty("SpawnHorizontalPadding");
            _spawnVerticalPaddingProp = serializedObject.FindProperty("SpawnVerticalPadding");
            _despawnHorizontalPaddingProp = serializedObject.FindProperty("DespawnHorizontalPadding");
            _fadeDurationProp = serializedObject.FindProperty("FadeDuration");
            _mapMaskEnabledProp = serializedObject.FindProperty("MapMaskEnabled");
            _manualMapSizeProp = serializedObject.FindProperty("ManualMapSize");
            _manualMapCenterProp = serializedObject.FindProperty("ManualMapCenter");
            _maskEdgeFadeWidthProp = serializedObject.FindProperty("MaskEdgeFadeWidth");
            _maskEdgeFadeStepsProp = serializedObject.FindProperty("MaskEdgeFadeSteps");
            _lifetimeDissolveEnabledProp = serializedObject.FindProperty("LifetimeDissolveEnabled");
            _lifetimeRangeProp = serializedObject.FindProperty("LifetimeRange");
            _dissolveDurationProp = serializedObject.FindProperty("DissolveDuration");
            _cloudColorProp = serializedObject.FindProperty("CloudColor");
            _cloudAlphaProp = serializedObject.FindProperty("CloudAlpha");
            _spriteMaterialProp = serializedObject.FindProperty("SpriteMaterial");
            _cameraProximityFadeEnabledProp = serializedObject.FindProperty("CameraProximityFadeEnabled");
            _cameraFadeOrthographicRangeProp = serializedObject.FindProperty("CameraFadeOrthographicRange");
            _closeCameraAlphaMultiplierProp = serializedObject.FindProperty("CloseCameraAlphaMultiplier");
            _cameraFadeStepsProp = serializedObject.FindProperty("CameraFadeSteps");
            _sortingLayerNameProp = serializedObject.FindProperty("SortingLayerName");
            _sortingOrderProp = serializedObject.FindProperty("SortingOrder");
            _shadowsEnabledProp = serializedObject.FindProperty("ShadowsEnabled");
            _cloudHeightProp = serializedObject.FindProperty("CloudHeight");
            _shadowOffsetProp = serializedObject.FindProperty("ShadowOffset");
            _shadowOffsetPerHeightProp = serializedObject.FindProperty("ShadowOffsetPerHeight");
            _shadowColorProp = serializedObject.FindProperty("ShadowColor");
            _shadowAlphaMultiplierProp = serializedObject.FindProperty("ShadowAlphaMultiplier");
            _shadowScaleMultiplierProp = serializedObject.FindProperty("ShadowScaleMultiplier");
            _shadowScalePerHeightProp = serializedObject.FindProperty("ShadowScalePerHeight");
            _shadowAlphaHeightFadeProp = serializedObject.FindProperty("ShadowAlphaHeightFade");
            _shadowSortingOrderOffsetProp = serializedObject.FindProperty("ShadowSortingOrderOffset");
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptReference();
            DrawDocumentationSection();
            DrawLivePreviewSection();
            DrawGeneralSection();
            DrawSpritesSection();
            DrawMovementSection();
            DrawMapMaskSection();
            DrawDissolveSection();
            DrawViewSection();
            DrawShadowsSection();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptReference()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var script = MonoScript.FromScriptableObject((CloudsSettings)target);
                EditorGUILayout.ObjectField("Скрипт", script, typeof(MonoScript), false);
            }
            EditorGUILayout.Space(4f);
        }

        private void DrawDocumentationSection()
        {
            _documentationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_documentationFoldout,
                new GUIContent("Як працює", "Коротке пояснення системи хмаринок"));

            if (_documentationFoldout)
            {
                EditorGUILayout.HelpBox(
                    "Система може одразу розкласти стартові хмаринки по всій мапі, а наступні створює біля краю мапи або камери залежно від режиму спавну. " +
                    "Маска мапи не дає хмаринкам показуватися за межами світу, а піксельний край робить вхід і вихід ступінчастим. " +
                    "Висота хмаринки автоматично впливає на позицію, розмір і прозорість тіні.",
                    MessageType.None);
                EditorGUILayout.HelpBox(
                    "Щоб швидко заповнити список, виділіть один або кілька Sprite/Texture у Project і натисніть 'Додати виділені', або перетягніть їх у зону спрайтів. " +
                    "Шанс є вагою вибору, а індикатор показує приблизну частку кожного спрайта серед усіх активних варіантів.",
                    MessageType.None);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawLivePreviewSection()
        {
            _previewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_previewFoldout,
                new GUIContent("Прев'ю", "Живий перегляд хмари, тіні, прозорості, швидкості та розчинення"));

            if (_previewFoldout)
            {
                Sprite sprite = ResolvePreviewSprite();
                Rect rect = GUILayoutUtility.GetRect(0f, LivePreviewHeight, GUILayout.ExpandWidth(true));
                DrawLivePreview(rect, sprite);
                EditorGUILayout.HelpBox(
                    "Прев'ю оновлюється автоматично. Воно показує три хмаринки з різною фазою руху, масштабом, швидкістю, тінню, прозорістю, маскою мапи та розчиненням.",
                    MessageType.None);

                if (sprite == null)
                    EditorGUILayout.HelpBox("Додайте хоча б один спрайт хмаринки, щоб побачити реальне прев'ю.", MessageType.Info);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawGeneralSection()
        {
            _generalFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_generalFoldout,
                new GUIContent("Загальне", "Базові параметри кількості та частоти появи хмаринок"));

            if (_generalFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_enabledProp, new GUIContent("Увімкнено", "Вмикає або вимикає систему хмаринок"));
                EditorGUILayout.PropertyField(_maxActiveCloudsProp, new GUIContent("Максимум хмаринок", "Скільки хмаринок може існувати одночасно"));
                EditorGUILayout.PropertyField(_initialCloudsProp, new GUIContent("На старті", "Скільки хмаринок створити одразу після запуску сцени"));
                EditorGUILayout.PropertyField(_initialCloudsStartInViewProp, new GUIContent("Одразу в зоні спавну", "Якщо увімкнено, стартові хмаринки одразу з'являються всередині обраної зони спавну"));
                EditorGUILayout.PropertyField(_spawnIntervalRangeProp, new GUIContent("Інтервал спавну", "Діапазон часу між появою нових хмаринок у секундах"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawSpritesSection()
        {
            _spritesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_spritesFoldout,
                new GUIContent("Спрайти хмаринок", "Спрайти та шанс вибору кожного варіанта"));

            if (_spritesFoldout)
            {
                EditorGUILayout.HelpBox(
                    "Додайте один або кілька спрайтів хмаринок. Поле 'Шанс' працює як вага: якщо одна хмаринка має шанс 2, а інша 1, перша обиратиметься приблизно вдвічі частіше.",
                    MessageType.None);

                DrawSpriteDropArea();
                DrawSpriteAutomationButtons();

                using (new EditorGUILayout.HorizontalScope())
                {
                    int newSize = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Кількість", "Кількість варіантів хмаринок"), _cloudSpritesProp.arraySize));
                    if (newSize != _cloudSpritesProp.arraySize)
                        _cloudSpritesProp.arraySize = newSize;

                    if (GUILayout.Button(new GUIContent("Додати", "Додати порожній варіант хмаринки"), GUILayout.Width(86f)))
                        AddSpriteVariant(null);
                }

                float totalChance = CalculateTotalChance();
                for (int i = 0; i < _cloudSpritesProp.arraySize; i++)
                    DrawSpriteVariant(_cloudSpritesProp.GetArrayElementAtIndex(i), i, totalChance);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawSpriteDropArea()
        {
            _dropAreaStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontStyle = FontStyle.Italic,
            };

            Rect dropRect = GUILayoutUtility.GetRect(0f, DropAreaHeight, GUILayout.ExpandWidth(true));
            EditorGUI.LabelField(dropRect, "Перетягніть сюди Sprite або texture з одним Sprite", _dropAreaStyle);

            Event current = Event.current;
            if (!dropRect.Contains(current.mousePosition))
                return;

            if (current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                current.Use();
            }
            else if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddSpritesFromObjects(DragAndDrop.objectReferences);
                current.Use();
            }
        }

        private void DrawSpriteAutomationButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Додати виділені", "Додати виділені Sprite або texture з одним Sprite")))
                    AddSpritesFromObjects(Selection.objects);

                if (GUILayout.Button(new GUIContent("Вирівняти шанси", "Поставити шанс 1 для всіх непорожніх спрайтів")))
                    SetEqualChances();

                if (GUILayout.Button(new GUIContent("Прибрати порожні", "Видалити рядки без спрайта або з шансом 0")))
                    RemoveEmptyVariants();
            }
        }

        private void DrawSpriteVariant(SerializedProperty variant, int index, float totalChance)
        {
            SerializedProperty spriteProp = variant.FindPropertyRelative("Sprite");
            SerializedProperty chanceProp = variant.FindPropertyRelative("Chance");
            float chance = Mathf.Max(0f, chanceProp.floatValue);
            float percent = totalChance > 0f ? chance / totalChance : 0f;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Варіант {index + 1}", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Видалити", "Видалити цей варіант"), GUILayout.Width(86f)))
                    {
                        _cloudSpritesProp.DeleteArrayElementAtIndex(index);
                        return;
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawSpritePreview(spriteProp.objectReferenceValue as Sprite);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.PropertyField(spriteProp, new GUIContent("Спрайт", "Спрайт хмаринки"));
                        chanceProp.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Шанс", "Вага вибору цього спрайта"), chanceProp.floatValue));
                        Rect progressRect = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
                        EditorGUI.ProgressBar(progressRect, percent, $"Ймовірність: {percent * 100f:0.#}%");
                    }
                }
            }
        }

        private void DrawMovementSection()
        {
            _movementFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_movementFoldout,
                new GUIContent("Рух і спавн", "Горизонтальний рух, позиція появи та плавне зникнення"));

            if (_movementFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spawnAreaModeProp, new GUIContent("Зона спавну", "CameraViewport = стара поведінка біля камери, MapBounds = природний розподіл по всій мапі"));
                EditorGUILayout.PropertyField(_minimumSpawnDistanceProp, new GUIContent("Мін. відстань", "Бажана мінімальна дистанція між активними хмаринками"));
                EditorGUILayout.PropertyField(_spawnPlacementAttemptsProp, new GUIContent("Спроби позиції", "Скільки разів система шукає менш скупчену позицію"));
                EditorGUILayout.PropertyField(_speedRangeProp, new GUIContent("Швидкість", "Діапазон горизонтальної швидкості"));
                EditorGUILayout.PropertyField(_scaleRangeProp, new GUIContent("Масштаб", "Діапазон випадкового масштабу хмаринок"));
                EditorGUILayout.PropertyField(_leftToRightChanceProp, new GUIContent("Шанс зліва направо", "Ймовірність руху хмаринки зліва направо"));
                EditorGUILayout.PropertyField(_spawnHorizontalPaddingProp, new GUIContent("Відступ появи X", "Наскільки далеко за екраном створювати хмаринку по горизонталі"));
                EditorGUILayout.PropertyField(_spawnVerticalPaddingProp, new GUIContent("Вертикальний запас", "Додаткова зона вище/нижче екрана, де може пройти хмаринка"));
                EditorGUILayout.PropertyField(_despawnHorizontalPaddingProp, new GUIContent("Відступ зникнення X", "Наскільки далеко за протилежним краєм хмаринка знищується"));
                EditorGUILayout.PropertyField(_fadeDurationProp, new GUIContent("Плавність появи", "Тривалість fade in та fade out у секундах"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawDissolveSection()
        {
            _dissolveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_dissolveFoldout,
                new GUIContent("Розчинення", "Плавне зникнення хмаринок після часу життя"));

            if (_dissolveFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_lifetimeDissolveEnabledProp, new GUIContent("Розчиняти з часом", "Якщо увімкнено, хмара після часу життя почне плавно зникати"));
                using (new EditorGUI.DisabledScope(!_lifetimeDissolveEnabledProp.boolValue))
                {
                    EditorGUILayout.PropertyField(_lifetimeRangeProp, new GUIContent("Час життя", "Діапазон секунд до початку розчинення"));
                    EditorGUILayout.PropertyField(_dissolveDurationProp, new GUIContent("Тривалість розчинення", "Скільки секунд триває плавне зникнення"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawMapMaskSection()
        {
            _mapMaskFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_mapMaskFoldout,
                new GUIContent("Маска мапи", "Обмеження хмаринок межами мапи та піксельний вхід/вихід біля краю"));

            if (_mapMaskFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_mapMaskEnabledProp, new GUIContent("Увімкнено", "Показувати хмаринки тільки в межах мапи"));
                using (new EditorGUI.DisabledScope(!_mapMaskEnabledProp.boolValue))
                {
                    EditorGUILayout.HelpBox("В ігровій сцені межі беруться з GridService автоматично. Ручні межі потрібні для сцен без GridService або для preview.", MessageType.None);
                    EditorGUILayout.PropertyField(_manualMapSizeProp, new GUIContent("Ручний розмір", "Розмір маски для сцен без GridService"));
                    EditorGUILayout.PropertyField(_manualMapCenterProp, new GUIContent("Ручний центр", "Центр маски для сцен без GridService"));
                    EditorGUILayout.PropertyField(_maskEdgeFadeWidthProp, new GUIContent("Ширина краю", "Скільки world units займає піксельний вхід/вихід біля маски"));
                    EditorGUILayout.PropertyField(_maskEdgeFadeStepsProp, new GUIContent("Піксельні кроки", "Кількість ступенів прозорості на краю маски"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private Sprite ResolvePreviewSprite()
        {
            return ResolvePreviewSprite(0);
        }

        private Sprite ResolvePreviewSprite(int offset)
        {
            int usableIndex = 0;
            for (int i = 0; i < _cloudSpritesProp.arraySize; i++)
            {
                SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(i);
                if (variant.FindPropertyRelative("Sprite").objectReferenceValue is Sprite sprite)
                {
                    if (usableIndex == offset)
                        return sprite;

                    usableIndex++;
                }
            }

            return offset > 0 ? ResolvePreviewSprite(0) : null;
        }

        private void DrawLivePreview(Rect rect, Sprite sprite)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.14f, 0.16f, 1f));
            DrawPreviewGrid(rect);

            Rect stageRect = new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height - 52f);
            EditorGUI.DrawRect(stageRect, new Color(0.08f, 0.09f, 0.1f, 1f));
            Rect maskRect = _mapMaskEnabledProp.boolValue
                ? new Rect(stageRect.x + 28f, stageRect.y + 18f, stageRect.width - 56f, stageRect.height - 36f)
                : stageRect;

            DrawPreviewMask(stageRect, maskRect);

            float speedMid = Mathf.Lerp(_speedRangeProp.vector2Value.x, _speedRangeProp.vector2Value.y, 0.5f);
            float previewCycle = Mathf.Max(2f, 5f / Mathf.Max(0.05f, speedMid));
            float normalizedTime = (float)(EditorApplication.timeSinceStartup % previewCycle) / previewCycle;

            GUI.BeginGroup(maskRect);
            Rect localMaskRect = new Rect(0f, 0f, maskRect.width, maskRect.height);
            for (int i = 0; i < PreviewCloudsCount; i++)
                DrawPreviewCloud(localMaskRect, normalizedTime, i);
            GUI.EndGroup();

            DrawPreviewArrow(stageRect, _leftToRightChanceProp.floatValue >= 0.5f ? 1f : -1f);
            DrawPreviewLabels(rect, speedMid, ResolvePreviewShadowOffset());
        }

        private void DrawPreviewCloud(Rect maskRect, float baseTime, int index)
        {
            Sprite sprite = ResolvePreviewSprite(index);
            float phase = Mathf.Repeat(baseTime + index * 0.32f, 1f);
            float direction = index == 1 ? -1f : 1f;
            float x = direction > 0f
                ? Mathf.Lerp(-36f, maskRect.width + 36f, phase)
                : Mathf.Lerp(maskRect.width + 36f, -36f, phase);
            float row = (index + 1f) / (PreviewCloudsCount + 1f);
            float y = Mathf.Lerp(maskRect.yMin + 24f, maskRect.yMax - 24f, row);
            float scale = Mathf.Lerp(_scaleRangeProp.vector2Value.x, _scaleRangeProp.vector2Value.y, index / (PreviewCloudsCount - 1f));
            float spriteSize = Mathf.Clamp(64f * scale, 34f, 128f);
            float dissolveFade = ResolvePreviewDissolveFade(phase);
            float fade = Mathf.Min(ResolvePreviewEdgeFade(phase), ResolvePreviewMaskEdgeFade(maskRect, x, y, spriteSize), dissolveFade);
            float alpha = _cloudAlphaProp.floatValue * fade;

            Vector2 shadowOffset = ResolvePreviewShadowOffset();
            if (_shadowsEnabledProp.boolValue)
            {
                Rect shadowRect = BuildPreviewSpriteRect(x + shadowOffset.x, y - shadowOffset.y, spriteSize * ResolvePreviewShadowScale());
                Color shadowColor = _shadowColorProp.colorValue;
                shadowColor.a *= _cloudAlphaProp.floatValue * ResolvePreviewShadowAlpha() * fade;
                DrawSpriteTexture(shadowRect, sprite, shadowColor);
            }

            Color cloudColor = _cloudColorProp.colorValue;
            cloudColor.a *= alpha;
            DrawSpriteTexture(BuildPreviewSpriteRect(x, y, spriteSize), sprite, cloudColor);
        }

        private static Rect BuildPreviewSpriteRect(float centerX, float centerY, float size)
        {
            return new Rect(centerX - size * 0.5f, centerY - size * 0.5f, size, size);
        }

        private static void DrawPreviewGrid(Rect rect)
        {
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.05f);
            for (float x = rect.x; x < rect.xMax; x += 24f)
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));
            for (float y = rect.y; y < rect.yMax; y += 24f)
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void DrawPreviewMask(Rect stageRect, Rect maskRect)
        {
            if (!_mapMaskEnabledProp.boolValue)
                return;

            EditorGUI.DrawRect(maskRect, new Color(0.1f, 0.13f, 0.14f, 1f));
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = new Color(0.7f, 0.95f, 1f, 0.55f);
            Handles.DrawAAPolyLine(2f,
                new Vector3(maskRect.xMin, maskRect.yMin),
                new Vector3(maskRect.xMax, maskRect.yMin),
                new Vector3(maskRect.xMax, maskRect.yMax),
                new Vector3(maskRect.xMin, maskRect.yMax),
                new Vector3(maskRect.xMin, maskRect.yMin));
            Handles.color = oldColor;
            Handles.EndGUI();

            DrawPixelMaskEdge(maskRect, true);
            DrawPixelMaskEdge(maskRect, false);
        }

        private static void DrawPixelMaskEdge(Rect maskRect, bool left)
        {
            float x = left ? maskRect.xMin : maskRect.xMax - 10f;
            for (int i = 0; i < 8; i++)
            {
                float alpha = (i % 2 == 0) ? 0.22f : 0.08f;
                EditorGUI.DrawRect(new Rect(x, maskRect.yMin + i * 12f, 10f, 8f), new Color(0.8f, 0.95f, 1f, alpha));
            }
        }

        private static void DrawSpriteTexture(Rect rect, Sprite sprite, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;

            if (sprite != null && sprite.texture != null)
            {
                Rect textureRect = sprite.textureRect;
                Rect uv = new Rect(
                    textureRect.x / sprite.texture.width,
                    textureRect.y / sprite.texture.height,
                    textureRect.width / sprite.texture.width,
                    textureRect.height / sprite.texture.height);
                GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, alphaBlend: true);
            }
            else
            {
                EditorGUI.DrawRect(rect, color);
            }

            GUI.color = oldColor;
        }

        private static void DrawPreviewArrow(Rect stageRect, float direction)
        {
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = new Color(0.8f, 0.9f, 1f, 0.5f);
            float y = stageRect.yMax - 14f;
            Vector3 from = new Vector3(stageRect.xMin + 18f, y);
            Vector3 to = new Vector3(stageRect.xMax - 18f, y);
            if (direction < 0f)
            {
                Vector3 temp = from;
                from = to;
                to = temp;
            }

            Handles.DrawLine(from, to);
            Handles.DrawLine(to, to + new Vector3(-direction * 9f, -5f));
            Handles.DrawLine(to, to + new Vector3(-direction * 9f, 5f));
            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void DrawPreviewLabels(Rect rect, float speed, Vector2 shadowOffset)
        {
            Rect labelRect = new Rect(rect.x + 12f, rect.yMax - 34f, rect.width - 24f, 22f);
            string dissolveText = _lifetimeDissolveEnabledProp.boolValue
                ? "розчинення увімкнено"
                : "розчинення вимкнено";
            string maskText = _mapMaskEnabledProp.boolValue ? "маска мапи увімкнена" : "маска вимкнена";
            string text = $"3 хмаринки   швидкість ~{speed:0.##} u/s   висота {_cloudHeightProp.floatValue:0.##}   тінь X {shadowOffset.x:0}px Y {shadowOffset.y:0}px   {maskText}   {dissolveText}";
            EditorGUI.LabelField(labelRect, text, EditorStyles.miniLabel);
        }

        private float ResolvePreviewMaskEdgeFade(Rect maskRect, float x, float y, float spriteSize)
        {
            if (!_mapMaskEnabledProp.boolValue || _maskEdgeFadeWidthProp.floatValue <= 0f)
                return 1f;

            float width = Mathf.Max(1f, _maskEdgeFadeWidthProp.floatValue * 32f);
            float half = spriteSize * 0.5f;
            float left = Mathf.Clamp01((x + half - maskRect.xMin) / width);
            float right = Mathf.Clamp01((maskRect.xMax - (x - half)) / width);
            float bottom = Mathf.Clamp01((y + half - maskRect.yMin) / width);
            float top = Mathf.Clamp01((maskRect.yMax - (y - half)) / width);
            float fade = Mathf.Min(left, right, bottom, top);
            int steps = Mathf.Max(1, _maskEdgeFadeStepsProp.intValue);
            return Mathf.Floor(fade * steps) / steps;
        }

        private Vector2 ResolvePreviewShadowOffset()
        {
            return (_shadowOffsetProp.vector2Value + _shadowOffsetPerHeightProp.vector2Value * _cloudHeightProp.floatValue) * 42f;
        }

        private float ResolvePreviewShadowScale()
        {
            return Mathf.Max(0.01f, _shadowScaleMultiplierProp.floatValue + _shadowScalePerHeightProp.floatValue * _cloudHeightProp.floatValue);
        }

        private float ResolvePreviewShadowAlpha()
        {
            return Mathf.Clamp01(_shadowAlphaMultiplierProp.floatValue / (1f + _cloudHeightProp.floatValue * _shadowAlphaHeightFadeProp.floatValue));
        }

        private float ResolvePreviewEdgeFade(float normalizedTime)
        {
            float fadePortion = 0.18f;
            float fadeIn = Mathf.Clamp01(normalizedTime / fadePortion);
            float fadeOut = Mathf.Clamp01((1f - normalizedTime) / fadePortion);
            return Mathf.Min(fadeIn, fadeOut);
        }

        private float ResolvePreviewDissolveFade(float normalizedTime)
        {
            if (!_lifetimeDissolveEnabledProp.boolValue)
                return 1f;

            float dissolveStart = 0.58f;
            if (normalizedTime <= dissolveStart)
                return 1f;

            return 1f - Mathf.Clamp01((normalizedTime - dissolveStart) / Mathf.Max(0.05f, 1f - dissolveStart));
        }

        private void DrawViewSection()
        {
            _viewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_viewFoldout,
                new GUIContent("Вигляд", "Колір, прозорість та порядок рендерингу хмаринок"));

            if (_viewFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_cloudColorProp, new GUIContent("Колір", "Білий колір зберігає оригінальні кольори спрайта"));
                EditorGUILayout.PropertyField(_cloudAlphaProp, new GUIContent("Прозорість", "Загальна прозорість хмаринок"));
                EditorGUILayout.PropertyField(_spriteMaterialProp, new GUIContent("Матеріал Sprite", "Необов'язковий sprite-compatible матеріал. Якщо порожньо, runtime використає Sprites/Default"));
                EditorGUILayout.PropertyField(_cameraProximityFadeEnabledProp, new GUIContent("Fade при близькому zoom", "Зменшує прозорість хмаринок, коли камера сильно наближена"));
                using (new EditorGUI.DisabledScope(!_cameraProximityFadeEnabledProp.boolValue))
                {
                    EditorGUILayout.PropertyField(_cameraFadeOrthographicRangeProp, new GUIContent("Zoom range fade", "X = близько і мінімальна прозорість, Y = далеко і повна прозорість"));
                    EditorGUILayout.PropertyField(_closeCameraAlphaMultiplierProp, new GUIContent("Прозорість зблизька", "Множник alpha при максимально близькому zoom"));
                    EditorGUILayout.PropertyField(_cameraFadeStepsProp, new GUIContent("Кроки fade", "Кількість ступенів прозорості для піксельного стилю"));
                }
                EditorGUILayout.PropertyField(_sortingLayerNameProp, new GUIContent("Шар сортування", "Назва sorting layer для SpriteRenderer"));
                EditorGUILayout.PropertyField(_sortingOrderProp, new GUIContent("Порядок сортування", "Порядок рендерингу хмаринок"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawShadowsSection()
        {
            _shadowsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_shadowsFoldout,
                new GUIContent("Тіні", "Темніша копія хмаринки з вертикальним offset для top-down 2D"));

            if (_shadowsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_shadowsEnabledProp, new GUIContent("Увімкнено", "Створювати темнішу копію як тінь"));
                using (new EditorGUI.DisabledScope(!_shadowsEnabledProp.boolValue))
                {
                    EditorGUILayout.PropertyField(_cloudHeightProp, new GUIContent("Висота хмари", "Висота над землею, від якої автоматично рахується тінь"));
                    EditorGUILayout.PropertyField(_shadowOffsetProp, new GUIContent("Базовий offset", "Базове зміщення тіні відносно хмаринки"));
                    EditorGUILayout.PropertyField(_shadowOffsetPerHeightProp, new GUIContent("Offset на висоту", "Додаткове зміщення тіні на одну одиницю висоти"));
                    EditorGUILayout.PropertyField(_shadowColorProp, new GUIContent("Колір тіні", "Колір темнішої копії"));
                    EditorGUILayout.PropertyField(_shadowAlphaMultiplierProp, new GUIContent("Базова прозорість", "Множник прозорості відносно хмаринки"));
                    EditorGUILayout.PropertyField(_shadowAlphaHeightFadeProp, new GUIContent("Ослаблення висотою", "Наскільки висота робить тінь слабшою"));
                    EditorGUILayout.PropertyField(_shadowScaleMultiplierProp, new GUIContent("Базовий масштаб", "Базовий множник масштабу тіні"));
                    EditorGUILayout.PropertyField(_shadowScalePerHeightProp, new GUIContent("Масштаб на висоту", "Додатковий масштаб тіні на одну одиницю висоти"));
                    EditorGUILayout.PropertyField(_shadowSortingOrderOffsetProp, new GUIContent("Зсув сортування", "Зсув sorting order тіні відносно хмаринки"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2f);
        }

        private void DrawValidation()
        {
            if (!HasUsableSprite())
            {
                EditorGUILayout.HelpBox(
                    "Додайте хоча б один спрайт хмаринки з шансом більше 0. Найшвидше: виділіть спрайти в Project і натисніть 'Додати виділені' або перетягніть їх у зону спрайтів.",
                    MessageType.Warning);
            }

            if (_initialCloudsProp.intValue > _maxActiveCloudsProp.intValue)
            {
                EditorGUILayout.HelpBox("Кількість хмаринок на старті не має перевищувати максимум активних хмаринок.", MessageType.Info);
            }

            if (_maxActiveCloudsProp.intValue <= 0)
            {
                EditorGUILayout.HelpBox("Максимум хмаринок дорівнює 0, тому система нічого не створить.", MessageType.Warning);
            }

            if (_initialCloudsProp.intValue <= 0)
            {
                EditorGUILayout.HelpBox("'На старті' дорівнює 0, тому після запуску сцени хмаринок одразу не буде.", MessageType.Info);
            }

            if (_initialCloudsProp.intValue > 0 && !_initialCloudsStartInViewProp.boolValue)
            {
                EditorGUILayout.HelpBox("'Одразу в кадрі' вимкнено: стартові хмаринки створяться за екраном і можуть з'явитися не одразу.", MessageType.Info);
            }

            Vector2 speedRange = _speedRangeProp.vector2Value;
            if (speedRange.x <= 0f || speedRange.y <= 0f)
            {
                EditorGUILayout.HelpBox("Швидкість має бути більшою за 0, інакше хмаринки не рухатимуться.", MessageType.Warning);
            }

            if (_lifetimeDissolveEnabledProp.boolValue && _dissolveDurationProp.floatValue <= 0f)
            {
                EditorGUILayout.HelpBox("Розчинення з часом увімкнено, але тривалість 0: хмаринка зникне миттєво після завершення часу життя.", MessageType.Info);
            }
        }

        private bool HasUsableSprite()
        {
            for (int i = 0; i < _cloudSpritesProp.arraySize; i++)
            {
                SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(i);
                if (variant.FindPropertyRelative("Sprite").objectReferenceValue != null &&
                    variant.FindPropertyRelative("Chance").floatValue > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private float CalculateTotalChance()
        {
            float total = 0f;
            for (int i = 0; i < _cloudSpritesProp.arraySize; i++)
            {
                SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(i);
                if (variant.FindPropertyRelative("Sprite").objectReferenceValue != null)
                    total += Mathf.Max(0f, variant.FindPropertyRelative("Chance").floatValue);
            }

            return total;
        }

        private void AddSpriteVariant(Sprite sprite)
        {
            int index = _cloudSpritesProp.arraySize;
            _cloudSpritesProp.InsertArrayElementAtIndex(index);
            SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(index);
            variant.FindPropertyRelative("Sprite").objectReferenceValue = sprite;
            variant.FindPropertyRelative("Chance").floatValue = 1f;
        }

        private void AddSpritesFromObjects(Object[] objects)
        {
            List<Sprite> sprites = ExtractSprites(objects);
            for (int i = 0; i < sprites.Count; i++)
                AddSpriteVariant(sprites[i]);
        }

        private static List<Sprite> ExtractSprites(Object[] objects)
        {
            var sprites = new List<Sprite>();
            var seenSpriteIds = new HashSet<int>();
            if (objects == null)
                return sprites;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is Sprite directSprite)
                {
                    if (seenSpriteIds.Add(directSprite.GetInstanceID()))
                        sprites.Add(directSprite);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(objects[i]);
                if (string.IsNullOrEmpty(path))
                    continue;

                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                Sprite singleSprite = null;
                for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
                {
                    if (assets[assetIndex] is Sprite sprite)
                        singleSprite = sprite;
                }

                if (singleSprite != null && CountSpritesAtPath(assets) == 1)
                {
                    if (seenSpriteIds.Add(singleSprite.GetInstanceID()))
                        sprites.Add(singleSprite);
                }
            }

            return sprites;
        }

        private static int CountSpritesAtPath(Object[] assets)
        {
            int count = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite)
                    count++;
            }

            return count;
        }

        private void SetEqualChances()
        {
            for (int i = 0; i < _cloudSpritesProp.arraySize; i++)
            {
                SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(i);
                if (variant.FindPropertyRelative("Sprite").objectReferenceValue != null)
                    variant.FindPropertyRelative("Chance").floatValue = 1f;
            }
        }

        private void RemoveEmptyVariants()
        {
            for (int i = _cloudSpritesProp.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty variant = _cloudSpritesProp.GetArrayElementAtIndex(i);
                bool hasSprite = variant.FindPropertyRelative("Sprite").objectReferenceValue != null;
                bool hasChance = variant.FindPropertyRelative("Chance").floatValue > 0f;
                if (!hasSprite || !hasChance)
                    _cloudSpritesProp.DeleteArrayElementAtIndex(i);
            }
        }

        private static void DrawSpritePreview(Sprite sprite)
        {
            Rect rect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
            EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f, 1f));

            if (sprite == null || sprite.texture == null)
                return;

            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, alphaBlend: true);
        }
    }
}