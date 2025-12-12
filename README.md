# Structured Prompting in real projects — checklist & best practices

This repo is a small .NET console app that demonstrates **structured prompting** (schema-first prompting) end-to-end:

- Define a strict output contract (JSON Schema)
- Ask the LLM to return **only** JSON that matches that contract
- Parse + validate like any other external input
- Use the result directly in your program (table output)

If you want the “full narrative”: this project is meant to accompany a blog post in the direction of
**Structured Prompting in real projects — checklist & best practices**.

> Note: [To my blog post.](https://medium.com/medialesson/structured-prompting-in-real-projects-checklist-best-practices-c39fa789856b)

---

## Why you should care about structured prompting
Working with LLMs is fun. At first you write a prompt, get a response, copy‑paste, maybe tweak it by hand.
But once you start building real systems (chatbots, pipelines, data extraction tools…), suddenly you're dealing with:

- inconsistent outputs
- parsing failures
- manual cleanup
- and the creeping sense you might not hire yourself in six months if things grow

Free‑form prompting feels flexible, but in the long run it becomes breakable.
Small changes to phrasing or context can lead to big shifts in output structure.
That means fragile parsers (often regex-based), subtle bugs, and unpredictable integration behavior.

**Structured prompting** treats your prompt as a **data contract**:
you define exactly what the output should look like (fields, types, constraints),
you ask the LLM to honor that contract,
and you parse/validate the output like you would any other external data source.

At that point, you're effectively calling a simple API that returns data.

---

## A quick note on TOON (structured input + structured output)
If you followed my earlier deep dive into **TOON** ("JSON vs. TOON: A New Era of Structured Input"),
you already know how a more compact and semantically clean format can reduce ambiguity **before** the prompt even reaches the model.

TOON is not just a clever encoding trick — it's an example of how structured input and structured output naturally complement each other.
If you combine both ideas, you get a workflow that is more predictable, more token‑efficient, and less fragile as your system grows.

This repo focuses on the next step in that direction: **structured output**, backed by strict schemas and automatically validated responses.

---

## Practical benefits & where it pays off
Structured prompting shines especially in these scenarios:

- **Data extraction** from unstructured text (emails, tickets, logs, scraped HTML…) → output an object, not a story
- **Automating pipelines** → let the model produce JSON, then feed it into databases/APIs/other processing (no cleanup step)
- **Generating content with a fixed template** → required fields, enums, constraints instead of free‑text chaos
- **Maintaining and versioning prompts** → prompts become more like code: diffable, testable, maintainable

In larger projects or production environments, structured prompting often isn't a "nice to have" — it becomes essential.

---

## Checklist: best practices for structured prompting 🧰
Here’s a practical checklist to follow when using structured prompts:

### 1) Define a clear schema
Use JSON Schema (or your language’s typed models) to define exactly what output you expect:

- field names
- data types
- required vs. optional fields
- enums / constraints where appropriate

### 2) Provide a template / example output in your prompt
Instead of saying "please output JSON", show a minimal example (empty or with placeholder values).
This guides the model on structure and expected types and increases consistency.

### 3) Keep schemas as simple as possible initially
Start with only a few required fields.
Once outputs are reliable, extend the schema.

### 4) Validate every output programmatically
Do not assume the LLM always follows the schema.
Always parse and validate.
If parsing fails, implement retry logic or fallback handling.

### 5) Make schema and prompt versioned & maintainable
Treat prompt and schema like source code:
put them under version control and document them.
Changes should be traceable and reviewable.

### 6) Know when not to use structured prompting
For creative, open‑ended tasks (storytelling, marketing copy, brainstorming), a rigid schema may hamper quality.
Use structured prompting when you want precision, consistency, and machine-readable outputs.

---

## Minimal .NET example (as used in this repo)

### 1) Define your DTOs

```csharp
public class SalaryByDepartment
{
    public string Department { get; set; }
    public decimal AverageSalary { get; set; }
}

public class SalaryByDepartmentResponse
{
    public List<SalaryByDepartment> Items { get; set; }
}
```

### 2) Define a JSON Schema (simplified)

```json
{
  "type": "object",
  "properties": {
    "Items": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "Department": { "type": "string" },
          "AverageSalary": { "type": "number" }
        },
        "required": ["Department", "AverageSalary"],
        "additionalProperties": false
      }
    }
  },
  "required": ["Items"],
  "additionalProperties": false
}
```

### 3) Force structured output + validate

```csharp
var options = new ChatCompletionOptions
{
    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "SalaryByDepartmentResponse",
        BinaryData.FromString(schemaJson),
        "Schema for average salary grouped by department",
        true
    )
};

var messages = new List<ChatMessage>
{
    new SystemChatMessage(
        "You are a precise data analyst. Return ONLY valid JSON matching the JSON Schema."
    ),
    new UserChatMessage("Compute the average salary per department from this data: ...")
};

var result = await chatClient.CompleteChatAsync(messages, options);
var rawJson = result.Value.Content[0].Text;

// Parse + validate (always)
var parsed = JsonSerializer.Deserialize<SalaryByDepartmentResponse>(rawJson);
```

This is rudimentary, but it’s precisely this approach that makes LLM integrations maintainable, robust, and automatable.

---

## Output of the sample application
The app prints:

- the raw JSON returned by the model
- a parsed table of departments and averages
- token usage and duration

---

## What structured prompting does *not* solve (caveats & limitations)

- A schema can guarantee **structure**, but not **correctness**.
  The model may still output garbage — structurally valid JSON with wrong numbers.
- Complex or deeply nested schemas increase the risk of failures and token usage.
- Very large outputs can still be cut off or incomplete.
- In creative scenarios, structure can limit freedom — sometimes you want free text, not rigid objects.

---

## Final thoughts
Structured prompting is no longer just a toy.
If you expect outputs to be further processed, versioned, or automated, the small overhead of schema definition and validation quickly pays off:

- fewer parsing errors
- more predictable integrations
- cleaner interfaces between the model and your software

If you continue as before with JSON vs. TOON for token efficiency and clean data formats,
then structured output could be your next upgrade.

---

## Run the sample

### Prerequisites
- .NET SDK matching the target framework in the project file
  - The project currently targets `net10.0` (requires a .NET 10 SDK)
- An OpenAI API key

### Option A: provide the API key via environment variable

```bash
export OPENAI_API_KEY="..."

dotnet run --project StructuredPromptingMedium/StructuredPromptingMedium.csproj
```

### Option B: enter the key interactively
If `OPENAI_API_KEY` is not set, the app prompts you for the key (masked input).

---

## Where to look in the code

- `StructuredPromptingMedium/Program.cs`
  - JSON Schema definition (`GetSchema()`)
  - `ChatResponseFormat.CreateJsonSchemaFormat(...)` usage
  - parsing + validation (`TryDeserializeStructuredResponse`)
- `StructuredPromptingMedium/Models/SalaryByDepartment.cs`
- `StructuredPromptingMedium/Models/SalaryByDepartmentResponse.cs`

---

## Notes
- The default model is configured in `StructuredPromptingMedium/Utils/Statics.cs`.
- `complex_data.json` is copied to the output directory so the sample can run without manual file moves.
