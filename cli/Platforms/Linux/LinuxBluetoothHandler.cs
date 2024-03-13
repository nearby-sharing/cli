using Linux.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Platforms;
using ShortDev.Microsoft.ConnectedDevices.Platforms.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using Tmds.DBus;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxBluetoothHandler(Adapter adapter, ILEAdvertisingManager1 advertisingManager) : IBluetoothHandler
{
    readonly Adapter _adapter = adapter;
    readonly ILEAdvertisingManager1 _advertisingManager = advertisingManager;

    public static async ValueTask<LinuxBluetoothHandler> CreateAsync()
    {
        var advertisingManager = Connection.System.CreateProxy<ILEAdvertisingManager1>("org.bluez", "/org/bluez/hci0");

        var adapters = await BlueZManager.GetAdaptersAsync();
        return new(adapters.FirstOrDefault() ?? throw new InvalidOperationException("Could not get adapter"), advertisingManager);
    }

    public PhysicalAddress MacAddress => PhysicalAddress.None;

    public Task AdvertiseBLeBeaconAsync(AdvertiseOptions options, CancellationToken cancellationToken = default)
    {
        // ToDo: Implement
        // _advertisingManager.RegisterAdvertisementAsync()

        return Task.CompletedTask;
    }

    public async Task ScanBLeAsync(ScanOptions scanOptions, CancellationToken cancellationToken = default)
    {
        await _adapter.SetPoweredAsync(true);
        await _adapter.SetDiscoveryFilterAsync(new Dictionary<string, object>()
        {
            { "Transport", "le" },
            { "DuplicateData", false }
        });

        await _adapter.StartDiscoveryAsync();
        _adapter.DeviceFound += OnDeviceFound;

        await cancellationToken.AwaitCancellation();

        await _adapter.StopDiscoveryAsync();

        async Task OnDeviceFound(Adapter sender, DeviceFoundEventArgs eventArgs)
            => await ParseDeviceAsync(eventArgs.Device);

        async Task ParseDeviceAsync(Device device)
        {
            var data = await device.GetManufacturerDataAsync();
            if (!data.TryGetValue(Constants.BLeBeaconManufacturerId, out var beaconData))
                return;

            if (!BLeBeacon.TryParse((byte[])beaconData, out var beacon))
                return;

            scanOptions.OnDeviceDiscovered?.Invoke(beacon);
        }
    }

    public Task<CdpSocket> ConnectRfcommAsync(CdpDevice device, RfcommOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ListenRfcommAsync(RfcommOptions options, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
