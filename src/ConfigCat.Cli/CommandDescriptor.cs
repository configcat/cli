using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;

namespace ConfigCat.Cli
{
    public class CommandDescriptor
    {
        public CommandDescriptor(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsHidden { get; set; }

        public IEnumerable<Option> Options { get; set; } = Enumerable.Empty<Option>();

        public IEnumerable<Argument> Arguments { get; set; } = Enumerable.Empty<Argument>();

        public IEnumerable<string> Aliases { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<CommandDescriptor> SubCommands { get; set; } = Enumerable.Empty<CommandDescriptor>();

        public HandlerDescriptor Handler { get; set; }
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
}
