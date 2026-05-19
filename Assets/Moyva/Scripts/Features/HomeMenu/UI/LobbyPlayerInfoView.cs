using TMPro;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Віджет відображення базової інформації про гравця в лобі.
    /// Залежності: отримує дані з моделей лобі та рендерить у TMP-текст.
    /// </summary>
    public class LobbyPlayerInfoView : MonoBehaviour
    {
        /// <summary>Текст для відображення імені та ідентифікатора гравця.</summary>
        [SerializeField] private TextMeshProUGUI _text;

        /// <summary>Прив'язати модель гравця до UI-представлення.</summary>
        public void SetPlayerInfo(LobbyUserInfo userInfo)
        {
            // 1: Якщо текстовий компонент заданий, рендеримо дані гравця.
            if (_text != null)
            {
                _text.text = $"Player: {userInfo.UserName} (ID: {userInfo.UserId})";
            }
            else
            {
                // 2: Якщо посилання не налаштоване, логуємо помилку для швидкої діагностики сцени.
                Debug.LogError("LobbyPlayerInfoView: TextMeshProUGUI component is not assigned.");
            }
        }
    }
}