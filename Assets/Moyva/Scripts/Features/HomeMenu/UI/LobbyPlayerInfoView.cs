using TMPro;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class LobbyPlayerInfoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void SetPlayerInfo(LobbyUserInfo userInfo)
        {
            if (_text != null)
            {
                _text.text = $"Player: {userInfo.UserName} (ID: {userInfo.UserId})";
            }
            else
            {
                Debug.LogError("LobbyPlayerInfoView: TextMeshProUGUI component is not assigned.");
            }
        }
    }
}