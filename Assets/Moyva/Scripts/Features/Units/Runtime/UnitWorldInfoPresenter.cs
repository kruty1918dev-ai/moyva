using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitWorldInfoPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitService _unitService;
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly IHealthRegistry _healthRegistry;
        private readonly IUnitOwnershipQuery _ownershipQuery;
        private readonly IConstructionSessionCommands _constructionService;
        private readonly IConstructionUnitGarrisonRuntime _garrisonRuntime;

        public UnitWorldInfoPresenter(
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig,
            IUnitService unitService,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] IHealthRegistry healthRegistry = null,
            [InjectOptional] IUnitOwnershipQuery ownershipQuery = null,
            [InjectOptional] IConstructionSessionCommands constructionService = null,
            [InjectOptional] IConstructionUnitGarrisonRuntime garrisonRuntime = null)
        {
            _signalBus = signalBus;
            _unitClassConfig = unitClassConfig;
            _unitService = unitService;
            _economyInfoMediator = economyInfoMediator;
            _healthRegistry = healthRegistry;
            _ownershipQuery = ownershipQuery;
            _constructionService = constructionService;
            _garrisonRuntime = garrisonRuntime;
        }

        public UnitWorldInfoPresenter(
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig,
            IUnitService unitService,
            IEconomyInfoMediator economyInfoMediator)
            : this(
                signalBus,
                unitClassConfig,
                unitService,
                economyInfoMediator,
                null,
                null,
                null,
                null)
        {
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitInfoPanelRequestedSignal>(OnUnitInfoRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitInfoPanelRequestedSignal>(OnUnitInfoRequested);
        }

        private void OnUnitInfoRequested(UnitInfoPanelRequestedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            var unitTypeId = _unitService?.GetUnitTypeId(signal.UnitId);
            var config = string.IsNullOrWhiteSpace(unitTypeId)
                ? null
                : _unitClassConfig?.GetConfig(unitTypeId);

            var title = ResolveTitle(signal.UnitId, unitTypeId, config);
            var subtitle = BuildSubtitle(signal.UnitId, config, signal.Position);
            var content = BuildContent(signal.UnitId, config);

            _signalBus.Fire(new WorldInfoPanelRequestedSignal
            {
                Title = title,
                Subtitle = subtitle,
                Content = content,
            });
        }

        private static string ResolveTitle(string unitId, string unitTypeId, UnitClassConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.DisplayName))
                return config.DisplayName;

            return "Юніт";
        }

        private string BuildSubtitle(
            string unitId,
            UnitClassConfig config,
            UnityEngine.Vector2Int position)
        {
            var roleText = ResolveRoleText(config);
            string ownershipText = ResolveOwnershipText(unitId);

            if (_economyInfoMediator != null
                && _economyInfoMediator.TryGetSettlementContext(position, out var settlementContext))
            {
                var settlementName = string.IsNullOrWhiteSpace(settlementContext.SettlementName)
                    ? "Поселення"
                    : settlementContext.SettlementName;

                if (!string.IsNullOrWhiteSpace(settlementName))
                    return $"{roleText} • {ownershipText} • {settlementName}";
            }

            return $"{roleText} • {ownershipText}";
        }

        private string BuildContent(string unitId, UnitClassConfig config)
        {
            var sb = new StringBuilder();
            if (_healthRegistry != null
                && _healthRegistry.TryGet(unitId, out IHealth health))
            {
                sb.AppendLine($"Здоров'я: {health.CurrentHp} / {health.MaxHp}");
            }
            else if (config?.HitPoints > 0)
                sb.AppendLine($"Здоров'я: {config.HitPoints}");

            float stamina = _unitService.GetStamina(unitId);
            sb.AppendLine(config?.MovementPointsPerTurn > 0f
                ? $"Очки руху: {stamina:0.#} / {config.MovementPointsPerTurn:0.#}"
                : $"Очки руху: {stamina:0.#}");

            AppendMeaningfulFacts(config, sb);

            if (_garrisonRuntime != null)
                sb.AppendLine(_garrisonRuntime.IsGarrisoned(unitId)
                    ? "Стан: у гарнізоні"
                    : "Стан: у полі");

            return sb.ToString().TrimEnd();
        }

        private static bool AppendMeaningfulFacts(UnitClassConfig config, StringBuilder output)
        {
            if (config == null || output == null)
                return false;

            int startLength = output.Length;

            if (config.VisionRange > 0)
                output.AppendLine($"Огляд: {config.VisionRange}");

            int totalDamage = config.CuttingDamage + config.PenetratingDamage + config.CrushingDamage;
            if (totalDamage > 0)
                output.AppendLine($"Шкода: {UnitCombatCalculator.FormatDamageTriplet(config)}");

            int totalDefense = config.CuttingDefense + config.PenetratingDefense + config.CrushingDefense;
            if (totalDefense > 0)
                output.AppendLine($"Захист: {UnitCombatCalculator.FormatDefenseTriplet(config)}");

            return output.Length > startLength;
        }

        private string ResolveOwnershipText(string unitId)
        {
            string ownerId = _ownershipQuery?.GetUnitOwnerId(unitId);
            string localOwnerId = _constructionService?.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(ownerId)
                || string.IsNullOrWhiteSpace(localOwnerId))
            {
                return "власник невідомий";
            }

            return !string.IsNullOrWhiteSpace(ownerId)
                   && string.Equals(
                       ownerId.Trim(),
                       localOwnerId?.Trim(),
                       StringComparison.Ordinal)
                ? "ваш юніт"
                : "чужий юніт";
        }

        private static string ResolveRoleText(UnitClassConfig config)
        {
            if (config == null)
                return "Юніт";

            return config.Role == UnitRole.Military ? "Військовий юніт" : "Робітник";
        }

        private string FormatResources(IReadOnlyDictionary<string, float> resources, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);

            if (resources == null || resources.Count == 0)
            {
                sb.Append("Немає ресурсів.");
                return sb.ToString();
            }

            foreach (var entry in resources)
                sb.AppendLine($"- {ResolveResourceDisplayName(entry.Key)}: {entry.Value:0.#}");

            return sb.ToString().TrimEnd();
        }

        private string ResolveResourceDisplayName(string resourceId)
        {
            string mediated = _economyInfoMediator?.GetResourceDisplayName(resourceId);
            if (!string.IsNullOrWhiteSpace(mediated)
                && !string.Equals(mediated, resourceId, StringComparison.OrdinalIgnoreCase))
            {
                return mediated;
            }

            switch ((resourceId ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "material":
                case "materials": return "Матеріали";
                case "food": return "Їжа";
                case "money": return "Гроші";
                case "wood": return "Деревина";
                case "stone": return "Камінь";
                case "iron":
                case "iron-ore": return "Залізо";
                default: return "Ресурс";
            }
        }

        private static string ResolveUnitDisplayName(string unitTypeId, UnitClassConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.DisplayName))
                return config.DisplayName;

            return "Юніт";
        }
    }
}
