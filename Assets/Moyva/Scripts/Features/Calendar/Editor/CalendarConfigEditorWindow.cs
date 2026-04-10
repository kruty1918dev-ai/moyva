using System.IO;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Domain;
using Kruty1918.Moyva.Calendar.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Calendar.Editor
{
    /// <summary>
    /// Unity EditorWindow for configuring CalendarConfig.
    /// Menu: Moyva → Calendar → Config Hub
    /// </summary>
    public sealed class CalendarConfigEditorWindow : EditorWindow
    {
        private const string ConfigPath = "Assets/Moyva/calendar_config.dat";

        private static readonly GUIContent MonthsInYearContent = new(
            "Кількість місяців у році",
            "Скільки місяців має ігровий рік. Впливає на загальну тривалість року та на розрахунок дат.");
        private static readonly GUIContent DaysInMonthContent = new(
            "Кількість днів у місяці",
            "Скільки днів має кожен ігровий місяць. Разом із місяцями формує довжину року.");
        private static readonly GUIContent HoursInDayContent = new(
            "Кількість годин у добі",
            "Тривалість однієї ігрової доби. Усі межі дня/ночі задаються в межах цього діапазону.");

        private static readonly GUIContent StartYearContent = new(
            "Початковий рік",
            "Рік, з якого починається нова сесія гри.");
        private static readonly GUIContent StartMonthContent = new(
            "Початковий місяць",
            "Місяць старту сесії (від 1 до кількості місяців у році).");
        private static readonly GUIContent StartDayContent = new(
            "Початковий день",
            "День старту сесії (від 1 до кількості днів у місяці).");
        private static readonly GUIContent StartHourContent = new(
            "Початкова година",
            "Година, з якої починається гра в першій добі (від 0 до кінець доби).");

        private static readonly GUIContent DayStartHourContent = new(
            "Початок дня",
            "Година, коли починається фаза День. До цього може бути Світанок.");
        private static readonly GUIContent NightStartHourContent = new(
            "Початок ночі",
            "Година, коли починається фаза Ніч. Перед нею може бути Сутінки.");
        private static readonly GUIContent DawnDurationContent = new(
            "Тривалість світанку (год)",
            "Скільки годин триває плавний перехід Ніч → День.");
        private static readonly GUIContent DuskDurationContent = new(
            "Тривалість сутінок (год)",
            "Скільки годин триває плавний перехід День → Ніч.");

        private static readonly GUIContent HoursPerTurnContent = new(
            "Годин за 1 хід",
            "Скільки ігрових годин минає після одного ходу/кроку.");
        private static readonly GUIContent PreviewHourContent = new(
            "Година для прев'ю",
            "Оберіть годину, щоб побачити, яка фаза доби буде активною в цей момент.");
        private static readonly GUIContent PreviewTurnsContent = new(
            "Прев'ю через N ходів",
            "Показує дату/час і фазу доби після заданої кількості ходів від старту.");

        // Structure
        private int _monthsInYear      = 12;
        private int _daysInMonth       = 30;
        private int _hoursInDay        = 24;

        // Start date
        private int _startYear         = CalendarConfig.PeakUkraineYear;
        private int _startMonth        = 1;
        private int _startDay          = 1;
        private int _startHour         = 6;

        // Day/night boundaries
        private int _dayStartHour      = 6;
        private int _nightStartHour    = 20;
        private int _dawnDurationHours = 1;
        private int _duskDurationHours = 1;

        // Multiplayer
        private int _hoursPerTurn      = 1;

        private int _schemaVersion = CalendarConfig.CurrentSchemaVersion;

        // Preview
        private int _previewHour       = 12;
        private int _previewTurnsFromStart = 0;
        private Vector2 _scroll;
        private string _validationMessage;
        private MessageType _validationMessageType;

        [MenuItem("Moyva/Calendar/Config Hub")]
        public static void Open()
        {
            var window = GetWindow<CalendarConfigEditorWindow>("Календар: налаштування");
            window.minSize = new Vector2(480, 580);
            window.LoadFromFile();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Календар: детальні налаштування", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Тут ви керуєте структурою календаря, межами дня/ночі та швидкістю плину часу за хід. " +
                "Наведіть курсор на назву будь-якого параметра, щоб прочитати детальне пояснення.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawStructureSection();
            DrawStartDateSection();
            DrawDayNightSection();
            DrawMultiplayerSection();
            DrawPreviewSection();
            DrawValidation();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawStructureSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("1) Структура календаря", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Цей блок задає базову математику календаря: скільки місяців у році, днів у місяці та годин у добі.",
                MessageType.None);
            _monthsInYear = EditorGUILayout.IntSlider(MonthsInYearContent, _monthsInYear, 1, 24);
            _daysInMonth  = EditorGUILayout.IntSlider(DaysInMonthContent, _daysInMonth, 1, 60);
            _hoursInDay   = EditorGUILayout.IntSlider(HoursInDayContent, _hoursInDay, 1, 48);

            int hoursInMonth = _daysInMonth * _hoursInDay;
            int hoursInYear = _monthsInYear * hoursInMonth;
            EditorGUILayout.LabelField($"Місяць: {hoursInMonth} ігрових год", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Рік: {hoursInYear} ігрових год", EditorStyles.helpBox);
        }

        private void DrawStartDateSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("2) Стартова дата і час", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Це точка, з якої починається нова сесія. Якщо хочете старт з ранку, ставте годину ближче до початку дня.",
                MessageType.None);
            _startYear = EditorGUILayout.IntField(StartYearContent, _startYear);
            _startMonth = EditorGUILayout.IntSlider(StartMonthContent, _startMonth, 1, _monthsInYear);
            _startDay = EditorGUILayout.IntSlider(StartDayContent, _startDay, 1, _daysInMonth);
            _startHour = EditorGUILayout.IntSlider(StartHourContent, _startHour, 0, _hoursInDay - 1);

            EditorGUILayout.LabelField($"Старт: {_startYear:D4}-{_startMonth:D2}-{_startDay:D2} {_startHour:D2}:00", EditorStyles.helpBox);
        }

        private void DrawDayNightSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("3) Цикл дня і ночі", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Налаштуйте, коли починається день/ніч та скільки годин тривають плавні переходи (світанок і сутінки).",
                MessageType.None);
            _dayStartHour = EditorGUILayout.IntSlider(DayStartHourContent, _dayStartHour, 0, _hoursInDay - 1);
            _nightStartHour = EditorGUILayout.IntSlider(NightStartHourContent, _nightStartHour, 0, _hoursInDay - 1);
            _dawnDurationHours = EditorGUILayout.IntSlider(DawnDurationContent, _dawnDurationHours, 0, 6);
            _duskDurationHours = EditorGUILayout.IntSlider(DuskDurationContent, _duskDurationHours, 0, 6);

            int dawnStart = _dayStartHour - _dawnDurationHours;
            int duskStart = _nightStartHour - _duskDurationHours;
            int dayDuration = Mathf.Max(0, duskStart - _dayStartHour);
            int nightDuration = Mathf.Max(0, _hoursInDay - _nightStartHour + dawnStart);

            EditorGUILayout.LabelField($"Світанок: {Mathf.Max(0, dawnStart):D2}:00 → {_dayStartHour:D2}:00", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"День: {_dayStartHour:D2}:00 → {Mathf.Max(_dayStartHour, duskStart):D2}:00 ({dayDuration} год)", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Сутінки: {Mathf.Max(_dayStartHour, duskStart):D2}:00 → {_nightStartHour:D2}:00", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Ніч: {_nightStartHour:D2}:00 → {Mathf.Max(0, dawnStart):D2}:00 ({nightDuration} год, через північ)", EditorStyles.helpBox);
        }

        private void DrawMultiplayerSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("4) Швидкість часу по ходах", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Керує тим, як швидко минає ігровий час. Більше значення = швидше наближення вечора/ночі за кожен хід.",
                MessageType.None);
            _hoursPerTurn = EditorGUILayout.IntSlider(HoursPerTurnContent, _hoursPerTurn, 1, 24);

            int turnsPerDay = (_hoursInDay > 0 && _hoursPerTurn > 0) ? _hoursInDay / _hoursPerTurn : 0;
            EditorGUILayout.LabelField($"Ходів за одну ігрову добу: {turnsPerDay}", EditorStyles.helpBox);
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("5) Прев'ю фаз і таймлайн", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Цей блок допомагає одразу побачити, яка фаза активна в певну годину і як цикл виглядає по всій добі.",
                MessageType.None);
            _previewHour = EditorGUILayout.IntSlider(PreviewHourContent, _previewHour, 0, _hoursInDay - 1);

            var cfg = BuildConfig();
            DayPhase phase = GameCalendarService.ComputeDayPhase(cfg, _previewHour);

            Color boxColor = phase switch
            {
                DayPhase.Day   => new Color(1f, 0.95f, 0.6f),
                DayPhase.Night => new Color(0.15f, 0.15f, 0.3f),
                DayPhase.Dawn  => new Color(1f, 0.7f, 0.4f),
                DayPhase.Dusk  => new Color(0.6f, 0.3f, 0.5f),
                _              => Color.grey
            };

            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = boxColor;
            EditorGUILayout.LabelField($"Фаза о {_previewHour:D2}:00: {PhaseToUkrainian(phase)}", EditorStyles.helpBox);
            GUI.backgroundColor = prev;

            DrawPhaseTimeline(cfg);

            int maxTurns = Mathf.Max(1, _hoursInDay * 4);
            _previewTurnsFromStart = EditorGUILayout.IntSlider(PreviewTurnsContent, _previewTurnsFromStart, 0, maxTurns);

            long previewHours = (long)_previewTurnsFromStart * _hoursPerTurn;
            var previewDateTime = GameCalendarService.ComputeDateTime(cfg, previewHours);
            DayPhase previewPhase = GameCalendarService.ComputeDayPhase(cfg, previewDateTime.Hour);
            EditorGUILayout.LabelField(
                $"Після {_previewTurnsFromStart} ходів: {previewDateTime.Year:D4}-{previewDateTime.Month:D2}-{previewDateTime.Day:D2} {previewDateTime.Hour:D2}:00, фаза: {PhaseToUkrainian(previewPhase)}",
                EditorStyles.helpBox);
        }

        private void DrawPhaseTimeline(CalendarConfig cfg)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Шкала доби (по годинах)", EditorStyles.miniBoldLabel);

            Rect timelineRect = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));
            int hours = Mathf.Max(1, cfg.HoursInDay);
            float cellWidth = timelineRect.width / hours;

            for (int hour = 0; hour < hours; hour++)
            {
                DayPhase p = GameCalendarService.ComputeDayPhase(cfg, hour);
                Color c = GetPhaseColor(p);
                Rect r = new Rect(timelineRect.x + hour * cellWidth, timelineRect.y, cellWidth - 1f, timelineRect.height);
                EditorGUI.DrawRect(r, c);
            }

            Rect markRect = new Rect(
                timelineRect.x + Mathf.Clamp(_previewHour, 0, hours - 1) * cellWidth,
                timelineRect.y,
                Mathf.Max(2f, cellWidth * 0.25f),
                timelineRect.height);
            EditorGUI.DrawRect(markRect, Color.white);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("00:00", GUILayout.Width(50));
            EditorGUILayout.LabelField("...", GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField($"{hours - 1:D2}:00", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Легенда: День | Ніч | Світанок | Сутінки", EditorStyles.miniLabel);
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space(8);
            _validationMessage     = null;
            _validationMessageType = MessageType.None;

            if (_dayStartHour >= _nightStartHour)
            {
                _validationMessage = "Помилка: «Початок дня» має бути раніше за «Початок ночі».";
                _validationMessageType = MessageType.Error;
            }
            else if (_dawnDurationHours >= _dayStartHour)
            {
                _validationMessage = "Попередження: світанок занадто довгий і починається раніше початку доби.";
                _validationMessageType = MessageType.Warning;
            }
            else if (_nightStartHour + _duskDurationHours > _hoursInDay)
            {
                _validationMessage = "Попередження: сутінки виходять за межі доби. Рекомендовано зменшити їх тривалість.";
                _validationMessageType = MessageType.Warning;
            }

            if (_validationMessage != null)
                EditorGUILayout.HelpBox(_validationMessage, _validationMessageType);
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            bool hasErrors = _validationMessageType == MessageType.Error;
            using (new EditorGUI.DisabledScope(hasErrors))
            {
                if (GUILayout.Button("Зберегти конфіг", GUILayout.Height(30)))
                    SaveToFile();
            }

            if (GUILayout.Button("Завантажити конфіг", GUILayout.Height(30)))
                LoadFromFile();

            if (GUILayout.Button("Скинути до типових", GUILayout.Height(30)))
                ApplyConfig(CalendarConfig.Default());

            EditorGUILayout.EndHorizontal();
        }

        private void LoadFromFile()
        {
            var store = new CalendarBinaryConfigStore(ConfigPath);
            ApplyConfig(store.Load());
            Repaint();
        }

        private void SaveToFile()
        {
            string dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var store = new CalendarBinaryConfigStore(ConfigPath);
            store.Save(BuildConfig());
            AssetDatabase.Refresh();
            Debug.Log($"[Calendar] Конфігурацію збережено у {ConfigPath}");
        }

        private static Color GetPhaseColor(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Day => new Color(1f, 0.92f, 0.45f),
                DayPhase.Night => new Color(0.20f, 0.22f, 0.45f),
                DayPhase.Dawn => new Color(1f, 0.64f, 0.40f),
                DayPhase.Dusk => new Color(0.66f, 0.40f, 0.62f),
                _ => Color.gray
            };
        }

        private static string PhaseToUkrainian(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Day => "День",
                DayPhase.Night => "Ніч",
                DayPhase.Dawn => "Світанок",
                DayPhase.Dusk => "Сутінки",
                _ => "Невідомо"
            };
        }

        private void ApplyConfig(CalendarConfig c)
        {
            _schemaVersion      = c.SchemaVersion;
            _startYear          = c.StartYear;
            _startMonth         = c.StartMonth;
            _startDay           = c.StartDay;
            _startHour          = c.StartHour;
            _monthsInYear       = c.MonthsInYear;
            _daysInMonth        = c.DaysInMonth;
            _hoursInDay         = c.HoursInDay;
            _dayStartHour       = c.DayStartHour;
            _nightStartHour     = c.NightStartHour;
            _dawnDurationHours  = c.DawnDurationHours;
            _duskDurationHours  = c.DuskDurationHours;
            _hoursPerTurn       = c.HoursPerTurn;
        }

        private CalendarConfig BuildConfig() =>
            new CalendarConfig(
                _schemaVersion, _startYear, _startMonth, _startDay, _startHour,
                _monthsInYear, _daysInMonth, _hoursInDay,
                _dayStartHour, _nightStartHour, _dawnDurationHours, _duskDurationHours,
                _hoursPerTurn);
    }
}
