#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Construction.Editor
{
[System.Serializable]
public sealed class BuildingTemplateLibrarySO : MoyvaJsonConfigObject
    {
        [FolderPath(RequireExistingPath = true)]
        public string DefaultOutputFolder = "Assets/Moyva/Data/ScriptableObjects/Construction/Buildings";

        [AssetsOnly]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<BuildingArchetypeSO> Archetypes = new List<BuildingArchetypeSO>();
    }
}

#endif
