using System;
using System.IO;

namespace ledbox.iOS
{
    public class FilePath : IFilePath
    {
        public string GetFilePath()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(docs, "..", "tmp", "HTML");
        }
    }
}