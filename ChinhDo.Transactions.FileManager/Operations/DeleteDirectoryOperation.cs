// -----------------------------------------------------------------------
// <copyright file="DeleteDirectoryOperation.cs" company="Paragon Software Group">
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

namespace FileTransactionManager.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using FileTransactionManager.Heplers;
    using FileTransactionManager.Interfaces;

    /// <summary>
    /// Deletes the specified directory and all its contents.
    /// </summary>
    [DataContract]
    internal sealed class DeleteDirectoryOperation : IRollbackableOperation, IDisposable
    {
        [DataMember]
        private readonly string path;
        [DataMember]
        private string backupPath;
        [DataMember]
        private bool disposed;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The directory path to delete.</param>
        public DeleteDirectoryOperation(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Disposes the resources used by this class.
        /// </summary>
        ~DeleteDirectoryOperation()
        {
            this.InnerDispose();
        }

        public void Execute()
        {
            if (Directory.Exists(this.path))
            {
                string temp = FileUtils.GetTempFileName(string.Empty);
                MoveDirectory(this.path, temp);
                this.backupPath = temp;
            }
        }

        public void Rollback()
        {
            if (Directory.Exists(this.backupPath))
            {
                string parentDirectory = Path.GetDirectoryName(this.path);
                if (!Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                MoveDirectory(this.backupPath, this.path);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.InnerDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Moves a directory, recursively, from one path to another.
        /// This is a version of <see cref="Directory.Move"/> that works across volumes.
        /// </summary>
        private static void MoveDirectory(string sourcePath, string destinationPath)
        {
            if (Directory.GetDirectoryRoot(sourcePath) == Directory.GetDirectoryRoot(destinationPath))
            {
                // The source and destination volumes are the same, so we can do the much less expensive Directory.Move.
                Directory.Move(sourcePath, destinationPath);
            }
            else
            {
                // The source and destination volumes are different, so we have to resort to a copy/delete.
                CopyDirectory(new DirectoryInfo(sourcePath), new DirectoryInfo(destinationPath));
                Directory.Delete(sourcePath, true);
            }
        }

        private static void CopyDirectory(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            if (!destinationDirectory.Exists)
            {
                destinationDirectory.Create();
            }

            foreach (FileInfo sourceFile in sourceDirectory.GetFiles())
            {
                sourceFile.CopyTo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));
            }

            foreach (DirectoryInfo sourceSubDirectory in sourceDirectory.GetDirectories())
            {
                string destinationSubDirectoryPath = Path.Combine(destinationDirectory.FullName, sourceSubDirectory.Name);
                CopyDirectory(sourceSubDirectory, new DirectoryInfo(destinationSubDirectoryPath));
            }
        }

        /// <summary>
        /// Disposes the resources of this class.
        /// </summary>
        private void InnerDispose()
        {
            if (!this.disposed)
            {
                if (Directory.Exists(this.backupPath))
                {
                    Directory.Delete(this.backupPath, true);
                }

                this.disposed = true;
            }
        }
    }
}
