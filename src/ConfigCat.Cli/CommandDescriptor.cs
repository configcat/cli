using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;

namespace ConfigCat.Cli;

public class CommandDescriptor(string name, string description, string example = null)
{
    public string Name { get; } = name;

    public string Description { get; } = description;

    public string Example { get; } = example;

    public bool IsHidden { get; init; }

    public IEnumerable<Option> Options { get; init; } = Enumerable.Empty<Option>();

    public IEnumerable<Argument> Arguments { get; init; } = Enumerable.Empty<Argument>();

    public IEnumerable<string> Aliases { get; init; } = Enumerable.Empty<string>();

    public IEnumerable<CommandDescriptor> SubCommands { get; init; } = Enumerable.Empty<CommandDescriptor>();

    public HandlerDescriptor Handler { get; init; }
}

public class HandlerDescriptor(Type handlerType, MethodInfo method)
{
    public MethodInfo Method { get; } = method;

    public Type HandlerType { get; } = handlerType;
}