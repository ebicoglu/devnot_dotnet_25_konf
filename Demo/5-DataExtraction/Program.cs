using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;

////////   1.) BUILD THE DEPENDENCY INJECTION   //////// 
var builder = Host.CreateApplicationBuilder();
var client = builder.Services.AddChatClient(new OpenAIChatClient(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")), "gpt-4o")).Build();

////////   2.) BOOKS   //////// 
@"
Oğuz Atay’ın kaleminden çıkan 724 sayfalık Tutunamayanlar, modern bireyin toplumla çatışmasını anlatır.
To Kill a Mockingbird is a 336-page novel by Harper Lee, dealing with racism and justice.
"
.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
.ToList()
.ForEach(ExtractAsync);


Console.ReadKey();
return;

async void ExtractAsync(string line)
{
    var message = "Extract information about the book from the text: " + line;
    var response = await client.CompleteAsync<BookInfo>(message);
    if (response.TryGetResult(out var bookInfo))
    {
        var serialized = JsonSerializer.Serialize(bookInfo, 
            new JsonSerializerOptions
            {
                WriteIndented = true, 
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        Console.WriteLine(serialized);
    }
}

internal record BookInfo(
    BookLanguage Language,
    string Title,
    string Author,
    int PageCount,
    string Summary
);

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum BookLanguage { Turkish, English }



/* SAMPLE INPUTS
Oğuz Atay’ın kaleminden çıkan 724 sayfalık Tutunamayanlar, modern bireyin toplumla çatışmasını anlatır.
İnce Mehmed, Yaşar Kemal’in yazdığı, 432 sayfada halk kahramanlığını destanlaştıran bir romandır.
Beyaz Gemi, Cengiz Aytmatov’un 144 sayfalık mitolojik öğelerle bezeli yalnızlık hikayesidir.
Mehmet Rauf’un Eylül adlı eseri, 296 sayfa boyunca melankolik bir aşk üçgenini işler.
To Kill a Mockingbird is a 336-page novel by Harper Lee, dealing with racism and justice.
1984, written by George Orwell, spans 328 pages and explores surveillance and totalitarianism.
The Great Gatsby by F. Scott Fitzgerald is a 180-page story about wealth and broken dreams.
The Catcher in the Rye, 277 pages by J.D. Salinger, focuses on teenage rebellion and hypocrisy.
*/
