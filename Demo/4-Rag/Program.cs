using System.Text;
using LangChain.Databases;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Splitters.Text;
Console.OutputEncoding = Encoding.UTF8;

/*DECLARE*/
var vectorDb = new SqLiteVectorDatabase("my-vector-database.db");

var embeddingModel = new OpenAiEmbeddingModel(
    provider: new OpenAiProvider(apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")),
    id: "text-embedding-ada-002" /* Better performance:  text-embedding-3-large > text-embedding-ada-002 > text-embedding-3-small */
);

await CreateVectorDb();

var vectorCollection = await vectorDb.GetCollectionAsync("robot_mop");

var model = new OpenAiChatModel(
    provider: new OpenAiProvider(apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")),
    id: "gpt-3.5-turbo-0125"
);

while (true)
{
    /* 
       %5 şarjım var. Yazılım güncelleme yapabilir miyim?
       Hangi modlar vardır? 
    */
    Console.Write("YOU: ");
    var inputText = Console.ReadLine();

    var similarDocuments = await vectorCollection.GetSimilarDocuments(
        request: inputText,
        embeddingModel: embeddingModel,
        amount: 3, //max count of similar results
        searchType: VectorSearchType.Similarity,
        scoreThreshold: 0.75F
    );

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"\n{similarDocuments.Count} references found in the PDF...\n");

    var prompt = $@"Based on the Mi Robot Vacuum Mop user manual, 
answer the question briefly as accurately and concisely as possible.
Use the information below to answer the question.

### Retrieved Context:
{similarDocuments.AsString(Environment.NewLine)}

### Question:
{inputText}

### Answer:";

    var answer = await model.GenerateAsync(prompt, new OpenAiChatSettings
    {
        Temperature = 0.1
    });

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("AGENT: " + answer + "\n");
    Console.ResetColor();
}

async Task CreateVectorDb()
{
    var docsAdded = await vectorDb.IsCollectionExistsAsync("robot_mop");

    var collection = await vectorDb.GetOrCreateCollectionAsync(
        collectionName: "robot_mop",
        dimensions: 3072 /* text-embedding-3-large dimension is 3072.  text-embedding-ada-002 dimension is 1536 */
    );

    if (docsAdded)
    {
        return;
    }

    await collection.AddDocumentsFromAsync<PdfPigPdfLoader>(
        embeddingModel: embeddingModel,
        dataSource: DataSource.FromPath("robot-mop.pdf"),
        textSplitter: new RecursiveCharacterTextSplitter(
            chunkSize: 500, //≈ 350–400 words in Turkish, which balances context and relevance.
            chunkOverlap: 50) //50 ensures continuity for long instructions or procedures split across chunks.
        );

    Console.WriteLine("PDF vectorized.");
}