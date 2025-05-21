using System.Numerics.Tensors;
using Microsoft.Extensions.AI;
using SmartComponents.LocalEmbeddings;

namespace Classification;

/*
 * The .NET Smart Components include sample convenience APIs for calculating embeddings (LocalEmbeddings) locally on your server.
 * These can be used to compare the semantic similarity of text.
 * These APIs have now been updated to wrap the ONNX-based embeddings support in Semantic Kernel and
 * then demonstrate how you can build further capabilities on top,
 * such as automatic model acquisition, simplified semantic search,
 * and alternative embedding representations. If you find these additions useful,
 * you can include them in your own apps and libraries.
 * Otherwise, you can just use the Semantic Kernel APIs directly.
 */
public class LocalEmbeddingsGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly LocalEmbedder _embedder = new();

    public EmbeddingGeneratorMetadata Metadata { get; } = new("local");

    public void Dispose() => _embedder.Dispose();

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var embeddings = _embedder.EmbedRange(values);
        var result = new GeneratedEmbeddings<Embedding<float>>(embeddings.Select(e =>
            new Embedding<float>(Normalize(e.Embedding.Values))));
        return Task.FromResult(result);
    }

    public object? GetService(Type serviceType, object? key = null)
        => key is null ? this : null;

    private static ReadOnlyMemory<float> Normalize(ReadOnlyMemory<float> vec)
    {
        var buffer = new float[vec.Length];
        TensorPrimitives.Divide(vec.Span, TensorPrimitives.Norm(vec.Span), buffer);
        return buffer;
    }
}
