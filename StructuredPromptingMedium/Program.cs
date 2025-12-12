using System.ClientModel;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using OpenAI;
using OpenAI.Chat;
using StructuredPromptingMedium.Models;
using StructuredPromptingMedium.Utils;

string fileContent = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "complex_data.json"));
JsonNode responseSchema = GetSchema();

ConsoleHelper.CreateHeader();

string openAIKey = GetOpenAIKey();

OpenAIClient client = new(openAIKey);
ChatClient chatClient = client.GetChatClient(Statics.GPT_MODEL);

ChatCompletionOptions options = new()
{
    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "SalaryByDepartmentResponse",
        BinaryData.FromString(responseSchema.ToString()),
        "Schema for average salary grouped by department",
        true),
};

List<ChatMessage> messages =
[
    new SystemChatMessage(
        "You are a careful data analyst. Your task: compute the average salary by department from the provided employee data. " +
        "Return ONLY a JSON value that matches the JSON Schema. No prose. No markdown. No extra keys. " +
        "Use the Department string exactly as found in the data. AverageSalary must be a decimal number."),
    new UserChatMessage(
        $"Employee data (JSON):{Environment.NewLine}{fileContent}{Environment.NewLine}{Environment.NewLine}" +
        "Question: What's the average salary by department?"),
];

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]USER:[/]");
AnsiConsole.WriteLine("What's the average salary by department?");

Stopwatch stopwatch = Stopwatch.StartNew();
ClientResult<ChatCompletion> chatCompletionsResponse = await chatClient.CompleteChatAsync(messages, options);
stopwatch.Stop();

string rawModelText = chatCompletionsResponse.Value.Content[0].Text;

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]MODEL (raw JSON):[/]");
AnsiConsole.WriteLine(rawModelText);

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]RESULT (parsed):[/]");

SalaryByDepartmentResponse? parsed = TryDeserializeStructuredResponse(rawModelText);
if (parsed is null || parsed.Items.Count == 0)
{
    AnsiConsole.MarkupLine("[red]Could not parse a valid structured response.[/]");
}
else
{
    Table table = new();
    table.Border(TableBorder.Ascii);
    table.Expand();
    table.AddColumn("Department");
    table.AddColumn(new TableColumn("AverageSalary").RightAligned());

    foreach (SalaryByDepartment row in parsed.Items.OrderBy(r => r.Department, StringComparer.OrdinalIgnoreCase))
    {
        table.AddRow(row.Department, row.AverageSalary.ToString("0.00"));
    }

    AnsiConsole.Write(table);
}

ChatTokenUsage usageInfo = chatCompletionsResponse.Value.Usage;
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"[grey]Tokens: prompt={usageInfo.InputTokenCount}, completion={usageInfo.OutputTokenCount}. Duration={stopwatch.ElapsedMilliseconds}ms.[/]");

static JsonNode GetSchema()
{
    // Manually construct the JSON schema to comply with OpenAI's requirements
    JsonObject schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["Items"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["Department"] = new JsonObject
                        {
                            ["type"] = "string"
                        },
                        ["AverageSalary"] = new JsonObject
                        {
                            ["type"] = "number"
                        }
                    },
                    ["required"] = new JsonArray { "Department", "AverageSalary" },
                    ["additionalProperties"] = false
                }
            }
        },
        ["required"] = new JsonArray { "Items" },
        ["additionalProperties"] = false
    };
    
    return schema;
}

static string GetOpenAIKey()
{
    const string envVar = "OPENAI_API_KEY";
    string? keyFromEnv = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(keyFromEnv))
    {
        return keyFromEnv.Trim();
    }

    return ConsoleHelper.GetSecretString($"Please insert your [yellow]OpenAI[/] API key (or set [grey]{envVar}[/]):");
}

static SalaryByDepartmentResponse? TryDeserializeStructuredResponse(string raw)
{
    if (string.IsNullOrWhiteSpace(raw))
    {
        return null;
    }

    // With JSON-schema response format this should already be pure JSON,
    // but we keep this defensive cleanup for demos/blog posts.
    string trimmed = raw.Trim();
    if (trimmed.StartsWith("```", StringComparison.Ordinal))
    {
        int firstNewline = trimmed.IndexOf('\n');
        if (firstNewline >= 0)
        {
            trimmed = trimmed[(firstNewline + 1)..];
        }
        trimmed = trimmed.Replace("```", string.Empty).Trim();
    }

    JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    try
    {
        using var _ = JsonDocument.Parse(trimmed);
        return JsonSerializer.Deserialize<SalaryByDepartmentResponse>(trimmed, jsonOptions);
    }
    catch
    {
        return null;
    }
}