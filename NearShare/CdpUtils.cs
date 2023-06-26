using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.Platforms.Network;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using Spectre.Console;
using System.Net;

namespace NearShare;

internal static class CdpUtils
{
    public static ConnectedDevicesPlatform CreatePlatform(string? deviceName, DeviceType deviceType = DeviceType.Linux)
    {
        ConnectedDevicesPlatform cdp = new(new()
        {
            Name = deviceName ?? Environment.MachineName,
            OemManufacturerName = Environment.UserName,
            OemModelName = Environment.UserDomainName,
            Type = deviceType,
            DeviceCertificate = ConnectedDevicesPlatform.CreateDeviceCertificate(CdpEncryptionParams.Default),
            LoggerFactory = ConnectedDevicesPlatform.CreateLoggerFactory(AnsiConsole.WriteLine)
        });

        NetworkHandler networkHandler = new();
        cdp.AddTransport(new NetworkTransport(networkHandler));

        return cdp;
    }

    sealed class NetworkHandler : INetworkHandler
    {
        public IPAddress GetLocalIp()
            => INetworkHandler.GetLocalIpDefault();
    }
}
