using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Shared.Localization;
using NUnit.Framework;
using UnityEngine;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    // P077: the world tooltip adapter feeds the shared HTML tooltip layer —
    // building hover shows a short localized name at the pointer, while UI
    // hovers, preview visuals and empty space clear it.
    public sealed class GameplayWorldTooltipServiceTests
    {
        private GameObject _targetGo;
        private ConstructionBuildingPointerTarget _target;

        [TearDown]
        public void TearDown()
        {
            if (_targetGo != null) UnityEngine.Object.DestroyImmediate(_targetGo);
        }

        [Test]
        public void HoverOverBuilding_ShowsLocalizedNameAtPointer()
        {
            var host = new RecordingHost();
            var pointer = new StubPointer(new Vector2(320, 240));
            var service = new GameplayWorldTooltipService(
                host, pointer, inputPolicy: null,
                buildings: new StubRegistry("mill", "Water Mill"),
                localization: new EchoLocalization());
            _targetGo = new GameObject("mill visual");
            _target = ConstructionBuildingPointerTarget.AttachOrUpdate(_targetGo, "mill", Vector2Int.zero, false);
            service.RaycastTarget = _ => _target;

            service.Tick();

            Assert.That(host.WorldText, Is.EqualTo("Water Mill"));
            Assert.That(host.WorldPosition, Is.EqualTo(new Vector2(320, 240)));
        }

        [Test]
        public void HoverOverUi_ClearsTooltip()
        {
            var host = new RecordingHost();
            var policy = new StubPolicy { OverUi = true };
            var service = new GameplayWorldTooltipService(
                host, new StubPointer(new Vector2(10, 10)), policy);
            service.RaycastTarget = _ => throw new InvalidOperationException("must not raycast over UI");

            service.Tick();

            Assert.That(host.WorldText, Is.Null);
        }

        [Test]
        public void PreviewVisual_ProducesNoTooltip()
        {
            var host = new RecordingHost();
            var service = new GameplayWorldTooltipService(
                host, new StubPointer(Vector2.one * 50), inputPolicy: null,
                buildings: new StubRegistry("mill", "Water Mill"));
            _targetGo = new GameObject("preview");
            _target = ConstructionBuildingPointerTarget.AttachOrUpdate(_targetGo, "mill", Vector2Int.zero, true);
            service.RaycastTarget = _ => _target;

            service.Tick();

            Assert.That(host.WorldText, Is.Null);
        }

        [Test]
        public void EmptyHit_AndUnknownBuilding_ClearOrFallBack()
        {
            var host = new RecordingHost();
            var service = new GameplayWorldTooltipService(
                host, new StubPointer(Vector2.one * 50), inputPolicy: null,
                buildings: new StubRegistry("mill", "Water Mill"));
            service.RaycastTarget = _ => null;

            service.Tick();
            Assert.That(host.WorldText, Is.Null);

            _targetGo = new GameObject("unknown visual");
            _target = ConstructionBuildingPointerTarget.AttachOrUpdate(_targetGo, "mystery", Vector2Int.zero, false);
            service.RaycastTarget = _ => _target;

            service.Tick();
            Assert.That(host.WorldText, Is.EqualTo("mystery"));
        }

        [Test]
        public void NoPointer_ClearsTooltip()
        {
            var host = new RecordingHost();
            var service = new GameplayWorldTooltipService(
                host, new StubPointer(null), inputPolicy: null);

            service.Tick();

            Assert.That(host.WorldText, Is.Null);
        }

        private sealed class StubPointer : IConstructionPointerInputSource
        {
            private readonly ConstructionPointerSnapshot _snapshot;
            public StubPointer(Vector2? position)
            {
                _snapshot = position.HasValue
                    ? new ConstructionPointerSnapshot(true, false, false, false,
                        position.Value, 0, 1, ConstructionPointerDeviceKind.Mouse, false)
                    : ConstructionPointerSnapshot.None;
            }
            public ConstructionPointerSnapshot ReadPointerSnapshot() => _snapshot;
        }

        private sealed class StubPolicy : IGameplayInputPolicy
        {
            public bool OverUi;
            public bool CanProcess(GameplayInputKind inputKind, Vector2 screenPosition, int pointerId = -1) => true;
            public bool IsPointerOverUi(Vector2 screenPosition, int pointerId = -1, bool interactiveOnly = true) => OverUi;
            public bool TryBeginPointerCapture(GameplayInputKind inputKind, Vector2 screenPosition, int pointerId = -1) => false;
            public void EndPointerCapture(GameplayInputKind inputKind, int pointerId = -1) { }
            public IDisposable AcquireBlock(GameplayInputKind inputMask, object owner) => null;
        }

        private sealed class StubRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _definition;
            public StubRegistry(string id, string displayName)
                => _definition = new BuildingDefinition { Id = id, DisplayName = displayName };
            public BuildingDefinition[] GetAll() => new[] { _definition };
            public BuildingDefinition GetById(string id) => id == _definition.Id ? _definition : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category) => new[] { _definition };
            public WallCollectionDefinition[] GetWallCollections() => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }

        private sealed class EchoLocalization : ILocalizationService
        {
            public string CurrentLanguageId => "en";
            public LocalizationLanguage CurrentLanguage => default;
            public IReadOnlyList<LocalizationLanguage> SupportedLanguages => Array.Empty<LocalizationLanguage>();
            public int CurrentLanguageIndex => 0;
            public event Action LanguageChanged { add { } remove { } }
            public string T(string key) => key;
            public string TF(string key, params object[] args) => key;
            public string TN(string oneKey, string pluralKey, int count) => count == 1 ? oneKey : pluralKey;
            public bool TrySetLanguage(string languageId) => true;
        }

        private sealed class RecordingHost : IUnityHtmlHost
        {
            public string WorldText;
            public Vector2 WorldPosition;
            public IUnityHtmlMotion Motion => null;
            public UnityHtmlScrollSettings ScrollSettings { get; set; }
            public UnityHtmlMountResult Mount(RectTransform root, UnityHtmlDocument document,
                IReadOnlyDictionary<string, object> globals = null) => UnityHtmlMountResult.Success();
            public void Unmount() { }
            public bool UpdateRegion(string elementId, string html) => true;
            public bool UpdateRegions(IReadOnlyDictionary<string, string> regions,
                IReadOnlyDictionary<string, object> globals = null) => true;
            public bool SetValue(string elementId, string value) => true;
            public void SetWorldTooltip(string text, Vector2 screenPosition)
            {
                WorldText = text;
                WorldPosition = screenPosition;
            }
            public void Dispose() { }
        }
    }
}
