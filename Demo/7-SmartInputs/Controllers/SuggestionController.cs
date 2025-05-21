using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class SuggestionController(OpenAIChatCompletionService chatCompletionService) : Controller
{
    public IActionResult Index() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Get([FromBody] SmartTextAreaRequest request)
    {
        try
        {
            var response = await GetSuggestionFromAI(request.TextBefore, request.TextAfter);
            return Json(new { success = true, suggestion = response });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    private async Task<string> GetSuggestionFromAI(string textBefore, string textAfter)
    {
        const string systemPrompt =
            @"Predict what text would insert at the cursor position indicated by ^^^.
Only give predictions for which you have an EXTREMELY high confidence that the user would insert that EXACT text.
Do not make up new information. If you're not sure, just reply with NO_PREDICTION.

RULES:
1. Reply with OK:, then in square brackets the predicted text, then END_INSERTION, and no other output.
2. If there isn't enough information to predict any words that the user would type next, just reply with the word NO_PREDICTION.
3. NEVER invent new information. If you can't be sure what the user is about to type, ALWAYS stop the prediction with END_INSERTION.";

        ChatHistory chatHistory =
        [
            new ChatMessageContent(AuthorRole.System, systemPrompt),
            new ChatMessageContent(AuthorRole.User, $"{textBefore}^^^{textAfter}")
        ];

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.7f, // Lower temperature for more focused, deterministic suggestions
            MaxTokens = 100,    // Limit response length for suggestions
            FrequencyPenalty = 0.1f, // Slight frequency penalty to avoid repetition
            PresencePenalty = 0.1f   // Slight presence penalty to encourage diversity
        };

        var response = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings);

        if (response.Count == 0)
        {
            Console.WriteLine("NO AI RESPONSE!");
            return string.Empty;
        }

        var rawResponse = response.First().Content;
        Console.WriteLine($"AI RESPONSE: {rawResponse}");

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return string.Empty;
        }

        if (rawResponse == "NO_PREDICTION")
        {
            return string.Empty;
        }

        //normalize AI response
        rawResponse = rawResponse
            .Replace("OK: [", "OK:[")
            .Replace("] END_INSERTION", "]END_INSERTION");
        if (!rawResponse.StartsWith("OK:[", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var suggestion = rawResponse.Substring(4).TrimEnd(']', ' ');

        // Clean up the response by removing the "] END_INSERTION" suffix
        if (suggestion.EndsWith("]END_INSERTION"))
        {
            suggestion = suggestion.Substring(0, suggestion.Length - "]END_INSERTION".Length);
        }

        // Trim after first sentence if multiple sentences
        var trimAfter = suggestion.IndexOfAny(['.', '?', '!']);
        if (trimAfter > 0 && suggestion.Length > trimAfter + 1 && suggestion[trimAfter + 1] == ' ')
        {
            suggestion = suggestion.Substring(0, trimAfter + 1);
        }

        // Remove leading space if there's already a space before cursor
        if (textBefore.Length > 0 && textBefore[textBefore.Length - 1] == ' ')
        {
            suggestion = suggestion.TrimStart(' ');
        }

        return suggestion;
    }
}

public class SmartTextAreaRequest
{
    public string TextBefore { get; set; } = string.Empty;
    public string TextAfter { get; set; } = string.Empty;
}