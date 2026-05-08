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
        private const float DropAreaHeight = 48f;

        private SerializedProperty _enabledProp;
        private SerializedProperty _maxActiveCloudsProp;
        private SerializedProperty _initialCloudsProp;
        private SerializedProperty _spawnIntervalRangeProp;
        private SerializedProperty _cloudSpritesProp;
        private SerializedProperty _speedRangeProp;
        private SerializedProperty _scaleRangeProp;
        private SerializedProperty _leftToRightChanceProp;
        private SerializedProperty _spawnHorizontalPaddingProp;
        private SerializedProperty _spawnVerticalPaddingProp;
        private SerializedProperty _despawnHorizontalPaddingProp;
        private SerializedProperty _fadeDurationProp;
        private SerializedProperty _cloudColorProp;
        private SerializedProperty _cloudAlphaProp;
        private SerializedProperty _sortingLayerNameProp;
        private SerializedProperty _sortingOrderProp;
        private SerializedProperty _shadowsEnabledProp;
        private SerializedProperty _shadowOffsetProp;
        private SerializedProperty _shadowColorProp;
        private SerializedProperty _shadowAlphaMultiplierProp;
        private SerializedProperty _shadowScaleMultiplierProp;
        private SerializedProperty _shadowSortingOrderOffsetProp;

        private bool _generalFoldout = true;
        private bool _spritesFoldout = true;
        private bool _movementFoldout = true;
        private bool _viewFoldout = true;
        private bool _shadowsFoldout = true;
        private bool _documentationFoldout = true;

        private GUIStyle _dropAreaStyle;

        private void OnEnable()
        {
            _enabledProp = serializedObject.FindProperty("Enabled");
            _maxActiveCloudsProp = serializedObject.FindProperty("MaxActiveClouds");
            _initialCloudsProp = serializedObject.FindProperty("InitialClouds");
            _spawnIntervalRangeProp = serializedObject.FindProperty("SpawnIntervalRange");
            _cloudSpritesProp = serializedObject.FindProperty("CloudSprites");
            _speedRangeProp = serializedObject.FindProperty("SpeedRange");
            _scaleRangeProp = serializedObject.FindProperty("ScaleRange");
            _leftToRightChanceProp = serializedObject.FindProperty("LeftToRightChance");
            _spawnHorizontalPaddingProp = serializedObject.FindProperty("SpawnHorizontalPadding");
            _spawnVerticalPaddingProp = serializedObject.FindProperty("SpawnVerticalPadding");
            _despawnHorizontalPaddingProp = serializedObject.FindProperty("DespawnHorizontalPadding");
            _fadeDurationProp = serializedObject.FindProperty("FadeDuration");
            _cloudColorProp = serializedObject.FindProperty("CloudColor");
            _cloudAlphaProp = serializedObject.FindProperty("CloudAlpha");
            _sortingLayerNameProp = serializedObject.FindProperty("SortingLayerName");
            _sortingOrderProp = serializedObject.FindProperty("SortingOrder");
            _shadowsEnabledProp = serializedObject.FindProperty("ShadowsEnabled");
            _shadowOffsetProp = serializedObject.FindProperty("ShadowOffset");
            _shadowColorProp = serializedObject.FindProperty("ShadowColor");
            _shadowAlphaMultiplierProp = serializedObject.FindProperty("ShadowAlphaMultiplier");
            _shadowScaleMultiplierProp = serializedObject.FindProperty("ShadowScaleMultiplier");
            _shadowSortingOrderOffsetProp = serializedObject.FindProperty("ShadowSortingOrderOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptReference();
            DrawDocumentationSection();
            DrawGeneralSection();
            DrawSpritesSection();
            DrawMovementSection();
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
                    "Система створює хмаринки за межами камери, рухає їх тільки горизонтально та плавно прибирає за протилежним краєм. " +
                    "Кожна хмаринка може мати темнішу копію-тінь із вертикальним зміщенням, щоб у top-down 2D вона виглядала нижче на мапі.",
                    MessageType.None);
                EditorGUILayout.HelpBox(
                    "Щоб швидко заповнити список, виділіть один або кілька Sprite/Texture у Project і натисніть 'Додати виділені', або перетягніть їх у зону спрайтів. " +
                    "Шанс є вагою вибору, а індикатор показує приблизну частку кожного спрайта серед усіх активних варіантів.",
                    MessageType.None);
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
            EditorGUI.LabelField(dropRect, "Перетягніть сюди Sprite або Texture з нарізаними Sprite", _dropAreaStyle);

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
                if (GUILayout.Button(new GUIContent("Додати виділені", "Додати всі виділені Sprite або Sprite всередині виділених Texture")))
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

        private void DrawViewSection()
        {
            _viewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_viewFoldout,
                new GUIContent("Вигляд", "Колір, прозорість та порядок рендерингу хмаринок"));

            if (_viewFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_cloudColorProp, new GUIContent("Колір", "Білий колір зберігає оригінальні кольори спрайта"));
                EditorGUILayout.PropertyField(_cloudAlphaProp, new GUIContent("Прозорість", "Загальна прозорість хмаринок"));
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
                    EditorGUILayout.PropertyField(_shadowOffsetProp, new GUIContent("Offset тіні", "Зміщення тіні відносно хмаринки. Y < 0 виглядає нижче на top-down мапі"));
                    EditorGUILayout.PropertyField(_shadowColorProp, new GUIContent("Колір тіні", "Колір темнішої копії"));
                    EditorGUILayout.PropertyField(_shadowAlphaMultiplierProp, new GUIContent("Прозорість тіні", "Множник прозорості відносно хмаринки"));
                    EditorGUILayout.PropertyField(_shadowScaleMultiplierProp, new GUIContent("Масштаб тіні", "Множник масштабу тіні"));
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

            Vector2 speedRange = _speedRangeProp.vector2Value;
            if (speedRange.x <= 0f || speedRange.y <= 0f)
            {
                EditorGUILayout.HelpBox("Швидкість має бути більшою за 0, інакше хмаринки не рухатимуться.", MessageType.Warning);
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
            if (objects == null)
                return sprites;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is Sprite directSprite)
                {
                    sprites.Add(directSprite);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(objects[i]);
                if (string.IsNullOrEmpty(path))
                    continue;

                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
                {
                    if (assets[assetIndex] is Sprite sprite)
                        sprites.Add(sprite);
                }
            }

            return sprites;
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