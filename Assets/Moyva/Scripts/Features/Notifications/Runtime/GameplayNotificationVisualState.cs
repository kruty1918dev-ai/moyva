namespace Kruty1918.Moyva.Notifications.Runtime
{
    internal readonly struct GameplayNotificationVisualState
    {
        public GameplayNotificationVisualState(
            bool isActive,
            float alpha,
            float anchoredPositionY)
        {
            IsActive = isActive;
            Alpha = alpha;
            AnchoredPositionY = anchoredPositionY;
        }

        public bool IsActive { get; }
        public float Alpha { get; }
        public float AnchoredPositionY { get; }
    }
}
