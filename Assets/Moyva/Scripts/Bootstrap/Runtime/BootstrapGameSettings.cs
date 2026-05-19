using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [Serializable]
    public sealed class InitialResourceEntry
    {
        [ResourceIdAttribute]
        [SerializeField]
        public string ResourceId = string.Empty;

        [Min(0f)]
        [SerializeField]
        public float Amount = 10f;

        public InitialResourceEntry() { }

        public InitialResourceEntry(string resourceId, float amount)
        {
            ResourceId = resourceId;
            Amount = amount;
        }
    }

    [Serializable]
    public sealed class BootstrapGameSettings
    {
        [Header("Initial Resources")]
        [Tooltip("Список ресурсів, які гравець отримує на старт нової гри.")]
        [SerializeField]
        public List<InitialResourceEntry> InitialResources = new()
        {
            new InitialResourceEntry("food", 50f),
            new InitialResourceEntry("wood", 30f),
        };

        public BootstrapGameSettings() { }
    }
}
