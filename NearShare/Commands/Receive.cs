using NearShare.Commands;
using ShortDev.Microsoft.ConnectedDevices.NearShare;
using ShortDev.Microsoft.ConnectedDevices.Platforms.Network;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

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
            using var cdp = CdpUtils.CreatePlatform(deviceName);

            CancellationTokenSource tokenSource = new();
            cdp.Listen(tokenSource.Token);
            cdp.Advertise(tokenSource.Token);

            ReceiveHandler handler = new();
            NearShareReceiver.Start(cdp, handler);

            var transferToken = await handler;

            NearShareReceiver.Stop();
            tokenSource.Cancel();

            if (transferToken is UriTransferToken uriTransfer)
            {
                AnsiConsole.MarkupLine($"Received from [green]{uriTransfer.DeviceName}[/]");
                AnsiConsole.MarkupLine($"[white]{uriTransfer.Uri}[/]");
            }
            else if (transferToken is FileTransferToken fileTransfer)
            {
                if (!force && !AnsiConsole.Confirm($"Do you want to receive file \"{fileTransfer.FileNames}\" from {fileTransfer.DeviceName}?", defaultValue: true))
                {
                    fileTransfer.Cancel();
                    return;
                }

                // ToDo: Create filestreams
                // fileTransfer.Accept();
            }
            else
                throw new UnreachableException();

        }, path, deviceName, forceOption);
        return command;
    }

    sealed class NetworkHandler : INetworkHandler
    {
        public IPAddress GetLocalIp()
            => INetworkHandler.GetLocalIpDefault();
    }

    sealed class ReceiveHandler : INearSharePlatformHandler
    {
        readonly TaskCompletionSource<TransferToken> _promise = new();
        public void OnFileTransfer(FileTransferToken transfer)
            => _promise.TrySetResult(transfer);

        public void OnReceivedUri(UriTransferToken transfer)
            => _promise.TrySetResult(transfer);

        public TaskAwaiter<TransferToken> GetAwaiter()
            => _promise.Task.GetAwaiter();
    }
}
