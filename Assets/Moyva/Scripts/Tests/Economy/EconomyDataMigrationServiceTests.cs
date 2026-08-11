#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Editor;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyDataMigrationServiceTests
    {
        [Test]
        public void Migrate_ShouldUpgradeFromV1ToCurrent()
        {
            var database = MoyvaJsonObjectFactory.Create<EconomyDatabaseSO>();
            database.SchemaVersion = 1;

            var service = new EconomyDataMigrationService();
            var report = service.Migrate(database);

            Assert.IsTrue(report.Changed);
            Assert.AreEqual(EconomySchema.CurrentVersion, database.SchemaVersion);
            Assert.IsTrue(report.Steps.Exists(step => step.Contains("1 -> 2")));
        }

        [Test]
        public void Migrate_ShouldKeepCurrentVersion_WhenAlreadyCurrent()
        {
            var database = MoyvaJsonObjectFactory.Create<EconomyDatabaseSO>();
            database.SchemaVersion = EconomySchema.CurrentVersion;

            var service = new EconomyDataMigrationService();
            var report = service.Migrate(database);

            Assert.IsFalse(report.Changed);
            Assert.AreEqual(EconomySchema.CurrentVersion, report.ToVersion);
        }
    }
}

#endif
