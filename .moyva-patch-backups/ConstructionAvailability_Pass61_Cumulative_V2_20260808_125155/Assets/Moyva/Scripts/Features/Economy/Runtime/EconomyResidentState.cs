namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyResidentState
    {
        public EconomyResidentState(
            int age,
            float hp,
            float comfort,
            bool houseCollapsed,
            string professionId = null)
        {
            Age = age;
            Hp = hp;
            Comfort = comfort;
            HouseCollapsed = houseCollapsed;
            ProfessionId = professionId?.Trim();
        }

        public int Age { get; }
        public float Hp { get; }
        public float Comfort { get; }
        public bool HouseCollapsed { get; }
        public string ProfessionId { get; }
    }
}
