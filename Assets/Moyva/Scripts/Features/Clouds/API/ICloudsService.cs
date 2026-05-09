namespace Kruty1918.Moyva.Clouds.API
{
    public interface ICloudsService
    {
        int ActiveCloudsCount { get; }
        void ClearClouds();
        void SpawnCloud();
    }
}