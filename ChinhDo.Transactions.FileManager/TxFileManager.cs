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

    [DataContract]
    public class TxFileManager : IFileManager
    {
        /// <summary>
        /// Initializes the <see cref="TxFileManager"/> class.
        /// </summary>
        /// 

        [DataMember]
        private TxEnlistment enlistment;

        public TxFileManager()
        {
            FileUtils.EnsureTempFolderExists();
        }

        #region IFileOperations

        /// <summary>Appends the specified string the file, creating the file if it doesn't already exist.</summary>
        /// <param name="path">The file to append the string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public void AppendAllText(string path, string contents)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new AppendAllTextOperation(path, contents));
            }
            else
            {
                File.AppendAllText(path, contents);
            }
        }

        /// <summary>Copies the specified <paramref name="sourceFileName"/> to <paramref name="destFileName"/>.</summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten, otherwise false.</param>
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new CopyOperation(sourceFileName, destFileName, overwrite));
            }
            else
            {
                File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        /// <summary>Creates all directories in the specified path.</summary>
        /// <param name="path">The directory path to create.</param>
        public void CreateDirectory(string path)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new CreateDirectoryOperation(path));
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>Creates all directories in the specified path.</summary>
        /// <param name="pathToFile">The directory path to create.</param>
        public void CreateFile(string pathToFile)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new CreateFileOperation(pathToFile));
            }
            else
            {
                File.Create(pathToFile);
            }
        }

        /// <summary>Deletes the specified file. An exception is not thrown if the file does not exist.</summary>
        /// <param name="path">The file to be deleted.</param>
        public void Delete(string path)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new DeleteFileOperation(path));
            }
            else
            {
                File.Delete(path);
            }
        }

        /// <summary>Deletes the specified directory and all its contents. An exception is not thrown if the directory does not exist.</summary>
        /// <param name="path">The directory to be deleted.</param>
        public void DeleteDirectory(string path)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new DeleteDirectoryOperation(path));
            }
            else
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>Moves the specified file to a new location.</summary>
        /// <param name="srcFileName">The name of the file to move.</param>
        /// <param name="destFileName">The new path for the file.</param>
        public void Move(string srcFileName, string destFileName)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new MoveOperation(srcFileName, destFileName));
            }
            else
            {
                File.Move(srcFileName, destFileName);
            }
        }

        /// <summary>Take a snapshot of the specified file. The snapshot is used to rollback the file later if needed.</summary>
        /// <param name="fileName">The file to take a snapshot for.</param>
        public void Snapshot(string fileName)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new SnapshotOperation(fileName));
            }
        }

        /// <summary>Creates a file, write the specified <paramref name="contents"/> to the file.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public void WriteAllText(string path, string contents)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new WriteAllTextOperation(path, contents));
            }
            else
            {
                File.WriteAllText(path, contents);
            }
        }

        /// <summary>Creates a file, write the specified <paramref name="contents"/> to the file.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The bytes to write to the file.</param>
        public void WriteAllBytes(string path, byte[] contents)
        {
            if (IsInTransaction())
            {
                EnlistOperation(new WriteAllBytesOperation(path, contents));
            }
            else
            {
                File.WriteAllBytes(path, contents);
            }
        }

        #endregion

        /// <summary>Determines whether the specified path refers to a directory that exists on disk.</summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>True if the directory exists.</returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>Determines whether the specified file exists.</summary>
        /// <param name="path">The file to check.</param>
        /// <returns>True if the file exists.</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>Gets the files in the specified directory.</summary>
        /// <param name="path">The directory to get files.</param>
        /// <param name="handler">The <see cref="FileEventHandler"/> object to call on each file found.</param>
        /// <param name="recursive">if set to <c>true</c>, include files in sub directories recursively.</param>
        public void GetFiles(string path, FileEventHandler handler, bool recursive)
        {
            foreach (string fileName in Directory.GetFiles(path))
            {
                bool cancel = false;
                handler(fileName, ref cancel);
                if (cancel)
                {
                    return;
                }
            }

            // Check subdirs
            if (recursive)
            {
                foreach (string folderName in Directory.GetDirectories(path))
                {
                    GetFiles(folderName, handler, recursive);
                }
            }
        }

        /// <summary>Creates a temporary file name. File is not automatically created.</summary>
        /// <param name="extension">File extension (with the dot).</param>
        public string GetTempFileName(string extension)
        {
            string retVal = FileUtils.GetTempFileName(extension);

            Snapshot(retVal);

            return retVal;
        }

        /// <summary>Creates a temporary file name. File is not automatically created.</summary>
        public string GetTempFileName()
        {
            return GetTempFileName(".tmp");
        }

        /// <summary>Gets a temporary directory.</summary>
        /// <returns>The path to the newly created temporary directory.</returns>
        public string GetTempDirectory()
        {
            return GetTempDirectory(Path.GetTempPath(), string.Empty);
        }

        /// <summary>Gets a temporary directory.</summary>
        /// <param name="parentDirectory">The parent directory.</param>
        /// <param name="prefix">The prefix of the directory name.</param>
        /// <returns>Path to the temporary directory. The temporary directory is created automatically.</returns>
        public string GetTempDirectory(string parentDirectory, string prefix)
        {
            Guid g = Guid.NewGuid();
            string dirName = Path.Combine(parentDirectory, prefix + g.ToString().Substring(0, 16));

            CreateDirectory(dirName);

            return dirName;
        }

        public void UniverseRun(FileOperations operationType, object[] operationParams)
        {
            ExecuteHelper.ExecuteOperation(this, operationType, operationParams);
        }

        #region Private

        /// <summary>Dictionary of transaction enlistment objects for the current thread.</summary>

        private Dictionary<string, TxEnlistment> _enlistments;

        private readonly object _enlistmentsLock = new object();

        private bool IsInTransaction()
        {
            return Transaction.Current != null;
        }
        //tod
        private void EnlistOperation(IRollbackableOperation operation)
        {
            Transaction tx = Transaction.Current;

            lock (_enlistmentsLock)
            {
                if (_enlistments == null)
                {
                    _enlistments = new Dictionary<string, TxEnlistment>();
                }

                if (!_enlistments.TryGetValue(tx.TransactionInformation.LocalIdentifier, out enlistment))
                {
                    enlistment = new TxEnlistment(tx);
                    _enlistments.Add(tx.TransactionInformation.LocalIdentifier, enlistment);
                }

                enlistment.EnlistOperation(operation);
            }
        }

        public void RollbackAfterCrash()
        {
            enlistment.RollbackAfterCrash();
        }

        #endregion
    }
}
