using Kruty1918.Moyva.Construction.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingMigrationWindow : OdinEditorWindow
    {
        [AssetsOnly]
        [Required]
        public BuildingRegistrySO Registry;

        [FolderPath(RequireExistingPath = false)]
        public string OutputFolder = "Assets/Moyva/SO/Construction/Buildings";

        public bool AddLegacyFogRevealModules = true;

        [ShowInInspector]
        [ReadOnly]
        public string LastReport { get; private set; }

        [MenuItem("Moyva/Tools/Construction/Building Migration", priority = 33)]
        public static void Open()
        {
            GetWindow<BuildingMigrationWindow>("Building Migration").Show();
        }

        [Button(ButtonSizes.Large)]
        public void Migrate()
        {
            var report = BuildingMigrationUtility.MigrateLegacyRegistry(Registry, OutputFolder, AddLegacyFogRevealModules);
            LastReport = report.ToString() + "\n" + string.Join("\n", report.Messages);
            Debug.Log($"[BuildingMigration] {LastReport}");
        }
    }
}
