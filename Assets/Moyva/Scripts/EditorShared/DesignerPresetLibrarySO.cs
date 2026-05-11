using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    [CreateAssetMenu(menuName = "Moyva/Designers/Designer Preset Library", fileName = "DesignerPresetLibrary")]
    public sealed class DesignerPresetLibrarySO : ScriptableObject
    {
        public List<UnitDesignerPreset> UnitPresets = new List<UnitDesignerPreset>();
        public List<BuildingDesignerPreset> BuildingPresets = new List<BuildingDesignerPreset>();
        public List<FogDesignerPreset> FogPresets = new List<FogDesignerPreset>();
        public List<EconomyDesignerPreset> EconomyPresets = new List<EconomyDesignerPreset>();
    }

    [Serializable]
    public sealed class UnitDesignerPreset
    {
        public string Name = "Unit Preset";
        public UnitClassConfig Template = new UnitClassConfig();
    }

    [Serializable]
    public sealed class BuildingDesignerPreset
    {
        public string Name = "Building Preset";
        public BuildingDefinition Template = new BuildingDefinition();
    }

    [Serializable]
    public sealed class FogDesignerPreset
    {
        public string Name = "Fog Preset";
        public FogOfWarSettings Template;
    }

    [Serializable]
    public sealed class EconomyDesignerPreset
    {
        public string Name = "Economy Preset";
        public EconomyDatabaseSO Template;
    }
}
