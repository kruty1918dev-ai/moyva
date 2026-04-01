using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Moyva/Construction/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject
    {
        [SerializeField] private List<BuildingConfig> _configs = new();
        public IReadOnlyList<BuildingConfig> Configs => _configs;
    }
}
