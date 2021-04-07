using System;
using System.Text;
using System.Threading;

namespace ConfigCat.Cli.Utils
{
    interface IPrompt
    {
        string GetString(string label, string defaultValue = null);

        string GetMaskedString(string label, CancellationToken token, string defaultValue = null);
    }

    class Prompt : IPrompt
    {
        private readonly IExecutionContextAccessor accessor;

        public Prompt(IExecutionContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public string GetString(string label, string defaultValue = null)
        {
            label = defaultValue != null ? $"{label} [default: {defaultValue}]" : label;
            this.accessor.ExecutionContext.Output.Write($"{label}: ");
            var result = Console.ReadLine();
            return result.IsEmpty() ? defaultValue : result;
        }

        public string GetMaskedString(string label, CancellationToken token, string defaultValue = null)
        {
            var builder = new StringBuilder();

            label = defaultValue != null ? $"{label} [default: {defaultValue}]" : label;
            this.accessor.ExecutionContext.Output.Write($"{label}: ");

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(intercept: true);

                if(key.Key == ConsoleKey.Escape)
                {
                    builder.Clear();
                    return defaultValue;
                }
                else if(key.Key == ConsoleKey.Backspace && builder.Length > 0)
                {
                    this.accessor.ExecutionContext.Output.Write(Constants.Backspace);
                    builder.Remove(builder.Length - 1, 1);
                }
                else if(!char.IsControl(key.KeyChar))
                {
                    this.accessor.ExecutionContext.Output.Write("*");
                    builder.Append(key.KeyChar);
                }


            } while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape && !token.IsCancellationRequested);

            this.accessor.ExecutionContext.Output.WriteLine();

            if (builder.Length == 0)
                return defaultValue;

            return builder.ToString().Trim();
        }
    }
}
