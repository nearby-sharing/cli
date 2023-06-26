using System.CommandLine;

namespace NearShare.Commands;

internal interface INearShareCommand
{
    static abstract Command CreateCommand();
}
