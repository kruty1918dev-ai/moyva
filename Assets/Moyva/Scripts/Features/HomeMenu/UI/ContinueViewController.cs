using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер екрана Continue зі списком слотів збережень.
    /// Підтримує додавання, видалення, очищення та refresh елементів слотів.
    /// </summary>
    public class ContinueViewController : MonoBehaviour, IContinueViewController, IInitializable
    {
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private WorldItemViewComponent _slotPrefab;

        public event Action<GameSlotInfo> OnSlotSelected;

        // Keep track of spawned items by stable save slot key.
        private readonly Dictionary<string, WorldItemViewComponent> _spawned
            = new Dictionary<string, WorldItemViewComponent>(StringComparer.Ordinal);

        // Keep the last known GameSlotInfo for each slot (used to refresh displayed data).
        private readonly Dictionary<string, GameSlotInfo> _slotInfos
            = new Dictionary<string, GameSlotInfo>(StringComparer.Ordinal);

        private readonly Stack<WorldItemViewComponent> _pool = new Stack<WorldItemViewComponent>();

        public void Initialize()
        {
            Awake();
        }

        void Awake()
        {
            if (_slotsContainer == null)
                Debug.LogError("[ContinueViewController] _slotsContainer is not assigned.");
            if (_slotPrefab == null)
                Debug.LogError("[ContinueViewController] _slotPrefab is not assigned.");
        }

        public void AddSlot(GameSlotInfo slot)
        {
            // 1: Без префаба або контейнера не можемо відрендерити слот.
            if (_slotPrefab == null || _slotsContainer == null) return;

            string key = BuildSlotKey(slot);

            // 2: Якщо слот уже існує в UI, перев'язуємо його на оновлені дані.
            // If already exists, update it
            if (_spawned.TryGetValue(key, out var existing))
            {
                _slotInfos[key] = slot;
                existing.Initialize(slot, s => OnSlotSelected?.Invoke(s));
                return;
            }

            // 3: Інакше беремо елемент з пулу або створюємо новий без синхронного canvas rebuild.
            var instance = GetOrCreateSlotItem();
            instance.name = key;
            instance.transform.SetParent(_slotsContainer, false);
            instance.gameObject.SetActive(true);
            instance.Initialize(slot, s => OnSlotSelected?.Invoke(s));
            _spawned[key] = instance;
            _slotInfos[key] = slot;
            MarkSlotsLayoutDirty();
        }

        public void RemoveSlot(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) return;

            if (_spawned.TryGetValue(slotName, out var instance))
            {
                if (instance != null)
                    ReleaseSlotItem(instance);
                _spawned.Remove(slotName);
                _slotInfos.Remove(slotName);
                MarkSlotsLayoutDirty();
                return;
            }

            // Fallback: search children by name
            if (_slotsContainer == null)
                return;

            for (int i = 0; i < _slotsContainer.childCount; i++)
            {
                var child = _slotsContainer.GetChild(i);
                if (child.name == slotName)
                {
                    ReleaseSlotItem(child.GetComponent<WorldItemViewComponent>());
                    _slotInfos.Remove(slotName);
                    MarkSlotsLayoutDirty();
                    break;
                }
            }
        }

        public void ClearSlots()
        {
            foreach (var pair in _spawned)
                ReleaseSlotItem(pair.Value);

            _spawned.Clear();
            _slotInfos.Clear();

            MarkSlotsLayoutDirty();
        }

        public void RefreshSlots()
        {
            // Refresh all existing spawned items using the last known GameSlotInfo.
            // If a spawned item has no stored GameSlotInfo, it will be left as-is.
            foreach (var kvp in _spawned)
            {
                var key = kvp.Key;
                var instance = kvp.Value;
                if (instance == null) continue;

                if (_slotInfos.TryGetValue(key, out var info))
                {
                    instance.Initialize(info, s => OnSlotSelected?.Invoke(s));
                }
            }

            MarkSlotsLayoutDirty();
        }

        private static string BuildSlotKey(GameSlotInfo slot)
            => $"slot{Mathf.Clamp(slot.SlotIndex, 0, 99):D2}";

        private WorldItemViewComponent GetOrCreateSlotItem()
        {
            while (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                if (pooled != null)
                    return pooled;
            }

            return Instantiate(_slotPrefab, _slotsContainer);
        }

        private void ReleaseSlotItem(WorldItemViewComponent item)
        {
            if (item == null)
                return;

            item.gameObject.SetActive(false);
            _pool.Push(item);
        }

        private void MarkSlotsLayoutDirty()
        {
            if (_slotsContainer is not RectTransform rectTransform)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}
