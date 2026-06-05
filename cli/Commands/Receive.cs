using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.NearShare;
using Spectre.Console;
using System.Collections.Frozen;
using System.CommandLine;
using System.Diagnostics;

namespace NearShare.Commands;

internal class Receive : INearShareCommand
{
    public static Command CreateCommand()
    {
        Option<string> pathOption = new("--path", "-p")
        {
            Required = true,
            Description = "Directory files should be saved to. Only used for file transfers."
        };

        Option<string> deviceNameOption = new("--deviceName", "-n")
        {
            Required = false,
            Description = "DeviceName used for advertisement. If not specified, the device name from the platform will be used.",
            DefaultValueFactory = _ => Environment.MachineName
        };

        Option<bool> forceOption = new("--force", "-f")
        {
            Required = false,
            Description = "Whether to not show a confirm for file transfers",
            DefaultValueFactory = _ => false
        };

        Command command = new("receive", description: "Receive from a remote device")
        {
            pathOption,
            deviceNameOption,
            forceOption
        };
        command.SetAction(async ctx =>
        {
            var path = ctx.GetRequiredValue(pathOption);
            var deviceName = ctx.GetRequiredValue(deviceNameOption);
            var force = ctx.GetRequiredValue(forceOption);

            using var cdp = CdpUtils.CreatePlatform(deviceName);

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
                    return -1;
                }

                fileTransfer.Accept(
                    fileTransfer.ToFrozenDictionary(
                        x => x.Id,
                        x => (Stream)File.OpenWrite(Path.Combine(path, Path.GetFileName(x.Name)))
                    )
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

            return 0;
        });
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
