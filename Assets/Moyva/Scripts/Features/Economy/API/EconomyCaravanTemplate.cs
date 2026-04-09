using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [CreateAssetMenu(menuName = "Moyva/Economy/Caravan Template", fileName = "EconomyCaravanTemplate")]
    public sealed class EconomyCaravanTemplate : ScriptableObject
    {
        [SerializeField] private string _templateId;
        [SerializeField] private List<string> _allowedResourceIds = new List<string>();
        [SerializeField] private int _capacity = 1;
        [SerializeField] private int _defaultPriority = 1;
        [SerializeField] private bool _useLoopDelivery;

        public string TemplateId => _templateId;
        public IReadOnlyList<string> AllowedResourceIds => _allowedResourceIds;
        public int Capacity => _capacity;
        public int DefaultPriority => _defaultPriority;
        public bool UseLoopDelivery => _useLoopDelivery;
    }
}
