Eliza.Core.AsciiArt.WellcomeScreen();
var eliza = new Eliza.Core.ElizaManager();
Console.WriteLine(eliza.initial);

while (true)
{
    Console.Write("YOU: ");
    var input = Console.ReadLine();
    Console.ForegroundColor= ConsoleColor.Cyan;
    Console.WriteLine($"ELIZA: {eliza.ProcessInput(input)}");
    Console.ResetColor();
}
