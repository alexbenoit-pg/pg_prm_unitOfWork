namespace ChinhDo.Transactions
{
    using System;
    using System.IO;
    using ChinhDo.Transactions.Interfaces;

    /// <summary>
    /// Rollbackable operation which Rename a file Name
    /// </summary>
    [Serializable]
    internal sealed class RenameFileOperation : IRollbackableOperation
    {
        private readonly string sourceFileName;
        private  string destFileName;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to Rename.</param>
        /// <param name="destFileName">The new name for the file.</param>
        public RenameFileOperation(string sourceFileName, string destFileName)
        {
            this.sourceFileName = sourceFileName;
            this.destFileName = destFileName;
        }

        public void Execute()
        {
            FileInfo info = new FileInfo(sourceFileName);
            if (info.Directory != null)
            {
                string directoryPath = info.Directory.FullName + "\\" + destFileName;

                File.Move(sourceFileName, directoryPath);
                destFileName = directoryPath;
            }
            else
            {
                throw new Exception();
            }
        }

        public void Rollback()
        {
            File.Move(destFileName, sourceFileName);
        }

    }


}

