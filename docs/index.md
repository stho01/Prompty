---
layout: home

hero:
  name: "Promty"
  text: "Build Beautiful CLI Tools"
  tagline: A powerful and flexible command-line parser and command executor framework for .NET
  image:
    src: /logo.png
    alt: Promty
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/stho01/Promty

features:
  - icon: 🎯
    title: Type-Safe Argument Binding
    details: Automatically bind command-line arguments to strongly-typed classes with full IntelliSense support.

  - icon: 🚩
    title: Long & Short Flags
    details: Support for both --verbose and -v style flags with automatic help text generation.

  - icon: 📝
    title: Automatic Help Generation
    details: Beautiful, table-formatted help text generated automatically from attributes.

  - icon: ⚡
    title: Process Command Execution
    details: Easily wrap external CLI tools and forward arguments seamlessly.

  - icon: 🎨
    title: Attribute-Based Configuration
    details: Use simple attributes to configure commands and arguments - minimal boilerplate.

  - icon: ✅
    title: Built-in Validation
    details: Automatic validation for required arguments with helpful error messages.

  - icon: 🔄
    title: Flags Enums
    details: Group related boolean flags into a single [Flags] enum for cleaner code.

  - icon: 🎁
    title: Flexible Architecture
    details: Extend Command<TArgs> or ProcessCommand base classes to suit your needs.
---

## Quick Example

```csharp
using Promty;

[Description("greet", "Greets a person by name")]
public class GreetCommand : Command<GreetCommand.Args>
{
    public class Args
    {
        [Description("name", "The name of the person to greet")]
        public string Name { get; set; } = string.Empty;

        [FlagAlias("uppercase", 'u')]
        [Description("Print the greeting in uppercase")]
        public bool Uppercase { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        var greeting = $"Hello, {args.Name}!";
        if (args.Uppercase) greeting = greeting.ToUpper();
        Console.WriteLine(greeting);
        return Task.FromResult(0);
    }
}
```

```bash
# Run your command
dotnet run -- greet Alice --uppercase
# Output: HELLO, ALICE!
```

## Installation

::: code-group

```bash [.NET CLI]
dotnet add package Promty
```

```xml [PackageReference]
<PackageReference Include="Promty" Version="1.0.0" />
```

:::
