using Promty;
using Promty.Attributes;

namespace Example.Commands;

[Description("dotnet", "Execute dotnet CLI commands")]
public class DotNetCommand : ProcessCommand
{
    protected override string ExecutablePath => "dotnet";
}
