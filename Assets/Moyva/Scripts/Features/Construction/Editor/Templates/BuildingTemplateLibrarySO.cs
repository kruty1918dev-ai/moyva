using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Templates/Building Template Library", fileName = "BuildingTemplateLibrary")]
    public sealed class BuildingTemplateLibrarySO : ScriptableObject
    {
        [FolderPath(RequireExistingPath = true)]
        public string DefaultOutputFolder = "Assets/Moyva/SO/Construction/Buildings";

        [AssetsOnly]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<BuildingArchetypeSO> Archetypes = new List<BuildingArchetypeSO>();
    }
}
