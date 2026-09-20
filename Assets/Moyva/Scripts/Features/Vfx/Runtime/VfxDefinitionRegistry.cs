using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Vfx.API;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>Реєстр правил VFX-ефектів: резолвить подію+контекст у правило спавну.</summary>
    public sealed class VfxDefinitionRegistry
    {
        private readonly Dictionary<string, VfxEffectRule> _rules =
            new Dictionary<string, VfxEffectRule>(StringComparer.OrdinalIgnoreCase);

        private VfxDefinitionRegistry() { }

        /// <summary>Усі зареєстровані правила.</summary>
        public IReadOnlyCollection<VfxEffectRule> Rules => _rules.Values;

        /// <summary>Будує реєстр із каталогу конфігурації.</summary>
        public static VfxDefinitionRegistry Build(VfxCatalogConfig config)
        {
            var registry = new VfxDefinitionRegistry();
            if (config?.effects == null)
                return registry;

            foreach (VfxEffectRule rule in config.effects)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.eventName))
                    continue;
                registry._rules[Key(rule.eventName, rule.context)] = rule;
            }

            return registry;
        }

        /// <summary>Резолвить правило за подією та контекстом.</summary>
        public VfxEffectRule Resolve(string eventName, string context = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return null;

            if (!string.IsNullOrWhiteSpace(context)
                && _rules.TryGetValue(Key(eventName, context), out VfxEffectRule contextual))
                return contextual;

            return _rules.TryGetValue(Key(eventName, null), out VfxEffectRule rule)
                ? rule
                : null;
        }

        /// <summary>Намагається резолвити правило за подією та контекстом.</summary>
        public bool TryResolve(string eventName, string context, out VfxEffectRule rule)
        {
            rule = Resolve(eventName, context);
            return rule != null;
        }

        private static string Key(string eventName, string context)
            => string.IsNullOrWhiteSpace(context)
                ? eventName.Trim()
                : eventName.Trim() + "|" + context.Trim();
    }
}
