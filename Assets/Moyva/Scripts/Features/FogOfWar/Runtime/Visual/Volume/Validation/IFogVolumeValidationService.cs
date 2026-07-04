namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Ізолює inspector/runtime validation rules від MonoBehaviour host-а.
    /// </summary>
    internal interface IFogVolumeValidationService
    {
        /// <summary>
        /// Формує коротке текстове резюме валідності поточного fog volume setup.
        /// </summary>
        /// <param name="host">Host-компонент, який надає settings і manager.</param>
        /// <returns>Людинозрозуміле повідомлення для inspector-а.</returns>
        string BuildValidationSummary(IFogVolumeValidationHost host);
    }
}
