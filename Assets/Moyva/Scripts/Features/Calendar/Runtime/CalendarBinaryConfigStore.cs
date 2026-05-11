using System;
using System.IO;
using Kruty1918.Moyva.Calendar.Config;
using UnityEngine;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Stores and retrieves <see cref="CalendarConfig"/> from a local binary file.
    /// Runtime-friendly; does not depend on UnityEditor.
    /// </summary>
    public sealed class CalendarBinaryConfigStore : ICalendarConfigStore
    {
        private readonly string _filePath;

        public CalendarBinaryConfigStore(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(Application.persistentDataPath, "calendar_config.dat");
        }

        public bool Exists() => File.Exists(_filePath);

        public CalendarConfig Load()
        {
            if (!Exists())
                return CalendarConfigLifecycle.ValidateAndFreeze(CalendarConfig.Default(),
                    message => Debug.LogWarning($"[Calendar] {message}"));

            try
            {
                using var fs = File.OpenRead(_filePath);
                using var br = new BinaryReader(fs);
                return CalendarConfigLifecycle.ValidateAndFreeze(ReadConfig(br),
                    message => Debug.LogWarning($"[Calendar] {message}"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Calendar] Failed to load config: {e.Message}. Using defaults.");
                return CalendarConfigLifecycle.ValidateAndFreeze(CalendarConfig.Default(),
                    message => Debug.LogWarning($"[Calendar] {message}"));
            }
        }

        public void Save(CalendarConfig config)
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var fs = File.Create(_filePath);
                using var bw = new BinaryWriter(fs);
                WriteConfig(bw, config);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Calendar] Failed to save config: {e.Message}");
            }
        }

        internal static void WriteConfig(BinaryWriter bw, CalendarConfig c)
        {
            bw.Write(c.SchemaVersion);
            bw.Write(c.StartYear);
            bw.Write(c.StartMonth);
            bw.Write(c.StartDay);
            bw.Write(c.StartHour);
            bw.Write(c.MonthsInYear);
            bw.Write(c.DaysInMonth);
            bw.Write(c.HoursInDay);
            bw.Write(c.DayStartHour);
            bw.Write(c.NightStartHour);
            bw.Write(c.DawnDurationHours);
            bw.Write(c.DuskDurationHours);
            bw.Write(c.HoursPerTurn);
        }

        internal static CalendarConfig ReadConfig(BinaryReader br)
        {
            int schemaVersion     = br.ReadInt32();
            int startYear         = br.ReadInt32();
            int startMonth        = br.ReadInt32();
            int startDay          = br.ReadInt32();
            int startHour         = br.ReadInt32();
            int monthsInYear      = br.ReadInt32();
            int daysInMonth       = br.ReadInt32();
            int hoursInDay        = br.ReadInt32();
            int dayStartHour      = br.ReadInt32();
            int nightStartHour    = br.ReadInt32();
            int dawnDurationHours = br.ReadInt32();
            int duskDurationHours = br.ReadInt32();
            int hoursPerTurn      = br.ReadInt32();

            return new CalendarConfig(
                schemaVersion, startYear, startMonth, startDay, startHour,
                monthsInYear, daysInMonth, hoursInDay,
                dayStartHour, nightStartHour, dawnDurationHours, duskDurationHours,
                hoursPerTurn);
        }
    }
}
