﻿using System;
using System.IO;
using System.Security.Authentication.ExtendedProtection;

namespace ChinhDo.Transactions
{
    /// <summary>
    /// Class that contains common code for those rollbackable file operations which need
    /// to backup a single file and restore it when Rollback() is called.
    /// </summary>
    [Serializable]
    abstract class SingleFileOperation : IRollbackableOperation, IDisposable
    {
        public readonly string path;
        public string backupPath;
        

        // tracks whether Dispose has been called
        private bool disposed;

        public SingleFileOperation(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Disposes the resources used by this class.
        /// </summary>
        //~SingleFileOperation()
        //{
        //    InnerDispose();
        //}

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
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources of this class.
        /// </summary>
        private void InnerDispose()
        {
            if (!disposed)
            {
                if (backupPath != null)
                {
                    FileInfo fi = new FileInfo(backupPath);
                    if (fi.IsReadOnly)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }
                    File.Delete(backupPath);
                }

                disposed = true;
            }
        }
    }
}