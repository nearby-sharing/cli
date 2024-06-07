using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxBluetoothHandler(BlueZManager manager, IAdapter1 adapter, PhysicalAddress macAddress) : IBluetoothHandler
{
    readonly BlueZManager _manager = manager;
    readonly IAdapter1 _adapter = adapter;
    public PhysicalAddress MacAddress { get; } = macAddress;

    public static async ValueTask<LinuxBluetoothHandler> CreateAsync()
    {
        var manager = await BlueZManager.CreateAsync();
        var adapters = await manager.GetAdaptersAsync();
        var adapter = adapters.FirstOrDefault() ?? throw new InvalidOperationException("Could not get adapter");

        var addressStr = await adapter.GetAddressPropertyAsync();
        var macAddress = PhysicalAddress.Parse(addressStr);

        return new(manager, adapter, macAddress);
    }

    public async Task AdvertiseBLeBeaconAsync(AdvertiseOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await _adapter.SetPoweredPropertyAsync(true);
            await _adapter.SetDiscoverablePropertyAsync(true);

            var advertisement = NearShareAdvertisement.Create(options);
            await _manager.AdvertiseAsync(advertisement, cancellationToken);

            await _adapter.SetDiscoverablePropertyAsync(false);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            throw;
        }
    }

    public async Task ScanBLeAsync(ScanOptions scanOptions, CancellationToken cancellationToken = default)
    {
        await _adapter.SetPoweredPropertyAsync(true);
        await _adapter.SetDiscoveryFilterAsync(new()
        {
            { "Transport", "le" },
            { "DuplicateData", false }
        });

        await _adapter.StartDiscoveryAsync();

        await foreach (var device in _manager.GetDevicesAsync())
            await ParseDeviceAsync(device);
        using var watcher = await _manager.ObjectManager.WatchInterfacesAddedAsync((ex, changes) =>
        {
            if (!changes.InterfacesAndProperties.ContainsKey("org.bluez.Device1"))
                return;

            _ = ParseDeviceAsync(new(_manager.Connection, "org.bluez", changes.ObjectPath));
        });

        await cancellationToken.AwaitCancellation();

        await _adapter.StopDiscoveryAsync();

        async Task ParseDeviceAsync(IDevice1 device)
        {
            var data = await device.GetManufacturerDataPropertyAsync();
            if (!data.TryGetValue(Constants.BLeBeaconManufacturerId, out var beaconData))
                return;

            if (!BLeBeacon.TryParse(beaconData.GetArray<byte>(), out var beacon))
                return;

            scanOptions.OnDeviceDiscovered?.Invoke(beacon);
        }
    }

    public async Task<CdpSocket> ConnectRfcommAsync(EndpointInfo endpoint, RfcommOptions options, CancellationToken cancellationToken = default)
    {
        var device = await _manager.GetDeviceAsync(endpoint.Address);

        var stream = await _manager.CreateRfcommSocketAsync(device, options.ServiceId!);
        return new()
        {
            Endpoint = endpoint,
            InputStream = stream,
            OutputStream = stream,
            Close = stream.Close
        };
    }

    public Task ListenRfcommAsync(RfcommOptions options, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
