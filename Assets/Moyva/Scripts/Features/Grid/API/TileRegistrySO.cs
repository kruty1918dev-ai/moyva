using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Опис одного типу тайла у реєстрі.
    /// Містить ідентифікатор, вагу руху та TileWorldCreator-дані.
    /// </summary>
    [System.Serializable]
    public class TileTypeDefinition
    {
        /// <summary>
        /// Унікальний ідентифікатор типу тайла.
        /// </summary>
        [SerializeField] private string _id;

        /// <summary>
        /// Вартість проходження тайла для логіки руху/патфайндингу.
        /// </summary>
        [SerializeField] private float _movementCost = 1f;

        /// <summary>
        /// TileWorldCreator preset, який відповідає цьому gameplay ID.
        /// </summary>
        [SerializeField] private TilePreset _tileWorldCreatorPreset;

        /// <summary>
        /// GUID blueprint layer у TWC configuration, якщо запис синхронізовано через wizard.
        /// </summary>
        [SerializeField] private string _tileWorldCreatorBlueprintLayerGuid;

        /// <summary>
        /// Назва blueprint layer у TWC configuration, використовується як fallback для GUID.
        /// </summary>
        [SerializeField] private string _tileWorldCreatorBlueprintLayerName;

        /// <summary>
        /// Legacy fallback prefab. Новий runtime terrain visual має створювати TileWorldCreator.
        /// </summary>
        [SerializeField] private GameObject _visualPrefab;

        /// <summary>
        /// Публічний доступ до унікального TileId.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Публічний доступ до ваги руху.
        /// </summary>
        public float MovementCost => _movementCost;

        /// <summary>
        /// TileWorldCreator preset, що використовується для побудови цього типу тайла.
        /// </summary>
        public TilePreset TileWorldCreatorPreset => _tileWorldCreatorPreset;

        /// <summary>
        /// GUID TWC blueprint layer для цього tile ID.
        /// </summary>
        public string TileWorldCreatorBlueprintLayerGuid => _tileWorldCreatorBlueprintLayerGuid;

        /// <summary>
        /// Назва TWC blueprint layer для цього tile ID.
        /// </summary>
        public string TileWorldCreatorBlueprintLayerName => _tileWorldCreatorBlueprintLayerName;

        /// <summary>
        /// Чи має цей запис TWC-прив'язку і не залежить від legacy prefab-візуалу.
        /// </summary>
        public bool UsesTileWorldCreator => _tileWorldCreatorPreset != null
            || !string.IsNullOrWhiteSpace(_tileWorldCreatorBlueprintLayerGuid)
            || !string.IsNullOrWhiteSpace(_tileWorldCreatorBlueprintLayerName);

        /// <summary>
        /// Legacy prefab для старого fallback-рендеру, якщо TWC вимкнений.
        /// </summary>
        public GameObject VisualPrefab => _visualPrefab;

        /// <summary>
        /// Префаб, з якого можна взяти висоту поверхні для placement logic.
        /// Спочатку використовується legacy override, далі TWC fill tile із preset.
        /// </summary>
        public GameObject SurfaceReferencePrefab
        {
            get
            {
                if (_visualPrefab != null)
                    return _visualPrefab;

                if (_tileWorldCreatorPreset == null)
                    return null;

                return _tileWorldCreatorPreset.gridtype == TilePreset.GridType.dual
                    ? _tileWorldCreatorPreset.DUALGRD_fillTile
                    : _tileWorldCreatorPreset.NRMGRD_fillTile;
            }
        }
    }

    /// <summary>
    /// ScriptableObject-реєстр усіх типів тайлів, доступних у проєкті.
    /// Є джерелом даних для сервісів сітки, генерації та візуалізації.
    /// </summary>
    [CreateAssetMenu(fileName = "TileRegistry", menuName = "Moyva/Grid/TileRegistry")]
    public class TileRegistrySO : ScriptableObject
    {
        /// <summary>
        /// Масив визначень тайлів, налаштований у інспекторі Unity.
        /// </summary>
        [SerializeField] private TileTypeDefinition[] _definitions;

        /// <summary>
        /// Публічний доступ до всіх визначень тайлів.
        /// </summary>
        public TileTypeDefinition[] Definitions => _definitions;
    }
}