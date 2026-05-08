namespace Kruty1918.Moyva.HomeMenu.API
{
    public struct PlayerUIWrapper
    {
        public string PlayerId { get; }
        public string Nickname { get; }

        public PlayerUIWrapper(string playerId, string nickname)
        {
            PlayerId = playerId;
            Nickname = nickname;
        }
    }
}