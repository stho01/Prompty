using Promty;
using Promty.Attributes;

namespace Commander.Commands;

[Description("git", "Execute git commands")]
public class GitCommand : ProcessCommand
{
    protected override string ExecutablePath => "git";
}
