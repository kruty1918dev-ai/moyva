using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public class BuildingConfig
    {
        [Tooltip("Унікальний ідентифікатор типу будівлі. Без підкреслень — використовуй дефіси або camelCase.")]
        public string TypeId;
        public string Name;
        public BuildingCategory Category;
        public GameObject Prefab;
        public Vector2Int Size = Vector2Int.one;
        public bool IsWall;
    }
}
