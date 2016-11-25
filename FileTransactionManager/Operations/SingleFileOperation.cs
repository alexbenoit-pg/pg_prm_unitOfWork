// -----------------------------------------------------------------------
// <copyright file="SingleFileOperation.cs" company="Paragon Software Group">
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

using FileTransactionManager.Heplers;

namespace FileTransactionManager.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using FileTransactionManager.Interfaces;

    /// <summary>
    /// Class that contains common code for those rollbackable file operations which need
    /// to backup a single file and restore it when Rollback() is called.
    /// </summary>
    [DataContract]
    internal abstract class SingleFileOperation : IRollbackableOperation, IBackupableOperation, IDisposable
    {
        [DataMember(Order = 10)]
        private readonly string path;
        [DataMember(Order = 11)]
        private string backupPath;

        // tracks whether Dispose has been called
        [DataMember(Order = 12)]
        private bool disposed;

        protected SingleFileOperation(string path)
        {
            this.path = path;
        }
        
        public string BackupFolder { get; set; }

        public string BackupPath
        {
            get
            {
                return this.backupPath;
            }

            set
            {
                this.backupPath = value;
            }
        }

        public string Path => this.path;

        public abstract void Execute();

        public void Rollback()
        {
            if (this.BackupPath != null)
            {
                string directory = System.IO.Path.GetDirectoryName(this.Path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(this.BackupPath, this.Path, true);
            }
            else
            {
                if (File.Exists(this.Path))
                {
                    File.Delete(this.Path);
                }
            }
        }

        public void BackupFile()
        {
            if (File.Exists(this.Path))
            {
                if (!Directory.Exists(this.BackupFolder))
                {
                    Directory.CreateDirectory(this.BackupFolder);
                }

                string temp = FileUtils.GetTempFileName(
                    this.BackupFolder,
                    System.IO.Path.GetExtension(this.Path));
                File.Copy(this.Path, temp);
                this.BackupPath = temp;
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