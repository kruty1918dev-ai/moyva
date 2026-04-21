using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Оверлей завантаження з прогрес-баром та текстовим статусом.
    /// Керується <see cref="Runtime.HomeMenuFlow"/>.
    /// </summary>
    public sealed class LoadingOverlayView : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private TMP_Text percentLabel;

        /// <summary>Оновлює стан прогрес-бару та тексту статусу.</summary>
        /// <param name="value01">Прогрес [0..1].</param>
        /// <param name="status">Текст статусу (може бути null).</param>
        public void SetProgress(float value01, string status = null)
        {
            value01 = Mathf.Clamp01(value01);
            if (progressBar != null) progressBar.value = value01;
            if (!string.IsNullOrEmpty(status) && statusLabel != null)
                statusLabel.text = status;
            if (percentLabel != null)
                percentLabel.text = $"{Mathf.RoundToInt(value01 * 100f)}%";
        }

        /// <summary>Задає візуальний стан показу.</summary>
        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }
    }
}
