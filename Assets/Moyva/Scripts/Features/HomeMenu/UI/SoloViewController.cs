using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class SoloViewController : MonoBehaviour, ISoloViewController, IInitializable
    {
        [SerializeField] private Button _nextButton;

        public event Action OnButtonNextClicked;

        public void Initialize()
        {
            Awake();
        }

        private void Awake()
        {
            if (_nextButton != null)
                _nextButton.onClick.AddListener(HandleNextClicked);
        }

        private void OnDestroy()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(HandleNextClicked);
        }

        private void HandleNextClicked()
        {
            OnButtonNextClicked?.Invoke();
        }
    }
}