// -----------------------------------------------------------------------
// <copyright file="CreateDirectoryOperation.cs" company="Paragon Software Group">
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
    using System.IO;
    using System.Runtime.Serialization;
    using FileTransactionManager.Interfaces;

    /// <summary>
    /// Creates all directories in the specified path.
    /// </summary>
    [DataContract]
    sealed class CreateDirectoryOperation : IRollbackableOperation
    {
        [DataMember]
        private readonly string path;
        [DataMember]
        private string backupPath;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        public CreateDirectoryOperation(string path)
        {
            this.path = path;
        }

        public void Execute()
        {
            string children = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = Path.GetDirectoryName(children);
            while (parent != null && !Directory.Exists(parent))
            {
                children = parent;
                parent = Path.GetDirectoryName(children);
            }

            if (Directory.Exists(children))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory(path);
                backupPath = children;
            }
        }

        public void Rollback()
        {
            if (backupPath != null)
            {
                Directory.Delete(backupPath, true);
            }
        }
    }
}
