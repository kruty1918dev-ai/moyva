using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class ContinueViewController : MonoBehaviour, IContinueViewController, IInitializable
    {
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private WorldItemViewComponent _slotPrefab;

        public event Action<GameSlotInfo> OnSlotSelected;

        // Keep track of spawned items by slot name
        private readonly Dictionary<string, WorldItemViewComponent> _spawned
            = new Dictionary<string, WorldItemViewComponent>(StringComparer.Ordinal);

        // Keep the last known GameSlotInfo for each slot (used to refresh displayed data)
        private readonly Dictionary<string, GameSlotInfo> _slotInfos
            = new Dictionary<string, GameSlotInfo>(StringComparer.Ordinal);

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
            if (_slotPrefab == null || _slotsContainer == null) return;

            string key = slot.SlotName ?? $"slot{slot.SlotIndex:D2}";

            // If already exists, update it
            if (_spawned.TryGetValue(key, out var existing))
            {
                _slotInfos[key] = slot;
                existing.Initialize(slot, s => OnSlotSelected?.Invoke(s));
                return;
            }

            var instance = Instantiate(_slotPrefab, _slotsContainer);
            instance.name = key;
            instance.Initialize(slot, s => OnSlotSelected?.Invoke(s));
            _spawned[key] = instance;
            _slotInfos[key] = slot;
        }

        public void RemoveSlot(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) return;

            if (_spawned.TryGetValue(slotName, out var instance))
            {
                if (instance != null)
                    Destroy(instance.gameObject);
                _spawned.Remove(slotName);
                _slotInfos.Remove(slotName);
                return;
            }

            // Fallback: search children by name
            for (int i = 0; i < _slotsContainer.childCount; i++)
            {
                var child = _slotsContainer.GetChild(i);
                if (child.name == slotName)
                {
                    Destroy(child.gameObject);
                    _slotInfos.Remove(slotName);
                    break;
                }
            }
        }

        public void ClearSlots()
        {
            _spawned.Clear();
            _slotInfos.Clear();
            for (int i = _slotsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_slotsContainer.GetChild(i).gameObject);
            }
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
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}