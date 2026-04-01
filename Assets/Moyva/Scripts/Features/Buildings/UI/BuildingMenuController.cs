using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Buildings.UI
{
    /// <summary>
    /// Головний контролер меню будівництва.
    /// Відображає три вкладки категорій (Військові / Цивільні / Індустріальні),
    /// кнопки підтвердження та скасування, обробляє Ctrl+Z / Ctrl+Y.
    ///
    /// Використання: Додайте цей компонент до Canvas > BuildingMenuPanel у сцені.
    /// Призначте посилання в інспекторі.
    /// </summary>
    public class BuildingMenuController : MonoBehaviour
    {
        [Header("Панель меню")]
        [SerializeField] private GameObject _menuPanel;

        [Header("Контейнери кнопок будівель")]
        [SerializeField] private Transform _militaryContainer;
        [SerializeField] private Transform _civilianContainer;
        [SerializeField] private Transform _industrialContainer;

        [Header("Кнопки управління")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        [Header("Префаб кнопки будівлі")]
        [SerializeField] private BuildingButtonUI _buildingButtonPrefab;

        [Inject] private IBuildingPlacementService _placementService;
        [Inject] private BuildingRegistrySO _buildingRegistry;

        private void Start()
        {
            PopulateMenu();

            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void Update()
        {
            if (!_placementService.IsPlacementModeActive) return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl) return;

            if (Input.GetKeyDown(KeyCode.Z))
                _placementService.Undo();
            else if (Input.GetKeyDown(KeyCode.Y))
                _placementService.Redo();
        }

        private void PopulateMenu()
        {
            PopulateCategory(BuildingCategory.Military, _militaryContainer);
            PopulateCategory(BuildingCategory.Civilian, _civilianContainer);
            PopulateCategory(BuildingCategory.Industrial, _industrialContainer);
        }

        private void PopulateCategory(BuildingCategory category, Transform container)
        {
            if (container == null) return;

            var configs = _buildingRegistry.GetByCategory(category);
            foreach (var config in configs)
            {
                var btn = Instantiate(_buildingButtonPrefab, container);
                btn.Setup(config, OnBuildingSelected);
            }
        }

        private void OnBuildingSelected(string typeId)
        {
            _placementService.StartPlacement(typeId);
        }

        private void OnConfirmClicked()
        {
            _placementService.Confirm();
        }

        private void OnCancelClicked()
        {
            _placementService.Cancel();
        }

        /// <summary>Показати меню будівництва</summary>
        public void Show() => _menuPanel.SetActive(true);

        /// <summary>Сховати меню будівництва</summary>
        public void Hide() => _menuPanel.SetActive(false);
    }
}
