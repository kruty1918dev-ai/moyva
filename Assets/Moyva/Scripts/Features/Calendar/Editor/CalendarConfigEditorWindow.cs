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

        // Structure
        private int _monthsInYear      = 12;
        private int _daysInMonth       = 30;
        private int _hoursInDay        = 24;

        // Start date
        private int _startYear         = 1;
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
        private Vector2 _scroll;
        private string _validationMessage;
        private MessageType _validationMessageType;

        [MenuItem("Moyva/Calendar/Config Hub")]
        public static void Open()
        {
            var window = GetWindow<CalendarConfigEditorWindow>("Calendar Config Hub");
            window.minSize = new Vector2(480, 580);
            window.LoadFromFile();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Calendar Configuration Hub", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure the in-game calendar structure, day/night boundaries, and multiplayer turn settings.",
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
            EditorGUILayout.LabelField("Calendar Structure", EditorStyles.boldLabel);
            _monthsInYear = EditorGUILayout.IntSlider("Months per Year",  _monthsInYear, 1, 24);
            _daysInMonth  = EditorGUILayout.IntSlider("Days per Month",   _daysInMonth,  1, 60);
            _hoursInDay   = EditorGUILayout.IntSlider("Hours per Day",    _hoursInDay,   1, 48);
        }

        private void DrawStartDateSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Start Date / Time", EditorStyles.boldLabel);
            _startYear  = EditorGUILayout.IntField("Start Year",  _startYear);
            _startMonth = EditorGUILayout.IntSlider("Start Month", _startMonth, 1, _monthsInYear);
            _startDay   = EditorGUILayout.IntSlider("Start Day",   _startDay,   1, _daysInMonth);
            _startHour  = EditorGUILayout.IntSlider("Start Hour",  _startHour,  0, _hoursInDay - 1);
        }

        private void DrawDayNightSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Day / Night Boundaries", EditorStyles.boldLabel);
            _dayStartHour      = EditorGUILayout.IntSlider("Day Start Hour",       _dayStartHour,      0, _hoursInDay - 1);
            _nightStartHour    = EditorGUILayout.IntSlider("Night Start Hour",     _nightStartHour,    0, _hoursInDay - 1);
            _dawnDurationHours = EditorGUILayout.IntSlider("Dawn Duration (hours)", _dawnDurationHours, 0, 6);
            _duskDurationHours = EditorGUILayout.IntSlider("Dusk Duration (hours)", _duskDurationHours, 0, 6);
        }

        private void DrawMultiplayerSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Multiplayer / Turns", EditorStyles.boldLabel);
            _hoursPerTurn = EditorGUILayout.IntSlider("Hours per Turn", _hoursPerTurn, 1, 24);

            int turnsPerDay = (_hoursInDay > 0 && _hoursPerTurn > 0) ? _hoursInDay / _hoursPerTurn : 0;
            EditorGUILayout.LabelField($"  → {turnsPerDay} turn(s) per in-game day", EditorStyles.helpBox);
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Day Phase Preview", EditorStyles.boldLabel);
            _previewHour = EditorGUILayout.IntSlider("Preview Hour", _previewHour, 0, _hoursInDay - 1);

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
            EditorGUILayout.LabelField($"  Phase at hour {_previewHour:D2}:00  →  {phase}", EditorStyles.helpBox);
            GUI.backgroundColor = prev;
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space(8);
            _validationMessage     = null;
            _validationMessageType = MessageType.None;

            if (_dayStartHour >= _nightStartHour)
            {
                _validationMessage     = "Day Start Hour must be less than Night Start Hour.";
                _validationMessageType = MessageType.Error;
            }
            else if (_dawnDurationHours >= _dayStartHour)
            {
                _validationMessage     = "Dawn Duration is longer than the time before Day Start Hour.";
                _validationMessageType = MessageType.Warning;
            }
            else if (_nightStartHour + _duskDurationHours > _hoursInDay)
            {
                _validationMessage     = "Dusk extends beyond midnight — consider reducing Dusk Duration.";
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
                if (GUILayout.Button("Save Config", GUILayout.Height(30)))
                    SaveToFile();
            }

            if (GUILayout.Button("Load Config", GUILayout.Height(30)))
                LoadFromFile();

            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
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
            Debug.Log($"[Calendar] Config saved to {ConfigPath}");
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
