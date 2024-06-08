using Microsoft.Win32.SafeHandles;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class RfcommProfile(string uuid) : OrgBluezProfile1, IMethodHandler
{
    protected override Connection Connection => throw new NotImplementedException();
    public override string Path { get; } = $"/de/shortdev/nearshare/profile0";
    public string Uuid { get; } = uuid;

    readonly TaskCompletionSource<Stream> _promise = new();
    public Task<Stream> ConnectionTask => _promise.Task;

    protected override ValueTask OnNewConnectionAsync(ObjectPath arg0, SafeFileHandle fd, Dictionary<string, VariantValue> properties)
    {
        try
        {
            Console.WriteLine(arg0);
            foreach (var (key, value) in properties)
                Console.WriteLine($"{key} = {value}");

            Socket socket = new(new SafeSocketHandle(fd.DangerousGetHandle(), ownsHandle: true));
            NetworkStream stream = new(socket, ownsSocket: true);
            _promise.TrySetResult(stream);
        }
        catch (Exception ex)
        {
            _promise.SetException(ex);
        }
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnRequestDisconnectionAsync(ObjectPath arg0)
    {
        // ToDo
        Console.WriteLine($"Request disconnect for {arg0}");
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnReleaseAsync()
        => ValueTask.CompletedTask;
}
