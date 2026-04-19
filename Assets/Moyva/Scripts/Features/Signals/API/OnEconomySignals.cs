using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Надсилається EconomyManager після завершення економічного тіку (Population → Workers → Production).
    /// Отримується: UI (оновлення ресурсів), SaveSystem, AI.
    /// </summary>
    public struct EconomyTickCompletedSignal
    {
        public string SettlementId;
        public string OwnerId;
        public int Turn;
        public int TotalPopulation;
        public int Arrivals;
        public int Deaths;
        public int ProductionCyclesCompleted;
    }

    /// <summary>
    /// Надсилається EconomyManager при створенні нового поселення (ратуша побудована).
    /// Отримується: UI, мініатюра на карті, FogOfWar.
    /// </summary>
    public struct SettlementCreatedSignal
    {
        public string SettlementId;
        public string OwnerId;
        public Vector2Int TownHallPosition;
    }

    /// <summary>
    /// Надсилається EconomyManager при деактивації поселення (населення = 0 або ратуша знищена).
    /// Отримується: UI, освітлення, AI.
    /// </summary>
    public struct SettlementDeactivatedSignal
    {
        public string SettlementId;
        public string OwnerId;
        public string Reason;
    }

    /// <summary>
    /// Надсилається EconomyManager коли ресурс поселення змінюється суттєво (виробництво або споживання).
    /// Отримується: UI ресурсної панелі.
    /// </summary>
    public struct SettlementResourceChangedSignal
    {
        public string SettlementId;
        public string OwnerId;
        public string ResourceId;
        public float NewAmount;
        public float Delta;
    }

    /// <summary>
    /// Надсилається EconomyManager при дефіциті критичного ресурсу (їжа, вода, дрова).
    /// Отримується: UI (попередження), AI (реакція).
    /// </summary>
    public struct ResourceDeficitSignal
    {
        public string SettlementId;
        public string OwnerId;
        public string ResourceId;
    }
}
