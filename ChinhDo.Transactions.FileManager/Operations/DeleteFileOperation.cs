namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using ChinhDo.Transactions.Heplers;

    /// <summary>
    /// Rollbackable operation which deletes a file. An exception is not thrown if the file does not exist.
    /// </summary>
    [Serializable]
    sealed class DeleteFileOperation : SingleFileOperation
    {
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to be deleted.</param>
        public DeleteFileOperation(string path)
            : base(path)
        {
        }

        public override void Execute()
        {
            if (File.Exists(path))
            {
                string temp = FileUtils.GetTempFileName(Path.GetExtension(path));
                File.Copy(path, temp);
                backupPath = temp;
            }

            File.Delete(path);
        }
    }
}
