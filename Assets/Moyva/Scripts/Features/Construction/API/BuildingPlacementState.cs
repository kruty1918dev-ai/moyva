namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingPlacementState
    {
        Idle,       // Будівля не вибрана, сесія не активна
        Placing,    // Гравець розставляє будівлі (вибрана будівля)
        Confirmed   // Усі pending-розміщення підтверджено (скидається до Idle)
    }
}
