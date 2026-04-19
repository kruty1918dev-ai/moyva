using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    /// <summary>
    /// Переопредяління параметрів економічних правил для конкретного світу / слоту збереження.
    /// Зберігає тільки змінені параметри (спільно з базовою конфіг).
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Economy/World Economy Override", fileName = "WorldEconomyOverride")]
    public sealed class WorldEconomyOverride : ScriptableObject
    {
        /// <summary>
        /// Пара: ID параметра -> нове значення.
        /// </summary>
        [SerializeField] private List<ParameterOverride> _overrides = new List<ParameterOverride>();

        [SerializeField] private string _worldIdentifier;

        public IReadOnlyList<ParameterOverride> Overrides => _overrides.AsReadOnly();
        public string WorldIdentifier => _worldIdentifier;

        /// <summary>
        /// Отримати перевизначене значення або null, якщо суміщено.
        /// </summary>
        public string GetOverrideValue(string parameterId)
        {
            var ov = _overrides.Find(o => o.ParameterId == parameterId);
            return ov?.Value;
        }

        /// <summary>
        /// Встановити перевизначення.
        /// </summary>
        public void SetOverride(string parameterId, string value)
        {
            var existing = _overrides.Find(o => o.ParameterId == parameterId);
            if (existing != null)
            {
                existing.Value = value;
            }
            else
            {
                _overrides.Add(new ParameterOverride { ParameterId = parameterId, Value = value });
            }
        }

        /// <summary>
        /// Видалити перевизначення (вернутися до дефолту).
        /// </summary>
        public void ClearOverride(string parameterId)
        {
            _overrides.RemoveAll(o => o.ParameterId == parameterId);
        }

        /// <summary>
        /// Очистити всі перевизначення.
        /// </summary>
        public void ClearAll()
        {
            _overrides.Clear();
        }

        [Serializable]
        public class ParameterOverride
        {
            [SerializeField] public string ParameterId;
            [SerializeField] public string Value;
        }
    }
}
