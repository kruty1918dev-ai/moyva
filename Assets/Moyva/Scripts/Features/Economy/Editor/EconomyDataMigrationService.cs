using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using UnityEditor;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyMigrationReport
    {
        public int FromVersion;
        public int ToVersion;
        public bool Changed;
        public List<string> Steps = new List<string>();
    }

    public sealed class EconomyDataMigrationService
    {
        public EconomyMigrationReport Migrate(EconomyDatabaseSO database)
        {
            var report = new EconomyMigrationReport
            {
                FromVersion = database == null ? 0 : database.SchemaVersion,
                ToVersion = database == null ? 0 : database.SchemaVersion,
            };

            if (database == null)
            {
                report.Steps.Add("Database is null. Migration skipped.");
                return report;
            }

            var version = database.SchemaVersion;
            if (version <= 0)
            {
                version = EconomySchema.InitialVersion;
                database.SchemaVersion = version;
                report.Changed = true;
                report.Steps.Add($"Normalized invalid schema version to {version}.");
            }

            if (version == 1 && EconomySchema.CurrentVersion >= 2)
            {
                MigrateV1ToV2(database, report);
                version = 2;
            }

            if (version < EconomySchema.CurrentVersion)
            {
                database.SchemaVersion = EconomySchema.CurrentVersion;
                report.Changed = true;
                report.Steps.Add($"Forced schema version to {EconomySchema.CurrentVersion}. No dedicated migration path found.");
            }

            if (version > EconomySchema.CurrentVersion)
                report.Steps.Add($"Database schema version {version} is newer than supported {EconomySchema.CurrentVersion}.");

            report.ToVersion = database.SchemaVersion;
            if (report.Changed)
                EditorUtility.SetDirty(database);

            return report;
        }

        private static void MigrateV1ToV2(EconomyDatabaseSO database, EconomyMigrationReport report)
        {
            database.SchemaVersion = 2;
            report.Changed = true;
            report.Steps.Add("Migrated 1 -> 2 (no-op data migration, schema marker updated).");
        }
    }
}
