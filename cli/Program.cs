using NearShare.Commands;
using Spectre.Console;
using System.CommandLine;

AnsiConsole.Write(
    new FigletText("NearShare").Color(Color.Yellow3)
);

RootCommand root = new(description: "Cross-platform NearShare (Project Rome) cli")
{
    Send.CreateCommand(),
    Receive.CreateCommand(),
#if WINDOWS
    WindowsUtils.CreateWfdLogsTestCommand(),
    WindowsUtils.CreateWfdGoTestCommand(),
    WindowsUtils.CreateWfdConnectTestCommand(),
#endif
};

return await root.InvokeAsync(args);
