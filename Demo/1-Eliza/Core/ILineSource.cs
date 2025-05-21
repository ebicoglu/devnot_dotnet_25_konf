namespace Eliza.Core
{
    public interface ILineSource
    {
        string ReadLine();

        void Close();
    }
}