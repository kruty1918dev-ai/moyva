using System;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    internal sealed class UnitCreationWizardWindow : EditorWindow
    {
        private enum WizardStep
        {
            Identity = 0,
            Visual = 1,
            Stats = 2,
            MovementAnimation = 3,
            Review = 4,
        }

        private UnitDesignerWindow _host;
        private UnitRegistrySO _registry;
        private UnitClassConfig _draft;
        private Sprite _previewSprite;
        private bool _createPrefabFromSprite = true;
        private WizardStep _step;
        private Vector2 _scroll;
        private string _error;
        private bool _showAdvancedStats;

        private static readonly Color Accent = new Color(0.18f, 0.62f, 0.67f);
        private const float PreviewAreaHeight = 170f;

        public static void Open(UnitDesignerWindow host, UnitRegistrySO registry)
        {
            var window = CreateInstance<UnitCreationWizardWindow>();
            window.titleContent = new GUIContent("Create Unit Wizard");
            window.minSize = new Vector2(560f, 520f);
            window._host = host;
            window._registry = registry;
            window.InitializeDraft();
            window.ShowUtility();
            window.Focus();
        }

        private void InitializeDraft()
        {
            _draft = new UnitClassConfig
            {
                TypeId = "unit-new",
                Role = UnitRole.Worker,
                CombatType = UnitCombatType.Infantry,
                HitPoints = 100,
                BaseLevel = 1,
                VisionRange = 3,
                BaseStamina = 100f,
                VisionHeightBoostPerLevel = 0f,
                CanSeeCrest = true,
                CrestVisibilityFactor = 1f,
                DownSlopeVisionBonus = 0f,
                SilhouettePenalty = 0f,
                StaminaRandomRange = new Vector2(-5f, 5f),
                AnimationSettings = new UnitClassConfig().AnimationSettings,
            };
        }

        private void OnGUI()
        {
            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(4f);

            switch (_step)
            {
                case WizardStep.Identity:
                    DrawIdentityStep();
                    break;
                case WizardStep.Visual:
                    DrawVisualStep();
                    break;
                case WizardStep.Stats:
                    DrawStatsStep();
                    break;
                case WizardStep.MovementAnimation:
                    DrawMovementStep();
                    break;
                case WizardStep.Review:
                    DrawReviewStep();
                    break;
            }

            EditorGUILayout.EndScrollView();
            DrawFooter();
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 54f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, Accent * 0.72f);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, 20f), "Створення нового юніта", EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 28f, rect.width - 24f, 18f), $"Крок {(int)_step + 1}/5: {GetStepTitle(_step)}", EditorStyles.miniBoldLabel);
        }

        private static string GetStepTitle(WizardStep step)
        {
            switch (step)
            {
                case WizardStep.Identity: return "Identity";
                case WizardStep.Visual: return "Visual";
                case WizardStep.Stats: return "Stats";
                case WizardStep.MovementAnimation: return "Movement + Animation";
                case WizardStep.Review: return "Review";
                default: return string.Empty;
            }
        }

        private void DrawIdentityStep()
        {
            EditorGUILayout.HelpBox("Вкажіть основну ідентичність юніта. TypeId має бути унікальним і без символу '_' (приклад: archer, worker-heavy, scout01).", MessageType.Info);

            _draft.TypeId = EditorGUILayout.TextField(new GUIContent("TypeId", "Унікальний ID юніта. Мін: 1 символ. Макс: 64 (рекомендовано). Потрібен для lookup/save/runtime."), _draft.TypeId ?? string.Empty);
            _draft.Role = (UnitRole)EditorGUILayout.EnumPopup(new GUIContent("Role", "Тип ролі юніта: Worker або Military. Потрібен для фільтрів і балансу."), _draft.Role);
            _draft.CombatType = (UnitCombatType)EditorGUILayout.EnumPopup(new GUIContent("Combat Type", "Бойовий клас (Infantry/Cavalry/SiegeMachine). Потрібен для сценаріїв бою."), _draft.CombatType);
            _draft.BaseLevel = EditorGUILayout.IntSlider(new GUIContent("Base Level [1..10]", "Мін 1, макс 10. Впливає на масштаб/вагу юніта і баланс."), Mathf.Clamp(_draft.BaseLevel, 1, 10), 1, 10);

            if (string.IsNullOrWhiteSpace(_draft.TypeId))
                EditorGUILayout.HelpBox("TypeId обов'язковий.", MessageType.Warning);
            else if (_draft.TypeId.Contains("_"))
                EditorGUILayout.HelpBox("TypeId не може містити '_'", MessageType.Warning);
        }

        private void DrawVisualStep()
        {
            EditorGUILayout.HelpBox("Оберіть prefab або sprite. Якщо є лише sprite, можна автоматично створити prefab при Create.", MessageType.Info);

            _draft.Prefab = EditorGUILayout.ObjectField(new GUIContent("Prefab", "Опціонально. Якщо не задано, можна створити з sprite на фінальному кроці."), _draft.Prefab, typeof(GameObject), false) as GameObject;
            _previewSprite = EditorGUILayout.ObjectField(new GUIContent("Sprite", "Sprite для preview та (опційно) автогенерації prefab."), _previewSprite, typeof(Sprite), false) as Sprite;
            _draft.CustomSprite = EditorGUILayout.ObjectField(new GUIContent("Custom Sprite", "Кастомний sprite для юніта. Якщо пусто і вибрано Sprite вище, буде підставлено його."), _draft.CustomSprite, typeof(Sprite), false) as Sprite;
            _createPrefabFromSprite = EditorGUILayout.ToggleLeft(new GUIContent("Створити prefab зі sprite під час Create", "Якщо Prefab пустий і є Sprite, буде згенеровано prefab у Assets/Moyva/Prefabs/Units."), _createPrefabFromSprite);

            DrawDraftPreview("Попередній вигляд");

            if (_draft.Prefab == null && _previewSprite == null && _draft.CustomSprite == null)
                EditorGUILayout.HelpBox("Потрібно задати Prefab або Sprite/Custom Sprite.", MessageType.Warning);
        }

        private void DrawStatsStep()
        {
            EditorGUILayout.HelpBox("Задайте базові stats і видимість. На кожному полі є range і призначення.", MessageType.Info);

            _draft.HitPoints = EditorGUILayout.IntSlider(new GUIContent("Hit Points [1..300]", "Мін 1, макс 300. Показує живучість юніта і впливає на preview HP."), Mathf.Clamp(_draft.HitPoints, 1, 300), 1, 300);
            _draft.BaseStamina = EditorGUILayout.Slider(new GUIContent("Base Stamina [0..300]", "Мін 0, макс 300. Запас активності/руху юніта."), Mathf.Clamp(_draft.BaseStamina, 0f, 300f), 0f, 300f);
            _draft.VisionRange = EditorGUILayout.IntSlider(new GUIContent("Vision Range [1..20]", "Мін 1, макс 20. Радіус перевірки тайлів перед LOS-фільтром."), Mathf.Clamp(_draft.VisionRange, 1, 20), 1, 20);
            _draft.StaminaRandomRange = EditorGUILayout.Vector2Field(new GUIContent("Stamina Random Range", "Розкид стартової стаміни. Рекомендовано в межах [-50..50], щоб уникати екстремумів."), _draft.StaminaRandomRange);

            _showAdvancedStats = EditorGUILayout.Foldout(_showAdvancedStats, "Розширені бойові/terrain параметри", true);
            if (_showAdvancedStats)
            {
                _draft.VisionHeightBoostPerLevel = EditorGUILayout.Slider(new GUIContent("Vision Height Boost [0..4]", "Мін 0, макс 4. Бонус огляду за рівень висоти."), Mathf.Clamp(_draft.VisionHeightBoostPerLevel, 0f, 4f), 0f, 4f);
                _draft.CanSeeCrest = EditorGUILayout.Toggle(new GUIContent("Can See Crest", "Чи може бачити верхній край пагорба знизу."), _draft.CanSeeCrest);
                _draft.CrestVisibilityFactor = EditorGUILayout.Slider(new GUIContent("Crest Visibility Factor [0..1]", "Мін 0, макс 1. Сила видимості верхнього краю."), Mathf.Clamp01(_draft.CrestVisibilityFactor), 0f, 1f);
                _draft.DownSlopeVisionBonus = EditorGUILayout.Slider(new GUIContent("Down Slope Vision Bonus [0..6]", "Мін 0, макс 6. Додаткова видимість вниз зі схилу."), Mathf.Clamp(_draft.DownSlopeVisionBonus, 0f, 6f), 0f, 6f);
                _draft.SilhouettePenalty = EditorGUILayout.Slider(new GUIContent("Silhouette Penalty [0..1]", "Мін 0, макс 1. Наскільки юніт помітний на краю."), Mathf.Clamp01(_draft.SilhouettePenalty), 0f, 1f);

                _draft.CuttingDamage = EditorGUILayout.IntSlider(new GUIContent("Cutting Damage [0..300]", "Ріжуча шкода: шаблі/мечі. Потрібно для damage-профілю."), Mathf.Clamp(_draft.CuttingDamage, 0, 300), 0, 300);
                _draft.PenetratingDamage = EditorGUILayout.IntSlider(new GUIContent("Penetrating Damage [0..300]", "Колюча шкода: списи/стріли."), Mathf.Clamp(_draft.PenetratingDamage, 0, 300), 0, 300);
                _draft.CrushingDamage = EditorGUILayout.IntSlider(new GUIContent("Crushing Damage [0..300]", "Дробляча шкода: важка зброя/облога."), Mathf.Clamp(_draft.CrushingDamage, 0, 300), 0, 300);

                _draft.CuttingDefense = EditorGUILayout.IntSlider(new GUIContent("Cutting Defense [0..300]", "Захист від ріжучої шкоди."), Mathf.Clamp(_draft.CuttingDefense, 0, 300), 0, 300);
                _draft.PenetratingDefense = EditorGUILayout.IntSlider(new GUIContent("Penetrating Defense [0..300]", "Захист від колючої шкоди."), Mathf.Clamp(_draft.PenetratingDefense, 0, 300), 0, 300);
                _draft.CrushingDefense = EditorGUILayout.IntSlider(new GUIContent("Crushing Defense [0..300]", "Захист від дроблячої шкоди."), Mathf.Clamp(_draft.CrushingDefense, 0, 300), 0, 300);
            }

            DrawDraftPreview("Stats Preview");
        }

        private void DrawMovementStep()
        {
            EditorGUILayout.HelpBox("Налаштуйте темп руху. Preview нижче показує, як юніт рухається до створення.", MessageType.Info);

            _draft.AnimationSettings.MoveDurationPerTile = EditorGUILayout.Slider(new GUIContent("Move Duration Per Tile [0.02..2.0]", "Мін 0.02, макс 2.0 сек/тайл. Менше = швидше."), Mathf.Clamp(_draft.AnimationSettings.MoveDurationPerTile, 0.02f, 2f), 0.02f, 2f);
            _draft.AnimationSettings.DelayOnTile = EditorGUILayout.Slider(new GUIContent("Delay On Tile [0..1.0]", "Мін 0, макс 1 сек. Пауза на вузлах маршруту."), Mathf.Clamp(_draft.AnimationSettings.DelayOnTile, 0f, 1f), 0f, 1f);

            DrawDraftPreview("Movement Preview");
        }

        private void DrawReviewStep()
        {
            EditorGUILayout.HelpBox("Перевірте всі параметри перед створенням. Можна повернутися назад і відредагувати.", MessageType.Info);

            EditorGUILayout.LabelField("TypeId", _draft.TypeId ?? string.Empty);
            EditorGUILayout.LabelField("Role", _draft.Role.ToString());
            EditorGUILayout.LabelField("Combat Type", _draft.CombatType.ToString());
            EditorGUILayout.LabelField("HP", _draft.HitPoints.ToString());
            EditorGUILayout.LabelField("Base Stamina", _draft.BaseStamina.ToString("0.##"));
            EditorGUILayout.LabelField("Vision", _draft.VisionRange.ToString());
            EditorGUILayout.LabelField("Base Level", _draft.BaseLevel.ToString());
            EditorGUILayout.LabelField("Prefab", _draft.Prefab != null ? _draft.Prefab.name : "(auto/none)");
            EditorGUILayout.LabelField("Sprite", _previewSprite != null ? _previewSprite.name : (_draft.CustomSprite != null ? _draft.CustomSprite.name : "(none)"));
            EditorGUILayout.LabelField("Move Duration", _draft.AnimationSettings.MoveDurationPerTile.ToString("0.00"));
            EditorGUILayout.LabelField("Delay On Tile", _draft.AnimationSettings.DelayOnTile.ToString("0.00"));

            DrawDraftPreview("Фінальний preview");
        }

        private void DrawDraftPreview(string title)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, PreviewAreaHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.13f, 0.14f) : new Color(0.82f, 0.84f, 0.86f));

            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 18f), title, EditorStyles.miniBoldLabel);

            float t = (float)(EditorApplication.timeSinceStartup % 1.0);
            float pingPong = Mathf.PingPong(t * 2f, 1f);
            float moveDuration = Mathf.Max(0.02f, _draft.AnimationSettings.MoveDurationPerTile);
            float delay = Mathf.Max(0f, _draft.AnimationSettings.DelayOnTile);
            float normalized = Mathf.Clamp01(pingPong / Mathf.Max(0.05f, moveDuration + delay));

            Vector2 left = new Vector2(rect.x + 80f, rect.center.y + 20f);
            Vector2 right = new Vector2(rect.xMax - 80f, rect.center.y + 20f);
            Vector2 pos = Vector2.Lerp(left, right, normalized);

            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.25f);
            Handles.DrawLine(left, right);
            Handles.DrawSolidDisc(left, Vector3.forward, 4f);
            Handles.DrawSolidDisc(right, Vector3.forward, 4f);
            Handles.EndGUI();

            Sprite sprite = ResolvePreviewSprite();
            float size = Mathf.Lerp(36f, 70f, Mathf.InverseLerp(1f, 300f, Mathf.Max(1, _draft.HitPoints))) * Mathf.Lerp(0.92f, 1.1f, Mathf.InverseLerp(1f, 10f, Mathf.Max(1, _draft.BaseLevel)));
            Rect spriteRect = new Rect(pos.x - size * 0.5f, pos.y - size, size, size);

            if (sprite != null)
                GUI.DrawTexture(spriteRect, sprite.texture, ScaleMode.ScaleToFit, true);
            else
                EditorGUI.DrawRect(spriteRect, new Color(0.2f, 0.56f, 0.7f, 0.85f));

            Rect hpBg = new Rect(spriteRect.x, spriteRect.yMax + 6f, spriteRect.width, 6f);
            EditorGUI.DrawRect(hpBg, new Color(0f, 0f, 0f, 0.45f));
            float hpNorm = Mathf.InverseLerp(1f, 300f, Mathf.Max(1, _draft.HitPoints));
            EditorGUI.DrawRect(new Rect(hpBg.x, hpBg.y, hpBg.width * hpNorm, hpBg.height), new Color(0.15f, 0.78f, 0.38f, 0.95f));

            GUI.Label(new Rect(rect.x + 12f, rect.yMax - 22f, rect.width - 24f, 16f),
                $"HP {_draft.HitPoints} | Vision {_draft.VisionRange} | Move {_draft.AnimationSettings.MoveDurationPerTile:0.00}s | Delay {_draft.AnimationSettings.DelayOnTile:0.00}s",
                EditorStyles.centeredGreyMiniLabel);
        }

        private Sprite ResolvePreviewSprite()
        {
            if (_draft.CustomSprite != null)
                return _draft.CustomSprite;

            if (_previewSprite != null)
                return _previewSprite;

            if (_draft.Prefab != null && _draft.Prefab.TryGetComponent<SpriteRenderer>(out var sr))
                return sr.sprite;

            return null;
        }

        private void DrawFooter()
        {
            if (!string.IsNullOrWhiteSpace(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Error);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            using (new EditorGUI.DisabledScope(_step == WizardStep.Identity))
            {
                if (GUILayout.Button("Назад", GUILayout.Height(26f)))
                {
                    _error = null;
                    _step--;
                }
            }

            GUILayout.FlexibleSpace();

            if (_step != WizardStep.Review)
            {
                if (GUILayout.Button("Далі", GUILayout.Width(120f), GUILayout.Height(26f)))
                {
                    if (ValidateCurrentStep(out string stepError))
                    {
                        _error = null;
                        _step++;
                    }
                    else
                    {
                        _error = stepError;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Створити", GUILayout.Width(140f), GUILayout.Height(26f)))
                {
                    if (!ValidateAll(out string error))
                    {
                        _error = error;
                    }
                    else if (_host == null)
                    {
                        _error = "Unit Designer вікно недоступне.";
                    }
                    else if (_host.TryCreateUnitFromWizard(CloneDraft(), ResolvePreviewSprite(), _createPrefabFromSprite, out string createError))
                    {
                        Close();
                    }
                    else
                    {
                        _error = createError;
                    }
                }
            }

            if (GUILayout.Button("Скасувати", GUILayout.Width(120f), GUILayout.Height(26f)))
                Close();

            EditorGUILayout.EndHorizontal();
        }

        private bool ValidateCurrentStep(out string error)
        {
            switch (_step)
            {
                case WizardStep.Identity:
                    if (string.IsNullOrWhiteSpace(_draft.TypeId))
                    {
                        error = "TypeId обов'язковий.";
                        return false;
                    }

                    if (_draft.TypeId.Contains("_"))
                    {
                        error = "TypeId не може містити '_'";
                        return false;
                    }
                    break;
                case WizardStep.Visual:
                    if (_draft.Prefab == null && ResolvePreviewSprite() == null)
                    {
                        error = "Задайте Prefab або Sprite.";
                        return false;
                    }
                    break;
                case WizardStep.Stats:
                    if (_draft.HitPoints < 1 || _draft.VisionRange < 1 || _draft.BaseLevel < 1)
                    {
                        error = "Перевірте мінімальні значення: HP>=1, Vision>=1, Level>=1.";
                        return false;
                    }
                    break;
            }

            error = null;
            return true;
        }

        private bool ValidateAll(out string error)
        {
            if (string.IsNullOrWhiteSpace(_draft.TypeId))
            {
                error = "TypeId обов'язковий.";
                return false;
            }

            if (_draft.TypeId.Contains("_"))
            {
                error = "TypeId не може містити '_'";
                return false;
            }

            if (_draft.Prefab == null && ResolvePreviewSprite() == null)
            {
                error = "Потрібно задати Prefab або Sprite для створення.";
                return false;
            }

            if (_draft.HitPoints < 1 || _draft.BaseLevel < 1 || _draft.VisionRange < 1 || _draft.BaseStamina < 0f)
            {
                error = "Некоректні базові параметри.";
                return false;
            }

            error = null;
            return true;
        }

        private UnitClassConfig CloneDraft()
        {
            return new UnitClassConfig
            {
                TypeId = _draft.TypeId,
                Role = _draft.Role,
                CombatType = _draft.CombatType,
                BaseStamina = _draft.BaseStamina,
                VisionRange = _draft.VisionRange,
                VisionHeightBoostPerLevel = _draft.VisionHeightBoostPerLevel,
                CanSeeCrest = _draft.CanSeeCrest,
                CrestVisibilityFactor = _draft.CrestVisibilityFactor,
                DownSlopeVisionBonus = _draft.DownSlopeVisionBonus,
                SilhouettePenalty = _draft.SilhouettePenalty,
                HitPoints = _draft.HitPoints,
                BaseLevel = _draft.BaseLevel,
                CuttingDamage = _draft.CuttingDamage,
                PenetratingDamage = _draft.PenetratingDamage,
                CrushingDamage = _draft.CrushingDamage,
                CuttingDefense = _draft.CuttingDefense,
                PenetratingDefense = _draft.PenetratingDefense,
                CrushingDefense = _draft.CrushingDefense,
                Prefab = _draft.Prefab,
                CustomSprite = _draft.CustomSprite,
                StaminaRandomRange = _draft.StaminaRandomRange,
                AnimationSettings = _draft.AnimationSettings,
                AnimationClips = _draft.AnimationClips != null ? new System.Collections.Generic.List<UnitAnimationClip>(_draft.AnimationClips) : new System.Collections.Generic.List<UnitAnimationClip>()
            };
        }
    }
}
