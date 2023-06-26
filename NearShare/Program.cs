
using NearShare;
using Spectre.Console;
using System.CommandLine;

AnsiConsole.Write(
    new FigletText("NearShare").Color(Color.Yellow3)
);

RootCommand root = new(description: "Cross-platform NearShare (Project Rome) cli")
{
    Send.CreateCommand(),
    Receive.CreateCommand()
};

await root.InvokeAsync(args);