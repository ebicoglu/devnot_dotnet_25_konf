using Newtonsoft.Json;

namespace WordPrediction.Models
{
    public class NgramContainer<T>
    {
        internal T[] Ngrams { get; }

        public NgramContainer(params T[] args)
        {
            Ngrams = args;
        }

        public override bool Equals(object o)
        {
            if (o is NgramContainer<T> testObj)
            {
                return Ngrams.OrderBy(a => a).ToArray().SequenceEqual(testObj.Ngrams.OrderBy(a => a).ToArray());
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                var defaultVal = default(T);
                foreach (var member in Ngrams.Where(a => a != null && !a.Equals(defaultVal)))
                {
                    hash = hash * 23 + member.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Ngrams);
        }
    }
}
