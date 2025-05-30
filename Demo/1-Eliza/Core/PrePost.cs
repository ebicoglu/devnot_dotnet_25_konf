// class translated from Java
// Credit goes to Charles Hayden http://www.chayden.net/eliza/Eliza.html

namespace Eliza.Core
{
    /// <summary>Eliza pre-post entry (two words).</summary>
    /// <remarks>
    /// Eliza pre-post entry (two words).
    /// This is used to store pre transforms or post transforms.
    /// </remarks>
    public class PrePost
    {
        /// <summary>The words</summary>
        internal string src;

        internal string dest;

        /// <summary>Initialize the pre-post entry.</summary>
        /// <remarks>Initialize the pre-post entry.</remarks>
        internal PrePost(string src, string dest)
        {
            this.src = src;
            this.dest = dest;
        }

        /// <summary>Print the pre-post entry.</summary>
        /// <remarks>Print the pre-post entry.</remarks>
        public virtual void Print(int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                ConsoleHelper.Write(" ");
            }
            ConsoleHelper.WriteLine("pre-post: " + src + "  " + dest);
        }

        /// <summary>Get src.</summary>
        /// <remarks>Get src.</remarks>
        public virtual string Src() => src;

        /// <summary>Get dest.</summary>
        /// <remarks>Get dest.</remarks>
        public virtual string Dest() => dest;
    }
}