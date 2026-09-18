namespace Kruty1918.Moyva.Combat.API
{
    public readonly struct CombatCommandPreview
    {
        public CombatCommandPreview(
            string attackerEntityId,
            string targetEntityId,
            int expectedDamage,
            bool targetWouldDie)
        {
            AttackerEntityId = attackerEntityId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            ExpectedDamage = expectedDamage < 0 ? 0 : expectedDamage;
            TargetWouldDie = targetWouldDie;
        }

        public string AttackerEntityId { get; }
        public string TargetEntityId { get; }
        public int ExpectedDamage { get; }
        public bool TargetWouldDie { get; }
    }

    public readonly struct CombatCommandResult
    {
        public CombatCommandResult(
            bool succeeded,
            string attackerEntityId,
            string targetEntityId,
            int damageApplied,
            bool targetDied,
            string reason)
        {
            Succeeded = succeeded;
            AttackerEntityId = attackerEntityId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            DamageApplied = damageApplied < 0 ? 0 : damageApplied;
            TargetDied = targetDied;
            Reason = reason ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string AttackerEntityId { get; }
        public string TargetEntityId { get; }
        public int DamageApplied { get; }
        public bool TargetDied { get; }
        public string Reason { get; }

        public static CombatCommandResult Rejected(
            string attackerEntityId,
            string targetEntityId,
            string reason)
            => new(false, attackerEntityId, targetEntityId, 0, false, reason);
    }
}
