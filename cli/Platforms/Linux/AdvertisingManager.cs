using ShortDev.Microsoft.ConnectedDevices;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class AdvertisingManager(Connection connection) : IAsyncDisposable
{
    public Connection Connection { get; } = connection;
    readonly OrgBluezLEAdvertisingManager1 _advertisingManager = new(connection, "org.bluez", "/org/bluez/hci0");

    public async ValueTask AdvertiseAsync(OrgBluezLEAdvertisement1 advertisement, CancellationToken cancellationToken)
    {
        Connection.AddMethodHandler(advertisement);
        await _advertisingManager.RegisterAdvertisementAsync(advertisement.Path, []);

        await cancellationToken.AwaitCancellation();

        await _advertisingManager.UnregisterAdvertisementAsync(advertisement.Path);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisconnectedAsync();
        Connection.Dispose();
    }

    public static async ValueTask<AdvertisingManager> CreateAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        Connection connection = new(address);
        await connection.ConnectAsync();
        return new(connection);
    }
}
