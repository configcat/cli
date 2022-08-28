namespace System;

internal static class StringExtension
{
    public static string TrimToFitColumn(this string text)
        => text == null ? "\"\"" : text.Length > 30 ? $"\"{text[0..28]}...\"" : $"\"{text}\"";
}