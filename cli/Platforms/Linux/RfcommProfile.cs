using System.Net.Sockets;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Platforms.Linux;
internal sealed class RfcommProfile(string uuid) : OrgBluezProfile1, IMethodHandler
{
    protected override Connection Connection => throw new NotImplementedException();
    public override string Path { get; } = $"/org/bluez/nearshare/profile/{uuid}";
    public string Uuid { get; } = uuid;

    async ValueTask IMethodHandler.HandleMethodAsync(MethodContext context)
    {
        // Workaround for https://github.com/affederaffe/Tmds.DBus.SourceGenerator/issues/15
        if (context.Request.InterfaceAsString == "org.bluez.Profile1" &&
            context.Request.MemberAsString == "NewConnection" &&
            context.Request.SignatureAsString == "oha{sv}")
        {
            Read();
            void Read()
            {
                Reader reader = context.Request.GetBodyReader();
                var device = reader.ReadObjectPath();
                var socketHandle = reader.ReadHandle<SafeSocketHandle>();
                var properties = reader.ReadDictionary_aesv();
                OnNewConnection(device, socketHandle, properties);
            }

            if (!context.NoReplyExpected)
                Reply();
            void Reply()
            {
                MessageWriter writer = context.CreateReplyWriter(null);
                context.Reply(writer.CreateMessage());
                writer.Dispose();
            }

            return;
        }

        // Fallback to generated implementation
        await HandleMethodAsync(context);
    }

    // This is a wrong signature.
    // We need a handle not an uint32!
    protected override ValueTask OnNewConnectionAsync(ObjectPath arg0, uint fd, Dictionary<string, VariantValue> arg2)
        => throw new NotSupportedException();

    readonly TaskCompletionSource<Stream> _promise = new();
    public Task<Stream> ConnectionTask => _promise.Task;

    void OnNewConnection(ObjectPath device, SafeSocketHandle? fd, Dictionary<string, VariantValue> properties)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(fd);

            Socket socket = new(fd);
            NetworkStream stream = new(socket, ownsSocket: true);
            _promise.TrySetResult(stream);
        }
        catch (Exception ex)
        {
            _promise.SetException(ex);
        }
    }

    protected override ValueTask OnRequestDisconnectionAsync(ObjectPath arg0)
    {
        // ToDo
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnReleaseAsync()
        => ValueTask.CompletedTask;
}
