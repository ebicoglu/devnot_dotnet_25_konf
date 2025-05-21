using System.Numerics.Tensors;
using System.Text;
using Classification;
using Microsoft.Extensions.AI;
Console.OutputEncoding = Encoding.UTF8;

var inputs = new[] {
    "OpenAI is the most popular AI tool",
    "The Galatasaray's manager is Okan Buruk",
    "COVID-19 infections are growing",
};

var categories = new[] {
    "Technology", 
    "Sport", 
    "Health" 
};

/*
    WORKS OFFLINE!
    .NET Smart Components library includes 
    sample convenience APIs for calculating embeddings (LocalEmbeddings) 
    locally on your server.
 */

 foreach (var input in inputs)
 {
     Console.WriteLine(input);
     var label = await ClassifyAsync(input);
     Console.WriteLine(" -=> Category: " + label + " <=-\n\r");
 }

 Console.ReadKey();
 return;

 async Task<string> ClassifyAsync(string text)
{
    // Better use models here => https://huggingface.co/models?pipeline_tag=zero-shot-classification
    var embeddingGenerator = new LocalEmbeddingsGenerator();

    // Don't do this in a real application as it's very inefficient. Firstly you should be
    // using a proper zero-shot classification model. Secondly, the candidate embeddings
    // could be precomputed, not recomputed for each call. Thirdly they could be indexed
    // for faster nearest-neighbour search.
    var inputEmbedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(text);

    var candidatesWithEmbeddings = await embeddingGenerator.GenerateAndZipAsync(categories);

    /*TensorPrimitives.CosineSimilarity -
    Kosinüs Benzerliği Nedir?
        Kosinüs benzerliği, iki vektör arasındaki açının kosinüsünü alarak benzerliği ölçer.

    Kullanım Amacı:
        Metin benzerliği (örneğin: cümle embedding'leri karşılaştırmak)
        Makine öğrenmesi çıktılarını kıyaslamak
        Tavsiye sistemlerinde benzer kullanıcı veya ürünleri bulmak
    */

    var candidates = candidatesWithEmbeddings.Select(candidate => new
    {
        candidate,
        score = TensorPrimitives.CosineSimilarity(candidate.Embedding.Vector.Span, inputEmbedding.Span)
    }).ToList();

    foreach (var x in candidates)
    {
        Console.WriteLine(" - " + x.candidate.Value + ": %" + Math.Round(x.score * 100));
    }

    return candidates
        .OrderByDescending(x => x.score)
        .Select(x => x.candidate.Value)
        .First();
}
