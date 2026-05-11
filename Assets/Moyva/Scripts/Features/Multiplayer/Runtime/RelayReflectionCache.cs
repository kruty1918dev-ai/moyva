using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Caches reflection metadata for Relay SDK access and validates it once during startup.
    /// </summary>
    internal static class RelayReflectionCache
    {
        private static readonly object Sync = new object();
        private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new ConcurrentDictionary<string, PropertyInfo>();

        private static bool _initialized;
        private static bool _valid;
        private static string _validationError;

        private static Type _relayServiceType;
        private static PropertyInfo _instanceProperty;
        private static MethodInfo[] _createAllocationMethods;
        private static MethodInfo[] _getJoinCodeMethods;
        private static MethodInfo[] _joinAllocationMethods;

        public static bool TryValidate(out string error)
        {
            EnsureInitialized();
            error = _validationError;
            return _valid;
        }

        public static object GetRelayServiceInstance()
        {
            EnsureInitialized();
            if (!_valid)
                throw new InvalidOperationException(_validationError ?? "Relay reflection metadata is invalid.");

            var instance = _instanceProperty.GetValue(null);
            if (instance == null)
                throw new InvalidOperationException("RelayService.Instance is null.");

            return instance;
        }

        public static MethodInfo ResolveRelayMethod(string methodName, object[] args)
        {
            EnsureInitialized();
            if (!_valid)
                throw new InvalidOperationException(_validationError ?? "Relay reflection metadata is invalid.");

            var candidates = methodName switch
            {
                "CreateAllocationAsync" => _createAllocationMethods,
                "GetJoinCodeAsync" => _getJoinCodeMethods,
                "JoinAllocationAsync" => _joinAllocationMethods,
                _ => Array.Empty<MethodInfo>()
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                var method = candidates[i];
                var parameters = method.GetParameters();
                if (parameters.Length != args.Length)
                    continue;

                var compatible = true;
                for (int p = 0; p < parameters.Length; p++)
                {
                    var arg = args[p];
                    if (arg == null)
                        continue;

                    var parameterType = parameters[p].ParameterType;
                    if (!parameterType.IsInstanceOfType(arg) && parameterType != arg.GetType())
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                    return method;
            }

            throw new MissingMethodException(_relayServiceType.FullName, methodName);
        }

        public static T ReadProperty<T>(object source, string propertyName)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var key = source.GetType().FullName + "::" + propertyName;
            var property = PropertyCache.GetOrAdd(key, _ =>
            {
                return source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            });

            if (property == null)
                throw new MissingMemberException(source.GetType().FullName, propertyName);

            var value = property.GetValue(source);
            if (value is T typed)
                return typed;

            if (value == null)
                throw new InvalidOperationException($"Property {propertyName} is null on {source.GetType().FullName}.");

            return (T)Convert.ChangeType(value, typeof(T));
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (Sync)
            {
                if (_initialized)
                    return;

                try
                {
                    _relayServiceType =
                        Type.GetType("Unity.Services.Relay.RelayService, Unity.Services.Relay")
                        ?? Type.GetType("Unity.Services.Relay.RelayService, Unity.Services.Multiplayer");

                    if (_relayServiceType == null)
                    {
                        Invalidate("RelayService type is not available in loaded assemblies.");
                        return;
                    }

                    _instanceProperty = _relayServiceType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (_instanceProperty == null)
                    {
                        Invalidate("RelayService.Instance property is missing.");
                        return;
                    }

                    _createAllocationMethods = ResolveTaskMethods(_relayServiceType, "CreateAllocationAsync");
                    _getJoinCodeMethods = ResolveTaskMethods(_relayServiceType, "GetJoinCodeAsync");
                    _joinAllocationMethods = ResolveTaskMethods(_relayServiceType, "JoinAllocationAsync");

                    if (_createAllocationMethods.Length == 0)
                    {
                        Invalidate("RelayService.CreateAllocationAsync method is missing.");
                        return;
                    }

                    if (_getJoinCodeMethods.Length == 0)
                    {
                        Invalidate("RelayService.GetJoinCodeAsync method is missing.");
                        return;
                    }

                    if (_joinAllocationMethods.Length == 0)
                    {
                        Invalidate("RelayService.JoinAllocationAsync method is missing.");
                        return;
                    }

                    _valid = true;
                    _validationError = string.Empty;
                }
                catch (Exception exception)
                {
                    Invalidate($"Relay reflection validation failed: {exception.Message}");
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        private static MethodInfo[] ResolveTaskMethods(Type type, string methodName)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var list = new System.Collections.Generic.List<MethodInfo>();
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
                    continue;

                if (!typeof(Task).IsAssignableFrom(method.ReturnType))
                    continue;

                list.Add(method);
            }

            return list.ToArray();
        }

        private static void Invalidate(string reason)
        {
            _valid = false;
            _validationError = reason;
        }
    }
}
