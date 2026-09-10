namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyResidentState
    {
        public EconomyResidentState(
            int age,
            float hp,
            float comfort,
            bool houseCollapsed,
            string professionId = null,
            long recruitmentQueueId = 0,
            string militaryUnitId = null)
        {
            RecruitmentQueueId = recruitmentQueueId;
            MilitaryUnitId = militaryUnitId ?? string.Empty;
            Age = age;
            Hp = hp;
            Comfort = comfort;
            HouseCollapsed = houseCollapsed;
            ProfessionId = professionId?.Trim();
        }

        public long RecruitmentQueueId { get; }
        public string MilitaryUnitId { get; }
        public bool IsCivilian => RecruitmentQueueId == 0 && string.IsNullOrEmpty(MilitaryUnitId);
        public bool CanRecruit => IsCivilian && Age >= 16 && Age < 60 && Hp > 0f;
        public EconomyResidentState WithMilitaryAssignment(long queueId, string unitId = null)
            => new EconomyResidentState(Age, Hp, Comfort, HouseCollapsed, ProfessionId, queueId, unitId);
        public int Age { get; }
        public float Hp { get; }
        public float Comfort { get; }
        public bool HouseCollapsed { get; }
        public string ProfessionId { get; }
    }
}
