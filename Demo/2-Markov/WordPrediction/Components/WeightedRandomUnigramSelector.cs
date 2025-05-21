namespace WordPrediction.Components
{
    public class WeightedRandomUnigramSelector<T> : IUnigramSelector<T>
    {
        public T SelectUnigram(IEnumerable<T> ngrams)
        {
            return ngrams.OrderBy(a => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
