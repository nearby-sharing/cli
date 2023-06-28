using NearShare.Commands;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.NearShare;
using ShortDev.Microsoft.ConnectedDevices.Platforms;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using Spectre.Console;
using System.CommandLine;

namespace NearShare;

internal class Send : INearShareCommand
{
    public static Command CreateCommand()
    {
        Option<string> deviceName = new("--deviceName", description: "DeviceName used for advertisement")
        {
            IsRequired = false,
        };
        deviceName.AddAlias("-n");

        Option<string> urlOption = new("--url", description: "Url to send")
        {
            IsRequired = false,
        };
        deviceName.AddAlias("-u");

        Option<string> fileOption = new("--file", description: "File path to send")
        {
            IsRequired = false,
        };
        deviceName.AddAlias("-f");

        Command command = new("send", description: "Send to a remote device")
        {
            deviceName,
            urlOption,
            fileOption
        };
        command.SetHandler(async (deviceName, filePath, uriStr) =>
        {
            using var cdp = CdpUtils.CreatePlatform(deviceName);

            HashSet<CdpDevice> devices = new();
            void OnDeviceDiscovered(ICdpTransport sender, CdpDevice device, BLeBeacon advertisement)
                => devices.Add(device);

            await AnsiConsole.Status().StartAsync("Looking for devices", async ctx =>
            {
                CancellationTokenSource tokenSource = new();
                cdp.DeviceDiscovered += OnDeviceDiscovered;
                cdp.Discover(tokenSource.Token);

                await Task.Delay(4_000);

                tokenSource.Cancel();
                cdp.DeviceDiscovered -= OnDeviceDiscovered;
            });

            if (devices.Count == 0)
            {
                AnsiConsole.Markup("[maroon]No devices found[/]");
                return;
            }

            CdpDevice device = AnsiConsole.Prompt(
                new SelectionPrompt<CdpDevice>()
                    .Title("Choose a device")
                    .AddChoices(devices)
                    .UseConverter(device => $@"{device.Name} via {device.Endpoint.TransportType} {device.Type}")
            );

            NearShareSender sender = new(cdp);
            if (Uri.TryCreate(uriStr, UriKind.Absolute, out var uri))
            {
                await sender.SendUriAsync(device, uri);
            }
            else
            {
                var fileContent = File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath);

                await AnsiConsole.Progress().StartAsync(async ctx =>
                {
                    var task = ctx.AddTask(fileName);
                    Progress<NearShareProgress> progress = new();
                    progress.ProgressChanged += (s, e) =>
                    {
                        task.Increment(e.BytesSent / e.TotalBytesToSend);
                    };

                    await sender.SendFileAsync(
                        device,
                        CdpFileProvider.FromBuffer(fileName, fileContent),
                        progress
                    );
                });
            }
        }, deviceName, fileOption, urlOption);
        return command;
    }
}
