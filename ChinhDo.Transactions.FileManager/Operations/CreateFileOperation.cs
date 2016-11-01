using System;
using System.IO;
namespace ChinhDo.Transactions.Operations
{
    using ChinhDo.Transactions.Interfaces;

    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    [Serializable]
    sealed class CreateFileOperation : IRollbackableOperation
    {
        public readonly string path;

        public CreateFileOperation(string pathToFile)
        {
            this.path = pathToFile;
        }

        public void Execute()
        {
            var parentFolders = path.Split('\\');
            string tempPath = "";

            for (var i = 0; i < parentFolders.Length - 1; i++)
            {
                tempPath = Path.Combine(tempPath, parentFolders[i] + "\\");
                if (!Directory.Exists(tempPath))
                {
                    throw new Exception($"Folder {tempPath} is not exist.");
                }
            }

            if (!File.Exists(path))
                File.Create(path);
        }

        public void Rollback()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
