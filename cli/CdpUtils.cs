using Microsoft.Extensions.Logging;
using NearShare.Platforms.Linux;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices.Transports.Network;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NearShare;
internal static partial class CdpUtils
{
    public static async ValueTask<ConnectedDevicesPlatform> CreatePlatformAsync(string? deviceName)
    {
        DeviceType deviceType = DeviceType.Linux;
        if (OperatingSystem.IsWindows())
            deviceType = DeviceType.Windows10Desktop;
        else if (OperatingSystem.IsMacOS())
            deviceType = DeviceType.iPad; // ToDo: Is there an entry for MacOs?

        var loggerFactory = LoggerFactory.Create(builder => { });

        ConnectedDevicesPlatform cdp = new(new()
        {
            Name = string.IsNullOrEmpty(deviceName) ? Environment.MachineName : deviceName,
            OemManufacturerName = Environment.UserName,
            OemModelName = Environment.UserDomainName,
            Type = deviceType,
            DeviceCertificate = ConnectedDevicesPlatform.CreateDeviceCertificate(CdpEncryptionParams.Default)
        }, loggerFactory);

        NetworkHandler networkHandler = new();
        cdp.AddTransport<NetworkTransport>(new(networkHandler));

        if (OperatingSystem.IsLinux())
        {
            var btHandler = await LinuxBluetoothHandler.CreateAsync();
            cdp.AddTransport<BluetoothTransport>(new(btHandler));
        }

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
