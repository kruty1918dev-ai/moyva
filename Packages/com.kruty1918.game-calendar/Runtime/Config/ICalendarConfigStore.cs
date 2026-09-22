using Kruty1918.Calendar.Config;

namespace Kruty1918.Calendar.Config
{
    /// <summary>
    /// Runtime abstraction for loading and saving CalendarConfig.
    /// Does NOT depend on UnityEditor.
    /// </summary>
    public interface ICalendarConfigStore
    {
        CalendarConfig Load();
        void Save(CalendarConfig config);
        bool Exists();
    }
}
