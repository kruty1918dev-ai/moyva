namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>Privacy classification for contract fields and events.</summary>
    public enum PrivacyClass
    {
        /// <summary>Gameplay/system data, safe under default consent.</summary>
        Gameplay = 0,
        /// <summary>Pseudonymous identifiers (installation/session ids).</summary>
        Pseudonymous = 1,
        /// <summary>Extended research/ML consent required.</summary>
        Research = 2,
        /// <summary>Never collected automatically; reserved marker for audits.</summary>
        Restricted = 3,
    }
}
