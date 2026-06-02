using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Gameplay-метадані одного шару генератора (TileWorldCreator blueprint-шару).
    /// Ключем є <see cref="LayerId"/> — ідентифікатор шару графа генератора.
    /// TileWorldCreator не зберігає прохідність/вартість руху, тому ці дані живуть тут.
    /// </summary>
    [System.Serializable]
    public class TerrainLayerProfile
    {
        [Tooltip("Ідентифікатор шару генератора (GeneratorLayerDefinition.Id) або його назва.")]
        [SerializeField] private string _layerId;

        [Tooltip("Людиночитна назва шару (для зручності в інспекторі).")]
        [SerializeField] private string _displayName;

        [Tooltip("Чи можна ходити по тайлах цього шару.")]
        [SerializeField] private bool _walkable = true;

        [Tooltip("Вартість проходження тайла для патфайндингу. 0 = непрохідний.")]
        [SerializeField] private float _movementCost = 1f;

        [Tooltip("Чи заборонено будувати на тайлах цього шару.")]
        [SerializeField] private bool _buildBlocked;

        [Tooltip("Зсув висоти поверхні для розміщення юнітів/будівель.")]
        [SerializeField] private float _surfaceOffset;

        public string LayerId => _layerId;
        public string DisplayName => _displayName;
        public bool Walkable => _walkable;
        public float MovementCost => _movementCost;
        public bool BuildBlocked => _buildBlocked;
        public float SurfaceOffset => _surfaceOffset;
    }

    /// <summary>
    /// Реєстр gameplay-профілів шарів. Замінює залежність від класичного TileRegistry:
    /// сітка зберігає id шару у клітинці, а цей ассет визначає правила руху/будівництва.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainLayerProfiles", menuName = "Moyva/Grid/Terrain Layer Profiles")]
    public class TerrainLayerProfileSO : ScriptableObject
    {
        [SerializeField] private TerrainLayerProfile[] _profiles = System.Array.Empty<TerrainLayerProfile>();

        [Tooltip("Профіль за замовчуванням для шарів без явного запису.")]
        [SerializeField] private TerrainLayerProfile _fallback;

        private Dictionary<string, TerrainLayerProfile> _lookup;

        public IReadOnlyList<TerrainLayerProfile> Profiles => _profiles;

        /// <summary>
        /// Повертає профіль для заданого id шару або fallback, якщо запис відсутній.
        /// </summary>
        public TerrainLayerProfile GetProfile(string layerId)
        {
            EnsureLookup();
            if (!string.IsNullOrEmpty(layerId) && _lookup.TryGetValue(layerId, out var profile))
                return profile;
            return _fallback;
        }

        public bool TryGetProfile(string layerId, out TerrainLayerProfile profile)
        {
            EnsureLookup();
            if (!string.IsNullOrEmpty(layerId) && _lookup.TryGetValue(layerId, out profile))
                return true;
            profile = _fallback;
            return profile != null;
        }

        /// <summary>
        /// Вартість руху для шару. 0 (або відсутній/непрохідний профіль) = непрохідно.
        /// </summary>
        public float GetMovementCost(string layerId)
        {
            var profile = GetProfile(layerId);
            if (profile == null || !profile.Walkable)
                return 0f;
            return profile.MovementCost;
        }

        public bool IsBuildBlocked(string layerId)
        {
            var profile = GetProfile(layerId);
            return profile != null && profile.BuildBlocked;
        }

        public float GetSurfaceOffset(string layerId)
        {
            var profile = GetProfile(layerId);
            return profile != null ? profile.SurfaceOffset : 0f;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
                return;

            _lookup = new Dictionary<string, TerrainLayerProfile>();
            if (_profiles == null)
                return;

            for (int i = 0; i < _profiles.Length; i++)
            {
                var profile = _profiles[i];
                if (profile == null || string.IsNullOrEmpty(profile.LayerId))
                    continue;
                _lookup[profile.LayerId] = profile;
            }
        }

        private void OnDisable() => _lookup = null;
    }
}
