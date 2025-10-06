using Promty;
using Promty.Attributes;

namespace Example.Commands;

[Description("git", "Execute git commands")]
public class GitCommand : ProcessCommand
{
    protected override string ExecutablePath => "git";
}
