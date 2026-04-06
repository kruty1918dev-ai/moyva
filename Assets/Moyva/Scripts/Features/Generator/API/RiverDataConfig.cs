using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/RiverDataConfig", fileName = "RiverDataConfig")]
    public class RiverDataConfig : ScriptableObject
    {
        [Tooltip("ID об'єкта, яким буде промальовано русло річки в ObjectMap.")]
        [MapObjectId] public string BaseObjectId = "river";

        [Tooltip("Скільки річок генератор спробує побудувати на карті.")]
        [Min(1)] public int RiversCount = 1;

        [Header("Path Diversity")]
        [Tooltip("Штраф за проходження клітинкою, яку вже використовувала інша річка. Більше значення = рідше прямі перетини.")]
        [Range(0f, 500f)] public float UsedCellPenalty = 120f;

        [Tooltip("Додатковий штраф за близькість до вже прокладених русел у вказаному радіусі.")]
        [Range(0f, 200f)] public float NearRiverPenalty = 35f;

        [Tooltip("Радіус для штрафу близькості до існуючих річок.")]
        [Range(0, 4)] public int NearRiverRadius = 1;
    }
}