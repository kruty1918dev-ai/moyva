using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Bootstrap.Runtime
{
[System.Serializable]
public sealed class BootstrapInstallerConfigSO : MoyvaJsonConfigObject
    {
        [SerializeField] private BootstrapGameSettings _gameSettings = new();
        [SerializeField] private StartingPositionInitializerSettings _startingPositionSettings = new();

        public BootstrapGameSettings GameSettings => _gameSettings;
        public StartingPositionInitializerSettings StartingPositionSettings => _startingPositionSettings;
    }
}
