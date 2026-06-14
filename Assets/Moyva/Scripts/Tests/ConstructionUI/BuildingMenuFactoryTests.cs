using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.UI;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.ConstructionUI
{
    [TestFixture]
    public class BuildingMenuFactoryTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _createdObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void BuildMenuItems_ShouldSortBuildingsInsideCategory()
        {
            var factory = new BuildingMenuFactory();
            var definitions = new[]
            {
                CreateDefinition("b2", "Ярмарок", BuildingCategory.Civilian),
                CreateDefinition("b1", "Кузня", BuildingCategory.Civilian),
                CreateDefinition("m1", "Вежа", BuildingCategory.Military),
            };

            var result = factory.BuildMenuItems(definitions, null, null);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Вежа", result[0].DisplayName);
            Assert.AreEqual("Кузня", result[1].DisplayName);
            Assert.AreEqual("Ярмарок", result[2].DisplayName);
        }

        [Test]
        public void ExtractSpriteForMenu_ShouldUsePrefabSpriteRenderer()
        {
            var factory = new BuildingMenuFactory();
            var prefab = CreatePrefabWithSprite("militia");
            var iconFallback = CreateSprite(4, 4);

            var definition = new BuildingDefinition
            {
                Id = "militia",
                DisplayName = "Ополчення",
                Category = BuildingCategory.Military,
                Prefab = prefab,
                Icon = iconFallback
            };

            var sprite = factory.ExtractSpriteForMenu(definition, null, null);

            Assert.IsNotNull(sprite);
            Assert.AreNotSame(iconFallback, sprite);
        }

        [Test]
        public void ExtractSpriteForMenu_ShouldFallbackToIcon_WhenPrefabHasNoSprite()
        {
            var factory = new BuildingMenuFactory();
            var prefab = new GameObject("empty-prefab");
            _createdObjects.Add(prefab);
            var iconFallback = CreateSprite(8, 8);

            var definition = new BuildingDefinition
            {
                Id = "empty",
                DisplayName = "Порожньо",
                Category = BuildingCategory.Industrial,
                Prefab = prefab,
                Icon = iconFallback
            };

            var sprite = factory.ExtractSpriteForMenu(definition, null, null);

            Assert.AreSame(iconFallback, sprite);
        }

        [Test]
        public void BuildMenuItems_ShouldUseRuntimePreview_WhenCachedPreviewExists()
        {
            var factory = new BuildingMenuFactory();
            var iconFallback = CreateSprite(4, 4);
            var runtimePreview = CreateSprite(32, 32);
            var definition = new BuildingDefinition
            {
                Id = "workshop",
                DisplayName = "Майстерня",
                Category = BuildingCategory.Industrial,
                Icon = iconFallback,
                RuntimePreview = runtimePreview,
            };

            var result = factory.BuildMenuItems(new[] { definition }, null, null);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(runtimePreview, result[0].PreviewSprite);
        }

        private BuildingDefinition CreateDefinition(string id, string name, BuildingCategory category)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = name,
                Category = category,
                Icon = CreateSprite(2, 2),
                Prefab = null
            };
        }

        private GameObject CreatePrefabWithSprite(string objectName)
        {
            var go = new GameObject(objectName);
            _createdObjects.Add(go);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSprite(16, 16);
            return go;
        }

        private Sprite CreateSprite(int width, int height)
        {
            var texture = new Texture2D(width, height);
            _createdObjects.Add(texture);

            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            _createdObjects.Add(sprite);
            return sprite;
        }
    }
}
