using System;
using System.IO;

namespace ChinhDo.Transactions
{
    [Serializable]
    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    sealed class CreateFileOperation : IRollbackableOperation
    {
        public readonly string path;

        public CreateFileOperation(string pathToFile, string fileName, string fileExtention)
        {
            this.path = $"{pathToFile}\\{fileName}.{fileExtention}";
        }

        public void Execute()
        {
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
