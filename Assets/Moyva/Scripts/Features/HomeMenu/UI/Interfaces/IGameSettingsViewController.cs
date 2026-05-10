using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Graphics;

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
        GraphicsQualityProfile GraphicsProfile { get; set; }
        int TargetFrameRate { get; set; }
        float RenderScale { get; set; }
        bool DynamicRenderScale { get; set; }
        bool CloseZoomOptimization { get; set; }
        int TextureMipmapLimit { get; set; }
        int AntiAliasing { get; set; }
        bool VSync { get; set; }
        bool Shadows { get; set; }
        bool AnisotropicFiltering { get; set; }
        float LodBias { get; set; }

        event Action<string> OnPlayerNameChanged;
        event Action<float> OnMasterVolumeChanged;
        event Action<float> OnMusicVolumeChanged;
        event Action<float> OnSfxVolumeChanged;
        event Action<float> OnUiVolumeChanged;
        event Action<bool> OnMutedChanged;
        event Action<GraphicsQualityProfile> OnGraphicsProfileChanged;
        event Action<int> OnTargetFrameRateChanged;
        event Action<float> OnRenderScaleChanged;
        event Action<bool> OnDynamicRenderScaleChanged;
        event Action<bool> OnCloseZoomOptimizationChanged;
        event Action<int> OnTextureMipmapLimitChanged;
        event Action<int> OnAntiAliasingChanged;
        event Action<bool> OnVSyncChanged;
        event Action<bool> OnShadowsChanged;
        event Action<bool> OnAnisotropicFilteringChanged;
        event Action<float> OnLodBiasChanged;
        event Action OnResetGraphicsClicked;
        event Action OnDeleteSavesClicked;

        void Refresh(LocalGameSettings settings);
        void RefreshGraphics(GraphicsSettingsData settings);
        void SetInteractable(bool interactable);
    }
}
