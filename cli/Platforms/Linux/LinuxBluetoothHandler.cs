extern alias DBusHighLevel;

using DBusHighLevel::Tmds.DBus;
using Linux.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxBluetoothHandler(Adapter adapter, PhysicalAddress macAddress) : IBluetoothHandler
{
    readonly Adapter _adapter = adapter;
    public PhysicalAddress MacAddress { get; } = macAddress;

    public static async ValueTask<LinuxBluetoothHandler> CreateAsync()
    {
        var adapters = await BlueZManager.GetAdaptersAsync();
        var adapter = adapters.FirstOrDefault() ?? throw new InvalidOperationException("Could not get adapter");

        var addressStr = await adapter.GetAddressAsync();
        var macAddress = PhysicalAddress.Parse(addressStr);

        return new(adapter, macAddress);
    }

    public async Task AdvertiseBLeBeaconAsync(AdvertiseOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await _adapter.SetPoweredAsync(true);
            await _adapter.SetDiscoverableAsync(true);

            var advertisement = NearShareAdvertisement.Create(options);

            await using var helper = await AdvertisingManager.CreateAsync(Address.System);
            await helper.AdvertiseAsync(advertisement, cancellationToken);

            await _adapter.SetDiscoverableAsync(false);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            throw;
        }
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

    public Task<CdpSocket> ConnectRfcommAsync(EndpointInfo device, RfcommOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ListenRfcommAsync(RfcommOptions options, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
