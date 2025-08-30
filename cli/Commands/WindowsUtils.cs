#if WINDOWS
using NearShare.Windows.WiFiDirect;
using ShortDev.Microsoft.ConnectedDevices;
using System.CommandLine;
using System.Net.NetworkInformation;

namespace NearShare.Commands;
internal static class WindowsUtils
{
    public static Command CreateWfdLogsTestCommand()
    {
        Command command = new("windows-wfd-logs");
        command.SetHandler(async ctx =>
        {
            using var handle = WiFiDirectHandle.Open();
            handle.Notification += (ref WFDNotificationData data, nint context) =>
            {
                Console.WriteLine($"Notification: {data.NotificationCode}, Source: {data.NotificationSource}");
            };

            await ctx.GetCancellationToken().AwaitCancellation();
        });
        return command;
    }

    public static Command CreateWfdGoTestCommand()
    {
        Command command = new("windows-wfd-go");
        command.SetHandler(async ctx =>
        {
            using var handle = WiFiDirectHandle.Open();
            handle.Notification += (ref WFDNotificationData data, nint context) =>
            {
                Console.WriteLine($"Notification: {data.NotificationCode}, Source: {data.NotificationSource}");
            };

            using var group = WiFiDirectGroup.Start(handle);

            await ctx.GetCancellationToken().AwaitCancellation();
        });
        return command;
    }

    public static Command CreateWfdConnectTestCommand()
    {
        Argument<PhysicalAddress> addressOption = new("address", value => PhysicalAddress.Parse(value.Tokens[0].Value));

        Command command = new("windows-wfd-connect")
        {
            addressOption
        };

        command.SetHandler(async address =>
        {
            using var handle = WiFiDirectHandle.Open();
            handle.Notification += (ref WFDNotificationData data, nint context) =>
            {
                Console.WriteLine($"Notification: {data.NotificationCode}, Source: {data.NotificationSource}");
            };

            var session = await WiFiDirectSession.ConnectAsync(handle, address);
        }, addressOption);
        return command;
    }
}
#endif
