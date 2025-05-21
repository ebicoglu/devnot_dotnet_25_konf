namespace WordPrediction.Models
{
    public class StateStatistic<TGram>
    {
        internal StateStatistic(NgramContainer<TGram> state, IEnumerable<TGram> valuesAtState)
        {
            State = state.Ngrams;
            var groupedValues = valuesAtState.GroupBy(x => x).ToList();
            Next = groupedValues.Select(a => new NgramStatistic<TGram>
            {
                Value = a.Key,
                Count = a.Count(),
                Probability = Math.Round(((double)a.Count() / (double)groupedValues.Sum(x => x.Count())) * 100, 2)
            }).OrderByDescending(x => x.Probability);
        }

        public TGram[] State { get; set; }
        public IEnumerable<NgramStatistic<TGram>> Next { get; set; }
    }
}
