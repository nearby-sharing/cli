using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class NearShareAdvertisement : OrgBluezLEAdvertisement1
{
    public const string ObjectPath = "/org/bluez/nearshare/advertisement0";

    protected override Connection Connection => throw new NotImplementedException();
    public override string Path { get; } = ObjectPath;

    protected override ValueTask OnReleaseAsync()
        => ValueTask.CompletedTask;

    public static NearShareAdvertisement Create(AdvertiseOptions options)
    {
        WriteableBuffer beaconData = new(options.BeaconData.ToArray());
        return new()
        {
            LocalName = options.BeaconData.DeviceName,
            Type = "peripheral",
            ManufacturerData = new()
            {
                { (ushort)options.ManufacturerId, beaconData.AsVariant() }
            }
        };
    }

    readonly struct WriteableBuffer(ReadOnlyMemory<byte> buffer) : IDBusWritable
    {
        readonly ReadOnlyMemory<byte> _buffer = buffer;

        public void WriteTo(ref MessageWriter writer)
            => writer.WriteArray(_buffer.Span);

        public Variant AsVariant()
            => CreateVariant("ay"u8, this);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        extern static Variant CreateVariant(Utf8Span signature, IDBusWritable value);
    }
}
