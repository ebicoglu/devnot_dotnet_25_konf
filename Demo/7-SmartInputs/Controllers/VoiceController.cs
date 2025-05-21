using System.Text.Json;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

public class VoiceController(OpenAIClient openAiClient) : Controller
{
    public IActionResult Index() { return View(); }

    [HttpPost]
    public async Task<IActionResult> ProcessAudio(IFormFile file)
    {
        try
        {
            var audio = await GetAudio(file);
            var response = await SendToAi(audio);
            var car = TransformToObject(response);

            return Json(new { success = true, car });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return Json(new { success = true, error = ex.Message });
        }
    }

    private static CarEntity? TransformToObject(IReadOnlyList<ChatMessageContent> response)
    {
        if (response.Count == 0)
        {
            return null;
        }
        var rawResponse = response.First().Content;
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CarEntity>(rawResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task<IReadOnlyList<ChatMessageContent>> SendToAi(byte[] audio)
    {
        ChatHistory chatHistory =
        [
            new ChatMessageContent(AuthorRole.System,
                " You are an assistant that extracts car information from audio" +
                " and returns it strictly as JSON following schema: " + GetJsonSchema<CarEntity>() +
                " Respond with JSON only. Do not include any explanation or extra text."),

                new ChatMessageContent(AuthorRole.User, [new AudioContent(audio, mimeType: "audio/mp3")])
        ];

        var audioService = new OpenAIChatCompletionService("gpt-4o-audio-preview", openAiClient);
        return await audioService.GetChatMessageContentsAsync(chatHistory);
    }

    private static async Task<byte[]> GetAudio(IFormFile audio)
    {
        using var stream = new MemoryStream();
        await audio.CopyToAsync(stream);
        var file = stream.ToArray();
        //await System.IO.File.WriteAllBytesAsync("audio_debug.mp3", file); // for debugging
        return file;
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

}