#if WINDOWS
using NearShare.Windows.Sender;
using NearShare.Windows.WiFiDirect;
using ShortDev.Microsoft.ConnectedDevices;
using Spectre.Console;
using System.CommandLine;
using System.Net.NetworkInformation;
using Windows.ApplicationModel.Internal.DataTransfer.NearShare;
using Windows.System.RemoteSystems;

namespace NearShare.Commands;
internal static class WindowsUtils
{
    public static Command CreateRomeTestCommand()
    {
        Command command = new("windows-watch-devices");
        command.SetHandler(() => AnsiConsole.Status().Start("Watching devices", ctx =>
        {

            var watcher = RemoteSystem.CreateWatcher();
            watcher.RemoteSystemAdded += Watcher_RemoteSystemAdded;
            watcher.RemoteSystemRemoved += Watcher_RemoteSystemRemoved;
            watcher.RemoteSystemUpdated += Watcher_RemoteSystemUpdated;
            watcher.Start();

            void Watcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
            {
                AnsiConsole.WriteLine($"Updated {args.RemoteSystem.DisplayName} {args.RemoteSystem.Id}");
            }

            void Watcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
            {
                AnsiConsole.WriteLine($"Removed {args.RemoteSystemId}");
            }

            async void Watcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
            {
                AnsiConsole.WriteLine($"Added {args.RemoteSystem.DisplayName} {args.RemoteSystem.Id}");

                Progress<SendDataProgress> progress = new();
                progress.ProgressChanged += OnProgress;
                try
                {
                    await NearShareSender.SendAsync(args.RemoteSystem, new Uri("https://shortdev.de"), progress);
                }
                catch (Exception ex)
                {

                }

                static void OnProgress(object? sender, SendDataProgress e)
                {

                }
            }

            AutoResetEvent @event = new(false);
            @event.WaitOne();
        }));
        return command;
    }

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
