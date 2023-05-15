using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;

namespace ConfigCat.Cli;

public class CommandDescriptor
{
    public CommandDescriptor(string name, string description, string example = null)
    {
        this.Name = name;
        this.Description = description;
        this.Example = example;
    }

    public string Name { get; }

    public string Description { get; }
    
    public string Example { get; }

    public bool IsHidden { get; init; }

    public IEnumerable<Option> Options { get; init; } = Enumerable.Empty<Option>();

    public IEnumerable<Argument> Arguments { get; init; } = Enumerable.Empty<Argument>();

    public IEnumerable<string> Aliases { get; init; } = Enumerable.Empty<string>();

    public IEnumerable<CommandDescriptor> SubCommands { get; init; } = Enumerable.Empty<CommandDescriptor>();

    public HandlerDescriptor Handler { get; init; }
}

public class HandlerDescriptor
{
    public HandlerDescriptor(Type handlerType, MethodInfo method)
    {
        this.HandlerType = handlerType;
        this.Method = method;
    }

    public MethodInfo Method { get; }

    public Type HandlerType { get; }
}