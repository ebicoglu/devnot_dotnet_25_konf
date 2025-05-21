namespace Eliza.Core;

public static class AsciiArt
{
    public static void WellcomeScreen()
    {
        string asciiArt = @"
EEEEEE  LL      IIII    ZZZZZZ    AAAAA
EE      LL       II        ZZ    AA   AA
EEEEEE  LL       II      ZZZ     AAAAAAA
EE      LL       II     ZZ       AA   AA
EEEEEE  LLLLLL  IIII  ZZZZZZ     AA   AA



";
        Console.WriteLine(asciiArt);
    }
}