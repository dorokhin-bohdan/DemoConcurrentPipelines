using ConcurrentPipelines.Common.Extensions;
using Spectre.Console;

namespace ConcurrentPipelines.Common.Helpers;

public static class ConsoleHelper
{
    public static void PrintBlockMessage(string prefix, string message)
    {
        var color = prefix.ToHexColor();
        var escapedBlockName = prefix.EscapeMarkup();
        var formattedBlockName = $"[{color}][[{escapedBlockName}]][/]";
        var formattedThreadId = $"[blue][[Thread #{Environment.CurrentManagedThreadId}]][/]";

        AnsiConsole.MarkupLine($"{formattedBlockName}{formattedThreadId} {message.EscapeMarkup()}");
    }
}