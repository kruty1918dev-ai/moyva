using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Кнопка одного соціального посилання. Бере дані з
    /// <see cref="SocialLinkEntry"/> та делегує відкриття в
    /// <see cref="ISocialLinksService"/>.
    /// </summary>
    public sealed class SocialLinkButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text label;

        private SocialLinkEntry _entry;
        private ISocialLinksService _service;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(Click);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(Click);
        }

        /// <summary>Ініціалізує кнопку з записом і сервісом.</summary>
        public void Bind(SocialLinkEntry entry, ISocialLinksService service)
        {
            _entry   = entry;
            _service = service ?? throw new ArgumentNullException(nameof(service));

            if (label != null)
                label.text = string.IsNullOrEmpty(entry.DisplayName) ? entry.Id : entry.DisplayName;

            if (iconImage != null)
            {
                if (entry.Icon != null)
                {
                    iconImage.sprite  = entry.Icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (button != null)
                button.interactable = entry.IsValid;
        }

        private void Click()
        {
            _service?.OpenLink(_entry);
        }
    }
}
