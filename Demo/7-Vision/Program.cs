using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using Vision;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

////////   1.) BUILD THE DEPENDENCY INJECTION   //////// 
var builder = Host.CreateApplicationBuilder();
var client = builder.Services.AddChatClient(
    new OpenAIChatClient(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")), "gpt-4o")
).UseFunctionInvocation().Build();

////////  2.) AIFunction  ////////
var options = new ChatOptions
{
    Tools = [
        AIFunctionFactory.Create(AnomalyDetected) /* THIS WILL BE EXECUTED IF SOMETHING WRONG */
    ],
    Temperature = 0 /* highly consistent, factual, and repeatable outputs */
};

//////// 3.) ITERATE IMAGES  ////////
var imageFiles = Directory.GetFiles("cam", "*.png");
foreach (var imagePath in imageFiles)
{
    await ExtractInfoAsync(imagePath);
}

Console.ReadKey();
return;

async Task ExtractInfoAsync(string imagePath)
{
    var cameraName = Path.GetFileNameWithoutExtension(imagePath);

    var message = new ChatMessage(ChatRole.User, new List<AIContent>
    {
        new TextContent("You are a Quality Assurance Agent. " +
                        "Respond with anomalies that are visible. Do not assume or speculate! " +
                        $"This image is from the camera: {cameraName}. " +
                        "Extract information from this camera. " +
                        "Call `AnomalyDetected` function if egg is cracked, package damaged, camera is broken."),

        new ImageContent(await File.ReadAllBytesAsync(imagePath), "image/png")
    });

    var response = await client.CompleteAsync<CameraEntity>([message], options);
    var serialized = JsonSerializer.Serialize(response.Result, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(serialized + "\n" + new string('_', 30) + "\n");
}

//Detect traffic alerts, camera issues, frauds, spams, audit logs, defective products, faulty machines, etc...
void AnomalyDetected(CameraEntity cameraEntity, string reason)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"*** ALERT *** \n{cameraEntity.CameraName}: {reason}");
    Console.ResetColor();
}
