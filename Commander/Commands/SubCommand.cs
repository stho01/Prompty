using Promty;
using Promty.Attributes;

namespace Commander.Commands;

[Description("sub", "Execute sub commands")]
public class SubCommand : ProcessCommand
{
    protected override string ExecutablePath => "Commander.SubCommands.exe";
}