using System;

namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>
    /// Marks an ITelemetryEvent implementation for catalog/editor tooling.
    /// Runtime never reflects over this attribute — it exists for Editor
    /// catalog generation and code-gen tooling only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TelemetryEventAttribute : Attribute
    {
        public string ContractId { get; }
        public int Version { get; }
        public string Description { get; set; }
        public string Producer { get; set; }

        public TelemetryEventAttribute(string contractId, int version)
        {
            ContractId = contractId;
            Version = version;
        }
    }
}
