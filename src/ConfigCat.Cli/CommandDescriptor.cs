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

    public IEnumerable<Option> Options { get; init; } = [];

    public IEnumerable<Argument> Arguments { get; init; } = [];

    public IEnumerable<string> Aliases { get; init; } = [];

    public IEnumerable<CommandDescriptor> SubCommands { get; init; } = [];

    public HandlerDescriptor Handler { get; init; }
}

public class HandlerDescriptor(Type handlerType, MethodInfo method)
{
    public MethodInfo Method { get; } = method;

    public Type HandlerType { get; } = handlerType;
}