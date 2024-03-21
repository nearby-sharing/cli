using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using Spectre.Console;
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
        var beaconData = options.BeaconData.ToArray();
        AnsiConsole.WriteLine(BitConverter.ToString(beaconData));

        NearShareAdvertisement advertisement = new();
        advertisement.BackingProperties.LocalName = options.BeaconData.DeviceName;
        advertisement.BackingProperties.Type = "peripheral";
        advertisement.BackingProperties.ManufacturerData = new()
        {
            { (ushort)options.ManufacturerId, new("ay", new DBusByteArrayItem(beaconData)) }
        };

        // Nullability
        advertisement.BackingProperties.ServiceData = [];
        advertisement.BackingProperties.ServiceUUIDs = [];
        advertisement.BackingProperties.SolicitUUIDs = [];
        advertisement.BackingProperties.Includes = [];

        return advertisement;
    }
}
