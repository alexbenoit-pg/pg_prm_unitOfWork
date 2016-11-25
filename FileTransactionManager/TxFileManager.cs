// -----------------------------------------------------------------------
// <copyright file="TxFileManager.cs" company="Paragon Software Group">
// EXCEPT WHERE OTHERWISE STATED, THE INFORMATION AND SOURCE CODE CONTAINED 
// HEREIN AND IN RELATED FILES IS THE EXCLUSIVE PROPERTY OF PARAGON SOFTWARE
// GROUP COMPANY AND MAY NOT BE EXAMINED, DISTRIBUTED, DISCLOSED, OR REPRODUCED
// IN WHOLE OR IN PART WITHOUT EXPLICIT WRITTEN AUTHORIZATION FROM THE COMPANY.
// 
// Copyright (c) 1994-2016 Paragon Software Group, All rights reserved.
// 
// UNLESS OTHERWISE AGREED IN A WRITING SIGNED BY THE PARTIES, THIS SOFTWARE IS
// PROVIDED "AS-IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE, ALL OF WHICH ARE HEREBY DISCLAIMED. IN NO EVENT SHALL THE
// AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF NOT ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------

namespace FileTransactionManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Transactions;
    using FileTransactionManager.Heplers;
    using FileTransactionManager.Interfaces;
    using FileTransactionManager.Operations;
    using Newtonsoft.Json;

    [DataContract]
    public class TxFileManager : IFileManager, IDisposable
    {
        /// <summary>Dictionary of transaction enlistment objects for the current thread.</summary>
        private readonly object enlistmentsLock = new object();
        [DataMember]
        private string tempFolder;
        [DataMember] 
        [JsonConverter(typeof(OperationJsonConverter))]
        private List<IRollbackableOperation> journal; private Dictionary<string, TxEnlistment> enlistments;
        
        public string TempFolder {
            get
            {
                return this.tempFolder;
            }
            set
            {
                this.tempFolder = value;
            }
        }

        #region IFileOperations

        /// <summary>Appends the specified string the file, creating the file if it doesn't already exist.</summary>
        /// <param name="path">The file to append the string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public void AppendAllText(string path, string contents)
        {
           this.EnlistOperation(new AppendAllTextOperation(path, contents));
        }

        /// <summary>Copies the specified <paramref name="sourceFileName"/> to <paramref name="destFileName"/>.</summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten, otherwise false.</param>
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            this.EnlistOperation(new CopyOperation(sourceFileName, destFileName, overwrite));
        }

        /// <summary>Creates all directories in the specified path.</summary>
        /// <param name="pathToFile">The directory path to create.</param>
        public void CreateFile(string pathToFile)
        {
            this.EnlistOperation(new CreateFileOperation(pathToFile));
        }

        /// <summary>Deletes the specified file. An exception is not thrown if the file does not exist.</summary>
        /// <param name="path">The file to be deleted.</param>
        public void Delete(string path)
        {
            this.EnlistOperation(new DeleteFileOperation(path));
        }

        /// <summary>Moves the specified file to a new location.</summary>
        /// <param name="srcFileName">The name of the file to move.</param>
        /// <param name="destFileName">The new path for the file.</param>
        public void Move(string srcFileName, string destFileName)
        {
            this.EnlistOperation(new MoveOperation(srcFileName, destFileName));
        }
        
        /// <summary>Creates a file, write the specified <paramref name="contents"/> to the file.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public void WriteAllText(string path, string contents)
        {
            this.EnlistOperation(new WriteAllTextOperation(path, contents));
        }

        #endregion

        public void UniverseRun(FileOperations operationType, object[] operationParams)
        {
            ExecuteHelper.ExecuteOperation(this, operationType, operationParams);
        }

        public void Rollback()
        {
            new TxEnlistment().Rollback(this.journal);
        }

        #region Private
        
        private void EnlistOperation(IRollbackableOperation operation)
        {
            Transaction tx = Transaction.Current;
            TxEnlistment enlistment;

            lock (this.enlistmentsLock)
            {
                if (this.enlistments == null)
                {
                    this.enlistments = new Dictionary<string, TxEnlistment>();
                }

                if (!this.enlistments.TryGetValue(tx.TransactionInformation.LocalIdentifier, out enlistment))
                {
                    enlistment = new TxEnlistment(tx);
                    this.enlistments.Add(tx.TransactionInformation.LocalIdentifier, enlistment);
                }

                if (operation is IBackupableOperation)
                {
                    ((IBackupableOperation) operation).BackupFolder = this.TempFolder;
                }

                enlistment.EnlistOperation(operation);
                this.journal = new List<IRollbackableOperation>();
                this.journal.AddRange(enlistment.GetJournal());
            }
        }

        #endregion

        public void Dispose()
        {
            if (Directory.Exists(this.TempFolder))
            {
                Directory.Delete(this.TempFolder, true);
            }
        }
    }
}
