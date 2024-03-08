#if MACOS
using ShortDev.Microsoft.ConnectedDevices.Platforms;
using ShortDev.Microsoft.ConnectedDevices.Platforms.Bluetooth;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using IOBluetooth;

namespace NearShare.Platforms.MacOs;

[SupportedOSPlatform("macos")]
internal sealed class MacBluetoothHandler : IBluetoothHandler
{
    static MacBluetoothHandler()
    {
        ObjCRuntime.Dlfcn.dlopen("/System.Library/Frameworks/IOBluetooth.framework/IOBluetooth", 0);
    }

    public PhysicalAddress MacAddress => throw new NotImplementedException();

    public Task AdvertiseBLeBeaconAsync(AdvertiseOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ScanBLeAsync(ScanOptions scanOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<CdpSocket> ConnectRfcommAsync(CdpDevice device, RfcommOptions options, CancellationToken cancellationToken = default)
    {
        var btDevice = BluetoothDevice.DeviceWithAddressString(device.Endpoint.Address);

        var serviceRecord = await btDevice.QuerySdpService(options.ServiceId);

        serviceRecord.GetRFCOMMChannelID(out var channelId);
        // ToDo: btDevice.OpenRfcommChannelAsync(out var channel, channelId, )

        throw new NotImplementedException();
    }

    public Task ListenRfcommAsync(RfcommOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
#endif