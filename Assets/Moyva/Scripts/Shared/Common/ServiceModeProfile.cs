using System;
using Kruty1918.Moyva.Shared.Graphics;

namespace Kruty1918.Moyva.Shared.Common
{
    public enum ServiceRuntimeMode
    {
        Menu = 0,
        Gameplay = 1
    }

    public enum ServiceLogLevel
    {
        Quiet = 0,
        Standard = 1,
        Verbose = 2
    }

    public readonly struct ServiceModeProfile
    {
        public readonly ServiceLogLevel LogLevel;
        public readonly TimeSpan ConnectivityWaitTimeout;
        public readonly TimeSpan ConnectivityQuickProbeTimeout;
        public readonly TimeSpan JoinCodeResolveTimeout;
        public readonly TimeSpan JoinCodePollInterval;
        public readonly bool ApplyGraphicsProfile;
        public readonly bool RespectCustomGraphicsProfile;
        public readonly GraphicsQualityProfile GraphicsProfile;

        public ServiceModeProfile(
            ServiceLogLevel logLevel,
            TimeSpan connectivityWaitTimeout,
            TimeSpan connectivityQuickProbeTimeout,
            TimeSpan joinCodeResolveTimeout,
            TimeSpan joinCodePollInterval,
            bool applyGraphicsProfile,
            bool respectCustomGraphicsProfile,
            GraphicsQualityProfile graphicsProfile)
        {
            LogLevel = logLevel;
            ConnectivityWaitTimeout = connectivityWaitTimeout;
            ConnectivityQuickProbeTimeout = connectivityQuickProbeTimeout;
            JoinCodeResolveTimeout = joinCodeResolveTimeout;
            JoinCodePollInterval = joinCodePollInterval;
            ApplyGraphicsProfile = applyGraphicsProfile;
            RespectCustomGraphicsProfile = respectCustomGraphicsProfile;
            GraphicsProfile = graphicsProfile;
        }

        public bool IsVerboseLogging => LogLevel == ServiceLogLevel.Verbose;
    }

    public interface IServiceModeProfileProvider
    {
        ServiceModeProfile Get(ServiceRuntimeMode mode);
    }

    public static class ServiceModeProfileDefaults
    {
        public static readonly ServiceModeProfile Menu = new ServiceModeProfile(
            logLevel: ServiceLogLevel.Verbose,
            connectivityWaitTimeout: TimeSpan.FromSeconds(8),
            connectivityQuickProbeTimeout: TimeSpan.FromSeconds(6),
            joinCodeResolveTimeout: TimeSpan.FromSeconds(15),
            joinCodePollInterval: TimeSpan.FromMilliseconds(250),
            applyGraphicsProfile: true,
            respectCustomGraphicsProfile: true,
            graphicsProfile: GraphicsQualityProfile.Balanced);

        public static readonly ServiceModeProfile Gameplay = new ServiceModeProfile(
            logLevel: ServiceLogLevel.Standard,
            connectivityWaitTimeout: TimeSpan.FromSeconds(5),
            connectivityQuickProbeTimeout: TimeSpan.FromSeconds(4),
            joinCodeResolveTimeout: TimeSpan.FromSeconds(12),
            joinCodePollInterval: TimeSpan.FromMilliseconds(200),
            applyGraphicsProfile: true,
            respectCustomGraphicsProfile: true,
            graphicsProfile: GraphicsQualityProfile.Auto);
    }

    public sealed class ServiceModeProfileProvider : IServiceModeProfileProvider
    {
        public ServiceModeProfile Get(ServiceRuntimeMode mode)
        {
            return mode == ServiceRuntimeMode.Gameplay
                ? ServiceModeProfileDefaults.Gameplay
                : ServiceModeProfileDefaults.Menu;
        }
    }
}