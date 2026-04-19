using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Налаштування для превью генерації в Graph Editor.
    /// Створіть через: Assets → Create → Moyva/Generator/Editor Preview Settings.
    /// Прив'яжіть ScriptableObject-и з налаштуваннями генератора,
    /// щоб граф міг використовувати сервіси (NoiseProvider, BiomeResolver тощо)
    /// без запуску гри.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Generator/Editor Preview Settings",
        fileName = "EditorPreviewSettings")]
    public sealed class EditorPreviewSettings : ScriptableObject
    {
        [Header("Preview Map Size")]
        [Tooltip("Ширина мапи для превью (у тайлах).")]
        [Min(4)]
        [SerializeField] private int _previewWidth = 64;

        [Tooltip("Висота мапи для превью (у тайлах).")]
        [Min(4)]
        [SerializeField] private int _previewHeight = 64;

        [Header("Generator Settings")]
        [Tooltip("Налаштування шуму (Perlin noise). Потрібно для HeightSourceNode.")]
        [SerializeField] private DataNoiseSettings _noiseSettings;

        [Tooltip("Налаштування висотної карти. Потрібно для HeightToTileNode.")]
        [SerializeField] private HeightMapSettings _heightMapSettings;

        [Tooltip("Налаштування біомів. Потрібно для BiomeResolverNode.")]
        [SerializeField] private DataBiomesSettings _biomesSettings;

        [Tooltip("Налаштування WFC. Потрібно для WFC-вузлів.")]
        [SerializeField] private WFCDataSettings _wfcDataSettings;

        [Header("Tile Visual Lookup")]
        [Tooltip("Реєстр тайлів для візуального превью (спрайти).")]
        [SerializeField] private TileRegistrySO _tileRegistry;

        [Header("Object & Building Registries")]
        [Tooltip("Реєстр об'єктів карти (дерева, каміння, річки тощо). Потрібен для композитного превью.")]
        [SerializeField] private MapObjectRegistrySO _mapObjectRegistry;

        [Tooltip("Реєстр будівель. Потрібен для композитного превью.")]
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        public int PreviewWidth => _previewWidth;
        public int PreviewHeight => _previewHeight;
        public DataNoiseSettings NoiseSettings => _noiseSettings;
        public HeightMapSettings HeightMapSettings => _heightMapSettings;
        public DataBiomesSettings BiomesSettings => _biomesSettings;
        public WFCDataSettings WFCDataSettings => _wfcDataSettings;
        public TileRegistrySO TileRegistry => _tileRegistry;
        public MapObjectRegistrySO MapObjectRegistry => _mapObjectRegistry;
        public BuildingRegistrySO BuildingRegistry => _buildingRegistry;
    }
}
