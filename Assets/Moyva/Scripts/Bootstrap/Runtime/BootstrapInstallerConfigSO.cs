using UnityEngine;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Bootstrap.Runtime
{
[System.Serializable]
public sealed class BootstrapInstallerConfigSO : JsonConfigObject
    {
        [SerializeField] private BootstrapGameSettings _gameSettings = new();
        [SerializeField] private StartingPositionInitializerSettings _startingPositionSettings = new();

        public BootstrapGameSettings GameSettings => _gameSettings;
        public StartingPositionInitializerSettings StartingPositionSettings => _startingPositionSettings;
    }
}
