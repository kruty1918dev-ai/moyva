using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Editor.Shared
{
    /// <summary>
    /// Відсіює відоме нешкідливе повідомлення Odin 4.0.2 про відсутнє поле
    /// UnityEngine.EntityId.MagicVersion у новіших версіях Unity.
    /// Усунено в Odin 4.0.2.2 — після оновлення плагіна цей фільтр можна видалити.
    /// </summary>
    [InitializeOnLoad]
    internal static class OdinEntityIdWarningFilter
    {
        private const string SuppressedFragment = "EntityId.MagicVersion";

        static OdinEntityIdWarningFilter()
        {
            // Встановлює фільтр Debug.unityLogger до ініціалізації Odin-залежних watcher-ів.
            if (Debug.unityLogger.logHandler is FilteringLogHandler)
                return;

            Debug.unityLogger.logHandler = new FilteringLogHandler(Debug.unityLogger.logHandler);
        }

        private sealed class FilteringLogHandler : ILogHandler
        {
            private readonly ILogHandler _inner;

            public FilteringLogHandler(ILogHandler inner)
            {
                _inner = inner;
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                string message = format;
                if (args != null && args.Length > 0)
                {
                    try { message = string.Format(format, args); }
                    catch (FormatException) { message = format; }
                }

                if (message != null && message.IndexOf(SuppressedFragment, StringComparison.Ordinal) >= 0)
                    return;

                _inner.LogFormat(logType, context, format, args);
            }

            public void LogException(Exception exception, Object context)
            {
                _inner.LogException(exception, context);
            }
        }
    }
}
