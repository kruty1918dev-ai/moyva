using System;
using System.Collections.Generic;
using System.Globalization;
using Kruty1918.Moyva.Units.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow
    {
        private const string CombatDefenderPrefsKey = "Moyva.UnitDesigner.CombatDefenderTypeId";
        private const string CombatMatrixPrefsKey = "Moyva.UnitDesigner.CombatShowMatrix";

        private static readonly Color CuttingColor = new Color(0.82f, 0.28f, 0.22f);
        private static readonly Color PenetratingColor = new Color(0.24f, 0.56f, 0.82f);
        private static readonly Color CrushingColor = new Color(0.77f, 0.62f, 0.24f);
        private static readonly Color ArmorColor = new Color(0.48f, 0.55f, 0.62f);

        private static readonly UnitCombatPreset[] CombatTablePresets =
        {
            new UnitCombatPreset("Swordman", "swordman", UnitCombatType.Infantry, 100, 1, 1, 25, 5, 5, 10, 5, 5),
            new UnitCombatPreset("Spearman", "spearman", UnitCombatType.Infantry, 100, 1, 1, 5, 25, 0, 5, 5, 5),
            new UnitCombatPreset("Archer", "archer", UnitCombatType.Infantry, 100, 1, 1, 0, 25, 0, 2, 3, 0),
            new UnitCombatPreset("Light cavalry", "light-cavalry", UnitCombatType.Cavalry, 100, 2, 1, 25, 5, 5, 10, 5, 0),
            new UnitCombatPreset("Heavy cavalry", "heavy-cavalry", UnitCombatType.Cavalry, 100, 2, 1, 5, 35, 5, 15, 5, 5),
            new UnitCombatPreset("Horse archer", "horse-archer", UnitCombatType.Cavalry, 100, 2, 1, 0, 25, 0, 5, 5, 0),
            new UnitCombatPreset("Catapult", "catapult", UnitCombatType.SiegeMachine, 100, 1, 1, 0, 0, 50, 5, 5, 0),
            new UnitCombatPreset("Ram", "ram", UnitCombatType.SiegeMachine, 100, 1, 1, 0, 0, 40, 20, 20, 10),
        };

        private Vector2 _combatRulesScroll;
        private Vector2 _combatPreviewScroll;
        private Vector2 _combatMatrixScroll;
        private string _combatDefenderTypeId = string.Empty;
        private bool _combatShowMatrix = true;
        private int _combatPresetIndex;

        private void InitializeCombatDesigner()
        {
            _combatDefenderTypeId = EditorPrefs.GetString(CombatDefenderPrefsKey, string.Empty);
            _combatShowMatrix = EditorPrefs.GetBool(CombatMatrixPrefsKey, true);
        }

        private void DisposeCombatDesigner()
        {
            EditorPrefs.SetString(CombatDefenderPrefsKey, _combatDefenderTypeId ?? string.Empty);
            EditorPrefs.SetBool(CombatMatrixPrefsKey, _combatShowMatrix);
        }

        private bool IsCombatWorkspaceActive() => _workspaceMode == WorkspaceMode.CombatSystem;

        private void OnCombatWorkspaceSelected()
        {
            ResolveCombatDefenderIndex();
        }

        private void DrawCombatWorkspace()
        {
            ResolveCombatDefenderIndex();

            EditorGUILayout.BeginHorizontal();
            DrawUnitListPanel(GUILayout.Width(Mathf.Clamp(position.width * 0.23f, 280f, 350f)));
            DrawCombatRulesPanel(GUILayout.MinWidth(360f), GUILayout.Width(Mathf.Clamp(position.width * 0.34f, 380f, 540f)));
            DrawCombatPreviewPanel(GUILayout.MinWidth(420f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCombatCompactSection(SerializedProperty unit)
        {
            BeginSection("Бій", "d_ParticleSystem Icon", "HP, тип юніта, рівень, шкода і захист для бойової системи.");
            DrawCombatGeneralFields(unit, compact: true);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Шкода", EditorStyles.miniBoldLabel);
            DrawCombatTripleFields(unit, "PenetratingDamage", "CuttingDamage", "CrushingDamage", "Колюча", "Ріжуча", "Дроб.", 60);

            EditorGUILayout.LabelField("Захист", EditorStyles.miniBoldLabel);
            DrawCombatTripleFields(unit, "PenetratingDefense", "CuttingDefense", "CrushingDefense", "Колючий", "Ріжучий", "Дроб.", 40);

            if (GUILayout.Button(new GUIContent("Відкрити бойове preview", "Перейти у режим з duel preview, матрицею переваг і пресетами з таблиці."), GUILayout.Height(24f)))
            {
                _workspaceMode = WorkspaceMode.CombatSystem;
                OnCombatWorkspaceSelected();
            }

            EndSection();
        }

        private void DrawCombatRulesPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Система бою", IconContent("d_ParticleSystem Icon", string.Empty, "Редагування бойових чисел вибраного юніта."));

            if (!HasSelectedUnit())
            {
                EditorGUILayout.HelpBox("Оберіть юніта, щоб налаштувати його бойовий профіль.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var unit = SelectedUnitProperty();
            _combatRulesScroll = EditorGUILayout.BeginScrollView(_combatRulesScroll);
            DrawCombatGeneralSection(unit);
            DrawCombatDamageSection(unit);
            DrawCombatDefenseSection(unit);
            DrawCombatPresetSection(unit);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawCombatGeneralSection(SerializedProperty unit)
        {
            BeginSection("Загальні бойові параметри", "d_Settings", "Тип юніта, HP, базовий рівень і дальність огляду з таблиці.");
            EditorGUILayout.HelpBox("Окремої системи зброї немає: спис, меч або таран описуються бойовим профілем самого юніта через три типи шкоди.", MessageType.None);
            DrawCombatGeneralFields(unit, compact: false);
            EndSection();
        }

        private void DrawCombatGeneralFields(SerializedProperty unit, bool compact)
        {
            var combatType = unit.FindPropertyRelative("CombatType");
            var hp = unit.FindPropertyRelative("HitPoints");
            var level = unit.FindPropertyRelative("BaseLevel");
            var vision = unit.FindPropertyRelative("VisionRange");

            if (combatType == null || hp == null || level == null || vision == null)
            {
                EditorGUILayout.HelpBox("Бойові поля ще не доступні у SerializedObject. Перекомпілюй/перевідкрий Unity, якщо це лишиться після збірки.", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(combatType, new GUIContent("Тип юніта", "Infantry, Cavalry або Siege Machine."));
            hp.intValue = EditorGUILayout.IntSlider(new GUIContent("HP", "Очки здоров'я з таблиці бойових характеристик."), Mathf.Max(1, hp.intValue), 1, 300);
            level.intValue = EditorGUILayout.IntSlider(new GUIContent("Базовий рівень", "Кожна різниця рівня дає 10% модифікатора шкоди, обмежено 50%..150%."), Mathf.Max(1, level.intValue), 1, 10);
            vision.intValue = EditorGUILayout.IntSlider(new GUIContent("Базова дальність огляду", "Базова дистанція огляду. Використовує існуюче поле Vision Range."), Mathf.Max(1, vision.intValue), 1, 20);

            if (!compact)
            {
                DrawMetricBar("HP", hp.intValue, 0f, 300f, Good, "Скільки шкоди треба завдати, щоб перемогти юніта.");
                DrawMetricBar("Рівень", level.intValue, 1f, 10f, Accent, "Впливає на множник шкоди проти нижчих або вищих рівнів.");
            }
        }

        private void DrawCombatDamageSection(SerializedProperty unit)
        {
            BeginSection("Розрахунок шкоди", "d_Profiler.Rendering", "Три типи атаки з таблиці: колюча, ріжуча, дробляча.");
            DrawCombatValueSlider(unit, "PenetratingDamage", "Колюча шкода", PenetratingColor, 80, "Списи, стріли та точкові пробивні удари. Кавалерія особливо чутлива до цього профілю.");
            DrawCombatValueSlider(unit, "CuttingDamage", "Ріжуча шкода", CuttingColor, 80, "Мечі, шаблі та інші удари лезом.");
            DrawCombatValueSlider(unit, "CrushingDamage", "Дробляча шкода", CrushingColor, 80, "Катапульти, тарани, булави та важкі удари.");
            DrawCombatProfileStrip("Профіль шкоди", GetInt(unit, "CuttingDamage"), GetInt(unit, "PenetratingDamage"), GetInt(unit, "CrushingDamage"), 80);
            EndSection();
        }

        private void DrawCombatDefenseSection(SerializedProperty unit)
        {
            BeginSection("Розрахунок захисту", "d_Shield Icon", "Кожен захист поглинає відповідний тип шкоди перед сумуванням.");
            DrawCombatValueSlider(unit, "PenetratingDefense", "Колючий захист", PenetratingColor, 60, "Захист від колючої шкоди.");
            DrawCombatValueSlider(unit, "CuttingDefense", "Ріжучий захист", CuttingColor, 60, "Захист від ріжучої шкоди.");
            DrawCombatValueSlider(unit, "CrushingDefense", "Дроблячий захист", CrushingColor, 60, "Захист від дроблячої шкоди.");
            DrawCombatProfileStrip("Профіль захисту", GetInt(unit, "CuttingDefense"), GetInt(unit, "PenetratingDefense"), GetInt(unit, "CrushingDefense"), 60);
            EndSection();
        }

        private void DrawCombatPresetSection(SerializedProperty unit)
        {
            BeginSection("Таблиця юнітів", "d_UnityEditor.InspectorWindow", "Швидке заповнення даних зі скріншота таблиці.");

            string[] presetNames = new string[CombatTablePresets.Length];
            for (int i = 0; i < CombatTablePresets.Length; i++)
                presetNames[i] = CombatTablePresets[i].UnitName;

            int matchingPreset = FindPresetIndexForUnit(unit);
            if (matchingPreset >= 0 && Event.current.type == EventType.Layout)
                _combatPresetIndex = matchingPreset;

            _combatPresetIndex = EditorGUILayout.Popup(new GUIContent("Рядок таблиці", "Рядок із наданої таблиці."), Mathf.Clamp(_combatPresetIndex, 0, CombatTablePresets.Length - 1), presetNames);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Застосувати до вибраного", "Підставити HP, дальність огляду, базовий рівень, шкоду і захист з вибраного рядка."), GUILayout.Height(26f)))
                    ApplyCombatPreset(unit, CombatTablePresets[_combatPresetIndex], applyIdentity: false, clearPrefab: false);

                if (GUILayout.Button(new GUIContent("Створити/оновити всі", "Додати або оновити 8 юнітів із таблиці, не видаляючи існуючі prefab."), GUILayout.Height(26f)))
                    ApplyAllCombatPresets();
            }

            UnitCombatPreset preset = CombatTablePresets[Mathf.Clamp(_combatPresetIndex, 0, CombatTablePresets.Length - 1)];
            EditorGUILayout.HelpBox($"{preset.UnitName}: {preset.CombatType}, HP {preset.HitPoints}, огляд {preset.BaseViewDistance}, шкода {FormatPresetDamageTriplet(preset)}, захист {FormatPresetDefenseTriplet(preset)}.", MessageType.None);
            EndSection();
        }

        private void DrawCombatPreviewPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Бій: preview і переваги", IconContent("d_Animation.EventMarker", string.Empty, "Візуалізація дуелі, шкоди, захистів і матчапів."));

            if (!HasSelectedUnit() || _configs == null || _configs.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Для preview потрібні хоча б один атакер і один захисник у registry.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _combatPreviewScroll = EditorGUILayout.BeginScrollView(_combatPreviewScroll);
            int defenderIndex = DrawCombatDefenderSelector();
            var attackerProperty = SelectedUnitProperty();
            var defenderProperty = _configs.GetArrayElementAtIndex(defenderIndex);
            var attacker = BuildCombatConfig(attackerProperty);
            var defender = BuildCombatConfig(defenderProperty);
            var duel = UnitCombatCalculator.CalculateDuel(attacker, defender);

            DrawBattlefieldPreview(attackerProperty, defenderProperty, duel);
            DrawCombatOutcomeSummary(attacker, defender, duel);
            DrawAttackBreakdown("Атака вибраного юніта", attacker, defender, duel.AttackerAttack);
            DrawAttackBreakdown("Контратака захисника", defender, attacker, duel.DefenderCounterAttack);
            DrawCombatNuances(attacker, defender, duel);
            DrawCombatMatrixSection();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private int DrawCombatDefenderSelector()
        {
            string[] displayNames = BuildCombatUnitDisplayNames();
            int defenderIndex = ResolveCombatDefenderIndex();
            int nextIndex = EditorGUILayout.Popup(new GUIContent("Захисник", "Юніт, проти якого симулюється бій."), defenderIndex, displayNames);
            if (nextIndex != defenderIndex && nextIndex >= 0 && nextIndex < _configs.arraySize)
            {
                defenderIndex = nextIndex;
                _combatDefenderTypeId = GetString(_configs.GetArrayElementAtIndex(defenderIndex), "TypeId");
            }

            return defenderIndex;
        }

        private void DrawBattlefieldPreview(SerializedProperty attackerProperty, SerializedProperty defenderProperty, UnitCombatDuel duel)
        {
            Rect rect = GUILayoutUtility.GetRect(360f, 250f, GUILayout.ExpandWidth(true));
            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.13f, 0.14f) : new Color(0.78f, 0.82f, 0.84f));
            DrawPreviewGrid(rect, 28f);

            var attackerPrefab = GetObject<GameObject>(attackerProperty, "Prefab");
            var defenderPrefab = GetObject<GameObject>(defenderProperty, "Prefab");
            var attackerSprite = ResolveSprite(attackerPrefab);
            var defenderSprite = ResolveSprite(defenderPrefab);

            Rect attackerFrame = new Rect(rect.x + 34f, rect.y + 54f, 112f, 112f);
            Rect defenderFrame = new Rect(rect.xMax - 146f, rect.y + 54f, 112f, 112f);
            DrawSpriteOrPrefab(attackerFrame, attackerSprite, attackerPrefab, true);
            DrawSpriteOrPrefab(defenderFrame, defenderSprite, defenderPrefab, true);

            Vector2 attackStart = new Vector2(attackerFrame.xMax + 18f, attackerFrame.center.y - 10f);
            Vector2 attackEnd = new Vector2(defenderFrame.xMin - 18f, defenderFrame.center.y - 10f);
            Vector2 counterStart = new Vector2(defenderFrame.xMin - 18f, defenderFrame.center.y + 22f);
            Vector2 counterEnd = new Vector2(attackerFrame.xMax + 18f, attackerFrame.center.y + 22f);

            DrawArrow(attackStart, attackEnd, Good, 3f);
            DrawArrow(counterStart, counterEnd, Warn, 2f);
            DrawDamageBubble(new Rect((attackStart.x + attackEnd.x) * 0.5f - 42f, attackStart.y - 30f, 84f, 24f), duel.AttackerAttack.TotalDamage, Good);
            DrawDamageBubble(new Rect((counterStart.x + counterEnd.x) * 0.5f - 42f, counterStart.y + 8f, 84f, 24f), duel.DefenderCounterAttack.TotalDamage, Warn);

            string attackerName = GetString(attackerProperty, "TypeId");
            string defenderName = GetString(defenderProperty, "TypeId");
            GUI.Label(new Rect(attackerFrame.x - 12f, rect.y + 12f, attackerFrame.width + 24f, 20f), string.IsNullOrWhiteSpace(attackerName) ? "Атакер" : attackerName, PreviewTitleStyle());
            GUI.Label(new Rect(defenderFrame.x - 12f, rect.y + 12f, defenderFrame.width + 24f, 20f), string.IsNullOrWhiteSpace(defenderName) ? "Захисник" : defenderName, PreviewTitleStyle());

            int attackerHp = Mathf.Max(1, GetInt(attackerProperty, "HitPoints"));
            int defenderHp = Mathf.Max(1, GetInt(defenderProperty, "HitPoints"));
            DrawHpBar(new Rect(attackerFrame.x, attackerFrame.yMax + 12f, attackerFrame.width, 18f), attackerHp - duel.DefenderCounterAttack.TotalDamage, attackerHp, "після контратаки");
            DrawHpBar(new Rect(defenderFrame.x, defenderFrame.yMax + 12f, defenderFrame.width, 18f), defenderHp - duel.AttackerAttack.TotalDamage, defenderHp, "після удару");

            GUI.Label(new Rect(rect.x + 12f, rect.yMax - 28f, rect.width - 24f, 18f), ResolveOutcomeText(duel), EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawCombatOutcomeSummary(UnitClassConfig attacker, UnitClassConfig defender, UnitCombatDuel duel)
        {
            BeginSection("Підсумок дуелі", "d_TimelineSelector", "Скільки ударів потрібно кожному юніту, щоб перемогти іншого.");
            Color outcomeColor = duel.Outcome switch
            {
                UnitCombatOutcome.AttackerAdvantage => Good,
                UnitCombatOutcome.DefenderAdvantage => Bad,
                _ => Warn,
            };

            Rect badge = GUILayoutUtility.GetRect(0f, 32f, GUILayout.ExpandWidth(true));
            DrawPanelBackground(badge, outcomeColor * 0.72f);
            GUI.Label(badge, ResolveOutcomeText(duel), BadgeStyle());

            DrawMetricBar("Удар", duel.AttackerAttack.TotalDamage, 0f, 80f, Good, "Скільки HP захисник втрачає за один удар атакера.");
            DrawMetricBar("Контратака", duel.DefenderCounterAttack.TotalDamage, 0f, 80f, Warn, "Скільки HP атакер втрачає за один удар захисника.");
            DrawMetricBar("Перевага", duel.AdvantageScore, -8f, 8f, outcomeColor, "Позитивне значення означає, що вибраний атакер перемагає за менше ударів.");

            EditorGUILayout.LabelField($"{attacker.TypeId}: {FormatHits(duel.AttackerAttack.HitsToDefeat)} до перемоги | {defender.TypeId}: {FormatHits(duel.DefenderCounterAttack.HitsToDefeat)} до перемоги", EditorStyles.miniLabel);
            EndSection();
        }

        private void DrawAttackBreakdown(string title, UnitClassConfig attacker, UnitClassConfig defender, UnitCombatBreakdown breakdown)
        {
            BeginSection(title, "d_Profiler.CPU", "Кожен тип шкоди мінус matching захист, потім множник рівня.");
            EditorGUILayout.LabelField($"{attacker.TypeId} -> {defender.TypeId} | level x{breakdown.LevelMultiplier:0.00}", EditorStyles.miniBoldLabel);
            DrawDamageTypeBreakdown("Колюча", breakdown.PenetratingRawDamage, breakdown.PenetratingDefense, breakdown.PenetratingEffectiveDamage, PenetratingColor);
            DrawDamageTypeBreakdown("Ріжуча", breakdown.CuttingRawDamage, breakdown.CuttingDefense, breakdown.CuttingEffectiveDamage, CuttingColor);
            DrawDamageTypeBreakdown("Дробляча", breakdown.CrushingRawDamage, breakdown.CrushingDefense, breakdown.CrushingEffectiveDamage, CrushingColor);
            EditorGUILayout.LabelField($"До рівня: {breakdown.EffectiveDamageBeforeLevel} | заблоковано: {breakdown.BlockedDamageTotal} | фінальна шкода: {breakdown.TotalDamage}", EditorStyles.miniLabel);
            EndSection();
        }

        private void DrawCombatNuances(UnitClassConfig attacker, UnitClassConfig defender, UnitCombatDuel duel)
        {
            BeginSection("Нюанси та переваги", "d_console.infoicon", "Короткі висновки з поточної пари юнітів.");
            var lines = BuildCombatNuanceLines(attacker, defender, duel);
            for (int i = 0; i < lines.Count; i++)
                EditorGUILayout.HelpBox(lines[i], MessageType.None);
            EndSection();
        }

        private void DrawCombatMatrixSection()
        {
            BeginSection("Матриця матчапів", "d_GridLayoutGroup Icon", "Клітинка показує фінальну шкоду/кількість ударів атакера у рядку проти захисника в колонці.");
            _combatShowMatrix = EditorGUILayout.ToggleLeft(new GUIContent("Показувати матрицю", "Швидкий огляд, хто кого контрить у всьому UnitRegistry."), _combatShowMatrix);
            if (!_combatShowMatrix)
            {
                EndSection();
                return;
            }

            if (_configs == null || _configs.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Немає юнітів для матриці.", MessageType.Info);
                EndSection();
                return;
            }

            const float labelWidth = 112f;
            const float cellWidth = 58f;
            const float rowHeight = 30f;
            float width = labelWidth + _configs.arraySize * cellWidth;
            float height = rowHeight + _configs.arraySize * rowHeight;
            _combatMatrixScroll = EditorGUILayout.BeginScrollView(_combatMatrixScroll, GUILayout.Height(Mathf.Min(300f, height + 22f)));
            Rect matrix = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));

            GUI.Label(new Rect(matrix.x, matrix.y, labelWidth, rowHeight), "А \\ З", MatrixHeaderStyle());
            for (int col = 0; col < _configs.arraySize; col++)
            {
                string defenderName = ShortUnitName(GetString(_configs.GetArrayElementAtIndex(col), "TypeId"));
                GUI.Label(new Rect(matrix.x + labelWidth + col * cellWidth, matrix.y, cellWidth, rowHeight), defenderName, MatrixHeaderStyle());
            }

            for (int row = 0; row < _configs.arraySize; row++)
            {
                var attackerProperty = _configs.GetArrayElementAtIndex(row);
                var attacker = BuildCombatConfig(attackerProperty);
                GUI.Label(new Rect(matrix.x, matrix.y + rowHeight + row * rowHeight, labelWidth, rowHeight), ShortUnitName(attacker.TypeId), MatrixHeaderStyle());

                for (int col = 0; col < _configs.arraySize; col++)
                {
                    var defender = BuildCombatConfig(_configs.GetArrayElementAtIndex(col));
                    var duel = UnitCombatCalculator.CalculateDuel(attacker, defender);
                    Rect cell = new Rect(matrix.x + labelWidth + col * cellWidth, matrix.y + rowHeight + row * rowHeight, cellWidth - 2f, rowHeight - 2f);
                    DrawMatrixCell(cell, duel, row == col);
                }
            }

            EditorGUILayout.EndScrollView();
            EndSection();
        }

        private void DrawCombatValueSlider(SerializedProperty unit, string propertyName, string label, Color color, int max, string tooltip)
        {
            var property = unit.FindPropertyRelative(propertyName);
            if (property == null)
                return;

            property.intValue = EditorGUILayout.IntSlider(new GUIContent(label, tooltip), Mathf.Max(0, property.intValue), 0, max);
            Rect rect = GUILayoutUtility.GetRect(0f, 8f, GUILayout.ExpandWidth(true));
            float t = Mathf.InverseLerp(0f, max, property.intValue);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2f, rect.width, 4f), new Color(1f, 1f, 1f, 0.08f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2f, rect.width * Mathf.Clamp01(t), 4f), color);
        }

        private void DrawCombatTripleFields(SerializedProperty unit, string first, string second, string third, string firstLabel, string secondLabel, string thirdLabel, int max)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCompactCombatInt(unit, first, firstLabel, max);
                DrawCompactCombatInt(unit, second, secondLabel, max);
                DrawCompactCombatInt(unit, third, thirdLabel, max);
            }
        }

        private void DrawCompactCombatInt(SerializedProperty unit, string propertyName, string label, int max)
        {
            var property = unit.FindPropertyRelative(propertyName);
            if (property == null)
                return;

            property.intValue = EditorGUILayout.IntField(label, Mathf.Clamp(property.intValue, 0, max));
        }

        private void DrawCombatProfileStrip(string label, int cutting, int penetrating, int crushing, int max)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y, 110f, 18f), label, EditorStyles.miniLabel);
            Rect bar = new Rect(rect.x + 114f, rect.y + 6f, rect.width - 120f, 14f);
            DrawPanelBackground(bar, EditorGUIUtility.isProSkin ? new Color(0.07f, 0.08f, 0.09f) : new Color(0.72f, 0.75f, 0.77f));

            int total = Mathf.Max(1, cutting + penetrating + crushing);
            float x = bar.x;
            DrawStripSegment(ref x, bar, penetrating / (float)total, Mathf.InverseLerp(0f, max, penetrating), PenetratingColor);
            DrawStripSegment(ref x, bar, cutting / (float)total, Mathf.InverseLerp(0f, max, cutting), CuttingColor);
            DrawStripSegment(ref x, bar, crushing / (float)total, Mathf.InverseLerp(0f, max, crushing), CrushingColor);
            GUI.Label(new Rect(bar.x, bar.yMax + 1f, bar.width, 13f), $"Колюча {penetrating} / Ріжуча {cutting} / Дробляча {crushing}", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawStripSegment(ref float x, Rect bar, float width01, float strength01, Color color)
        {
            float width = bar.width * Mathf.Clamp01(width01);
            if (width <= 0.5f)
                return;

            Color segment = Color.Lerp(color * 0.45f, color, Mathf.Clamp01(strength01));
            EditorGUI.DrawRect(new Rect(x, bar.y, width, bar.height), segment);
            x += width;
        }

        private void DrawDamageTypeBreakdown(string label, int raw, int defense, int effective, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y + 3f, 88f, 18f), label, EditorStyles.miniLabel);
            Rect bar = new Rect(rect.x + 92f, rect.y + 6f, rect.width - 190f, 12f);
            DrawPanelBackground(bar, EditorGUIUtility.isProSkin ? new Color(0.07f, 0.08f, 0.09f) : new Color(0.72f, 0.75f, 0.77f));
            float rawWidth = bar.width * Mathf.InverseLerp(0f, 80f, raw);
            float effectiveWidth = bar.width * Mathf.InverseLerp(0f, 80f, effective);
            EditorGUI.DrawRect(new Rect(bar.x, bar.y, rawWidth, bar.height), color * 0.32f);
            EditorGUI.DrawRect(new Rect(bar.x, bar.y, effectiveWidth, bar.height), color);
            GUI.Label(new Rect(bar.xMax + 8f, rect.y + 2f, 90f, 18f), $"{raw}-{defense}={effective}", EditorStyles.miniLabel);
        }

        private void DrawHpBar(Rect rect, int current, int max, string label)
        {
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);
            DrawPanelBackground(rect, new Color(0f, 0f, 0f, 0.32f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * (current / (float)max), rect.height), Color.Lerp(Bad, Good, current / (float)max));
            GUI.Label(rect, $"HP {current}/{max} {label}", BadgeStyle());
        }

        private void DrawDamageBubble(Rect rect, int damage, Color color)
        {
            DrawPanelBackground(rect, color * 0.82f);
            GUI.Label(rect, $"-{damage} HP", BadgeStyle());
        }

        private void DrawArrow(Vector2 from, Vector2 to, Color color, float width)
        {
            DrawLine(from, to, color, width);
            Vector2 direction = (to - from).normalized;
            Vector2 side = new Vector2(-direction.y, direction.x);
            Vector2 p1 = to - direction * 12f + side * 6f;
            Vector2 p2 = to - direction * 12f - side * 6f;
            DrawLine(to, p1, color, width);
            DrawLine(to, p2, color, width);
        }

        private void DrawMatrixCell(Rect rect, UnitCombatDuel duel, bool sameUnit)
        {
            Color color;
            if (sameUnit)
                color = ArmorColor * 0.7f;
            else if (duel.Outcome == UnitCombatOutcome.AttackerAdvantage)
                color = Good * 0.72f;
            else if (duel.Outcome == UnitCombatOutcome.DefenderAdvantage)
                color = Bad * 0.72f;
            else
                color = Warn * 0.62f;

            DrawPanelBackground(rect, color);
            GUI.Label(rect, $"{duel.AttackerAttack.TotalDamage}/{FormatHits(duel.AttackerAttack.HitsToDefeat)}", MatrixCellStyle());
        }

        private List<string> BuildCombatNuanceLines(UnitClassConfig attacker, UnitClassConfig defender, UnitCombatDuel duel)
        {
            var lines = new List<string>();
            if (duel.Outcome == UnitCombatOutcome.AttackerAdvantage)
                lines.Add($"{attacker.TypeId} має перевагу: йому потрібно {FormatHits(duel.AttackerAttack.HitsToDefeat)} ударів, а опоненту {FormatHits(duel.DefenderCounterAttack.HitsToDefeat)}.");
            else if (duel.Outcome == UnitCombatOutcome.DefenderAdvantage)
                lines.Add($"{defender.TypeId} контрить цю атаку: його контратака перемагає швидше.");
            else
                lines.Add("Матчап рівний за кількістю ударів до перемоги.");

            AddDefenseNuance(lines, defender.TypeId, duel.AttackerAttack);
            AddDefenseNuance(lines, attacker.TypeId, duel.DefenderCounterAttack);
            lines.Add($"Домінантна шкода атакера: {DamageTypeLabel(duel.AttackerAttack.DominantDamageType)}. Домінантна шкода захисника: {DamageTypeLabel(duel.DefenderCounterAttack.DominantDamageType)}.");

            if (duel.AttackerAttack.LevelMultiplier > 1.01f)
                lines.Add("Рівень атакера підсилює шкоду у цьому бою.");
            else if (duel.AttackerAttack.LevelMultiplier < 0.99f)
                lines.Add("Рівень захисника зменшує шкоду атакера.");

            return lines;
        }

        private void AddDefenseNuance(List<string> lines, string defenderName, UnitCombatBreakdown attack)
        {
            if (!attack.CanDealDamage)
            {
                lines.Add("Атакер не має жодного типу шкоди, тому не може реально вести бій.");
                return;
            }

            if (attack.BlockedDamageTotal >= attack.RawDamageTotal * 0.5f)
                lines.Add($"{defenderName} поглинає щонайменше половину базової шкоди, тож цей захист суттєвий.");
            else if (attack.BlockedDamageTotal <= attack.RawDamageTotal * 0.2f)
                lines.Add($"Захист {defenderName} поглинає мало шкоди: профіль атакера добре проходить через броню.");
        }

        private string[] BuildCombatUnitDisplayNames()
        {
            if (_configs == null || _configs.arraySize == 0)
                return Array.Empty<string>();

            string[] result = new string[_configs.arraySize];
            for (int i = 0; i < _configs.arraySize; i++)
            {
                var unit = _configs.GetArrayElementAtIndex(i);
                string typeId = GetString(unit, "TypeId");
                result[i] = string.IsNullOrWhiteSpace(typeId) ? $"#{i + 1}" : typeId;
            }

            return result;
        }

        private int ResolveCombatDefenderIndex()
        {
            if (_configs == null || _configs.arraySize == 0)
                return 0;

            if (!string.IsNullOrWhiteSpace(_combatDefenderTypeId))
            {
                for (int i = 0; i < _configs.arraySize; i++)
                {
                    if (string.Equals(GetString(_configs.GetArrayElementAtIndex(i), "TypeId"), _combatDefenderTypeId, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            int fallback = _configs.arraySize > 1 ? (_selectedIndex + 1) % _configs.arraySize : Mathf.Max(0, _selectedIndex);
            fallback = Mathf.Clamp(fallback, 0, _configs.arraySize - 1);
            _combatDefenderTypeId = GetString(_configs.GetArrayElementAtIndex(fallback), "TypeId");
            return fallback;
        }

        private UnitClassConfig BuildCombatConfig(SerializedProperty unit)
        {
            return new UnitClassConfig
            {
                TypeId = GetString(unit, "TypeId"),
                Role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1),
                CombatType = (UnitCombatType)Mathf.Clamp(GetEnumIndex(unit, "CombatType"), 0, Enum.GetValues(typeof(UnitCombatType)).Length - 1),
                BaseStamina = GetFloat(unit, "BaseStamina"),
                VisionRange = Mathf.Max(1, GetInt(unit, "VisionRange")),
                HitPoints = Mathf.Max(1, GetInt(unit, "HitPoints")),
                BaseLevel = Mathf.Max(1, GetInt(unit, "BaseLevel")),
                CuttingDamage = Mathf.Max(0, GetInt(unit, "CuttingDamage")),
                PenetratingDamage = Mathf.Max(0, GetInt(unit, "PenetratingDamage")),
                CrushingDamage = Mathf.Max(0, GetInt(unit, "CrushingDamage")),
                CuttingDefense = Mathf.Max(0, GetInt(unit, "CuttingDefense")),
                PenetratingDefense = Mathf.Max(0, GetInt(unit, "PenetratingDefense")),
                CrushingDefense = Mathf.Max(0, GetInt(unit, "CrushingDefense")),
            };
        }

        private void ApplyAllCombatPresets()
        {
            if (_registryObject == null || _configs == null)
                return;

            _registryObject.Update();
            for (int i = 0; i < CombatTablePresets.Length; i++)
            {
                UnitCombatPreset preset = CombatTablePresets[i];
                int index = FindUnitIndexByPreset(preset);
                bool created = index < 0;
                if (created)
                {
                    index = _configs.arraySize;
                    _configs.InsertArrayElementAtIndex(index);
                }

                ApplyCombatPreset(_configs.GetArrayElementAtIndex(index), preset, applyIdentity: true, clearPrefab: created);
            }

            _registryObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
            SaveSelectedPreference();
        }

        private void ApplyCombatPreset(SerializedProperty unit, UnitCombatPreset preset, bool applyIdentity, bool clearPrefab)
        {
            if (unit == null)
                return;

            if (applyIdentity)
                unit.FindPropertyRelative("TypeId").stringValue = preset.TypeId;

            unit.FindPropertyRelative("Role").enumValueIndex = (int)UnitRole.Military;
            unit.FindPropertyRelative("CombatType").enumValueIndex = (int)preset.CombatType;
            unit.FindPropertyRelative("VisionRange").intValue = preset.BaseViewDistance;
            unit.FindPropertyRelative("HitPoints").intValue = preset.HitPoints;
            unit.FindPropertyRelative("BaseLevel").intValue = preset.BaseLevel;
            unit.FindPropertyRelative("CuttingDamage").intValue = preset.CuttingDamage;
            unit.FindPropertyRelative("PenetratingDamage").intValue = preset.PenetratingDamage;
            unit.FindPropertyRelative("CrushingDamage").intValue = preset.CrushingDamage;
            unit.FindPropertyRelative("CuttingDefense").intValue = preset.CuttingDefense;
            unit.FindPropertyRelative("PenetratingDefense").intValue = preset.PenetratingDefense;
            unit.FindPropertyRelative("CrushingDefense").intValue = preset.CrushingDefense;

            var stamina = unit.FindPropertyRelative("BaseStamina");
            if (stamina != null && stamina.floatValue <= 0f)
                stamina.floatValue = 100f;

            if (clearPrefab)
                unit.FindPropertyRelative("Prefab").objectReferenceValue = null;

            _registryObject?.ApplyModifiedProperties();
            if (_registry != null)
                EditorUtility.SetDirty(_registry);
        }

        private int FindPresetIndexForUnit(SerializedProperty unit)
        {
            string typeId = GetString(unit, "TypeId");
            for (int i = 0; i < CombatTablePresets.Length; i++)
            {
                if (PresetMatchesUnit(CombatTablePresets[i], typeId))
                    return i;
            }

            return -1;
        }

        private int FindUnitIndexByPreset(UnitCombatPreset preset)
        {
            if (_configs == null)
                return -1;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (PresetMatchesUnit(preset, GetString(_configs.GetArrayElementAtIndex(i), "TypeId")))
                    return i;
            }

            return -1;
        }

        private static bool PresetMatchesUnit(UnitCombatPreset preset, string typeId)
        {
            string normalized = ToKebabId(typeId);
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            return normalized == preset.TypeId
                || normalized == ToKebabId(preset.UnitName)
                || (preset.TypeId == "swordman" && normalized == "swordsman");
        }

        private static void SetCombatDefaults(SerializedProperty unit)
        {
            unit.FindPropertyRelative("CombatType")?.SetEnum((int)UnitCombatType.Infantry);
            unit.FindPropertyRelative("HitPoints")?.SetInt(100);
            unit.FindPropertyRelative("BaseLevel")?.SetInt(1);
            unit.FindPropertyRelative("CuttingDamage")?.SetInt(0);
            unit.FindPropertyRelative("PenetratingDamage")?.SetInt(0);
            unit.FindPropertyRelative("CrushingDamage")?.SetInt(0);
            unit.FindPropertyRelative("CuttingDefense")?.SetInt(0);
            unit.FindPropertyRelative("PenetratingDefense")?.SetInt(0);
            unit.FindPropertyRelative("CrushingDefense")?.SetInt(0);
        }

        private static void CopyCombatSerializedValues(SerializedProperty source, SerializedProperty destination)
        {
            CopyRelativeEnum(source, destination, "CombatType");
            CopyRelativeInt(source, destination, "HitPoints");
            CopyRelativeInt(source, destination, "BaseLevel");
            CopyRelativeInt(source, destination, "CuttingDamage");
            CopyRelativeInt(source, destination, "PenetratingDamage");
            CopyRelativeInt(source, destination, "CrushingDamage");
            CopyRelativeInt(source, destination, "CuttingDefense");
            CopyRelativeInt(source, destination, "PenetratingDefense");
            CopyRelativeInt(source, destination, "CrushingDefense");
        }

        private static string ValidateCombatFields(SerializedProperty unit)
        {
            if (GetInt(unit, "HitPoints") < 1)
                return "HP має бути не менше 1.";

            if (GetInt(unit, "BaseLevel") < 1)
                return "Base Level має бути не менше 1.";

            UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);
            if (role == UnitRole.Military && GetCombatDamageTotal(unit) <= 0)
                return "Бойовий юніт не має жодної шкоди.";

            return null;
        }

        private static int GetCombatDamageTotal(SerializedProperty unit)
        {
            return Mathf.Max(0, GetInt(unit, "CuttingDamage"))
                + Mathf.Max(0, GetInt(unit, "PenetratingDamage"))
                + Mathf.Max(0, GetInt(unit, "CrushingDamage"));
        }

        private static void CopyRelativeInt(SerializedProperty source, SerializedProperty destination, string propertyName)
        {
            var sourceProperty = source.FindPropertyRelative(propertyName);
            var destinationProperty = destination.FindPropertyRelative(propertyName);
            if (sourceProperty != null && destinationProperty != null)
                destinationProperty.intValue = sourceProperty.intValue;
        }

        private static void CopyRelativeEnum(SerializedProperty source, SerializedProperty destination, string propertyName)
        {
            var sourceProperty = source.FindPropertyRelative(propertyName);
            var destinationProperty = destination.FindPropertyRelative(propertyName);
            if (sourceProperty != null && destinationProperty != null)
                destinationProperty.enumValueIndex = sourceProperty.enumValueIndex;
        }

        private static string ResolveOutcomeText(UnitCombatDuel duel)
        {
            return duel.Outcome switch
            {
                UnitCombatOutcome.AttackerAdvantage => "Перевага атакера",
                UnitCombatOutcome.DefenderAdvantage => "Перевага захисника",
                _ => "Рівний обмін",
            };
        }

        private static string DamageTypeLabel(UnitDamageType type)
        {
            return UnitCombatCalculator.GetDamageTypeLabel(type);
        }

        private static string FormatPresetDamageTriplet(UnitCombatPreset preset)
        {
            return $"Колюча {preset.PenetratingDamage} / Ріжуча {preset.CuttingDamage} / Дробляча {preset.CrushingDamage}";
        }

        private static string FormatPresetDefenseTriplet(UnitCombatPreset preset)
        {
            return $"Колючий {preset.PenetratingDefense} / Ріжучий {preset.CuttingDefense} / Дроблячий {preset.CrushingDefense}";
        }

        private static string FormatHits(int hits)
        {
            return hits == int.MaxValue ? "∞" : hits.ToString(CultureInfo.InvariantCulture);
        }

        private static string ShortUnitName(string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId))
                return "?";

            return typeId.Length <= 10 ? typeId : typeId.Substring(0, 10);
        }

        private static GUIStyle MatrixHeaderStyle()
        {
            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
            };
        }

        private static GUIStyle MatrixCellStyle()
        {
            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private readonly struct UnitCombatPreset
        {
            public UnitCombatPreset(
                string unitName,
                string typeId,
                UnitCombatType combatType,
                int hitPoints,
                int baseViewDistance,
                int baseLevel,
                int cuttingDamage,
                int penetratingDamage,
                int crushingDamage,
                int cuttingDefense,
                int penetratingDefense,
                int crushingDefense)
            {
                UnitName = unitName;
                TypeId = typeId;
                CombatType = combatType;
                HitPoints = hitPoints;
                BaseViewDistance = baseViewDistance;
                BaseLevel = baseLevel;
                CuttingDamage = cuttingDamage;
                PenetratingDamage = penetratingDamage;
                CrushingDamage = crushingDamage;
                CuttingDefense = cuttingDefense;
                PenetratingDefense = penetratingDefense;
                CrushingDefense = crushingDefense;
            }

            public string UnitName { get; }
            public string TypeId { get; }
            public UnitCombatType CombatType { get; }
            public int HitPoints { get; }
            public int BaseViewDistance { get; }
            public int BaseLevel { get; }
            public int CuttingDamage { get; }
            public int PenetratingDamage { get; }
            public int CrushingDamage { get; }
            public int CuttingDefense { get; }
            public int PenetratingDefense { get; }
            public int CrushingDefense { get; }
        }
    }

    internal static class SerializedPropertyCombatExtensions
    {
        public static void SetInt(this SerializedProperty property, int value)
        {
            if (property != null)
                property.intValue = value;
        }

        public static void SetEnum(this SerializedProperty property, int value)
        {
            if (property != null)
                property.enumValueIndex = value;
        }
    }
}