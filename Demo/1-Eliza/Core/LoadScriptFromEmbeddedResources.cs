namespace Eliza.Core
{
    public class LoadScriptFromEmbeddedResources : ILineSource
    {
        private readonly StreamReader _reader;

        public LoadScriptFromEmbeddedResources()
        {
            var names = this.GetType().Assembly.GetManifestResourceNames();
            var stream = this.GetType().Assembly.GetManifestResourceStream(names.First());
            if (stream == null)
            {
                throw new ArgumentException("script file is not an embedded resource!");
            }

            _reader = new StreamReader(stream);
        }

        #region ILineSource implementation

        public string ReadLine() => _reader.ReadLine();

        public void Close() => _reader.Dispose();

        #endregion ILineSource implementation
    }
}