namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using ChinhDo.Transactions.Interfaces;

    /// <summary>
    /// Class that contains common code for those rollbackable file operations which need
    /// to backup a single file and restore it when Rollback() is called.
    /// </summary>
    [DataContract]
    abstract class SingleFileOperation : IRollbackableOperation, IDisposable
    {
        [DataMember]
        public readonly string path;
        [DataMember]
        public string backupPath;

        // tracks whether Dispose has been called
        [DataMember]
        private bool disposed;

        public SingleFileOperation(string path)
        {
            this.path = path;
        }

        public abstract void Execute();

        public void Rollback()
        {
            if (backupPath != null)
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(backupPath, path, true);
            }
            else
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}

