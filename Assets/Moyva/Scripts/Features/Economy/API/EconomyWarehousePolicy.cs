using System;
using System.Collections.Generic;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Economy.API
{
    [Serializable]
    public sealed class EconomyWarehousePolicyEntry
    {
        [SerializeField] private string _resourceId;
        [SerializeField] private bool _consumptionAllowed = true;
        [SerializeField] private int _priority = 1;
        [SerializeField] private int _reserveAmount;

        public string ResourceId => _resourceId;
        public bool ConsumptionAllowed => _consumptionAllowed;
        public int Priority => _priority;
        public int ReserveAmount => _reserveAmount;
    }
[System.Serializable]
public sealed class EconomyWarehousePolicy : MoyvaJsonConfigObject
    {
        [SerializeField] private EconomyWarehouseType _warehouseType;
        [SerializeField] private List<EconomyWarehousePolicyEntry> _entries = new List<EconomyWarehousePolicyEntry>();

        public EconomyWarehouseType WarehouseType => _warehouseType;
        public IReadOnlyList<EconomyWarehousePolicyEntry> Entries => _entries;
    }
}
