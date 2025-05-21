using System.Text.Json;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;

public class PasteController(OpenAIClient openAiClient) : Controller
{
    public IActionResult Index() { return View(); }

    [HttpPost]
    public async Task<IActionResult> ProcessPaste([FromBody] string pastedText)
    {
        try
        {
            var aiResponse = await SendToAi(pastedText);
            var car = TransformToObject(aiResponse);
            return Json(new { success = true, car = car });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    private async Task<IReadOnlyList<ChatMessageContent>> SendToAi(string pastedText)
    {
        ChatHistory chatHistory =
        [
            new ChatMessageContent(AuthorRole.System,
                " You are an assistant that extracts car information from the text pasted from clipboard" +
                " and returns it strictly as JSON following schema: " + GetJsonSchema<CarEntity>() +
                " Respond with JSON only. Do not include any explanation or extra text. Here's the text: " + Environment.NewLine + pastedText,
                modelId:  "gpt-4o-mini")
        ];

        var textService = new OpenAIChatCompletionService("gpt-4o-mini", openAiClient);
        return await textService.GetChatMessageContentsAsync(chatHistory);
    }

    private static CarEntity? TransformToObject(IReadOnlyList<ChatMessageContent> response)
    {
        if (response.Count == 0)
        {
            return null;
        }

        var rawResponse = response.First().Content;
        Console.WriteLine(rawResponse);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return null;
        }

        rawResponse = TrimJsonWrapper(rawResponse);

        return JsonSerializer.Deserialize<CarEntity>(rawResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private static string GetJsonSchema<T>()
    {
        return JsonSerializer.Serialize(
            JsonSerializerOptions.Default.GetJsonSchemaAsNode(
                typeof(T),
                new() { TreatNullObliviousAsNonNullable = true }
            )
        );
    }

    private static string TrimJsonWrapper(string aiRawResponse)
    {
        return aiRawResponse
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();
    }

}