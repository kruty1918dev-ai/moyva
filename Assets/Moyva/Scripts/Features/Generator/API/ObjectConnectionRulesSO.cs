using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public enum ObjectAutoTilePreset
    {
        Empty = 0,
        River = 1,
        Road = 2,
    }

    /// <summary>
    /// 4-бітна маска сусідів: N=1, E=2, S=4, W=8
    /// Визначає який варіант об'єкта використовувати залежно від оточення.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Generator/ObjectConnectionRules", fileName = "ObjectConnectionRules")]
    public class ObjectConnectionRulesSO : ScriptableObject
    {
        [Tooltip("Групи правил автотайлінгу для об'єктів. Кожна група описує, як базовий об'єкт має змінюватися залежно від сусідів, наприклад для річок, доріг або огорож.")]
        public List<ObjectAutoTileGroup> Groups = new();

        public void ApplyPreset(ObjectAutoTilePreset preset)
        {
            switch (preset)
            {
                case ObjectAutoTilePreset.Empty:
                    Groups.Clear();
                    break;
                case ObjectAutoTilePreset.River:
                    ApplyLinePreset("river");
                    break;
                case ObjectAutoTilePreset.Road:
                    ApplyLinePreset("road");
                    break;
            }
        }

        public void ApplyLinePreset(string baseId)
        {
            string id = string.IsNullOrWhiteSpace(baseId) ? "object" : baseId.Trim();

            Groups.Clear();
            Groups.Add(new ObjectAutoTileGroup
            {
                BaseObjectId = id,
                FallbackId = id,
                Variants = new List<ObjectAutoTileVariant>
                {
                    new ObjectAutoTileVariant { NeighborMask = 5, VariantId = id + "-vertical" },
                    new ObjectAutoTileVariant { NeighborMask = 10, VariantId = id + "-horizontal" },

                    new ObjectAutoTileVariant { NeighborMask = 3, VariantId = id + "-corner-ne" },
                    new ObjectAutoTileVariant { NeighborMask = 6, VariantId = id + "-corner-se" },
                    new ObjectAutoTileVariant { NeighborMask = 12, VariantId = id + "-corner-sw" },
                    new ObjectAutoTileVariant { NeighborMask = 9, VariantId = id + "-corner-nw" },

                    new ObjectAutoTileVariant { NeighborMask = 7, VariantId = id + "-t-east" },
                    new ObjectAutoTileVariant { NeighborMask = 11, VariantId = id + "-t-north" },
                    new ObjectAutoTileVariant { NeighborMask = 13, VariantId = id + "-t-west" },
                    new ObjectAutoTileVariant { NeighborMask = 14, VariantId = id + "-t-south" },

                    new ObjectAutoTileVariant { NeighborMask = 15, VariantId = id + "-cross" },

                    new ObjectAutoTileVariant { NeighborMask = 1, VariantId = id + "-end-n" },
                    new ObjectAutoTileVariant { NeighborMask = 2, VariantId = id + "-end-e" },
                    new ObjectAutoTileVariant { NeighborMask = 4, VariantId = id + "-end-s" },
                    new ObjectAutoTileVariant { NeighborMask = 8, VariantId = id + "-end-w" },
                }
            });
        }

        public bool TryResolve(string baseId, int neighborMask, out string variantId)
        {
            foreach (var group in Groups)
            {
                if (group.BaseObjectId != baseId) continue;

                foreach (var variant in group.Variants)
                {
                    if (variant.NeighborMask == neighborMask)
                    {
                        variantId = variant.VariantId;
                        return true;
                    }
                }

                variantId = group.FallbackId;
                return !string.IsNullOrEmpty(variantId);
            }

            variantId = null;
            return false;
        }
    }

    [Serializable]
    public class ObjectAutoTileGroup
    {
        [Tooltip("Базовий ID об'єкта, для якого застосовується ця група правил. Саме його шукає нода автотайлінгу перед підбором конкретного варіанта.")]
        [MapObjectId] public string BaseObjectId;
        [Tooltip("Фолбек-варіант, який буде використано, якщо для конкретної маски сусідів не знайдено окреме правило. Дає змогу уникнути пустих клітинок.")]
        [MapObjectId] public string FallbackId;
        [Tooltip("Список конкретних варіантів об'єкта для різних поєднань сусідів. Тут задаються прямі сегменти, кути, трійники, хрести та інші форми.")]
        public List<ObjectAutoTileVariant> Variants = new();
    }

    [Serializable]
    public class ObjectAutoTileVariant
    {
        [Tooltip("4-бітна маска сусідів: N=1, E=2, S=4, W=8. Саме за цією маскою система визначає, коли потрібно використати цей варіант замість базового об'єкта.")]
        [Range(0, 15)]
        public int NeighborMask;
        [Tooltip("ID конкретного варіанта об'єкта, який слід підставити, якщо NeighborMask збігається з оточенням клітинки.")]
        [MapObjectId] public string VariantId;
    }
}
