namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    using ChinhDo.Transactions.Interfaces;

    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    [DataContract]
    sealed class CreateFileOperation : IRollbackableOperation
    {
        [DataMember]
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
                File.Create(path).Close();
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
