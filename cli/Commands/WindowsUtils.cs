#if WINDOWS
using NearShare.Windows.WiFiDirect;
using ShortDev.Microsoft.ConnectedDevices;
using System.CommandLine;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NearShare.Commands;

internal static class WindowsUtils
{
    static unsafe void WfdNotificationHandler(ref WFDNotificationData data, nint context)
    {
        switch (data.NotificationCode)
        {
            case WFDNotificationCode.PeerStateChanged:
                var peerData = *(WFD_PEER_CHANGED_NOTIFICATION_DATA*)data.pData;
                Console.WriteLine($"Peer {peerData.state}: {peerData.someId} auth={peerData.authAlgorithm}");
                break;
            default:
                Console.WriteLine($"Notification: {data.NotificationCode}, Source: {data.NotificationSource}");
                break;
        }
    }

    public static Command CreateWfdLogsTestCommand()
    {
        Command command = new("windows-wfd-logs");
        command.SetAction(async (ctx, cancellation) =>
        {
            using var handle = WiFiDirectHandle.Open();
            handle.Notification += WfdNotificationHandler;

            await cancellation.AwaitCancellation();
        });
        return command;
    }

    public static Command CreateWfdGoTestCommand()
    {
        Command command = new("windows-wfd-go");
        command.SetAction(async (ctx, cancellation) =>
        {
            using var handle = WiFiDirectHandle.Open();
            handle.Notification += WfdNotificationHandler;

            using var group = WiFiDirectGroup.Start(handle);

            await cancellation.AwaitCancellation();
        });
        return command;
    }

    public static Command CreateWfdConnectTestCommand()
    {
        Argument<PhysicalAddress> addressOption = new("address")
        {
            CustomParser = value => PhysicalAddress.Parse(value.Tokens[0].Value)
        };

        Argument<string> ssidOption = new("ssid");
        Argument<string> passphraseOption = new("passphrase");

        Command command = new("windows-wfd-connect")
        {
            addressOption,
            ssidOption,
            passphraseOption,
        };

        command.SetAction(async ctx =>
        {
            var address = ctx.GetRequiredValue(addressOption);
            var ssid = ctx.GetRequiredValue(ssidOption);
            var passphrase = ctx.GetRequiredValue(passphraseOption);

            using var handle = WiFiDirectHandle.Open();
            handle.Notification += WfdNotificationHandler;

            var session = await WiFiDirectSession.ConnectAsync(handle, address, ssid, passphrase);
        });
        return command;
    }
}
#endif
