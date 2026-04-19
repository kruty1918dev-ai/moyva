using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitSelectionVisualService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IUnitService _unitService;
        private readonly SpriteSelectionHighlighter _selectionHighlighter = new();

        private string _selectedUnitId;

        public UnitSelectionVisualService(SignalBus signalBus, IUnitService unitService)
        {
            _signalBus = signalBus;
            _unitService = unitService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _selectionHighlighter.Clear();
            _selectedUnitId = null;
        }

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.Unit || string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                _selectedUnitId = null;
                _selectionHighlighter.Clear();
                return;
            }

            _selectedUnitId = signal.ObjectId;
            _selectionHighlighter.Clear();
            _selectionHighlighter.Apply(_unitService.GetUnitObject(signal.ObjectId));
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(_selectedUnitId, signal.UnitId, StringComparison.Ordinal))
                return;

            _selectedUnitId = null;
            _selectionHighlighter.Clear();
        }
    }
}