using System;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class MapObjectWorldInfoPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IMapObjectEconomyService _mapObjectEconomyService;
        private readonly EconomyDatabaseSO _database;

        public MapObjectWorldInfoPresenter(
            SignalBus signalBus,
            [InjectOptional] IMapObjectEconomyService mapObjectEconomyService,
            [InjectOptional] EconomyDatabaseSO database)
        {
            _signalBus = signalBus;
            _mapObjectEconomyService = mapObjectEconomyService;
            _database = database;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<MapObjectInfoPanelRequestedSignal>(OnMapObjectInfoRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<MapObjectInfoPanelRequestedSignal>(OnMapObjectInfoRequested);
        }

        private void OnMapObjectInfoRequested(MapObjectInfoPanelRequestedSignal signal)
        {
            if (_mapObjectEconomyService == null)
                return;

            if (!_mapObjectEconomyService.TryGetEntry(signal.MapObjectId, out var entry) || entry == null)
                return;

            var title = string.IsNullOrWhiteSpace(entry.DisplayName)
                ? signal.MapObjectId
                : entry.DisplayName;

            var subtitle = entry.IsInteractable ? "Інтерактивний об'єкт" : "Неінтерактивний об'єкт";

            var content = BuildContent(entry);

            _signalBus.Fire(new WorldInfoPanelRequestedSignal
            {
                Title = title,
                Subtitle = subtitle,
                Content = content,
            });
        }

        private string BuildContent(MapObjectEconomyEntry entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ID: {entry.MapObjectId}");
            sb.AppendLine($"Інтерактивність: {(entry.IsInteractable ? "Так" : "Ні")}");
            sb.AppendLine($"Повертає ресурс: {(entry.YieldsResource ? "Так" : "Ні")}");

            if (entry.YieldsResource)
            {
                string resourceLabel = string.IsNullOrWhiteSpace(entry.HarvestResourceId)
                    ? "Не задано"
                    : entry.HarvestResourceId;

                if (_database != null && !string.IsNullOrWhiteSpace(entry.HarvestResourceId))
                {
                    var resource = _database.Resources
                        .FirstOrDefault(r => r != null && string.Equals(r.Id, entry.HarvestResourceId, StringComparison.Ordinal));

                    if (resource != null && !string.IsNullOrWhiteSpace(resource.DisplayName))
                        resourceLabel = resource.DisplayName;
                }

                sb.AppendLine($"Ресурс добування: {resourceLabel}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
