using Promty;
using Promty.Attributes;

namespace Example.Commands;

[Description("sub", "Execute sub commands")]
public class SubCommand : ProcessCommand
{
    protected override string ExecutablePath => "Example.SubCommands.exe";
}