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
        private readonly IOutput output;

        public Prompt(IOutput output)
        {
            this.output = output;
        }

        public async Task<string> GetStringAsync(string label,
            CancellationToken token,
            string defaultValue = null)
        {
            if (token.IsCancellationRequested ||
                output.IsOutputRedirected)
                return defaultValue;

            label = defaultValue is not null ? $"{label} [default: '{defaultValue}']" : label;
            this.output.Write($"{label}: ");

            var result = await output.ReadLineAsync(token);
            this.output.WriteLine();
            return result.IsEmpty() ? defaultValue : result;
        }

        public async Task<string> GetMaskedStringAsync(string label,
            CancellationToken token,
            string defaultValue = null)
        {
            if (token.IsCancellationRequested ||
                output.IsOutputRedirected)
                return defaultValue;

            label = defaultValue is not null ? $"{label} [default: {defaultValue}]" : label;
            this.output.Write($"{label}: ");

            var result = await output.ReadLineAsync(token, true);
            this.output.WriteLine();
            return result.IsEmpty() ? defaultValue : result;
        }

        public async Task<TItem> ChooseFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            TItem selectedValue = default)
        {
            if (token.IsCancellationRequested || this.output.IsOutputRedirected)
                return default;

            using var _ = this.output.CreateCursorHider();

            this.output.WriteLine();
            this.output.WriteUnderline(label);
            this.output.Write(":");
            this.output.WriteLine();

            this.output.WriteColored("(Use the ", ForegroundColorSpan.DarkGray());
            this.output.WriteColored("UP", ForegroundColorSpan.LightCyan());
            this.output.WriteColored(" and ", ForegroundColorSpan.DarkGray());
            this.output.WriteColored("DOWN", ForegroundColorSpan.LightCyan());
            this.output.WriteColored(" keys to navigate)", ForegroundColorSpan.DarkGray());
            this.output.WriteLine();
            this.output.WriteLine();

            int index = selectedValue is null || selectedValue.Equals(default) ? 0 : items.IndexOf(selectedValue);

            this.PrintChooseSection(index, items, labelSelector);
            this.output.SetCursorPosition(0, this.output.CursorTop - items.Count + index);

            ConsoleKeyInfo key;
            try
            {
                do
                {
                    key = await this.output.ReadKeyAsync(token, true);
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (index <= 0)
                            continue;

                        this.output.ClearCurrentLine();
                        this.PrintNonSelected(items[index], labelSelector);
                        this.output.MoveCursorUp(0);
                        this.output.ClearCurrentLine();
                        this.PrintSelected(items[--index], labelSelector, false);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (index >= items.Count - 1)
                            continue;

                        this.output.ClearCurrentLine();
                        this.PrintNonSelected(items[index], labelSelector);
                        this.output.MoveCursorDown(0);
                        this.output.ClearCurrentLine();
                        this.PrintSelected(items[++index], labelSelector, false);
                    }
                } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

                this.output.ClearCurrentLine();
                this.PrintSelected(items[index], labelSelector, true);
                this.output.SetCursorPosition(0, this.output.CursorTop + items.Count - index);
                this.output.WriteLine();

                return items[index];
            }
            catch (OperationCanceledException)
            {
                this.output.SetCursorPosition(0, this.output.CursorTop + items.Count - index);
                throw;
            }
        }

        public async Task<List<TItem>> ChooseMultipleFromListAsync<TItem>(string label,
            List<TItem> items,
            Func<TItem, string> labelSelector,
            CancellationToken token,
            List<TItem> preSelectedItems = null)
        {
            if (token.IsCancellationRequested || this.output.IsOutputRedirected)
                return default;

            using var _ = this.output.CreateCursorHider();

            this.output.WriteLine();
            this.output.WriteUnderline(label);
            this.output.Write(":");
            this.output.WriteLine();

            this.output.WriteColored("(Use the ", ForegroundColorSpan.DarkGray());
            this.output.WriteColored("UP", ForegroundColorSpan.LightCyan());
            this.output.WriteColored(" and ", ForegroundColorSpan.DarkGray());
            this.output.WriteColored("DOWN", ForegroundColorSpan.LightCyan());
            this.output.WriteColored(" keys to navigate, and ", ForegroundColorSpan.DarkGray());
            this.output.WriteColored("SPACE", ForegroundColorSpan.LightCyan());
            this.output.WriteColored(" to select)", ForegroundColorSpan.DarkGray());
            this.output.WriteLine();
            this.output.WriteLine();

            int index = 0;

            var selectedItems = preSelectedItems ?? new List<TItem>();
            this.PrintMultiChooseSection(items, labelSelector, selectedItems);
            this.output.SetCursorPosition(0, this.output.CursorTop - items.Count + index);
            ConsoleKeyInfo key;
            try
            {
                do
                {
                    key = await this.output.ReadKeyAsync(token, true);
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (index <= 0)
                            continue;

                        this.output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                        this.output.MoveCursorUp(0);
                        this.output.ClearCurrentLine();
                        this.PrintSelectedInMulti(items[--index], labelSelector, selectedItems);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (index >= items.Count - 1)
                            continue;

                        this.output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                        this.output.MoveCursorDown(0);
                        this.output.ClearCurrentLine();
                        this.PrintSelectedInMulti(items[++index], labelSelector, selectedItems);
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        var item = items[index];
                        if (selectedItems.Contains(item))
                        {
                            selectedItems.Remove(item);
                            this.output.ClearCurrentLine();
                            this.PrintSelected(item, labelSelector, false);
                        }
                        else
                        {
                            selectedItems.Add(item);
                            this.output.ClearCurrentLine();
                            this.PrintSelectedInMulti(item, labelSelector, selectedItems);
                        }
                    }
                } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

                this.output.ClearCurrentLine();
                this.PrintNonSelectedInMulti(items[index], labelSelector, selectedItems);
                this.output.SetCursorPosition(0, this.output.CursorTop + items.Count - index);
                this.output.WriteLine();

                return selectedItems;
            }
            catch (OperationCanceledException)
            {
                this.output.SetCursorPosition(0, this.output.CursorTop + items.Count - index);
                throw;
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

                this.output.WriteLine();
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

                this.output.WriteLine();
            }
        }

        private void PrintSelected<TItem>(TItem item,
            Func<TItem, string> labelSelector,
            bool isHighlight,
            bool showIndicator = false)
        {
            if (isHighlight)
            {
                this.output.WriteNonAnsiColor($"| {(showIndicator ? ">" : " ")} {labelSelector(item)}", ConsoleColor.White, ConsoleColor.DarkMagenta);
                return;
            }

            this.output.WriteNonAnsiColor("|", ConsoleColor.DarkGray);
            this.output.WriteNonAnsiColor($" > ", ConsoleColor.Magenta);
            this.output.Write(labelSelector(item));
        }

        private void PrintNonSelected<TItem>(TItem item, Func<TItem, string> labelSelector)
        {
            this.output.WriteNonAnsiColor($"|   {labelSelector(item)}", ConsoleColor.DarkGray);
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