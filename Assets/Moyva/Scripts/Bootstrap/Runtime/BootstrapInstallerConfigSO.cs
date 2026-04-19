using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [CreateAssetMenu(menuName = "Moyva/Bootstrap/Installer Config", fileName = "BootstrapInstallerConfig")]
    public sealed class BootstrapInstallerConfigSO : ScriptableObject
    {
        [SerializeField] private BootstrapGameSettings _gameSettings = new();
        [SerializeField] private StartingPositionInitializerSettings _startingPositionSettings = new();

        public BootstrapGameSettings GameSettings => _gameSettings;
        public StartingPositionInitializerSettings StartingPositionSettings => _startingPositionSettings;
    }
}
