using System;
using System.Collections.Generic;
using System.Text;
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

        public UnitWorldInfoPresenter(
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig,
            IUnitService unitService,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator)
        {
            _signalBus = signalBus;
            _unitClassConfig = unitClassConfig;
            _unitService = unitService;
            _economyInfoMediator = economyInfoMediator;
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
            var subtitle = BuildSubtitle(config, signal.Position);
            var content = BuildContent(signal.UnitId, unitTypeId, config, signal.Position);

            _signalBus.Fire(new WorldInfoPanelRequestedSignal
            {
                Title = title,
                Subtitle = subtitle,
                Content = content,
            });
        }

        private static string ResolveTitle(string unitId, string unitTypeId, UnitClassConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.TypeId))
                return config.TypeId;

            if (!string.IsNullOrWhiteSpace(unitTypeId))
                return unitTypeId;

            return unitId;
        }

        private string BuildSubtitle(UnitClassConfig config, UnityEngine.Vector2Int position)
        {
            var roleText = ResolveRoleText(config);

            if (_economyInfoMediator != null
                && _economyInfoMediator.TryGetSettlementContext(position, out var settlementContext))
            {
                var settlementName = string.IsNullOrWhiteSpace(settlementContext.SettlementName)
                    ? settlementContext.SettlementId
                    : settlementContext.SettlementName;

                if (!string.IsNullOrWhiteSpace(settlementName))
                    return $"{roleText} • {settlementName}";
            }

            return roleText;
        }

        private string BuildContent(string unitId, string unitTypeId, UnitClassConfig config, UnityEngine.Vector2Int position)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Базова інформація");
            sb.AppendLine($"ID: {unitId}");

            if (!string.IsNullOrWhiteSpace(unitTypeId))
                sb.AppendLine($"Тип: {unitTypeId}");

            int beforeFacts = sb.Length;
            if (config != null && AppendMeaningfulFacts(config, sb))
                sb.Insert(beforeFacts, Environment.NewLine);
            else
                sb.AppendLine($"Роль: {ResolveRoleText(config)}");

            sb.AppendLine($"Поточна стаміна: {_unitService.GetStamina(unitId):0.#}");

            if (_economyInfoMediator != null
                && _economyInfoMediator.TryGetSettlementContext(position, out var settlementContext))
            {
                sb.AppendLine();
                sb.AppendLine(FormatResources(
                    _economyInfoMediator.GetSettlementResourceTotals(settlementContext.SettlementId),
                    "Ресурси поселення"));
                sb.AppendLine();
                sb.AppendLine(FormatResources(
                    _economyInfoMediator.GetOwnerResourceTotals(settlementContext.OwnerId),
                    "Ресурси власника"));
            }

            return sb.ToString().TrimEnd();
        }

        private static bool AppendMeaningfulFacts(UnitClassConfig config, StringBuilder output)
        {
            if (config == null || output == null)
                return false;

            int startLength = output.Length;

            if (!string.IsNullOrWhiteSpace(config.TypeId))
                output.AppendLine($"TypeId: {config.TypeId}");

            output.AppendLine(config.Role == UnitRole.Military
                ? "Прапорець: бойовий юніт"
                : "Прапорець: економічний юніт");

            if (config.BaseStamina > 0f)
                output.AppendLine($"Базова стаміна: {config.BaseStamina:0.#}");

            if (config.VisionRange > 0)
                output.AppendLine($"Дальність огляду: {config.VisionRange}");

            if (config.StaminaRandomRange != UnityEngine.Vector2.zero)
                output.AppendLine($"Рандом стаміни: {config.StaminaRandomRange.x:0.#} .. {config.StaminaRandomRange.y:0.#}");

            if (config.Prefab != null)
                output.AppendLine("Прапорець: має prefab");

            return output.Length > startLength;
        }

        private static string ResolveRoleText(UnitClassConfig config)
        {
            if (config == null)
                return "Юніт";

            return config.Role == UnitRole.Military ? "Військовий юніт" : "Робітник";
        }

        private static string FormatResources(IReadOnlyDictionary<string, float> resources, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);

            if (resources == null || resources.Count == 0)
            {
                sb.Append("Немає ресурсів.");
                return sb.ToString();
            }

            foreach (var entry in resources)
                sb.AppendLine($"- {entry.Key}: {entry.Value:0.#}");

            return sb.ToString().TrimEnd();
        }
    }
}
