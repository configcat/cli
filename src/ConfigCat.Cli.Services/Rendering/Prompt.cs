using ConfigCat.Cli.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering;

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

public class Prompt(IOutput output, CliOptions options) : IPrompt
{
    private const int DefaultPageSize = 15;

    public async Task<string> GetStringAsync(string label,
        CancellationToken token,
        string defaultValue = null)
    {
        if (token.IsCancellationRequested ||
            output.IsOutputRedirected ||
            options.IsNonInteractive)
            return defaultValue;

        output.Write(label)
            .WriteDarkGray(!defaultValue.IsEmpty() ? $" [default: '{defaultValue}']" : "")
            .Write(": ");

        var result = await output.ReadLineAsync(token);
        output.WriteLine();
        return result.IsEmpty() ? defaultValue : result;
    }

    public async Task<string> GetMaskedStringAsync(string label,
        CancellationToken token,
        string defaultValue = null)
    {
        if (token.IsCancellationRequested ||
            output.IsOutputRedirected ||
            options.IsNonInteractive)
            return defaultValue;

        output.Write(label)
            .WriteDarkGray(!defaultValue.IsEmpty() ? $" [default: '{defaultValue}']" : "")
            .Write(": ");

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
        if (token.IsCancellationRequested ||
            output.IsOutputRedirected ||
            options.IsNonInteractive)
            return default;

        using var _ = output.CreateCursorHider();

        output.Write(label).Write(":").WriteLine();

        output.WriteDarkGray("(Use the ").WriteCyan("UP").WriteDarkGray(" and ").WriteCyan("DOWN").WriteDarkGray(" keys to navigate)")
            .WriteLine().WriteLine();

        var pages = this.GetPages(items);
        var pageIndex = selectedValue is null || selectedValue.Equals(default(TItem)) ? 0 : pages.PageIndexOf(selectedValue);
        var page = pages[pageIndex];
        int index = this.PrintChooseSection(page, selectedValue, labelSelector, pageIndex, pages.Count);
        ConsoleKeyInfo key;
        try
        {
            do
            {
                key = await output.ReadKeyAsync(token, true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index <= 0)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelected(page[index], labelSelector);
                        output.MoveCursorUp(0).ClearCurrentLine();
                        this.PrintSelected(page[--index], labelSelector, false);
                        break;

                    case ConsoleKey.DownArrow:
                        if (index >= page.Count - 1 || page[index + 1] is null || page[index + 1].Equals(default(TItem)))
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelected(page[index], labelSelector);
                        output.MoveCursorDown(0).ClearCurrentLine();
                        this.PrintSelected(page[++index], labelSelector, false);
                        break;

                    case ConsoleKey.LeftArrow:
                        if (pageIndex <= 0)
                            continue;

                        output.SetCursorPosition(0, output.CursorTop - index);
                        page = pages[--pageIndex];
                        index = this.PrintChooseSection(page, selectedValue, labelSelector, pageIndex, pages.Count);
                        break;

                    case ConsoleKey.RightArrow:
                        if (pageIndex >= pages.Count - 1)
                            continue;

                        output.SetCursorPosition(0, output.CursorTop - index);
                        page = pages[++pageIndex];
                        index = this.PrintChooseSection(page, selectedValue, labelSelector, pageIndex, pages.Count);
                        break;
                }
            } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

            output.ClearCurrentLine();
            this.PrintSelected(page[index], labelSelector, true);
            output.SetCursorPosition(0, output.CursorTop + page.Count - index).WriteLine().ClearCurrentLine();

            return page[index];
        }
        catch (OperationCanceledException)
        {
            output.SetCursorPosition(0, output.CursorTop + page.Count - index).ClearCurrentLine();
            throw;
        }
    }

    public async Task<List<TItem>> ChooseMultipleFromListAsync<TItem>(string label,
        List<TItem> items,
        Func<TItem, string> labelSelector,
        CancellationToken token,
        List<TItem> preSelectedItems = null)
    {
        if (token.IsCancellationRequested ||
            output.IsOutputRedirected ||
            options.IsNonInteractive)
            return default;

        using var _ = output.CreateCursorHider();

        output.Write(label).Write(":").WriteLine();

        output.WriteDarkGray("(Use the ")
            .WriteCyan("UP")
            .WriteDarkGray(" and ")
            .WriteCyan("DOWN")
            .WriteDarkGray(" keys to navigate, and ")
            .WriteCyan("SPACE")
            .WriteDarkGray(" to select)")
            .WriteLine().WriteLine();

        int index = 0, pageIndex = 0;
        var selectedItems = preSelectedItems?.ToList() ?? [];

        var pages = this.GetPages(items);
        var page = pages[pageIndex];
        this.PrintMultiChooseSection(page, labelSelector, selectedItems, pageIndex, pages.Count);
        ConsoleKeyInfo key;
        try
        {
            do
            {
                key = await output.ReadKeyAsync(token, true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index <= 0)
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(page[index], labelSelector, selectedItems);
                        output.MoveCursorUp(0).ClearCurrentLine();
                        this.PrintSelectedInMulti(page[--index], labelSelector, selectedItems);
                        break;

                    case ConsoleKey.DownArrow:
                        if (index >= page.Count - 1 || page[index + 1] is null || page[index + 1].Equals(default(TItem)))
                            continue;

                        output.ClearCurrentLine();
                        this.PrintNonSelectedInMulti(page[index], labelSelector, selectedItems);
                        output.MoveCursorDown(0).ClearCurrentLine();
                        this.PrintSelectedInMulti(page[++index], labelSelector, selectedItems);
                        break;

                    case ConsoleKey.LeftArrow:
                        if (pageIndex <= 0)
                            continue;

                        output.SetCursorPosition(0, output.CursorTop - index);
                        page = pages[--pageIndex];
                        index = 0;
                        this.PrintMultiChooseSection(page, labelSelector, selectedItems, pageIndex, pages.Count);
                        break;

                    case ConsoleKey.RightArrow:
                        if (pageIndex >= pages.Count - 1)
                            continue;

                        output.SetCursorPosition(0, output.CursorTop - index);
                        page = pages[++pageIndex];
                        index = 0;
                        this.PrintMultiChooseSection(page, labelSelector, selectedItems, pageIndex, pages.Count);
                        break;

                    case ConsoleKey.Spacebar:
                        var item = page[index];
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
                        break;
                }
            } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter);

            output.ClearCurrentLine();
            this.PrintNonSelectedInMulti(page[index], labelSelector, selectedItems);
            output.SetCursorPosition(0, output.CursorTop + page.Count - index + (pages.Count > 1 ? 1 : 0)).ClearCurrentLine().WriteLine();

            return selectedItems;
        }
        catch (OperationCanceledException)
        {
            output.SetCursorPosition(0, output.CursorTop + page.Count - index + (pages.Count > 1 ? 1 : 0)).ClearCurrentLine();
            throw;
        }
    }

    private List<List<T>> GetPages<T>(List<T> source)
    {
        var bufferHeight = output.BufferHeight - 4;
        var pageSize = DefaultPageSize > bufferHeight ? bufferHeight : DefaultPageSize;
        return source.SplitFilled(pageSize);
    }

    private int PrintChooseSection<TItem>(
        List<TItem> items,
        TItem selectedItem,
        Func<TItem, string> labelSelector,
        int pageIndex,
        int pageLength)
    {
        var index = selectedItem is null || selectedItem.Equals(default(TItem)) ? 0 : items.IndexOf(selectedItem);
        index = index == -1 ? 0 : index;
        foreach (var item in items)
        {
            output.ClearCurrentLine();
            if (items.IndexOf(item) == index)
                this.PrintSelected(item, labelSelector, false);
            else
                this.PrintNonSelected(item, labelSelector);

            output.WriteLine();
        }

        if (pageLength > 1)
        {
            this.RenderPageSection(pageIndex, pageLength);
            output.SetCursorPosition(0, output.CursorTop - items.Count - 2 + index);
            return index;
        }

        output.SetCursorPosition(0, output.CursorTop - items.Count + index);
        return index;
    }

    private void PrintMultiChooseSection<TItem>(
        List<TItem> items,
        Func<TItem, string> labelSelector,
        List<TItem> preSelectedItems,
        int pageIndex,
        int pageLength)
    {
        foreach (var item in items)
        {
            output.ClearCurrentLine();
            int index = items.IndexOf(item);
            if (preSelectedItems.Contains(item))
                this.PrintSelected(item, labelSelector, true, index == 0);
            else if (index == 0)
                this.PrintSelected(item, labelSelector, false);
            else
                this.PrintNonSelected(item, labelSelector);

            output.WriteLine();
        }

        if (pageLength > 1)
        {
            this.RenderPageSection(pageIndex, pageLength);
            output.SetCursorPosition(0, output.CursorTop - items.Count - 2);
            return;
        }

        output.SetCursorPosition(0, output.CursorTop - items.Count);
    }

    private void RenderPageSection(int pageIndex, int pageLength)
    {
        output.WriteLine()
            .WriteColor("Page: ", ConsoleColor.DarkGray)
            .WriteColor($"{pageIndex + 1} / {pageLength}", ConsoleColor.Green)
            .WriteColor(" (Use the ", ConsoleColor.DarkGray)
            .WriteColor("<", ConsoleColor.Cyan)
            .WriteColor(" and ", ConsoleColor.DarkGray)
            .WriteColor(">", ConsoleColor.Cyan)
            .WriteColor(" keys to scroll between pages)", ConsoleColor.DarkGray)
            .WriteLine();
    }

    private void PrintSelected<TItem>(TItem item,
        Func<TItem, string> labelSelector,
        bool isHighlight,
        bool showIndicator = false)
    {
        if (isHighlight)
        {
            output.WriteColor($"| {(showIndicator ? ">" : " ")} {labelSelector(item)}", ConsoleColor.White, ConsoleColor.DarkMagenta);
            return;
        }

        output.WriteColor("|", ConsoleColor.DarkGray).WriteColor($" > ", ConsoleColor.Magenta).Write(labelSelector(item));
    }

    private void PrintNonSelected<TItem>(TItem item, Func<TItem, string> labelSelector)
    {
        if (item is null || item.Equals(default(TItem)))
        {
            output.WriteColor($"|", ConsoleColor.DarkGray);
            return;
        }

        output.WriteColor($"|   {labelSelector(item)}", ConsoleColor.DarkGray);
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