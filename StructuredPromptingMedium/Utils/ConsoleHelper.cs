using Spectre.Console;
using StructuredPromptingMedium.Models;

namespace StructuredPromptingMedium.Utils;

internal static class ConsoleHelper
{
    public static void CreateHeader()
    {
        AnsiConsole.Clear();

        Grid grid = new();
        grid.AddColumn();
        grid.AddRow(new FigletText("Structured Output").Centered().Color(Color.Red));
        grid.AddRow(Align.Center(new Panel("[red]Sample by Marius Schröder ([link]https://marius-schroeder.de[/])[/]")));

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();
    }
    
    public static string GetSecretString(
        string prompt)
    {
        CreateHeader();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .Secret()
                .PromptStyle("white")
                .ValidationErrorMessage("[red]Invalid prompt[/]")
                .Validate(prompt =>
                {
                    if (prompt.Length < 3)
                    {
                        return ValidationResult.Error("[red]Value too short[/]");
                    }

                    if (prompt.Length > 200)
                    {
                        return ValidationResult.Error("[red]Value too long[/]");
                    }

                    return ValidationResult.Success();
                }));
    }

    public static string GetString(
        string prompt)
    {
        CreateHeader();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
            .PromptStyle("white")
            .ValidationErrorMessage("[red]Invalid prompt[/]")
            .Validate(prompt =>
            {
                if (prompt.Length < 3)
                {
                    return ValidationResult.Error("[red]Value too short[/]");
                }

                if (prompt.Length > 200)
                {
                    return ValidationResult.Error("[red]Value too long[/]");
                }

                return ValidationResult.Success();
            }));
    }
}