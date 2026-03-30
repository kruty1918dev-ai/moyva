using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/GenerationRules", fileName = "GenerationRules")]
    public class GenerationRules : ScriptableObject
    {
        public bool GenerateRivers = true;
        public bool GenerateBiomes = true;
        public bool ApplyWFC = true;
    }
}