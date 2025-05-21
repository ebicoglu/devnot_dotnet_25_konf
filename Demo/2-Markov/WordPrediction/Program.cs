using System.Text;
using System.Text.RegularExpressions;
using WordPrediction.TokenisationStrategies;
Console.OutputEncoding = Encoding.UTF8;

var data = LoadTrainingData("traing-data.txt");
var model = Train(data);
Console.WriteLine("Write first word:");

while (true)
{
    var inputWord = Console.ReadLine();
    var predictions = GetPredictions(model, inputWord);
    Console.WriteLine("-------------");

    if (!predictions.Any())
    {
        Console.WriteLine("[No prediction]");
    }
    else
    {
        foreach (var nextWord in predictions)
        {
            Console.WriteLine("- " + nextWord.Text + " (" + nextWord.Occurence + ")");
        }
    }
}

static string LoadTrainingData(string path, bool normalize = true)
{
    Console.WriteLine("[Loading data...]");
    var data = File.ReadAllText(path).ToLower();

    if (normalize)
    {
        return Regex.Replace(data, @"[\r\n]|[^a-zA-ZğüşöçİĞÜŞÖÇı ]", m =>
        {
            return m.Value == "\r" || m.Value == "\n" ? " " : "";
        });
    }

    return data;
}

/*
   trainingData: Input text to train
   wordWindowSize: How many previous words to consider for creating predictions
*/
static StringMarkov Train(string trainingData, int wordWindowSize = 2)
{
    Console.WriteLine("[Training...]");
    var model = new StringMarkov(wordWindowSize);
    var lines = trainingData.Split(['\n', '.', '!', '?'], StringSplitOptions.RemoveEmptyEntries);
    model.Learn(lines);
    return model;
}

static List<Prediction> GetPredictions(StringMarkov model, string seedText, int maxResultCount = 5)
{
    return model.GetMatches(seedText)
        .Where(a => a != model.GetPrepadUnigram() && a != model.GetTerminatorUnigram())
        .GroupBy(a => a)
        .OrderByDescending(a => a.Count())
        .Select(a => new Prediction { Text = a.Key, Occurence = a.Count() })
        .Take(maxResultCount)
        .ToList();
}