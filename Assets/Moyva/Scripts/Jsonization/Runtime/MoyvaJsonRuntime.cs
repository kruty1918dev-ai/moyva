using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Центральне runtime-сховище JSON-конфігурації Moyva.
    /// </summary>
    /// <remarks>
    /// Runtime-потік:
    /// <c>Load -> Validate -> Resolve -> Freeze -> Consume</c>.
    ///
    /// Згенеровані JSON-файли <see cref="TextAsset"/> завантажуються з Resources один раз,
    /// індексуються за типом/схемою конфігурації та ID і ліниво матеріалізуються у frozen
    /// runtime-об'єкти. Завдяки цьому gameplay-код виконує лише пошук у словниках замість
    /// повторного парсингу JSON.
    /// </remarks>
    public static class MoyvaJsonRuntime
    {
        private static readonly object Sync = new();
        private static bool _loaded;
        private static Dictionary<string, JObject> _rawByTypeAndId;
        private static Dictionary<string, JObject> _rawBySchemaAndId;
        private static Dictionary<string, object> _frozen;
        private static MoyvaJsonAssetCatalog _catalog;
        private static JsonSerializer _serializer;
        private static string _fingerprint = string.Empty;

        /// <summary>
        /// Повертає, чи було вже завантажено згенероване runtime-сховище JSON.
        /// </summary>
        public static bool IsLoaded => _loaded;

        /// <summary>
        /// Повертає детермінований SHA-256 fingerprint усіх завантажених канонічних JSON-документів.
        /// </summary>
        /// <remarks>
        /// Звернення до цієї властивості спочатку примусово завантажує сховище.
        /// Fingerprint можна використовувати для виявлення невідповідностей конфігурації між runtime-клієнтами.
        /// </remarks>
        public static string ConfigFingerprint
        {
            get
            {
                EnsureLoaded();
                return _fingerprint;
            }
        }

        /// <summary>
        /// Гарантує, що runtime JSON-сховище буде ініціалізоване лише один раз.
        /// </summary>
        /// <remarks>
        /// Використовує double-checked locking, оскільки цей метод може викликатися з багатьох
        /// шляхів доступу до конфігурації. Фактичне завантаження виконує <see cref="LoadAll"/>.
        /// </remarks>
        public static void EnsureLoaded()
        {
            if (_loaded)
                return;

            lock (Sync)
            {
                if (_loaded)
                    return;

                // Завантажуємо весь набір згенерованої конфігурації перед тим, як позначити сховище
                // як ініціалізоване. Якщо LoadAll викине exception, _loaded залишиться false.
                LoadAll();
                _loaded = true;
            }
        }

        /// <summary>
        /// Знаходить один конфігураційний об'єкт за runtime-типом і стабільним JSON ID.
        /// </summary>
        /// <typeparam name="T">Очікуваний runtime-тип конфігурації.</typeparam>
        /// <param name="id">Стабільний ID конфігурації.</param>
        /// <returns>Frozen-екземпляр конфігурації або <c>null</c>, якщо його неможливо знайти.</returns>
        public static T Get<T>(string id) where T : class
        {
            return Get(typeof(T), id) as T;
        }

        /// <summary>
        /// Знаходить конфігураційний об'єкт за runtime <see cref="Type"/> і стабільним ID.
        /// </summary>
        /// <param name="type">Очікуваний runtime-тип конфігурації.</param>
        /// <param name="id">Стабільний ID конфігурації.</param>
        /// <returns>Закешований/frozen об'єкт або <c>null</c>, якщо сумісного документа не існує.</returns>
        /// <remarks>
        /// Основний пошук виконується за <c>type + id</c>. Migration fallback дозволяється, коли
        /// runtime-тип було перейменовано, але документ досі посилається на той самий source type.
        /// </remarks>
        public static object Get(Type type, string id)
        {
            EnsureLoaded();

            if (type == null || string.IsNullOrWhiteSpace(id))
                return null;

            string key = MakeKey(type.FullName, id);

            // Frozen-екземпляри спільні: кожен consumer того самого type/id отримує
            // той самий матеріалізований config-об'єкт.
            if (_frozen.TryGetValue(key, out var cached))
                return cached;

            if (!_rawByTypeAndId.TryGetValue(key, out var raw))
            {
                // Fallback to schema when a migrated type was renamed but its
                // JSON schema/id remained stable.
                var byId = _rawByTypeAndId
                    .FirstOrDefault(pair =>
                        pair.Key.EndsWith(
                            "|" + id,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            pair.Value.Value<string>("sourceType"),
                            type.FullName,
                            StringComparison.Ordinal));
                raw = byId.Value;
            }

            if (raw == null)
                return null;

            return DeserializeAndFreeze(type, id, raw);
        }

        /// <summary>
        /// Знаходить конфігураційний об'єкт за повним CLR-ім'ям типу та JSON ID.
        /// </summary>
        /// <param name="fullTypeName">Повне ім'я runtime-типу.</param>
        /// <param name="id">Стабільний ID конфігурації.</param>
        /// <returns>Знайдена frozen-конфігурація або <c>null</c>, якщо тип чи конфігурація невідомі.</returns>
        public static object GetByTypeName(string fullTypeName, string id)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(fullTypeName))
                return null;

            Type type = ResolveType(fullTypeName);
            if (type == null)
                return null;

            return Get(type, id);
        }

        /// <summary>
        /// Повертає всі завантажені конфігураційні документи, сумісні з <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Runtime-тип конфігурації для переліку.</typeparam>
        /// <returns>Read-only список матеріалізованих екземплярів конфігурації.</returns>
        public static IReadOnlyList<T> GetAll<T>() where T : class
        {
            EnsureLoaded();

            Type type = typeof(T);
            string prefix = type.FullName + "|";
            var result = new List<T>();

            foreach (var pair in _rawByTypeAndId)
            {
                if (!pair.Key.StartsWith(
                        prefix,
                        StringComparison.Ordinal))
                    continue;

                string id = pair.Value.Value<string>("id");
                if (Get(type, id) is T value)
                    result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// Migration compatibility for old Resources.Load&lt;T&gt; config calls.
        /// It maps the legacy resource basename to the same slug used by the
        /// exporter, then falls back only when exactly one config of T exists.
        /// This is not an authoring path; JSON remains the source of truth.
        /// </summary>
        /// <summary>
        /// Знаходить конфігурацію для legacy-коду, який раніше використовував <c>Resources.Load&lt;T&gt;</c>.
        /// </summary>
        /// <typeparam name="T">Expected configuration type.</typeparam>
        /// <param name="legacyResourcePath">Колишній Resources-шлях або базове ім'я asset-файлу.</param>
        /// <returns>
        /// Відповідна JSON-backed конфігурація або єдина конфігурація типу <typeparamref name="T"/>,
        /// якщо існує однозначний fallback; інакше <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Це лише шлях сумісності для міграції. JSON залишається авторитетним джерелом конфігурації.
        /// </remarks>
        public static T GetLegacyResource<T>(string legacyResourcePath)
            where T : class
        {
            EnsureLoaded();

            string id = LegacySlug(
                System.IO.Path.GetFileName(
                    (legacyResourcePath ?? string.Empty)
                    .Replace('\\', '/')));

            if (!string.IsNullOrWhiteSpace(id))
            {
                T exact = Get<T>(id);
                if (exact != null)
                    return exact;
            }

            IReadOnlyList<T> all = GetAll<T>();
            if (all.Count == 1)
                return all[0];

            return null;
        }

        /// <summary>
        /// Сумісна заміна legacy-доступу через <c>Resources.LoadAll&lt;T&gt;</c>.
        /// </summary>
        /// <typeparam name="T">Runtime configuration type.</typeparam>
        /// <param name="legacyResourcePath">
        /// Залишено для сумісності call-site; JSON-пошук виконується за типом і не потребує цього шляху.
        /// </param>
        /// <returns>Усі JSON-backed конфігурації типу <typeparamref name="T"/>.</returns>
        public static T[] GetAllLegacyResources<T>(string legacyResourcePath)
            where T : class
        {
            return GetAll<T>().ToArray();
        }

        /// <summary>
        /// Перетворює legacy-ім'я ресурсу/файлу на нормалізований slug, який використовується в експортованих JSON ID.
        /// </summary>
        /// <param name="value">Базове ім'я legacy-шляху або ідентифікатор.</param>
        /// <returns>Slug у нижньому регістрі з літерами/цифрами/дефісами або порожній рядок.</returns>
        private static string LegacySlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = new List<char>();
            bool dash = false;

            foreach (char raw in value.Trim())
            {
                char c = char.ToLowerInvariant(raw);
                if (char.IsLetterOrDigit(c))
                {
                    chars.Add(c);
                    dash = false;
                }
                else if (!dash && chars.Count > 0)
                {
                    chars.Add('-');
                    dash = true;
                }
            }

            while (chars.Count > 0 && chars[chars.Count - 1] == '-')
                chars.RemoveAt(chars.Count - 1);

            return new string(chars.ToArray());
        }

        /// <summary>
        /// Не generic-варіант <see cref="GetAll{T}"/> для динамічно визначених типів конфігурації.
        /// </summary>
        /// <param name="type">Runtime-тип конфігурації для переліку.</param>
        /// <returns>Усі матеріалізовані конфігурації для вказаного типу.</returns>
        public static IReadOnlyList<object> GetAll(Type type)
        {
            EnsureLoaded();
            if (type == null)
                return Array.Empty<object>();

            string prefix = type.FullName + "|";
            var result = new List<object>();

            foreach (var pair in _rawByTypeAndId)
            {
                if (!pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                string id = pair.Value.Value<string>("id");
                object value = Get(type, id);
                if (value != null)
                    result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// Знаходить Unity asset reference у runtime-каталозі asset-ів.
        /// </summary>
        /// <param name="key">Стабільний ключ asset-каталогу, збережений у JSON.</param>
        /// <returns>Unity-об'єкт, зареєстрований за цим ключем, або <c>null</c>, якщо його немає.</returns>
        public static UnityEngine.Object ResolveAsset(string key)
        {
            EnsureLoaded();
            return _catalog != null ? _catalog.Resolve(key) : null;
        }

        /// <summary>
        /// Очищає весь завантажений JSON runtime-стан, щоб наступний доступ виконав повне перезавантаження.
        /// </summary>
        /// <remarks>
        /// Призначено для явних development/editor reload-потоків. Звичайний gameplay не повинен
        /// постійно скидати це сховище.
        /// </remarks>
        public static void ResetForExplicitReload()
        {
            lock (Sync)
            {
                _loaded = false;
                _rawByTypeAndId = null;
                _rawBySchemaAndId = null;
                _frozen = null;
                _catalog = null;
                _serializer = null;
                _fingerprint = string.Empty;
                MoyvaJsonTypeRegistry.ClearCache();
            }
        }

        /// <summary>
        /// Завантажує, парсить, валідовує та індексує кожен згенерований runtime JSON-документ.
        /// </summary>
        /// <remarks>
        /// Метод також завантажує runtime asset catalog, відхиляє дублікати ID,
        /// обчислює канонічний fingerprint конфігурації та створює спільний serializer.
        /// </remarks>
        private static void LoadAll()
        {
            _rawByTypeAndId = new Dictionary<string, JObject>(
                StringComparer.OrdinalIgnoreCase);
            _rawBySchemaAndId = new Dictionary<string, JObject>(
                StringComparer.OrdinalIgnoreCase);
            _frozen = new Dictionary<string, object>(
                StringComparer.OrdinalIgnoreCase);

            var catalogPrefab =
                Resources.Load<GameObject>("MoyvaRuntimeAssetCatalog");
            _catalog = catalogPrefab != null
                ? catalogPrefab.GetComponent<MoyvaJsonAssetCatalog>()
                : null;

            // Згенерований runtime JSON — єдине редаговане джерело gameplay-конфігурації, яке читає цей runtime.
            TextAsset[] files =
                Resources.LoadAll<TextAsset>("MoyvaConfigGenerated");

            if (files == null || files.Length == 0)
            {
                throw new InvalidOperationException(
                    "[MoyvaJson] No generated runtime JSON TextAssets found. " +
                    "Run the Jsonization build sync.");
            }

            var canonicalForHash = new List<string>();

            foreach (TextAsset file in files.OrderBy(
                         value => value.name,
                         StringComparer.Ordinal))
            {
                if (file == null || string.IsNullOrWhiteSpace(file.text))
                    continue;

                JObject root;
                try
                {
                    root = JObject.Parse(file.text);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: invalid JSON. {ex.Message}",
                        ex);
                }

                string schema = root.Value<string>("schema");
                string id = root.Value<string>("id");
                string model = root.Value<string>("model");

                if (string.IsNullOrWhiteSpace(schema) ||
                    string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: root metadata " +
                        "schema/version/id/model is required.");
                }

                // Pass82 exported two Construction editor-authoring families into the generated
                // Resources folder. They were never runtime MoyvaJsonConfigObject models, so
                // they must not participate in the runtime registry. Keep the exception below
                // fail-closed for every other unresolved model/schema pair.
                if (IsLegacyEditorOnlyConstructionDocument(root, model, schema))
                    continue;

                Type resolvedType = MoyvaJsonTypeRegistry.ResolveConfigModel(model, schema);
                if (resolvedType == null)
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: no allow-listed runtime config type " +
                        $"for model='{model}', schema='{schema}'.");

                string sourceType = resolvedType.FullName;
                string typeKey = MakeKey(sourceType, id);
                if (_rawByTypeAndId.ContainsKey(typeKey))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] Duplicate config ID '{id}' " +
                        $"for sourceType '{sourceType}'.");
                }

                string schemaKey = MakeKey(schema, id);
                if (_rawBySchemaAndId.ContainsKey(schemaKey))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] Duplicate schema/id '{schema}/{id}'.");
                }

                // Зберігаємо обидва індекси: runtime-consumer-и переважно шукають за CLR-типом,
                // а migration/schema tooling може звертатися до того самого документа за schema.
                _rawByTypeAndId[typeKey] = root;
                _rawBySchemaAndId[schemaKey] = root;

                canonicalForHash.Add(
                    root.ToString(Formatting.None));
            }

            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(
                string.Join("\n", canonicalForHash));
            _fingerprint = BitConverter
                .ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            _serializer = CreateSerializer();
        }

        /// <summary>
        /// Виявляє legacy-документи Construction authoring, які випадково були експортовані
        /// у runtime Resources, хоча ніколи не були валідними runtime config-моделями.
        /// </summary>
        /// <param name="root">Розпарсений JSON root.</param>
        /// <param name="model">Ідентифікатор моделі документа.</param>
        /// <param name="schema">Ідентифікатор схеми документа.</param>
        /// <returns><c>true</c> лише для явно дозволених legacy editor-only сімейств.</returns>
        private static bool IsLegacyEditorOnlyConstructionDocument(
            JObject root,
            string model,
            string schema)
        {
            if (root == null || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(schema))
                return false;

            var migration = root["migration"] as JObject;
            string sourceAssetPath = migration?.Value<string>("sourceAssetPath");
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
                return false;

            string normalizedPath = sourceAssetPath.Replace('\\', '/');
            const string legacyTemplateRoot =
                "Assets/Moyva/Data/ScriptableObjects/Construction/Templates/";
            if (!normalizedPath.StartsWith(legacyTemplateRoot, StringComparison.OrdinalIgnoreCase))
                return false;

            return
                (string.Equals(model, "building-archetype", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-archetype", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(model, "building-template-library", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-template-library", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Матеріалізує один сирий JSON-документ у runtime-конфігураційний об'єкт і кешує його.
        /// </summary>
        /// <param name="type">Конкретний runtime-тип конфігурації.</param>
        /// <param name="id">Стабільний ID конфігурації.</param>
        /// <param name="root">Сирий проіндексований JSON-документ разом із метаданими Moyva.</param>
        /// <returns>Заповнений frozen runtime-екземпляр.</returns>
        /// <exception cref="InvalidOperationException">
        /// Викидається, якщо об'єкт неможливо створити, заповнити, знайти або провалідувати.
        /// </exception>
        private static object DeserializeAndFreeze(
            Type type,
            string id,
            JObject root)
        {
            string key = MakeKey(type.FullName, id);
            if (_frozen.TryGetValue(key, out var cached))
                return cached;

            object instance;
            try
            {
                instance = MoyvaJsonObjectFactory.Create(type);
                if (instance == null) throw new InvalidOperationException("factory returned null");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"[MoyvaJson] Cannot create {type.FullName} for '{id}'.",
                    ex);
            }

            // Cache before Populate so cyclic config references can resolve.
            _frozen[key] = instance;

            // У десеріалізацію передаємо лише дані моделі. Runtime/export metadata належать
            // самому MoyvaJson і не повинні трактуватися як поля config-моделі.
            JObject data = (JObject)root.DeepClone();
            data.Remove("$schema");
            data.Remove("schema");
            data.Remove("version");
            data.Remove("id");
            data.Remove("model");
            data.Remove("sourceType");
            data.Remove("sourceAssetGuid");
            data.Remove("sourceAssetPath");
            data.Remove("migration");

            try
            {
                // Schema drift через видалені/перейменовані optional-поля має бути видимим
                // для розробника, але не повинен робити весь config-об'єкт невалідним.
                WarnAboutUnknownJsonMembers(type, data, _serializer, id);

                using JsonReader reader = data.CreateReader();
                _serializer.Populate(reader, instance);
            }
            catch (Exception ex)
            {
                _frozen.Remove(key);
                throw new InvalidOperationException(
                    $"[MoyvaJson] Failed to deserialize '{id}' " +
                    $"as {type.FullName}: {ex.Message}",
                    ex);
            }

            ValidateObjectGraph(instance, id);

            if (instance is MoyvaJsonConfigObject config)
            {
                config.JsonId = id;
                config.JsonSchema = root.Value<string>("schema") ?? string.Empty;
                config.JsonVersion = root.Value<int?>("version") ?? 1;
                config.JsonSourcePath =
                    root.Value<string>("sourceAssetPath") ?? string.Empty;
                config.name = id;
            }

            return instance;
        }

        /// <summary>
        /// Повідомляє про JSON-властивості, яких не існує у визначеному C# serialization contract.
        /// </summary>
        /// <param name="type">Цільовий runtime-тип, який заповнюється.</param>
        /// <param name="data">JSON-об'єкт без метаданих, який буде десеріалізовано.</param>
        /// <param name="serializer">Serializer, contract resolver якого визначає допустимі імена полів.</param>
        /// <param name="configId">ID конфігурації, який додається до Warning.</param>
        /// <remarks>
        /// Невідомі поля розглядаються як backward-compatible schema drift:
        /// вони логуються для поточного об'єкта, після чого Newtonsoft їх ігнорує.
        ///
        /// Ця перевірка навмисно не приховує некоректні значення для відомих полів.
        /// Такі помилки й надалі проходять назовні з <see cref="JsonSerializer.Populate(JsonReader, object)"/>.
        /// </remarks>
        private static void WarnAboutUnknownJsonMembers(
            Type type,
            JObject data,
            JsonSerializer serializer,
            string configId)
        {
            if (type == null || data == null || serializer == null)
                return;

            var contract = serializer.ContractResolver.ResolveContract(type)
                as Newtonsoft.Json.Serialization.JsonObjectContract;

            if (contract == null)
                return;

            var knownProperties = new HashSet<string>(
                contract.Properties
                    .Where(property =>
                        !property.Ignored &&
                        !string.IsNullOrWhiteSpace(property.PropertyName))
                    .Select(property => property.PropertyName),
                StringComparer.OrdinalIgnoreCase);

            foreach (JProperty property in data.Properties())
            {
                if (knownProperties.Contains(property.Name))
                    continue;

                Debug.LogWarning(
                    $"[MoyvaJson] Unknown JSON member '{property.Name}' " +
                    $"in config '{configId}' for type '{type.FullName}'. " +
                    "The member will be ignored.");
            }
        }

        /// <summary>
        /// Створює спільний Newtonsoft serializer для runtime-конфігурації Moyva.
        /// </summary>
        /// <returns>Serializer з Moyva contract resolution і безпечними custom converter-ами.</returns>
        /// <remarks>
        /// Невідомі/застарілі JSON-поля ігноруються Newtonsoft після того, як вони були виведені
        /// як Warning через <see cref="WarnAboutUnknownJsonMembers"/>.
        /// Структурні, типові, converter- та validation-помилки залишаються критичними.
        /// </remarks>
        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                // Застарілі поля логуються через WarnAboutUnknownJsonMembers().
                // Відомі поля з некоректними значеннями й надалі викликають помилку.
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Include,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new MoyvaJsonContractResolver(),
            };

            settings.Converters.Add(new UnityValueConverter());
            settings.Converters.Add(new UnityObjectReferenceConverter());
            settings.Converters.Add(new ConfigReferenceConverter());
            settings.Converters.Add(new SafePolymorphicConverter());

            return JsonSerializer.Create(settings);
        }

        /// <summary>
        /// Валідовує заповнений serialized object graph після JSON-десеріалізації.
        /// </summary>
        /// <param name="root">Кореневий runtime config-об'єкт.</param>
        /// <param name="sourceId">ID конфігурації, який використовується у validation diagnostics.</param>
        private static void ValidateObjectGraph(object root, string sourceId)
        {
            if (root == null)
                return;

            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            ValidateRecursive(root, "$", sourceId, visited, 0);
        }

        /// <summary>
        /// Рекурсивно валідовує serialized-поля, запобігаючи циклам і надмірній глибині графа.
        /// </summary>
        /// <param name="value">Поточний об'єкт/значення.</param>
        /// <param name="path">Людиночитний JSON-подібний шлях, який використовується в помилках.</param>
        /// <param name="sourceId">ID конфігурації-власника.</param>
        /// <param name="visited">Набір reference identity, який використовується для захисту від циклів.</param>
        /// <param name="depth">Поточна глибина рекурсії.</param>
        private static void ValidateRecursive(
            object value,
            string path,
            string sourceId,
            HashSet<object> visited,
            int depth)
        {
            if (value == null || depth > 32)
                return;

            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || typeof(UnityEngine.Object).IsAssignableFrom(type))
                return;

            if (!type.IsValueType && !visited.Add(value))
                return;

            if (value is IEnumerable enumerable && !(value is string))
            {
                int index = 0;
                foreach (object item in enumerable)
                    ValidateRecursive(item, path + "[" + index++ + "]", sourceId, visited, depth + 1);
                return;
            }

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic || field.IsNotSerialized)
                    continue;

                bool serialized = field.IsPublic ||
                    field.GetCustomAttribute<SerializeField>() != null ||
                    field.GetCustomAttribute<SerializeReference>() != null;
                if (!serialized)
                    continue;

                object fieldValue = field.GetValue(value);
                string fieldPath = path + "." + field.Name.TrimStart('_');

                if (fieldValue is IConvertible convertible)
                {
                    var min = field.GetCustomAttribute<MinAttribute>();
                    if (min != null)
                    {
                        double numeric = Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture);
                        if (numeric < min.min)
                            throw new InvalidOperationException(
                                $"[MoyvaJson] {sourceId} {fieldPath}: expected >= {min.min}, got {numeric}.");
                    }

                    var range = field.GetCustomAttribute<RangeAttribute>();
                    if (range != null)
                    {
                        double numeric = Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture);
                        if (numeric < range.min || numeric > range.max)
                            throw new InvalidOperationException(
                                $"[MoyvaJson] {sourceId} {fieldPath}: expected {range.min}..{range.max}, got {numeric}.");
                    }
                }

                ValidateRecursive(fieldValue, fieldPath, sourceId, visited, depth + 1);
            }
        }

        /// <summary>
        /// Порівнює об'єкти за reference identity для виявлення циклів під час валідації графа.
        /// </summary>
        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        /// <summary>
        /// Створює внутрішній складений ключ словника, який використовується runtime-індексами конфігурації.
        /// </summary>
        private static string MakeKey(string a, string b)
        {
            return (a ?? string.Empty) + "|" + (b ?? string.Empty);
        }

        /// <summary>
        /// Знаходить повне CLR-ім'я типу серед assembly, завантажених у поточний AppDomain.
        /// </summary>
        /// <param name="fullName">Повне ім'я типу.</param>
        /// <returns>Знайдений тип або <c>null</c>, якщо його не завантажено чи він невідомий.</returns>
        private static Type ResolveType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// Зчитує підтримувані Unity value types із безпечного JSON-представлення.
        /// </summary>
        /// <remarks>
        /// Обробляє вектори, кватерніони, кольори, маски, rect, bounds, gradients та animation curves.
        /// Запис навмисно вимкнено, оскільки runtime JSON використовується лише для читання.
        /// </remarks>
        private sealed class UnityValueConverter : JsonConverter
        {
            /// <summary>
            /// Runtime converter працює лише на читання; серіалізація назад у JSON не підтримується.
            /// </summary>
            public override bool CanWrite => false;

            /// <summary>
            /// Повертає, чи підтримує цей converter запитаний Unity value type.
            /// </summary>
            public override bool CanConvert(Type t)
            {
                Type targetType = Nullable.GetUnderlyingType(t) ?? t;
                return targetType == typeof(Vector2) || targetType == typeof(Vector2Int) ||
                       targetType == typeof(Vector3) || targetType == typeof(Vector3Int) ||
                       targetType == typeof(Vector4) || targetType == typeof(Quaternion) ||
                       targetType == typeof(Color) || targetType == typeof(Color32) || targetType == typeof(LayerMask) ||
                       targetType == typeof(Rect) || targetType == typeof(RectInt) || targetType == typeof(Bounds) || targetType == typeof(BoundsInt) ||
                       targetType == typeof(Gradient) || targetType == typeof(AnimationCurve);
            }

            /// <summary>
            /// Перетворює JSON token у запитаний підтримуваний Unity value type.
            /// </summary>
            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                Type targetType = Nullable.GetUnderlyingType(t) ?? t;
                JToken token = JToken.Load(reader);

                if ((targetType == typeof(Color) || targetType == typeof(Color32))
                    && token.Type == JTokenType.String)
                {
                    string colorText = token.Value<string>();
                    if (!string.IsNullOrWhiteSpace(colorText))
                    {
                        string htmlColor = colorText.Trim();
                        if (!htmlColor.StartsWith("#", StringComparison.Ordinal))
                            htmlColor = "#" + htmlColor;

                        if (ColorUtility.TryParseHtmlString(htmlColor, out Color parsed))
                        {
                            if (targetType == typeof(Color32))
                                return (Color32)parsed;

                            return parsed;
                        }
                    }

                    throw new JsonSerializationException("Invalid Unity color value " + colorText);
                }

                if (targetType == typeof(LayerMask))
                {
                    int v = Convert.ToInt32(token, System.Globalization.CultureInfo.InvariantCulture);
                    LayerMask m = new LayerMask(); m.value = v; return m;
                }

                if (token is not JObject o)
                    throw new JsonSerializationException("Expected object for Unity value type " + targetType.FullName);

                float F(string n) => o.Value<float?>(n) ?? 0f;
                int I(string n) => o.Value<int?>(n) ?? 0;
                if (targetType == typeof(Vector2)) return new Vector2(F("x"), F("y"));
                if (targetType == typeof(Vector2Int)) return new Vector2Int(I("x"), I("y"));
                if (targetType == typeof(Vector3)) return new Vector3(F("x"), F("y"), F("z"));
                if (targetType == typeof(Vector3Int)) return new Vector3Int(I("x"), I("y"), I("z"));
                if (targetType == typeof(Vector4)) return new Vector4(F("x"), F("y"), F("z"), F("w"));
                if (targetType == typeof(Quaternion)) return new Quaternion(F("x"), F("y"), F("z"), F("w"));
                if (targetType == typeof(Color)) return new Color(F("r"), F("g"), F("b"), o.Value<float?>("a") ?? 1f);
                if (targetType == typeof(Color32)) return new Color32((byte)I("r"), (byte)I("g"), (byte)I("b"), (byte)(o.Value<int?>("a") ?? 255));
                if (targetType == typeof(Rect)) return new Rect(F("x"), F("y"), F("width"), F("height"));
                if (targetType == typeof(RectInt)) return new RectInt(I("x"), I("y"), I("width"), I("height"));
                if (targetType == typeof(Bounds)) return new Bounds(
                    new Vector3(o["center"]?.Value<float?>("x") ?? 0f, o["center"]?.Value<float?>("y") ?? 0f, o["center"]?.Value<float?>("z") ?? 0f),
                    new Vector3(o["size"]?.Value<float?>("x") ?? 0f, o["size"]?.Value<float?>("y") ?? 0f, o["size"]?.Value<float?>("z") ?? 0f));
                if (targetType == typeof(BoundsInt)) return new BoundsInt(
                    new Vector3Int(o["position"]?.Value<int?>("x") ?? 0, o["position"]?.Value<int?>("y") ?? 0, o["position"]?.Value<int?>("z") ?? 0),
                    new Vector3Int(o["size"]?.Value<int?>("x") ?? 0, o["size"]?.Value<int?>("y") ?? 0, o["size"]?.Value<int?>("z") ?? 0));
                if (targetType == typeof(Gradient))
                {
                    var gradient = new Gradient();
                    var colors = new List<GradientColorKey>();
                    var alphas = new List<GradientAlphaKey>();
                    if (o["colorKeys"] is JArray colorKeys)
                        foreach (JObject k in colorKeys.OfType<JObject>())
                            colors.Add(new GradientColorKey(
                                new Color(k["color"]?.Value<float?>("r") ?? 0f, k["color"]?.Value<float?>("g") ?? 0f, k["color"]?.Value<float?>("b") ?? 0f, k["color"]?.Value<float?>("a") ?? 1f),
                                k.Value<float?>("time") ?? 0f));
                    if (o["alphaKeys"] is JArray alphaKeys)
                        foreach (JObject k in alphaKeys.OfType<JObject>())
                            alphas.Add(new GradientAlphaKey(k.Value<float?>("alpha") ?? 1f, k.Value<float?>("time") ?? 0f));
                    gradient.SetKeys(colors.ToArray(), alphas.ToArray());
                    if (Enum.TryParse(o.Value<string>("mode"), true, out GradientMode mode)) gradient.mode = mode;
                    return gradient;
                }
                if (targetType == typeof(AnimationCurve))
                {
                    var list = new List<Keyframe>();
                    if (o["keys"] is JArray keys)
                    {
                        foreach (JObject k in keys.OfType<JObject>())
                        {
                            var frame = new Keyframe(
                                k.Value<float?>("time") ?? 0f,
                                k.Value<float?>("value") ?? 0f,
                                k.Value<float?>("inTangent") ?? 0f,
                                k.Value<float?>("outTangent") ?? 0f,
                                k.Value<float?>("inWeight") ?? 0f,
                                k.Value<float?>("outWeight") ?? 0f);
                            if (Enum.TryParse(k.Value<string>("weightedMode"), true, out WeightedMode weighted))
                                frame.weightedMode = weighted;
                            list.Add(frame);
                        }
                    }
                    var curve = new AnimationCurve(list.ToArray());
                    if (Enum.TryParse(o.Value<string>("preWrapMode"), true, out WrapMode pre)) curve.preWrapMode = pre;
                    if (Enum.TryParse(o.Value<string>("postWrapMode"), true, out WrapMode post)) curve.postWrapMode = post;
                    return curve;
                }
                throw new JsonSerializationException("Unsupported Unity value type " + targetType.FullName);
            }

            /// <summary>
            /// Запис навмисно не підтримується, оскільки runtime-конфігурація працює лише на читання.
            /// </summary>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => throw new NotSupportedException();
        }

        /// <summary>
        /// Знаходить Unity object reference за стабільним ключем asset-каталогу або дозволеним inline ScriptableObject.
        /// </summary>
        private sealed class UnityObjectReferenceConverter : JsonConverter
        {
            /// <summary>
            /// Runtime converter працює лише на читання.
            /// </summary>
            public override bool CanWrite => false;

            /// <summary>
            /// Обробляє поля, сумісні з <see cref="UnityEngine.Object"/>.
            /// </summary>
            public override bool CanConvert(Type objectType)
            {
                return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
            }

            /// <summary>
            /// Знаходить reference в asset-каталозі або створює дозволений inline ScriptableObject.
            /// </summary>
            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                string key = token.Value<string>("$asset");
                string inlineTypeName = token.Value<string>("$type");

                // Inline Unity-об'єкти дозволені лише для невеликої довіреної групи ScriptableObject-типів.
                if (!string.IsNullOrWhiteSpace(inlineTypeName))
                {
                    Type inlineType = ResolveType(inlineTypeName);
                    if (!IsAllowedInlineUnityObjectType(inlineType, objectType))
                    {
                        throw new JsonSerializationException(
                            $"Inline Unity object type '{inlineTypeName}' is not allow-listed for {objectType.FullName}.");
                    }

                    object instance = MoyvaJsonObjectFactory.Create(inlineType);
                    if (instance == null)
                        throw new JsonSerializationException($"Could not create inline Unity object '{inlineTypeName}'.");

                    token.Remove("$type");
                    using JsonReader tokenReader = token.CreateReader();
                    serializer.Populate(tokenReader, instance);
                    return instance;
                }

                UnityEngine.Object asset = ResolveAsset(key);

                if (asset == null)
                {
                    bool required = token.Value<bool?>("required") ?? false;
                    if (required)
                    {
                        throw new JsonSerializationException(
                            $"Unknown required Unity asset key '{key}'.");
                    }

                    return null;
                }

                if (!objectType.IsInstanceOfType(asset))
                {
                    throw new JsonSerializationException(
                        $"Asset '{key}' is {asset.GetType().FullName}, " +
                        $"expected {objectType.FullName}.");
                }

                return asset;
            }

            /// <summary>
            /// Обмежує створення inline Unity-об'єктів сумісними типами ScriptableObject
            /// із явно довірених namespace.
            /// </summary>
            private static bool IsAllowedInlineUnityObjectType(Type type, Type expectedType)
            {
                if (type == null || expectedType == null || type.IsAbstract ||
                    !expectedType.IsAssignableFrom(type) ||
                    !typeof(ScriptableObject).IsAssignableFrom(type))
                    return false;

                string ns = type.Namespace ?? string.Empty;
                return ns.StartsWith("Kruty1918.Moyva", StringComparison.Ordinal) ||
                       ns.StartsWith("GiantGrey.TileWorldCreator", StringComparison.Ordinal);
            }

            /// <summary>
            /// Writing is intentionally unsupported because runtime configuration is read-only.
            /// </summary>
            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Знаходить посилання між Moyva JSON config-об'єктами та обробляє безпечні inline config values.
        /// </summary>
        private sealed class ConfigReferenceConverter : JsonConverter
        {
            /// <summary>
            /// Runtime converter працює лише на читання.
            /// </summary>
            public override bool CanWrite => false;

            /// <summary>
            /// Обробляє об'єкти, успадковані від <see cref="MoyvaJsonConfigObject"/>.
            /// </summary>
            public override bool CanConvert(Type objectType)
            {
                return typeof(MoyvaJsonConfigObject).IsAssignableFrom(objectType);
            }

            /// <summary>
            /// Знаходить <c>$config</c> reference або матеріалізує inline config-об'єкт.
            /// </summary>
            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                JObject reference = token["$config"] as JObject;

                // Явні references повторно використовують канонічний frozen-екземпляр із цього сховища.
                if (reference != null)
                {
                    string id = reference.Value<string>("id");
                    string typeName = reference.Value<string>("sourceType");
                    string model = reference.Value<string>("model");

                    Type actual = !string.IsNullOrWhiteSpace(typeName)
                        ? ResolveType(typeName)
                        : MoyvaJsonTypeRegistry.ResolveConfigModel(
                            model,
                            MoyvaJsonTypeRegistry.SchemaForConfigType(objectType));
                    actual ??= objectType;

                    if (actual == null ||
                        !objectType.IsAssignableFrom(actual))
                    {
                        actual = objectType;
                    }

                    object resolved = Get(actual, id);
                    if (resolved == null)
                        throw new JsonSerializationException(
                            $"Unknown config reference '{actual.FullName}/{id}'.");
                    return resolved;
                }

                Type inlineType = objectType;
                if (objectType.IsAbstract || objectType.IsInterface)
                {
                    string typeId = token.Value<string>("$type") ?? token.Value<string>("type");
                    inlineType = MoyvaJsonTypeRegistry.Resolve(objectType, typeId);
                    if (inlineType == null)
                        throw new JsonSerializationException(
                            $"Unknown allow-listed inline config type '{typeId}' for {objectType.FullName}.");
                    token.Remove("$type");
                }

                object instance = existingValue ?? MoyvaJsonObjectFactory.Create(inlineType);
                using JsonReader nested = token.CreateReader();
                serializer.Populate(nested, instance);
                return instance;
            }

            /// <summary>
            /// Writing is intentionally unsupported because runtime configuration is read-only.
            /// </summary>
            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Матеріалізує abstract/interface значення, використовуючи лише type ID, зареєстровані у <see cref="MoyvaJsonTypeRegistry"/>.
        /// </summary>
        /// <remarks>
        /// Поліморфізм через CLR type name навмисно не вважається довіреним. Приймаються лише allow-listed mappings.
        /// </remarks>
        private sealed class SafePolymorphicConverter : JsonConverter
        {
            /// <summary>
            /// Runtime converter працює лише на читання.
            /// </summary>
            public override bool CanWrite => false;

            /// <summary>
            /// Обробляє abstract/interface значення, які не є Unity-об'єктами або Moyva config-об'єктами.
            /// </summary>
            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(string) ||
                    objectType.IsPrimitive ||
                    objectType.IsEnum ||
                    typeof(UnityEngine.Object).IsAssignableFrom(objectType) ||
                    typeof(MoyvaJsonConfigObject).IsAssignableFrom(objectType))
                {
                    return false;
                }

                return objectType.IsAbstract || objectType.IsInterface;
            }

            /// <summary>
            /// Знаходить дозволений polymorphic type ID і заповнює його конкретний екземпляр.
            /// </summary>
            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                string typeId =
                    token.Value<string>("$type") ??
                    token.Value<string>("type");

                // Ніколи не створюємо довільні CLR-типи з JSON. Registry виконує роль allow-list.
                Type actual =
                    MoyvaJsonTypeRegistry.Resolve(objectType, typeId);

                if (actual == null)
                {
                    throw new JsonSerializationException(
                        $"Unknown allow-listed type '{typeId}' " +
                        $"for {objectType.FullName}.");
                }

                token.Remove("$type");
                object instance = MoyvaJsonObjectFactory.Create(actual);

                using JsonReader nested = token.CreateReader();
                serializer.Populate(nested, instance);
                return instance;
            }

            /// <summary>
            /// Writing is intentionally unsupported because runtime configuration is read-only.
            /// </summary>
            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }
    }
}
