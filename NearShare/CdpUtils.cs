using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.Platforms.Network;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using Spectre.Console;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NearShare;

internal static partial class CdpUtils
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

    [SupportedOSPlatform("windows")]
    public static void UnblockPorts()
    {
        var hr = DeletePersistentUdpPortReservation(Constants.UdpPort, 1);
        if (hr != 0)
            throw new Win32Exception(hr);
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("Iphlpapi")]
    private static partial int DeletePersistentUdpPortReservation(ushort port, ushort range);
}
