using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// UI scaffold for the building selection panel.
    /// Renders one button per available building and fires <see cref="OnBuildingClicked"/> on selection.
    ///
    /// HOW TO WIRE IN UNITY:
    /// 1. Add this component to the root GameObject of your building selection panel.
    /// 2. Assign <b>itemContainer</b>: the Transform that will parent the generated buttons
    ///    (e.g. a Vertical Layout Group).
    /// 3. Assign <b>buttonPrefab</b>: a prefab that has a <see cref="BuildingButtonUI"/> component.
    ///    The prefab should also have a <c>Button</c> and a <c>Text</c> (or <c>TextMeshProUGUI</c>)
    ///    which <see cref="BuildingButtonUI"/> will populate automatically.
    /// 4. <see cref="ConstructionUIController"/> calls <see cref="Populate"/> at startup.
    ///    You do not need to call it manually.
    /// </summary>
    public class BuildingSelectionPanelUI : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Parent Transform for generated building buttons (e.g. Vertical Layout Group content).")]
        [SerializeField] private Transform itemContainer;

        [Tooltip("Prefab for each building button. Must have a BuildingButtonUI component.")]
        [SerializeField] private GameObject buttonPrefab;

        /// <summary>
        /// Fired when a building button is clicked.
        /// The argument is the building ID (matches <c>BuildingDefinition.Id</c>).
        /// </summary>
        public Action<string> OnBuildingClicked;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

        /// <summary>
        /// Populate the panel with the given building items.
        /// Clears any previously generated buttons first.
        /// Called automatically by <see cref="ConstructionUIController"/>.
        /// </summary>
        public void Populate(IEnumerable<BuildingListItemData> buildings)
        {
            ClearItems();

            if (itemContainer == null || buttonPrefab == null || buildings == null)
                return;

            foreach (var item in buildings)
            {
                var go = Instantiate(buttonPrefab, itemContainer);
                var btn = go.GetComponent<BuildingButtonUI>();
                if (btn != null)
                    btn.Setup(item, OnBuildingClicked);

                _spawnedButtons.Add(go);
            }
        }

        /// <summary>Remove all generated building buttons.</summary>
        public void ClearItems()
        {
            foreach (var go in _spawnedButtons)
            {
                if (go != null)
                    Destroy(go);
            }
            _spawnedButtons.Clear();
        }
    }
}
