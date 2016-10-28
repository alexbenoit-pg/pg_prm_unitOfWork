using System;
using System.IO;


namespace ChinhDo.Transactions
{
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
            string directoryPath = info.Directory.FullName + "\\" + destFileName;

            File.Move(sourceFileName, directoryPath);
            destFileName = directoryPath;
        }

        public void Rollback()
        {
            File.Move(destFileName, sourceFileName);
        }

    }


}

