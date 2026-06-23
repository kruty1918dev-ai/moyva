using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Centralized Ukrainian documentation for graph nodes, node parameters, ports and TWC wrapper entries.
    /// Runtime-safe: editor code can use it for search menu tooltips and inspectors, while runtime assemblies
    /// do not depend on UnityEditor.
    /// </summary>
    public static class GraphNodeDocumentation
    {
        public sealed class NodeDoc
        {
            public string Summary;
            public string WhenToUse;
            public string HowItWorks;
            public string Tips;
            public readonly Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, string> Inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, string> Outputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<string, NodeDoc> Docs = BuildDocs();

        public static string BuildSearchTooltip(NodeBase instance, string titleOverride = null, string descriptionOverride = null)
        {
            if (instance == null)
                return string.IsNullOrWhiteSpace(descriptionOverride) ? "Опис відсутній." : descriptionOverride;

            Type type = instance.GetType();
            var doc = GetDoc(type);
            string title = FirstNonEmpty(titleOverride, GetAttribute(type)?.Title, instance.Title, Nicify(type.Name));
            string category = FirstNonEmpty(GetAttribute(type)?.Category, instance.Category, "Other");
            string summary = FirstNonEmpty(doc?.Summary, descriptionOverride, GetAttribute(type)?.Description, BuildGenericNodeSummary(type));

            var sb = new StringBuilder(512);
            sb.AppendLine(title + " [" + category + "]");
            sb.AppendLine();
            sb.AppendLine("Призначення:");
            sb.AppendLine(summary);
            sb.AppendLine();
            sb.AppendLine("Коли використовувати:");
            sb.AppendLine(FirstNonEmpty(doc?.WhenToUse, BuildGenericWhenToUse(type)));
            sb.AppendLine();
            AppendPorts(sb, type, "Входи", instance.Inputs, true);
            sb.AppendLine();
            AppendPorts(sb, type, "Виходи", instance.Outputs, false);
            sb.AppendLine();
            sb.AppendLine("Порада:");
            sb.AppendLine(FirstNonEmpty(doc?.Tips, "Підключай ноду так, щоб типи портів збігалися. Якщо порт сірий або з'єднання не створюється — тип даних не сумісний."));
            return sb.ToString().TrimEnd();
        }

        public static string BuildNodeTooltip(NodeBase instance, NodeInfoAttribute attribute = null)
        {
            if (instance == null)
                return "Нода графа.";

            Type type = instance.GetType();
            var doc = GetDoc(type);
            string title = FirstNonEmpty(attribute?.Title, GetAttribute(type)?.Title, instance.Title, Nicify(type.Name));
            string category = FirstNonEmpty(attribute?.Category, GetAttribute(type)?.Category, instance.Category, "Other");
            string summary = FirstNonEmpty(doc?.Summary, attribute?.Description, GetAttribute(type)?.Description, BuildGenericNodeSummary(type));

            var sb = new StringBuilder(512);
            sb.AppendLine(title + " [" + category + "]");
            sb.AppendLine();
            sb.AppendLine(summary);
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(doc?.HowItWorks))
            {
                sb.AppendLine("Як працює:");
                sb.AppendLine(doc.HowItWorks);
                sb.AppendLine();
            }
            AppendPorts(sb, type, "Входи", instance.Inputs, true);
            sb.AppendLine();
            AppendPorts(sb, type, "Виходи", instance.Outputs, false);
            return sb.ToString().TrimEnd();
        }

        public static string BuildInspectorHeader(NodeBase instance)
        {
            if (instance == null)
                return "Нода графа. Дані недоступні.";

            Type type = instance.GetType();
            var doc = GetDoc(type);
            var attr = GetAttribute(type);
            string title = FirstNonEmpty(attr?.Title, instance.Title, Nicify(type.Name));
            string category = FirstNonEmpty(attr?.Category, instance.Category, "Other");
            string summary = FirstNonEmpty(doc?.Summary, attr?.Description, BuildGenericNodeSummary(type));

            var sb = new StringBuilder(768);
            sb.AppendLine(title + " [" + category + "]");
            sb.AppendLine();
            sb.AppendLine(summary);

            if (!string.IsNullOrWhiteSpace(doc?.WhenToUse))
            {
                sb.AppendLine();
                sb.AppendLine("Коли використовувати: " + doc.WhenToUse);
            }

            if (!string.IsNullOrWhiteSpace(doc?.HowItWorks))
            {
                sb.AppendLine();
                sb.AppendLine("Що змінює: " + doc.HowItWorks);
            }

            if (!string.IsNullOrWhiteSpace(doc?.Tips))
            {
                sb.AppendLine();
                sb.AppendLine("Порада: " + doc.Tips);
            }

            return sb.ToString().TrimEnd();
        }

        public static string BuildPortsInspectorText(NodeBase instance)
        {
            if (instance == null)
                return string.Empty;

            var sb = new StringBuilder(512);
            AppendPorts(sb, instance.GetType(), "Входи", instance.Inputs, true);
            sb.AppendLine();
            AppendPorts(sb, instance.GetType(), "Виходи", instance.Outputs, false);
            return sb.ToString().TrimEnd();
        }

        public static string BuildTwcSearchTooltip(string displayName, Type modifierType, bool isGenerator)
        {
            string title = FirstNonEmpty(displayName, modifierType != null ? Nicify(modifierType.Name) : "TileWorldCreator Node");
            string summary = GetTwcNodeSummary(title, modifierType, isGenerator);
            string behavior = isGenerator
                ? "Це TWC generator: створює нову bool[,] маску з нуля і зазвичай не потребує вхідної маски."
                : "Це TWC modifier: бере вхідну bool[,] маску, змінює її форму або фільтрує клітинки і повертає нову bool[,] маску.";

            return title + "\n\nПризначення:\n" + summary + "\n\nЯк працює:\n" + behavior + "\n\nТип даних:\nbool[,] маска.";
        }

        public static string BuildTwcInspectorHeader(string displayName, Type modifierType, bool isGenerator)
        {
            string title = FirstNonEmpty(displayName, modifierType != null ? Nicify(modifierType.Name) : "TileWorldCreator Node");
            return title + "\n\n" + GetTwcNodeSummary(title, modifierType, isGenerator) + "\n\n" +
                   (isGenerator
                       ? "Генератор створює маску з нуля. Використовуй його як початок chain-а шару."
                       : "Модифікатор змінює вхідну маску. Підключи Source/Input і передай результат далі у Tile Settings або Output.");
        }

        public static string GetParameterDescription(Type nodeType, string propertyPath, string displayName)
        {
            string key = NormalizePropertyKey(propertyPath, displayName);
            var doc = GetDoc(nodeType);
            if (doc != null && TryFind(doc.Parameters, key, out string text))
                return text;

            if (doc != null && TryFind(doc.Parameters, displayName, out text))
                return text;

            return BuildGenericParameterDescription(key, displayName);
        }

        public static string GetTwcParameterDescription(string displayName, Type modifierType, string propertyPath, string propertyDisplayName)
        {
            string key = NormalizePropertyKey(propertyPath, propertyDisplayName);
            string lower = (key ?? string.Empty).ToLowerInvariant();
            string owner = FirstNonEmpty(displayName, modifierType != null ? Nicify(modifierType.Name) : "TWC-модифікатора");

            if (lower.Contains("seed")) return "Seed цього TWC-модифікатора. Змінює випадковий результат без зміни логіки ноди.";
            if (lower.Contains("radius")) return "Радіус впливу. Більше значення розширює область дії, менше — робить ефект локальнішим.";
            if (lower.Contains("threshold")) return "Поріг відбору. Вищий поріг залишає менше клітинок; нижчий — більше клітинок.";
            if (lower.Contains("scale")) return "Масштаб/частота патерну. Великі значення зазвичай дають плавніші форми, малі — дрібніші деталі.";
            if (lower.Contains("octave")) return "Кількість шарів шуму. Більше октав додає дрібні деталі, але робить результат складнішим.";
            if (lower.Contains("smooth")) return "Сила згладжування. Збільшує округлість і прибирає одиночні випадкові клітинки.";
            if (lower.Contains("expand")) return "Розширення маски. Додає клітинки навколо наявної області.";
            if (lower.Contains("shrink")) return "Звуження маски. Прибирає крайові клітинки і залишає внутрішнє ядро форми.";
            if (lower.Contains("invert")) return "Інверсія логіки. True стає false, false стає true.";
            if (lower.Contains("width")) return "Ширина області або патерну для " + owner + ". Збільшення робить форму ширшою по X.";
            if (lower.Contains("height")) return "Висота області або патерну для " + owner + ". Збільшення робить форму вищою по Y або піднімає tiles, якщо це параметр висоти.";
            if (lower.Contains("count") || lower.Contains("amount")) return "Кількість/сила операції. Більше значення підсилює ефект або створює більше елементів.";
            if (lower.Contains("distance")) return "Відстань у клітинках. Визначає, наскільки далеко від джерела застосовується ефект.";
            if (lower.Contains("falloff")) return "Затухання ефекту. Більше значення робить перехід плавнішим.";
            if (lower.Contains("position") || lower.Contains("offset")) return "Зміщення позиції. Дозволяє посунути патерн або результат без зміни самої форми.";
            if (lower.Contains("rule")) return "Правило відбору. Визначає, які клітинки проходять фільтр і залишаються у результаті.";
            if (lower.Contains("mask")) return "Маска, з якою працює " + owner + ". True-клітинки беруть участь у генерації, false-клітинки ігноруються.";

            return "Параметр TWC-ноди " + owner + ". Змінює поведінку native TileWorldCreator generator/modifier. Точний ефект залежить від конкретного TWC типу, але результат завжди впливає на сформовану маску.";
        }

        public static string GetPortDescription(Type nodeType, string portName, Type valueType, bool input)
        {
            var doc = GetDoc(nodeType);
            if (doc != null)
            {
                var dict = input ? doc.Inputs : doc.Outputs;
                if (TryFind(dict, portName, out string text))
                    return text;
            }

            return BuildGenericPortDescription(portName, valueType, input);
        }

        public static string Nicify(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            raw = raw.Trim('_');
            if (raw.StartsWith("m_", StringComparison.Ordinal))
                raw = raw.Substring(2);

            var sb = new StringBuilder(raw.Length + 8);
            char previous = '\0';
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c == '_' || c == '-')
                {
                    sb.Append(' ');
                    previous = ' ';
                    continue;
                }

                if (i > 0 && char.IsUpper(c) && previous != ' ' && !char.IsUpper(previous))
                    sb.Append(' ');

                sb.Append(i == 0 ? char.ToUpperInvariant(c) : c);
                previous = c;
            }
            return sb.ToString();
        }

        private static NodeDoc GetDoc(Type type)
        {
            if (type == null)
                return null;

            if (Docs.TryGetValue(type.Name, out var doc))
                return doc;

            if (!string.IsNullOrEmpty(type.FullName) && Docs.TryGetValue(type.FullName, out doc))
                return doc;

            return null;
        }

        private static NodeInfoAttribute GetAttribute(Type type)
        {
            return type == null ? null : type.GetCustomAttribute<NodeInfoAttribute>(false);
        }

        private static void AppendPorts(StringBuilder sb, Type nodeType, string header, PortDefinition[] ports, bool input)
        {
            sb.AppendLine(header + ":");
            if (ports == null || ports.Length == 0)
            {
                sb.AppendLine("- немає");
                return;
            }

            for (int i = 0; i < ports.Length; i++)
            {
                PortDefinition port = ports[i];
                string name = string.IsNullOrWhiteSpace(port?.Name) ? "<без назви>" : port.Name;
                string typeName = FormatTypeName(port?.ValueType);
                string description = GetPortDescription(nodeType, name, port?.ValueType, input);
                sb.AppendLine("- " + name + " — " + typeName + ". " + description);
            }
        }

        private static string NormalizePropertyKey(string propertyPath, string displayName)
        {
            string key = FirstNonEmpty(propertyPath, displayName, string.Empty);

            int lastDot = key.LastIndexOf('.');
            if (lastDot >= 0 && lastDot + 1 < key.Length)
                key = key.Substring(lastDot + 1);

            if (key.StartsWith("data[", StringComparison.OrdinalIgnoreCase))
                key = displayName;

            key = key.Trim('_');
            if (key.StartsWith("m_", StringComparison.Ordinal))
                key = key.Substring(2);

            return key;
        }

        private static bool TryFind(Dictionary<string, string> map, string key, out string value)
        {
            value = null;
            if (map == null || string.IsNullOrWhiteSpace(key))
                return false;

            if (map.TryGetValue(key, out value))
                return true;

            string normalized = NormalizePropertyKey(key, key);
            if (map.TryGetValue(normalized, out value))
                return true;

            string nicified = Nicify(normalized);
            if (map.TryGetValue(nicified, out value))
                return true;

            foreach (var kvp in map)
            {
                if (string.Equals(NormalizePropertyKey(kvp.Key, kvp.Key), normalized, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Nicify(kvp.Key), nicified, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    return true;
                }
            }

            return false;
        }

        private static string BuildGenericNodeSummary(Type type)
        {
            string name = type == null ? "нода" : Nicify(type.Name);
            if (name.EndsWith("Node", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 4).Trim();
            return "Нода «" + name + "» обробляє дані графа і передає результат у наступні ноди через вихідні порти.";
        }

        private static string BuildGenericWhenToUse(Type type)
        {
            string name = type == null ? "цю ноду" : "«" + Nicify(type.Name) + "»";
            return "Використовуй " + name + ", коли потрібно додати відповідний етап у pipeline генерації шару.";
        }

        private static string BuildGenericParameterDescription(string key, string displayName)
        {
            string lower = (FirstNonEmpty(key, displayName, string.Empty)).ToLowerInvariant();
            if (lower.Contains("value")) return "Значення, яке ця нода віддає на вихід або використовує як константу.";
            if (lower.Contains("seed")) return "Seed для відтворюваної генерації. Однаковий seed дає однаковий результат за тих самих параметрів.";
            if (lower.Contains("scale")) return "Масштаб ефекту. Більші значення зазвичай роблять патерн ширшим/плавнішим, менші — детальнішим.";
            if (lower.Contains("offset")) return "Зміщення результату або координат. Дозволяє посунути патерн без зміни інших параметрів.";
            if (lower.Contains("weight")) return "Вага вибору. Більше значення збільшує шанс, що цей варіант буде використано.";
            if (lower.Contains("height")) return "Висота або вертикальний вплив. Змінює рівень, розмір або підняття результату залежно від ноди.";
            if (lower.Contains("width")) return "Ширина області/об'єкта. Збільшення розтягує результат по горизонталі.";
            if (lower.Contains("radius")) return "Радіус впливу в клітинках або world units. Більше значення розширює область дії.";
            if (lower.Contains("count") || lower.Contains("amount")) return "Кількість або сила операції. Більше значення підсилює ефект або створює більше елементів.";
            if (lower.Contains("mask")) return "Маска визначає, де нода має право працювати: true — активна клітинка, false — ігнорується.";
            if (lower.Contains("prefab")) return "Prefab, який буде використано для створення об'єктів у результаті генерації.";
            if (lower.Contains("material")) return "Material для візуального рендерингу створеної поверхні або об'єкта.";
            if (lower.Contains("texture")) return "Texture, з якої береться візуальний вигляд або зразок для генерації.";
            if (lower.Contains("enabled")) return "Вмикає або вимикає відповідну поведінку без видалення ноди чи даних.";
            if (lower.Contains("mode")) return "Режим роботи. Змінює алгоритм, за яким нода обробляє вхідні дані.";
            if (lower.Contains("type")) return "Тип або категорія поведінки. Визначає, як інтерпретувати дані цього параметра.";
            return "Параметр «" + FirstNonEmpty(displayName, Nicify(key), "без назви") + "» змінює поведінку цієї ноди. Зміни значення і перевір preview, щоб побачити вплив на результат.";
        }

        private static string BuildGenericPortDescription(string portName, Type valueType, bool input)
        {
            string direction = input ? "Вхід" : "Вихід";
            if (valueType == typeof(bool[,])) return direction + " булевої маски. True означає активну клітинку, false — порожню/заборонену.";
            if (valueType == typeof(float[,])) return direction + " числової мапи. Зазвичай використовується для висоти, ваги, шуму або щільності.";
            if (valueType == typeof(int[,])) return direction + " цілочислової мапи. Підходить для ID, індексів або дискретних рівнів.";
            if (valueType == typeof(string[,])) return direction + " string-мапи. Використовується для tile/object/biome IDs або WFC результатів.";
            if (valueType == typeof(bool)) return direction + " true/false значення для перемикачів або умов.";
            if (valueType == typeof(float)) return direction + " float-значення для параметрів сили, висоти, ваги або порогу.";
            if (valueType == typeof(int)) return direction + " int-значення для кількостей, індексів або seed-ів.";
            if (valueType == typeof(string)) return direction + " текстового значення або ID.";
            if (valueType == typeof(object)) return direction + " довільних даних. Тип визначається підключеною нодою.";
            return direction + " даних типу " + FormatTypeName(valueType) + ".";
        }

        private static string FormatTypeName(Type type)
        {
            if (type == null) return "unknown";
            if (type == typeof(bool[,])) return "bool[,] маска";
            if (type == typeof(float[,])) return "float[,] карта значень";
            if (type == typeof(int[,])) return "int[,] карта цілих значень";
            if (type == typeof(string[,])) return "string[,] карта ID/тайлів";
            if (type == typeof(object)) return "Any / object";
            if (type.IsArray) return FormatTypeName(type.GetElementType()) + "[]";
            return type.Name;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i].Trim();
            }
            return string.Empty;
        }

        private static string GetTwcNodeSummary(string title, Type modifierType, bool isGenerator)
        {
            string probe = (title + " " + (modifierType?.Name ?? string.Empty)).ToLowerInvariant();
            if (probe.Contains("shape")) return "Створює геометричну маску: коло, квадрат, прямокутник або іншу базову форму для острова, пляжу, озера чи області генерації.";
            if (probe.Contains("random noise") || probe.Contains("noise")) return "Створює випадковий шум для природних форм: плями суші, каміння, гори, лісові області або деталі маски.";
            if (probe.Contains("dot grid")) return "Створює регулярну сітку точок або клітинок. Корисно для тестів, шахових патернів і контрольованого розміщення.";
            if (probe.Contains("maze")) return "Створює лабіринтну маску з проходами. Використовуй для доріг, тунелів або стилізованих островів.";
            if (probe.Contains("cellular")) return "Створює cellular automata патерн: печери, органічні плями, групи островів або нерівні області.";
            if (probe.Contains("height")) return "Генерує або використовує height texture/height field для мап, де важлива плавна висота.";
            if (probe.Contains("path")) return "Створює або знаходить шлях між областями. Корисно для доріг, річок або проходів.";
            if (probe.Contains("add")) return "Об'єднує дві маски в одну. True-клітинки з обох джерел потрапляють у результат.";
            if (probe.Contains("subtract")) return "Віднімає одну маску з іншої. Корисно для вирізання води, річок, доріг або заборонених зон.";
            if (probe.Contains("invert")) return "Інвертує маску: активні клітинки стають неактивними, а неактивні — активними.";
            if (probe.Contains("smooth")) return "Згладжує маску, прибирає шумні одиночні клітинки і робить контури природнішими.";
            if (probe.Contains("expand")) return "Розширює маску назовні. Корисно для берегової лінії, буферів і збільшення області.";
            if (probe.Contains("shrink")) return "Звужує маску всередину. Корисно для hills/mountains, коли верхні шари мають бути всередині нижніх.";
            if (probe.Contains("select")) return "Відбирає клітинки за правилом або умовою. Корисно для фільтрації маски.";
            if (probe.Contains("boolean")) return "Виконує логічну операцію над масками: AND, OR, XOR або інший boolean-режим.";
            return isGenerator
                ? "TileWorldCreator generator. Створює нову маску, яку можна далі модифікувати, передати в Tile Settings або використати як helper layer."
                : "TileWorldCreator modifier. Змінює вхідну маску і повертає результат для подальшої генерації.";
        }

        private static Dictionary<string, NodeDoc> BuildDocs()
        {
            var docs = new Dictionary<string, NodeDoc>(StringComparer.OrdinalIgnoreCase);

            void Add(string key, string summary, string when, string how, string tips,
                Dictionary<string, string> parameters = null,
                Dictionary<string, string> inputs = null,
                Dictionary<string, string> outputs = null)
            {
                var doc = new NodeDoc
                {
                    Summary = summary,
                    WhenToUse = when,
                    HowItWorks = how,
                    Tips = tips
                };
                if (parameters != null) foreach (var kv in parameters) doc.Parameters[kv.Key] = kv.Value;
                if (inputs != null) foreach (var kv in inputs) doc.Inputs[kv.Key] = kv.Value;
                if (outputs != null) foreach (var kv in outputs) doc.Outputs[kv.Key] = kv.Value;
                docs[key] = doc;
            }

            Add("LayerMaskReferenceNode",
                "Повертає фінальну маску іншого graph layer-а, щоб використовувати її як базу або обмеження у поточному шарі.",
                "Коли треба будувати Sand від LandBase, Hills від Grass або Mountains від Hills без дублювання нод.",
                "Зчитує Output(Masks) або сумісний результат вибраного шару і віддає його як bool[,].",
                "Не створює tiles сам по собі. Це reference на data/helper layer.",
                new Dictionary<string, string> { { "sourceLayerId", "Шар-джерело, з якого береться фінальна маска. Користувач не має редагувати raw ID вручну; вибір має виконуватися через UI шару." } },
                null,
                new Dictionary<string, string> { { "Mask", "Маска вибраного шару." } });

            Add("OutputNode",
                "Фінальна нода шару. Вона явно каже графу, який результат цього layer-а треба вважати підсумковим.",
                "Має бути в кожному шарі, який віддає результат: Tiles, Objects, Masks або Internal Data.",
                "Берe підключений результат і реєструє його як output layer-а для preview, Layer Ref та runtime generation.",
                "Для helper-шару без TileSettings став Output Kind = Masks або Internal Data.",
                new Dictionary<string, string> { { "outputKind", "Тип результату шару. Tiles створює tile layer тільки за наявності TileSettings; Masks/Data не створюють runtime tile object і служать для інших шарів." } },
                new Dictionary<string, string>
                {
                    { "Tiles/Mask", "Фінальна маска або tile result для візуального tile-шару." },
                    { "Objects", "Фінальний набір об'єктів для object placement pipeline." },
                    { "Masks", "Фінальна bool[,] маска helper/data шару." },
                    { "Data", "Довільні внутрішні дані для наступних шарів або debug workflow." }
                });

            Add("TileSettingsNode",
                "Описує, які TilePreset-и і build-параметри треба застосувати до маски шару.",
                "Коли шар має реально створювати tiles/GameObject-и у TWC/runtime.",
                "Бере bool[,] Mask і перетворює її на renderable tile layer settings. Без цієї ноди шар вважається mask/data-only.",
                "Для helper mask layer не додавай TileSettingsNode.",
                new Dictionary<string, string>
                {
                    { "tileVariants", "Weighted список TilePreset-варіантів. Кожен variant може бути Top/Middle/Bottom і мати власну вагу та висоту." },
                    { "Preset", "TilePreset, який буде використано для цього варіанту тайла." },
                    { "Slot", "Позиція tile preset-а в TWC build layer: Top, Middle або Bottom." },
                    { "Weight", "Вага випадкового вибору variant-а. Більше значення — частіше використовується." },
                    { "TileHeight", "Висота цього tile variant-а. Впливає на вертикальний рівень tile layer-а." },
                    { "useDualGrid", "Вмикає dual-grid побудову, якщо tileset її підтримує. Дає кращі переходи на ізометричній сітці." },
                    { "scaleTileToCellSize", "Автоматично масштабує tile до розміру клітинки." },
                    { "layerYOffset", "Вертикальне зміщення всього шару. Корисно для уникнення z-fighting або підняття поверхонь." },
                    { "scaleOffset", "Додатковий scale-множник для створених tile object-ів." },
                    { "generateFlatSurface", "Створює пласку поверхню замість tile preset-ів. Корисно для води або простих площин." },
                    { "flatSurfaceMaterial", "Material для пласкої поверхні." },
                    { "tileLayerHeightOffset", "Додатковий height offset для tile layer-а відносно базової висоти шару." },
                    { "ignoreFillTiles", "Ігнорує fill tiles, якщо tileset/TWC використовує автозаповнення." },
                    { "meshGenerationOverride", "Перезаписує стандартну mesh-generation поведінку TWC для цього шару." },
                    { "mergeTiles", "Об'єднує tiles у меншу кількість mesh/object-ів для оптимізації." },
                    { "shadowCastingMode", "Режим відкидання тіней для згенерованих tile mesh-ів." },
                    { "objectLayer", "Unity Layer для згенерованих tile object-ів." },
                    { "renderingLayer", "Rendering layer mask для URP/HDRP lighting/rendering фільтрів." },
                    { "colliderType", "Тип collider-а для tile layer-а." },
                    { "tileColliderHeight", "Висота collider-а tile layer-а." },
                    { "tileColliderExtrusionHeight", "Додаткова extrusion-висота collider-а." },
                    { "invertCollisionWalls", "Інвертує логіку collision walls для edge/cell boundaries." }
                },
                new Dictionary<string, string> { { "Mask", "Маска клітинок, де треба створити tiles або flat surface." } },
                new Dictionary<string, string> { { "Mask", "Та сама маска, передана далі для Output/preview." }, { "Settings", "Build settings для TWC tile layer-а." } });

            Add("AddNode",
                "Універсальна typed-нода для додавання, merge, overlay і застосування маски B до бази A.",
                "Коли треба об'єднати маски, скласти numeric maps або зробити A Base + B Mask workflow.",
                "Тип портів визначається за підключеннями. У ApplyMask/SubtractMask порт B завжди bool[,] і не змінює тип результату.",
                "Для твого workflow A=база, B=маска вибирай Mode = ApplyMask.",
                new Dictionary<string, string>
                {
                    { "mode", "Режим операції: AddOrMerge, ApplyMask, SubtractMask, OverlayBOnA, Min або Max." },
                    { "valueKind", "Автоматично визначений тип даних. Користувач зазвичай не редагує це вручну." }
                },
                new Dictionary<string, string> { { "A Base", "Базова маска/мапа/значення, яке треба залишити або модифікувати." }, { "B", "Друга маска/мапа/значення для merge/add/overlay." }, { "B Mask", "Bool[,] маска, яка обмежує або вирізає A Base." } },
                new Dictionary<string, string> { { "Result", "Результат того самого типу, що A/Base." } });

            Add("BoolAndNode", "Логічне AND для двох bool[,] масок.", "Коли треба залишити тільки перетин двох масок.", "Результат true лише там, де A=true і B=true.", "Для union краще Bool Or або AddOrMerge.", inputs: new Dictionary<string, string> { { "A", "Перша маска." }, { "B", "Друга маска." } }, outputs: new Dictionary<string, string> { { "Result", "Перетин A і B." } });
            Add("BoolOrNode", "Логічне OR для двох bool[,] масок.", "Коли треба об'єднати дві області.", "Результат true там, де true хоча б одна маска.", "Корисно для складання кількох островів або зон.", inputs: new Dictionary<string, string> { { "A", "Перша маска." }, { "B", "Друга маска." } }, outputs: new Dictionary<string, string> { { "Result", "Об'єднана маска." } });
            Add("BoolSubtractNode", "Віднімає маску B з A.", "Коли треба вирізати воду, річку, дорогу або заборонену зону з бази.", "Результат true там, де A=true і B=false.", "Для carving workflow це основна операція.", inputs: new Dictionary<string, string> { { "A", "Базова маска." }, { "B", "Маска, яку треба вирізати." } }, outputs: new Dictionary<string, string> { { "Result", "A без B." } });
            Add("BoolXorNode", "Симетрична різниця двох масок.", "Коли треба залишити тільки клітинки, де A і B відрізняються.", "True там, де рівно одна з масок true.", "Рідше використовується для terrain, але корисно для debug/патернів.", inputs: new Dictionary<string, string> { { "A", "Перша маска." }, { "B", "Друга маска." } }, outputs: new Dictionary<string, string> { { "Result", "Симетрична різниця." } });
            Add("BoolInvertNode", "Інвертує bool[,] маску.", "Коли треба отримати воду з суші або заборонену область з дозволеної.", "True стає false, false стає true.", "Після Invert часто треба обмежити маску розміром world/map bounds.", inputs: new Dictionary<string, string> { { "Mask", "Маска для інверсії." } }, outputs: new Dictionary<string, string> { { "Result", "Інвертована маска." } });

            Add("BaseNoiseSettings", "Конфігурує noise parameters для генераторів шуму.", "Коли треба повторно використовувати однаковий noise profile.", "Формує NoiseSettings з scale/octaves/persistence/lacunarity/offset.", "Більший scale — плавніші форми; більше octaves — більше деталей.", new Dictionary<string, string>
            {
                { "scale", "Масштаб шуму. Більше — великі плавні області, менше — дрібні деталі." },
                { "octaves", "Кількість noise layers. Більше октав додає деталізацію." },
                { "persistance", "Як швидко слабшає амплітуда наступних октав." },
                { "lacunarity", "Як швидко зростає частота наступних октав." },
                { "Offset", "Зсув координат шуму по X/Y." }
            }, outputs: new Dictionary<string, string> { { "NoiseSettings", "Готовий набір параметрів шуму." } });

            Add("BoolValueNode", "Повертає bool-константу.", "Для ручних перемикачів і умовних входів.", "Вихід завжди дорівнює значенню поля Value.", "Не створює маску; це scalar bool.", new Dictionary<string, string> { { "value", "True або false значення, яке буде віддано на вихід." } }, outputs: new Dictionary<string, string> { { "Value", "Поточне bool-значення." } });
            Add("FloatValueNode", "Повертає float-константу.", "Для порогів, висот, ваг або strength-параметрів.", "Вихід завжди дорівнює значенню поля Value.", "Використовуй для числових входів інших нод.", new Dictionary<string, string> { { "value", "Float-значення." } }, outputs: new Dictionary<string, string> { { "Value", "Поточне float-значення." } });
            Add("IntValueNode", "Повертає int-константу.", "Для кількостей, індексів, seed offset або integer параметрів.", "Вихід завжди дорівнює значенню поля Value.", "Не плутай з float, якщо порт очікує десяткове число.", new Dictionary<string, string> { { "value", "Ціле число." } }, outputs: new Dictionary<string, string> { { "Value", "Поточне int-значення." } });
            Add("StringValueNode", "Повертає string-константу.", "Для ID, формул, назв або текстових параметрів.", "Вихід завжди дорівнює введеному рядку.", "Порожній рядок також є валідним значенням.", new Dictionary<string, string> { { "value", "Текстовий рядок, який буде віддано на вихід." } }, outputs: new Dictionary<string, string> { { "Value", "Поточний string." } });
            Add("SeedNode", "Глобальний seed графа.", "Коли треба зробити результат генерації відтворюваним.", "Встановлює GlobalSeed перед виконанням інших нод.", "Однаковий seed + однакові параметри = однаковий результат.", new Dictionary<string, string> { { "seed", "Глобальне зерно генерації." } });

            Add("SharedSettingsNode", "Реєструє спільні generator settings.", "Для доступу до shared settings у графі.", "Передає ScriptableObject налаштувань у context/registry.", "Це службова static нода; зазвичай одна на граф.", new Dictionary<string, string> { { "settings", "SharedGeneratorSettingsSO з глобальними параметрами генерації." } });
            Add("SubgraphInputNode", "Вхідний порт для вкладеного графа.", "Всередині subgraph, щоб отримати дані з батьківського графа.", "Читає input за ключем/порядком з SubgraphNode.", "Потрібна тільки всередині reusable macro/subgraph.");
            Add("SubgraphNode", "Виконує інший GraphAsset як вкладений модуль.", "Коли треба повторно використовувати готовий pipeline.", "Передає inputs у subgraph і повертає його outputs.", "Стеж за типами входів/виходів, щоб вони збігались.", new Dictionary<string, string> { { "subgraph", "GraphAsset, який буде виконано як вкладений граф." } });

            Add("PlacementMaskNode", "Формує маску розміщення об'єктів з include/exclude масок.", "Для object placement перед scatter nodes.", "Об'єднує дозволену область і виключає заборонені клітинки.", "Використовуй перед Object Scatter або Cluster Scatter.");
            Add("EdgeMaskNode", "Створює weighted band біля краю маски.", "Для трави, каміння або об'єктів вздовж берегів/країв.", "Рахує відстань до edge і формує float weights.", "Invert міняє фокус з краю на внутрішню/зовнішню область.", new Dictionary<string, string>
            {
                { "distanceFromEdge", "Відстань від краю в клітинках, де починається або концентрується ефект." },
                { "falloff", "Плавність затухання ваги від краю." },
                { "invert", "Інвертує вагову область." }
            });
            Add("ObjectScatterNode", "Створює одиночні scatter candidates у дозволеній масці.", "Для дерев, каміння, декору без кластерів.", "Випадково вибирає клітинки за rule/spacing/density.", "Для груп краще Cluster Scatter.", new Dictionary<string, string> { { "rule", "Правила щільності, відстані, random seed і фільтрів розміщення." } });
            Add("ClusterScatterNode", "Створює групові scatter candidates.", "Для кущів, лісів, таборів або груп декору.", "Спочатку вибирає центри кластерів, потім розкидає об'єкти навколо.", "Добре працює з EdgeMask або noise masks.", new Dictionary<string, string> { { "cluster", "Параметри розміру/кількості/радіуса кластерів." }, { "rule", "Загальні правила розміщення кандидатів." } });
            Add("ObjectLayerNode", "Пакує scatter candidates і prefab variants у generated object layer.", "Після scatter nodes, коли треба створити конкретний object layer.", "Бере кандидати, застосовує prefab list/rules і формує layer payload.", "Target Graph Layer можна використовувати для прив'язки до конкретного terrain layer.", new Dictionary<string, string>
            {
                { "layerName", "Назва generated object layer-а." },
                { "targetGraphLayerId", "Graph layer, до якого логічно прив'язаний object layer. Raw ID не редагувати вручну." },
                { "prefabs", "Список prefab variants для випадкового/weighted вибору." },
                { "rule", "Правила розміщення object candidates." },
                { "cluster", "Кластерні параметри, якщо layer використовує групове розміщення." }
            });
            Add("GrassCardGeneratorNode", "Генерує prefab/material для billboard/crossed-plane grass card.", "Для швидкого створення трави з texture/material прямо з графа.", "Налаштовує geometry mode, tint, alpha clip, wind і розміри." , "Після генерації використай prefab у object placement.", new Dictionary<string, string>
            {
                { "texture", "Texture трави або рослини." }, { "material", "Material для generated grass card." }, { "prefab", "Prefab, який буде створено/оновлено." },
                { "tint", "Колірний tint для трави." }, { "alphaClip", "Поріг прозорості. Більше значення жорсткіше обрізає texture." },
                { "crossedPlanes", "Кількість пересічених площин для об'ємності." }, { "geometryMode", "Спосіб побудови geometry." },
                { "width", "Ширина card-а." }, { "height", "Висота card-а." }, { "doubleSided", "Рендерити обидві сторони площин." },
                { "windWobble", "Вмикає wind wobble shader/parameter, якщо material це підтримує." }, { "colorVariation", "Випадкова варіація кольору між інстансами." }
            });
            Add("ObjectOutputToTWCNode", "Передає object placement результат у TWC/runtime output.", "Коли треба вивести object layer у фінальну генерацію.", "Конвертує generated object payload у формат, який забирає compiler/binding.", "Це фінальний міст для object placement chain-а.");

            Add("WaveFunctionCollapseNode", "Генерує string[,] tile map через Wave Function Collapse за input sample.", "Для патернових мап, де важливо зберегти локальні сусідства tile IDs.", "Читає зразок, витягує patterns і збирає output map із сумісних патернів.", "WFC чутливий до sample. Якщо часто fail — зменш pattern size або збільш max attempts.", new Dictionary<string, string>
            {
                { "patternSize", "Розмір локального патерну. Більше — точніше копіює стиль, але складніше згенерувати." },
                { "periodicInput", "Вважає input sample зацикленим по краях." },
                { "periodicOutput", "Вважає output map зацикленою по краях." },
                { "maxAttempts", "Скільки разів пробувати перегенерувати при contradiction/fail." },
                { "outputWidth", "Ширина output map. 0 може означати взяти ширину з context/map settings." },
                { "outputHeight", "Висота output map. 0 може означати взяти висоту з context/map settings." }
            });

            Add("TwcModifierNode", "Обгортка native TileWorldCreator generator/modifier у Moyva graph.", "Коли треба використати існуючий TWC Shapes, Random Noise, Smooth, Shrink, Add, Subtract тощо.", "Зберігає тип TWC modifier-а і його serialized settings; Execute повертає mask/result у graph pipeline.", "Не плутай Graph Math/Add з TWC Add: Graph Add — typed; TWC Add — native blueprint modifier.", new Dictionary<string, string>
            {
                { "modifierTypeName", "Повне ім'я TWC типу. Службове поле; користувач не редагує вручну." },
                { "modifier", "Serialized instance native TWC modifier-а з його параметрами." }
            });

            return docs;
        }
    }
}
