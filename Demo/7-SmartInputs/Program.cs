using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
 
var openAiClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
builder.Services.AddSingleton(openAiClient);

var chatCompletionService = new OpenAIChatCompletionService("gpt-4o", openAiClient);
builder.Services.AddSingleton(chatCompletionService);


var app = builder.Build();

if (!app.Environment.IsDevelopment()) { app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Paste}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
