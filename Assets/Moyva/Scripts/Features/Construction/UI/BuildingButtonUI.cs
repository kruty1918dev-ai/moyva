using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Компонент для кнопки окремої будівлі у меню вибору.
    /// Прикріпи до prefab кнопки, що використовується <see cref="BuildingSelectionPanelUI"/>.
    ///
    /// ЯК СТВОРИТИ PREFAB:
    /// 1. GameObject → UI → Button - TextMeshPro.
    /// 2. Додай компонент <see cref="BuildingButtonUI"/>.
    /// 3. Підключи <b>label</b> → дочірній TextMeshProUGUI.
    /// 4. Підключи <b>iconImage</b> → дочірній Image (для іконки будівлі).
    /// 5. <b>button</b> знаходиться автоматично у Awake якщо не призначений.
    /// 6. Збережи як prefab і перетягни у <see cref="BuildingSelectionPanelUI.buttonPrefab"/>.
    /// </summary>
    public class BuildingButtonUI : MonoBehaviour
    {
        [Tooltip("TextMeshProUGUI компонент з назвою будівлі.")]
        [SerializeField] private TextMeshProUGUI label;

        [Tooltip("Image компонент для іконки будівлі. Необов'язковий.")]
        [SerializeField] private Image iconImage;

        [Tooltip("Button компонент. Знаходиться автоматично у Awake якщо не призначений.")]
        [SerializeField] private Button button;

        [Tooltip("Масштаб кнопки у вибраному стані (щоб виділялась в меню).")]
        [SerializeField] private Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1f);

        [Header("Анімація натиску (DOTween)")]
        [Tooltip("Множник масштабу для анімації натиску кнопки.")]
        [SerializeField] private float pressScaleMultiplier = 1.08f;

        [Tooltip("Тривалість анімації натиску у секундах.")]
        [SerializeField] private float pressDuration = 0.16f;

        [Tooltip("Інтенсивність поштовху (кількість коливань).")]
        [SerializeField] private int pressVibrato = 8;

        [Tooltip("Пружність анімації натиску (0..1).")]
        [SerializeField] private float pressElasticity = 0.8f;

        private string _buildingId;
        private Action<string> _onClick;
        private Vector3 _defaultScale;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            _defaultScale = transform.localScale;

            if (label == null)
                Debug.LogWarning($"[BuildingButtonUI] Поле 'label' не призначено на '{name}'. Назва будівлі не відображатиметься.", this);
            if (button == null)
                Debug.LogWarning($"[BuildingButtonUI] Button компонент не знайдено на '{name}'.", this);
        }

        /// <summary>
        /// Ініціалізує кнопку даними будівлі та колбеком кліку.
        /// Викликається автоматично через <see cref="BuildingSelectionPanelUI.Populate"/>.
        /// </summary>
        public void Setup(BuildingListItemData data, Action<string> onClick)
        {
            _buildingId = data.Id;
            _onClick = onClick;

            if (label != null)
                label.text = data.DisplayName;

            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }

            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        /// <summary>Встановлює або знімає виділення кнопки (збільшення масштабу).</summary>
        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? selectedScale : _defaultScale;
        }

        private void HandleClick()
        {
            ConstructionButtonPressAnimator.AnimatePress(transform, pressScaleMultiplier, pressDuration, pressVibrato, pressElasticity);
            _onClick?.Invoke(_buildingId);
        }

        /// <summary>Повертає ID будівлі, прив'язаний до цієї кнопки.</summary>
        public string GetBuildingId() => _buildingId;

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }
    }
}
