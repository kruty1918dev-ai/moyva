using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/BiomesSettings", fileName = "DataBiomesSettings")]
    public class DataBiomesSettings : ScriptableObject
    {
        [Tooltip("Список правил біомів. Кожен елемент описує, який Tile ID треба поставити, якщо клітинка потрапляє в заданий діапазон висоти та вологості. Порядок важливий: перший відповідний запис буде застосований раніше за наступні.")]
        public BiomeData[] Biomes;

        [Tooltip("Тайл за замовчуванням для всіх клітинок, які не потрапили ні в один біом. Використовується як безпечний фолбек, щоб карта не залишалась з порожніми або невизначеними тайлами.")]
        [TileId] public string DefaultTileID = "grass";
        [Tooltip("Масштаб шуму вологості. Менші значення дають дрібні плями вологості, більші формують великі області сухих і вологих зон. Впливає на те, наскільки часто перемикаються біоми при однаковій висоті.")]
        public float MoistureScale = 1.0f;
    }
}