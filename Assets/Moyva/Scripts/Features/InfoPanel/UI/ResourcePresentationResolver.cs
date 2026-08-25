using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    /// <summary>
    /// Player-facing resource presentation resolver.
    /// Resource IDs stay internal; display name, icon and category are resolved
    /// from EconomyResourceDefinition. No compile-time dependency on Jsonization
    /// is required: the JSON runtime fallback is invoked through reflection.
    /// </summary>
    internal static class ResourcePresentationResolver
    {
        private const string JsonRuntimeTypeName =
            "Kruty1918.Moyva.Jsonization.MoyvaJsonRuntime";

        private static EconomyDatabaseSO _cachedDatabase;
        private static bool _loggedDatabaseResolution;
        private static bool _loggedFallbackFailure;
        private static readonly HashSet<string> MissingResourceIds =
            new HashSet<string>(StringComparer.Ordinal);

        public static bool TryResolve(
            string resourceId,
            EconomyDatabaseSO injectedDatabase,
            out string displayName,
            out Sprite icon,
            out EconomyResourceCategory category)
        {
            displayName = "Невідомий ресурс";
            icon = null;
            category = EconomyResourceCategory.None;

            if (string.IsNullOrWhiteSpace(resourceId))
                return false;

            string normalizedId = resourceId.Trim();
            EconomyDatabaseSO database = ResolveDatabase(injectedDatabase);
            IReadOnlyList<EconomyResourceDefinition> resources = database?.Resources;

            if (resources == null || resources.Count == 0)
            {
                WarnMissingOnce(
                    normalizedId,
                    "EconomyDatabaseSO unavailable or contains no resources");
                return false;
            }

            for (int i = 0; i < resources.Count; i++)
            {
                EconomyResourceDefinition resource = resources[i];
                if (resource == null ||
                    !string.Equals(
                        resource.Id,
                        normalizedId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                displayName = !string.IsNullOrWhiteSpace(resource.DisplayName)
                    ? resource.DisplayName.Trim()
                    : "Невідомий ресурс";
                icon = resource.Icon;
                category = resource.Category;
                return true;
            }

            WarnMissingOnce(
                normalizedId,
                $"definition not found in database '{database?.name ?? "<unnamed>"}'");
            return false;
        }

        public static EconomyDatabaseSO ResolveDatabase(
            EconomyDatabaseSO injectedDatabase)
        {
            if (HasResources(injectedDatabase))
            {
                Cache(injectedDatabase, "injected");
                return injectedDatabase;
            }

            if (HasResources(_cachedDatabase))
                return _cachedDatabase;

            EconomyDatabaseSO runtimeDatabase = ResolveFromJsonRuntime();
            if (HasResources(runtimeDatabase))
            {
                Cache(runtimeDatabase, "json-runtime-reflection");
                return runtimeDatabase;
            }

            EconomyDatabaseSO sceneDatabase = ResolveFromSceneComponents();
            if (HasResources(sceneDatabase))
            {
                Cache(sceneDatabase, "scene-component-field-fallback");
                return sceneDatabase;
            }

            return null;
        }

        private static EconomyDatabaseSO ResolveFromJsonRuntime()
        {
            try
            {
                Type runtimeType = null;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length && runtimeType == null; i++)
                    runtimeType = assemblies[i].GetType(JsonRuntimeTypeName, false);

                if (runtimeType == null)
                    return null;

                MethodInfo getAllGeneric = null;
                MethodInfo[] methods = runtimeType.GetMethods(
                    BindingFlags.Public | BindingFlags.Static);

                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (method.Name == "GetAll" &&
                        method.IsGenericMethodDefinition &&
                        method.GetGenericArguments().Length == 1 &&
                        method.GetParameters().Length == 0)
                    {
                        getAllGeneric = method;
                        break;
                    }
                }

                if (getAllGeneric == null)
                    return null;

                object raw = getAllGeneric
                    .MakeGenericMethod(typeof(EconomyDatabaseSO))
                    .Invoke(null, null);

                if (!(raw is IEnumerable enumerable))
                    return null;

                EconomyDatabaseSO best = null;
                int bestCount = -1;

                foreach (object entry in enumerable)
                {
                    EconomyDatabaseSO candidate = entry as EconomyDatabaseSO;
                    if (!HasResources(candidate))
                        continue;

                    int count = candidate.Resources.Count;
                    if (count <= bestCount)
                        continue;

                    best = candidate;
                    bestCount = count;
                }

                return best;
            }
            catch (Exception ex)
            {
                LogFallbackFailureOnce(
                    "JSON runtime reflection failed",
                    ex);
                return null;
            }
        }

        private static EconomyDatabaseSO ResolveFromSceneComponents()
        {
            try
            {
                MonoBehaviour[] behaviours =
                    Resources.FindObjectsOfTypeAll<MonoBehaviour>();

                EconomyDatabaseSO best = null;
                int bestCount = -1;

                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour == null ||
                        behaviour.gameObject == null ||
                        !behaviour.gameObject.scene.IsValid())
                    {
                        continue;
                    }

                    FieldInfo[] fields = behaviour.GetType().GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                    for (int f = 0; f < fields.Length; f++)
                    {
                        FieldInfo field = fields[f];
                        if (!typeof(EconomyDatabaseSO)
                                .IsAssignableFrom(field.FieldType))
                        {
                            continue;
                        }

                        EconomyDatabaseSO candidate = null;
                        try
                        {
                            candidate =
                                field.GetValue(behaviour) as EconomyDatabaseSO;
                        }
                        catch
                        {
                            // Ignore inaccessible/broken component fields.
                        }

                        if (!HasResources(candidate))
                            continue;

                        int count = candidate.Resources.Count;
                        if (count <= bestCount)
                            continue;

                        best = candidate;
                        bestCount = count;
                    }
                }

                return best;
            }
            catch (Exception ex)
            {
                LogFallbackFailureOnce(
                    "scene component fallback failed",
                    ex);
                return null;
            }
        }

        private static bool HasResources(EconomyDatabaseSO database)
            => database != null &&
               database.Resources != null &&
               database.Resources.Count > 0;

        private static void Cache(
            EconomyDatabaseSO database,
            string source)
        {
            _cachedDatabase = database;

            if (_loggedDatabaseResolution || database == null)
                return;

            _loggedDatabaseResolution = true;
        }

        private static void LogFallbackFailureOnce(
            string reason,
            Exception ex)
        {
            if (_loggedFallbackFailure)
                return;

            _loggedFallbackFailure = true;
        }

        private static void WarnMissingOnce(
            string resourceId,
            string reason)
        {
            if (!MissingResourceIds.Add(resourceId))
                return;
        }
    }
}
