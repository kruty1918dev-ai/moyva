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
        private readonly Kruty1918.Localization.ILocalizationService _loca;

        public UnitWorldInfoPresenter(
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig,
            IUnitService unitService,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] IHealthRegistry healthRegistry = null,
            [InjectOptional] IUnitOwnershipQuery ownershipQuery = null,
            [InjectOptional] IConstructionSessionCommands constructionService = null,
            [InjectOptional] IConstructionUnitGarrisonRuntime garrisonRuntime = null,
            [InjectOptional] Kruty1918.Localization.ILocalizationService localization = null)
        {
            _signalBus = signalBus;
            _unitClassConfig = unitClassConfig;
            _unitService = unitService;
            _economyInfoMediator = economyInfoMediator;
            _healthRegistry = healthRegistry;
            _ownershipQuery = ownershipQuery;
            _constructionService = constructionService;
            _garrisonRuntime = garrisonRuntime;
            _loca = localization;
        }

        private string T(string key) => _loca?.T(key) ?? key ?? string.Empty;
        private string TF(string key, params object[] args) => _loca?.TF(key, args) ?? key ?? string.Empty;

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

        private string ResolveTitle(string unitId, string unitTypeId, UnitClassConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.DisplayName))
                return T(config.DisplayName);

            return T("Unit");
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
                    ? T("Settlement")
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
                sb.AppendLine(TF("Health: {0} / {1}", health.CurrentHp, health.MaxHp));
            }
            else if (config?.HitPoints > 0)
                sb.AppendLine(TF("Health: {0}", config.HitPoints));

            float stamina = _unitService.GetStamina(unitId);
            sb.AppendLine(config?.MovementPointsPerTurn > 0f
                ? TF("Movement points: {0:0.#} / {1:0.#}", stamina, config.MovementPointsPerTurn)
                : TF("Movement points: {0:0.#}", stamina));

            AppendMeaningfulFacts(config, sb);

            if (_garrisonRuntime != null)
                sb.AppendLine(_garrisonRuntime.IsGarrisoned(unitId)
                    ? T("State: garrisoned")
                    : T("State: in the field"));

            return sb.ToString().TrimEnd();
        }

        private bool AppendMeaningfulFacts(UnitClassConfig config, StringBuilder output)
        {
            if (config == null || output == null)
                return false;

            int startLength = output.Length;

            if (config.VisionRange > 0)
                output.AppendLine(TF("Vision: {0}", config.VisionRange));

            int totalDamage = config.CuttingDamage + config.PenetratingDamage + config.CrushingDamage;
            if (totalDamage > 0)
                output.AppendLine(TF("Damage: {0}", UnitCombatCalculator.FormatDamageTriplet(config)));

            int totalDefense = config.CuttingDefense + config.PenetratingDefense + config.CrushingDefense;
            if (totalDefense > 0)
                output.AppendLine(TF("Defense: {0}", UnitCombatCalculator.FormatDefenseTriplet(config)));

            return output.Length > startLength;
        }

        private string ResolveOwnershipText(string unitId)
        {
            string ownerId = _ownershipQuery?.GetUnitOwnerId(unitId);
            string localOwnerId = _constructionService?.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(ownerId)
                || string.IsNullOrWhiteSpace(localOwnerId))
            {
                return T("owner unknown");
            }

            return !string.IsNullOrWhiteSpace(ownerId)
                   && string.Equals(
                       ownerId.Trim(),
                       localOwnerId?.Trim(),
                       StringComparison.Ordinal)
                ? T("your unit")
                : T("enemy unit");
        }

        private string ResolveRoleText(UnitClassConfig config)
        {
            if (config == null)
                return T("Unit");

            return config.Role == UnitRole.Military ? T("Military unit") : T("Worker");
        }

        private string FormatResources(IReadOnlyDictionary<string, float> resources, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);

            if (resources == null || resources.Count == 0)
            {
                sb.Append(T("No resources."));
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
                case "materials": return T("Materials");
                case "food": return T("Food");
                case "money": return T("Money");
                case "wood": return T("Wood");
                case "stone": return T("Stone");
                case "iron":
                case "iron-ore": return T("Iron");
                default: return T("Resource");
            }
        }

        private string ResolveUnitDisplayName(string unitTypeId, UnitClassConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.DisplayName))
                return T(config.DisplayName);

            return T("Unit");
        }
    }
}
