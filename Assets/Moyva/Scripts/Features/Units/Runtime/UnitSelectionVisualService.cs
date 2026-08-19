using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitSelectionVisualService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IUnitService _unitService;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly SpriteSelectionHighlighter _selectionHighlighter = new();
        private GameObject _selectionRing;
        private Material _selectionRingMaterial;

        private string _selectedUnitId;

        public UnitSelectionVisualService(
            SignalBus signalBus,
            IUnitService unitService,
            [InjectOptional] IUnitClassConfig unitClassConfig = null)
        {
            _signalBus = signalBus;
            _unitService = unitService;
            _unitClassConfig = unitClassConfig;
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
            ClearSelectionRing();
            _selectedUnitId = null;
        }

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.Unit || string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                _selectedUnitId = null;
                _selectionHighlighter.Clear();
                ClearSelectionRing();
                return;
            }

            _selectedUnitId = signal.ObjectId;
            _selectionHighlighter.Clear();
            ClearSelectionRing();
            GameObject unitObject = _unitService.GetUnitObject(signal.ObjectId);
            _selectionHighlighter.Apply(unitObject);
            ApplySelectionRing(unitObject, ResolveConfig(signal.ObjectId));
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(_selectedUnitId, signal.UnitId, StringComparison.Ordinal))
                return;

            _selectedUnitId = null;
            _selectionHighlighter.Clear();
            ClearSelectionRing();
        }

        private void ApplySelectionRing(
            GameObject unitObject,
            UnitClassConfig config)
        {
            if (unitObject == null)
                return;

            float radius = ResolveRingRadius(unitObject, config);
            _selectionRing = new GameObject("SelectionRing");
            _selectionRing.transform.SetParent(unitObject.transform, false);
            _selectionRing.transform.localPosition = new Vector3(0f, 0.035f, 0f);

            LineRenderer line = _selectionRing.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.startWidth = 0.06f;
            line.endWidth = 0.06f;
            line.numCornerVertices = 2;
            line.startColor = new Color(0.84f, 0.68f, 0.22f, 0.95f);
            line.endColor = line.startColor;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                            ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _selectionRingMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.DontSave,
                    color = Color.white,
                };
                line.sharedMaterial = _selectionRingMaterial;
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = index * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(index, new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        internal static float ResolveRingRadius(
            GameObject unitObject,
            UnitClassConfig config = null)
        {
            Renderer[] renderers = unitObject.GetComponentsInChildren<Renderer>(true);
            float markerScale = ResolveMarkerScale(config);
            if (renderers.Length == 0)
                return 0.55f * markerScale;

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
                bounds.Encapsulate(renderers[index].bounds);

            Vector3 localExtents = unitObject.transform.InverseTransformVector(
                bounds.extents);
            float radius = Mathf.Max(
                0.4f,
                Mathf.Max(Mathf.Abs(localExtents.x), Mathf.Abs(localExtents.z)) * 1.2f);
            return radius * markerScale;
        }

        internal static float ResolveMarkerScale(UnitClassConfig config)
            => config?.ResolvePresentation()?.ResolveSelectionMarkerScale() ?? 1f;

        private UnitClassConfig ResolveConfig(string unitId)
        {
            if (_unitClassConfig == null || string.IsNullOrWhiteSpace(unitId))
                return null;

            string unitTypeId = _unitService.GetUnitTypeId(unitId);
            return string.IsNullOrWhiteSpace(unitTypeId)
                ? null
                : _unitClassConfig.GetConfig(unitTypeId);
        }

        private void ClearSelectionRing()
        {
            if (_selectionRing != null)
                UnityEngine.Object.Destroy(_selectionRing);
            if (_selectionRingMaterial != null)
                UnityEngine.Object.Destroy(_selectionRingMaterial);
            _selectionRing = null;
            _selectionRingMaterial = null;
        }
    }
}
