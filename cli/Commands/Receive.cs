using NearShare.Commands;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.NearShare;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;

namespace NearShare;

internal class Receive : INearShareCommand
{
    public static Command CreateCommand()
    {
        Option<string> path = new("--path", description: "Directory files should be saved to")
        {
            IsRequired = true,
        };
        path.AddAlias("-p");

        Option<string> deviceName = new("--deviceName", description: "DeviceName used for advertisement")
        {
            IsRequired = false,
        };
        deviceName.AddAlias("-n");

        Option<bool> forceOption = new("--force", description: "Whether to not show a confirm for file transfers")
        {
            IsRequired = false,
        };

        Command command = new("receive", description: "Receive from a remote device")
        {
            path,
            deviceName
        };
        command.SetHandler(async (path, deviceName, force) =>
        {
            using var cdp = await CdpUtils.CreatePlatformAsync(deviceName);

            CancellationTokenSource tokenSource = new();
            cdp.Listen(tokenSource.Token);
            cdp.Advertise(tokenSource.Token);

            var transferToken = await ReceiveAsync(cdp);
            tokenSource.Cancel();

            if (transferToken is UriTransferToken uriTransfer)
            {
                AnsiConsole.MarkupLine($"Received from [green]{Markup.Escape(uriTransfer.DeviceName)}[/]");
                AnsiConsole.MarkupLine($"[white]{Markup.Escape(uriTransfer.Uri)}[/]");
            }
            else if (transferToken is FileTransferToken fileTransfer)
            {
                if (!force && !AnsiConsole.Confirm($"Do you want to receive file \"{Markup.Escape(string.Join(", ", fileTransfer.Select(x => x.Name)))}\" from {Markup.Escape(fileTransfer.DeviceName)}?", defaultValue: true))
                {
                    fileTransfer.Cancel();
                    return;
                }

                fileTransfer.Accept(
                    fileTransfer
                        .Select(x => File.OpenWrite(Path.Combine(path, Path.GetFileName(x.Name))))
                        .ToArray()
                );

                await AnsiConsole.Progress().StartAsync(async ctx =>
                {
                    var bytesTask = ctx.AddTask("Bytes");

                    TaskCompletionSource promise = new();
                    fileTransfer.Progress += progress =>
                    {
                        bytesTask.MaxValue = progress.TotalBytes;
                        bytesTask.Value = progress.TotalBytes;

                        if (fileTransfer.IsTransferComplete)
                            promise.TrySetResult();
                    };
                    await promise.Task;
                });
            }
            else
                throw new UnreachableException();

        }, path, deviceName, forceOption);
        return command;
    }

    static async ValueTask<TransferToken> ReceiveAsync(ConnectedDevicesPlatform cdp)
    {
        TaskCompletionSource<TransferToken> promise = new();

        NearShareReceiver.Register(cdp);
        try
        {
            NearShareReceiver.FileTransfer += OnTransfer;
            NearShareReceiver.ReceivedUri += OnTransfer;

            void OnTransfer(TransferToken transfer)
                => promise.TrySetResult(transfer);

            return await promise.Task;
        }
        finally
        {
            NearShareReceiver.Unregister();
        }
    }
}
