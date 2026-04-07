using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Component for an individual building selection button.
    /// Attach to the building button prefab used by <see cref="BuildingSelectionPanelUI"/>.
    ///
    /// HOW TO CREATE THE PREFAB:
    /// 1. Create a UI Button GameObject (GameObject → UI → Button).
    /// 2. Add this component to it.
    /// 3. Assign the <b>label</b> field to the button's child Text.
    /// 4. The <b>button</b> field is auto-found on Awake if not assigned.
    /// 5. Save as a prefab and drag into <see cref="BuildingSelectionPanelUI.buttonPrefab"/>.
    /// </summary>
    public class BuildingButtonUI : MonoBehaviour
    {
        [Tooltip("Text component showing the building display name.")]
        [SerializeField] private Text label;

        [Tooltip("Button component. Auto-found on Awake if not assigned.")]
        [SerializeField] private Button button;

        private string _buildingId;
        private Action<string> _onClick;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        /// <summary>
        /// Initialise this button with building data and a click callback.
        /// Called automatically by <see cref="BuildingSelectionPanelUI.Populate"/>.
        /// </summary>
        public void Setup(BuildingListItemData data, Action<string> onClick)
        {
            _buildingId = data.Id;
            _onClick = onClick;

            if (label != null)
                label.text = data.DisplayName;

            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void HandleClick() => _onClick?.Invoke(_buildingId);

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }
    }
}
