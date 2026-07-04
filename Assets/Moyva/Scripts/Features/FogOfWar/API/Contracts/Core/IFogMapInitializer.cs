namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за ініціалізацію runtime fog map.
    /// </summary>
    public interface IFogMapInitializer
    {
        void Initialize(int width, int height);
    }
}
