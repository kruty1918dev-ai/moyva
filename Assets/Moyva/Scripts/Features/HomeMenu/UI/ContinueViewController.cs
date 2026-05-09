using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
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

            string key = BuildSlotKey(slot);

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
            RebuildSlotsLayout();
        }

        public void RemoveSlot(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) return;

            if (_spawned.TryGetValue(slotName, out var instance))
            {
                if (instance != null)
                    DestroySlotObject(instance.gameObject);
                _spawned.Remove(slotName);
                _slotInfos.Remove(slotName);
                RebuildSlotsLayout();
                return;
            }

            // Fallback: search children by name
            for (int i = 0; i < _slotsContainer.childCount; i++)
            {
                var child = _slotsContainer.GetChild(i);
                if (child.name == slotName)
                {
                    DestroySlotObject(child.gameObject);
                    _slotInfos.Remove(slotName);
                    RebuildSlotsLayout();
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
                DestroySlotObject(_slotsContainer.GetChild(i).gameObject);
            }

            RebuildSlotsLayout();
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

            RebuildSlotsLayout();
        }

        private static string BuildSlotKey(GameSlotInfo slot)
            => $"slot{Mathf.Clamp(slot.SlotIndex, 0, 99):D2}";

        private static void DestroySlotObject(GameObject slotObject)
        {
            if (slotObject == null)
                return;

            slotObject.SetActive(false);
            Destroy(slotObject);
        }

        private void RebuildSlotsLayout()
        {
            if (_slotsContainer is not RectTransform rectTransform)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}