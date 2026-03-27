using UnityEngine;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Units.Runtime
{
    [CreateAssetMenu(fileName = "UnitRegistry", menuName = "Moyva/Units/UnitRegistry")]
    public class UnitRegistrySO : ScriptableObject
    {
        public List<UnitClassConfig> Configs;
    }
}