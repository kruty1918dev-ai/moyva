using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEditor;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class DesignerPresetApplier
    {
        public static bool ApplyUnitPreset(UnitDesignerPreset preset, UnitClassConfig target)
        {
            if (preset == null || preset.Template == null || target == null)
                return false;

            string preservedTypeId = target.TypeId;
            EditorUtility.CopySerializedManagedFieldsOnly(preset.Template, target);
            target.TypeId = preservedTypeId;
            return true;
        }

        public static bool ApplyBuildingPreset(BuildingDesignerPreset preset, BuildingDefinition target)
        {
            if (preset == null || preset.Template == null || target == null)
                return false;

            string preservedId = target.Id;
            EditorUtility.CopySerializedManagedFieldsOnly(preset.Template, target);
            target.Id = preservedId;
            return true;
        }

        public static bool ApplyFogPreset(FogDesignerPreset preset, FogOfWarSettings target)
        {
            if (preset == null || preset.Template == null || target == null)
                return false;

            EditorUtility.CopySerialized(preset.Template, target);
            return true;
        }

        public static bool ApplyEconomyPreset(EconomyDesignerPreset preset, EconomyDatabaseSO target)
        {
            if (preset == null || preset.Template == null || target == null)
                return false;

            EditorUtility.CopySerialized(preset.Template, target);
            return true;
        }
    }
}
