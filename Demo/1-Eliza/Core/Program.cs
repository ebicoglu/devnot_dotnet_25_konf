namespace Eliza.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Chat with Eliza");

            var eliza = new ElizaManager();
            
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                Console.WriteLine($"> {eliza.ProcessInput(input)}");
            }
        }
    }
}
