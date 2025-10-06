using Promty;
using Promty.Attributes;

namespace Commander.Commands;

[Description("dotnet", "Execute dotnet CLI commands")]
public class DotNetCommand : ProcessCommand
{
    protected override string ExecutablePath => "dotnet";
}
