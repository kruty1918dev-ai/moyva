using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface IGameSettingsViewController
    {
        string PlayerName { get; set; }
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        float UiVolume { get; set; }
        bool IsMuted { get; set; }

        event Action<string> OnPlayerNameChanged;
        event Action<float> OnMasterVolumeChanged;
        event Action<float> OnMusicVolumeChanged;
        event Action<float> OnSfxVolumeChanged;
        event Action<float> OnUiVolumeChanged;
        event Action<bool> OnMutedChanged;
        event Action OnDeleteSavesClicked;

        void Refresh(LocalGameSettings settings);
        void SetInteractable(bool interactable);
    }
}
