using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контролер solo-екрану з кнопкою переходу до наступного кроку.
    /// </summary>
    public class SoloViewController : MonoBehaviour, ISoloViewController, IInitializable
    {
        [SerializeField] private Button _nextButton;

        public event Action OnButtonNextClicked;
        private bool _bound;

        public void Initialize()
        {
            Bind();
        }

        private void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (_bound)
                return;

            if (_nextButton != null)
                _nextButton.onClick.AddListener(HandleNextClicked);

            _bound = true;
        }

        private void OnDestroy()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(HandleNextClicked);

            _bound = false;
        }

        private void HandleNextClicked()
        {
            OnButtonNextClicked?.Invoke();
        }
    }
}