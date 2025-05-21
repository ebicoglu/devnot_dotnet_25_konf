namespace WordPrediction.Components
{
    public class AlphabeticUnigramSelector<T> : IUnigramSelector<T>
    {
        public T SelectUnigram(IEnumerable<T> ngrams)
        {
            return ngrams
                .OrderBy(a => a)
                .FirstOrDefault();
        }
    }
}
