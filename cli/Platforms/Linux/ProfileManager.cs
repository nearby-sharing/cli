using ShortDev.Microsoft.ConnectedDevices;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class ProfileManager(Connection connection) : IAsyncDisposable
{
    public Connection Connection { get; } = connection;
    readonly OrgBluezProfileManager1 _profileManager = new(connection, "org.bluez", "/org/bluez");

    public async ValueTask RegisterAsync(RfcommProfile profile, CancellationToken cancellationToken)
    {
        Connection.AddMethodHandler(profile);
        await _profileManager.RegisterProfileAsync(profile.Path, profile.Uuid, []);

        await cancellationToken.AwaitCancellation();

        await _profileManager.UnregisterProfileAsync(profile.Path);
    }

    public async ValueTask DisposeAsync()
    {
        //await Connection.DisconnectedAsync();
        //Connection.Dispose();
    }

    public static async ValueTask<ProfileManager> CreateAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        Connection connection = new(address);
        await connection.ConnectAsync();
        return new(connection);
    }
}
