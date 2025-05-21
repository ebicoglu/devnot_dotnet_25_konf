namespace WordPrediction.Components
{
    public interface IUnigramSelector <TUnigram>
    {
        TUnigram SelectUnigram(IEnumerable<TUnigram> ngrams);
    }
}
