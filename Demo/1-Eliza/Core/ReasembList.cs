// class translated from Java
// Credit goes to Charles Hayden http://www.chayden.net/eliza/Eliza.html

namespace Eliza.Core
{
    /// <summary>Eliza reassembly list.</summary>
    /// <remarks>Eliza reassembly list.</remarks>
    public class ReasembList : List<string>
    {
        private const long serialVersionUID = 1L;

        /// <summary>Print the reassembly list.</summary>
        /// <remarks>Print the reassembly list.</remarks>
        public virtual void Print(int indent)
        {
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < indent; j++)
                {
                    ConsoleHelper.Write(" ");
                }
                string s = this[i];
                ConsoleHelper.WriteLine("reasemb: " + s);
            }
        }
    }
}