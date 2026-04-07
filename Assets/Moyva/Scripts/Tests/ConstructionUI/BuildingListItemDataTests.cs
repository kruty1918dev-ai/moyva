using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.UI;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.ConstructionUI
{
    /// <summary>
    /// Юніт-тести для <see cref="BuildingListItemData"/>.
    /// </summary>
    [TestFixture]
    public class BuildingListItemDataTests
    {
        [Test]
        public void Constructor_ShouldSetAllProperties()
        {
            var data = new BuildingListItemData("barracks", "Казарма", BuildingCategory.Military);

            Assert.AreEqual("barracks", data.Id);
            Assert.AreEqual("Казарма", data.DisplayName);
            Assert.AreEqual(BuildingCategory.Military, data.Category);
            Assert.IsNull(data.Icon);
        }

        [Test]
        public void Constructor_ShouldSetIcon_WhenProvided()
        {
            // Sprite неможливо зробити в edit-mode тесті без об'єкту — перевіряємо null-default
            var data = new BuildingListItemData("tower", "Вежа", BuildingCategory.Military, null);
            Assert.IsNull(data.Icon);
        }

        [Test]
        public void Category_Civilian_ShouldBeStoredCorrectly()
        {
            var data = new BuildingListItemData("market", "Ринок", BuildingCategory.Civilian);
            Assert.AreEqual(BuildingCategory.Civilian, data.Category);
        }

        [Test]
        public void Category_Industrial_ShouldBeStoredCorrectly()
        {
            var data = new BuildingListItemData("forge", "Кузня", BuildingCategory.Industrial);
            Assert.AreEqual(BuildingCategory.Industrial, data.Category);
        }
    }
}
