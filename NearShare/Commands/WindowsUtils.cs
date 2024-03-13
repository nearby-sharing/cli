#if WINDOWS
using NearShare.Windows.Sender;
using Spectre.Console;
using System.CommandLine;
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
                await NearShareSender.SendAsync(args.RemoteSystem, new("https://shortdev.de"));
            }

            AutoResetEvent @event = new(false);
            @event.WaitOne();
        }));
        return command;
    }
}
#endif
