var turkishLetters = "abcçdefgğhıijklmnoöprsştuüvyz".ToCharArray();
var letterIndex = turkishLetters.Select((c, i) => new { c, i }).ToDictionary(x => x.c, x => x.i);
var trainingData = File.ReadAllText("training-data.txt", System.Text.Encoding.UTF8).ToLower();

// Clean the text: keep only Turkish letters
var trainingLetters = new string(trainingData.Where(c => letterIndex.ContainsKey(c)).ToArray());

// Initialize transition count matrix
var transitions = new int[turkishLetters.Length, turkishLetters.Length];

// Count letter transitions
for (var i = 0; i < trainingLetters.Length - 1; i++)
{
    var current = trainingLetters[i];
    var next = trainingLetters[i + 1];

    if (letterIndex.ContainsKey(current) && letterIndex.ContainsKey(next))
    {
        transitions[letterIndex[current], letterIndex[next]]++;
    }
}


// Print
Console.WriteLine("Transition Probability Matrix:\n");
Console.Write($"    ");
foreach (var c in turkishLetters)
{
    Console.Write($"{c}   ");
}

Console.WriteLine();

for (var i = 0; i < turkishLetters.Length; i++)
{
    var rowSum = 0;
    for (var j = 0; j < turkishLetters.Length; j++)
    {
        rowSum += transitions[i, j];
    }

    Console.Write($"{turkishLetters[i]}: ");

    for (var j = 0; j < turkishLetters.Length; j++)
    {
        var percent = rowSum > 0 ? (transitions[i, j] * 100.0 / rowSum) : 0;
        Console.Write($"{percent,2:F0}% ");
    }

    Console.WriteLine();
}

Console.ReadKey();
