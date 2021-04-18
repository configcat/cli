using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public interface IPrompt
    {
        Task<string> GetStringAsync(string label,
            CancellationToken token,
            string defaultValue = null);

        Task<string> GetMaskedStringAsync(string label,
            CancellationToken token,
            string defaultValue = null);

        Task<TItem> ChooseFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            TItem selectedValue = default);

        Task<List<TItem>> ChooseMultipleFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            List<TItem> preSelectedItems = null);
    }

    public class Prompt : IPrompt
    {
        private readonly IExecutionContextAccessor accessor;

        public Prompt(IExecutionContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public async Task<string> GetStringAsync(string label,
            CancellationToken token,
            string defaultValue = null)
        {
            var output = this.accessor.ExecutionContext.Output;

            if (token.IsCancellationRequested ||
                output.IsOutputRedirected)
                return defaultValue;

            label = defaultValue is not null ? $"{label} [default: '{defaultValue}']" : label;
            output.Write($"{label}: ");

            var result = await output.ReadLineAsync(token);
            output.WriteLine();
            return result.IsEmpty() ? defaultValue : result;
        }

        public async Task<string> GetMaskedStringAsync(string label,
            CancellationToken token,
            string defaultValue = null)
        {
            var output = this.accessor.ExecutionContext.Output;

            if (token.IsCancellationRequested ||
                output.IsOutputRedirected)
                return defaultValue;

            label = defaultValue is not null ? $"{label} [default: {defaultValue}]" : label;
            output.Write($"{label}: ");

            var result = await output.ReadLineAsync(token, true);
            output.WriteLine();
            return result.IsEmpty() ? defaultValue : result;
        }

        public async Task<TItem> ChooseFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            TItem selectedValue = default)
        {
            var output = this.accessor.ExecutionContext.Output;
            if (token.IsCancellationRequested || output.IsOutputRedirected)
                return default;

            output.HideCursor();

            output.WriteLine();
            output.WriteUnderline(label);
            output.Write(":");
            output.WriteLine();

            output.WriteColored("(Use the ", ForegroundColorSpan.DarkGray());
            output.WriteColored("UP", ForegroundColorSpan.LightCyan());
            output.WriteColored(" and ", ForegroundColorSpan.DarkGray());
            output.WriteColored("DOWN", ForegroundColorSpan.LightCyan());
            output.WriteColored(" keys to navigate)", ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.WriteLine();

            int index = selectedValue is null || selectedValue.Equals(default) ? 0 : items.IndexOf(selectedValue);

            this.PrintChooseSection(index, items, labelSelector);
            output.SetCursorPosition(0, output.CursorTop - items.Count + index);

            ConsoleKeyInfo key;
            try
            {
                do
                {
                    key = await output.ReadKeyAsync(token, true);
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (index <= 0)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelected(items[index], labelSelector);
                        output.MoveCursorUp(0);
                        output.ClearCurrentLine();
                        this.PrintSelected(items[--index], labelSelector, false);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (index >= items.Count - 1)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelected(items[index], labelSelector);
                        output.MoveCursorDown(0);
                        output.ClearCurrentLine();
                        this.PrintSelected(items[++index], labelSelector, false);
                    }
                } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

                output.ClearCurrentLine();
                this.PrintSelected(items[index], labelSelector, true);
                output.SetCursorPosition(0, output.CursorTop + items.Count - index);
                output.WriteLine();

                return items[index];
            }
            catch (OperationCanceledException)
            {
                output.SetCursorPosition(0, output.CursorTop + items.Count - index);
                throw;
            }
            finally
            {
                output.ShowCursor();
            }
        }

        public async Task<List<TItem>> ChooseMultipleFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            List<TItem> preSelectedItems = null)
        {
            var output = this.accessor.ExecutionContext.Output;
            if (token.IsCancellationRequested || output.IsOutputRedirected)
                return default;

            output.HideCursor();

            output.WriteLine();
            output.WriteUnderline(label);
            output.Write(":");
            output.WriteLine();

            output.WriteColored("(Use the ", ForegroundColorSpan.DarkGray());
            output.WriteColored("UP", ForegroundColorSpan.LightCyan());
            output.WriteColored(" and ", ForegroundColorSpan.DarkGray());
            output.WriteColored("DOWN", ForegroundColorSpan.LightCyan());
            output.WriteColored(" keys to navigate, and ", ForegroundColorSpan.DarkGray());
            output.WriteColored("SPACE", ForegroundColorSpan.LightCyan());
            output.WriteColored(" to select)", ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.WriteLine();

            int index = 0;

            var selectedItems = preSelectedItems ?? new List<TItem>();
            this.PrintMultiChooseSection(items, labelSelector, selectedItems);
            output.SetCursorPosition(0, output.CursorTop - items.Count + index);
            ConsoleKeyInfo key;
            try
            {
                do
                {
                    key = await output.ReadKeyAsync(token, true);
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (index <= 0)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                        output.MoveCursorUp(0);
                        output.ClearCurrentLine();
                        this.PrintSelectedInMulti(items[--index], labelSelector, selectedItems);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (index >= items.Count - 1)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                        output.MoveCursorDown(0);
                        output.ClearCurrentLine();
                        this.PrintSelectedInMulti(items[++index], labelSelector, selectedItems);
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        var item = items[index];
                        if (selectedItems.Contains(item))
                        {
                            selectedItems.Remove(item);
                            output.ClearCurrentLine();
                            this.PrintSelected(item, labelSelector, false);
                        }
                        else
                        {
                            selectedItems.Add(item);
                            output.ClearCurrentLine();
                            this.PrintSelectedInMulti(item, labelSelector, selectedItems);
                        }
                    }
                } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

                output.ClearCurrentLine();
                this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                output.SetCursorPosition(0, output.CursorTop + items.Count - index);
                output.WriteLine();

                return selectedItems;
            }
            catch (OperationCanceledException)
            {
                output.SetCursorPosition(0, output.CursorTop + items.Count - index);
                throw;
            }
            finally
            {
                output.ShowCursor();
            }
        }

        private void PrintChooseSection<TItem>(
            int index,
            List<TItem> items,
            Func<TItem, string> labelSelector)
        {
            foreach (var item in items)
            {
                if (items.IndexOf(item) == index)
                    this.PrintSelected(item, labelSelector, false);
                else
                    this.PrintNonSelected(item, labelSelector);

                this.accessor.ExecutionContext.Output.WriteLine();
            }
        }

        private void PrintMultiChooseSection<TItem>(
            List<TItem> items,
            Func<TItem, string> labelSelector,
            List<TItem> preSelectedItems)
        {
            foreach (var item in items)
            {
                int index = items.IndexOf(item);
                if (preSelectedItems.Contains(item))
                    this.PrintSelected(item, labelSelector, true, index == 0);
                else if (index == 0)
                    this.PrintSelected(item, labelSelector, false);
                else
                    this.PrintNonSelected(item, labelSelector);

                this.accessor.ExecutionContext.Output.WriteLine();
            }
        }

        private void PrintSelected<TItem>(TItem item,
            Func<TItem, string> labelSelector,
            bool isHighlight,
            bool showIndicator = false)
        {
            var output = this.accessor.ExecutionContext.Output;
            if (isHighlight)
            {
                output.WriteNonAnsiColor($"| {(showIndicator ? ">" : " ")} {labelSelector(item)}", ConsoleColor.White, ConsoleColor.DarkMagenta);
                return;
            }

            output.WriteNonAnsiColor("|", ConsoleColor.DarkGray);
            output.WriteNonAnsiColor($" > ", ConsoleColor.Magenta);
            output.Write(labelSelector(item));
        }

        private void PrintNonSelected<TItem>(TItem item, Func<TItem, string> labelSelector)
        {
            var output = this.accessor.ExecutionContext.Output;
            output.WriteNonAnsiColor($"|   {labelSelector(item)}", ConsoleColor.DarkGray);
        }

        private void PrintSelectedInMulti<TItem>(TItem item, Func<TItem, string> labelSelector, List<TItem> selectedItems)
        {
            if (selectedItems.Contains(item))
                this.PrintSelected(item, labelSelector, true, true);
            else
                this.PrintSelected(item, labelSelector, false);
        }

        private void PrintNonSelectedInMulti<TItem>(TItem item, Func<TItem, string> labelSelector, List<TItem> selectedItems)
        {
            if (selectedItems.Contains(item))
                this.PrintSelected(item, labelSelector, true);
            else
                this.PrintNonSelected(item, labelSelector);
        }
    }
}