using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class WorldSetupViewController : MonoBehaviour, IWorldSetupViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _worldNameInput;
        [SerializeField] private TMP_InputField _seedInput;
        [SerializeField] private TMP_Dropdown _sizeDropdown;
        [SerializeField] private Button _nextButton;

        public string WorldName { get; set; }
        public int Seed { get; set; }
        public WorldSize Size { get; set; }

        public event Action OnButtonNextClicked;

        public void Initialize()
        {
            Awake();
        }

        void Awake()
        {
            // TODO: Add listeners to UI elements to update properties and invoke OnButtonNextClicked when nextButton is clicked.
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}