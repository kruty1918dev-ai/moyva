using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Shared.Diagnostics
{
    internal sealed class AsyncGlobalErrorHandlerService : IInitializable, IDisposable
    {
        public void Initialize()
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public void Dispose()
        {
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError($"[AsyncErrorHandler] UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.LogError($"[AsyncErrorHandler] UnhandledException: {e.ExceptionObject}");
        }
    }

}
