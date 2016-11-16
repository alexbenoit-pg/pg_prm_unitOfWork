// -----------------------------------------------------------------------
// <copyright file="CreateFileOperation.cs" company="Paragon Software Group">
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
    using FileTransactionManager.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    [DataContract]
    sealed class CreateFileOperation : IRollbackableOperation
    {
        [DataMember]
        public readonly string path;

        public CreateFileOperation(string pathToFile)
        {
            this.path = pathToFile;
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileOperations Type
        {
            get
            {
                return FileOperations.CreateFile;
            }
        }

        public void Execute()
        {
            var parentFolders = path.Split('\\');
            string tempPath = "";

            for (var i = 0; i < parentFolders.Length - 1; i++)
            {
                tempPath = Path.Combine(tempPath, parentFolders[i] + "\\");
                if (!Directory.Exists(tempPath))
                {
                    throw new Exception($"Folder {tempPath} is not exist.");
                }
            }

            if (!File.Exists(path))
                File.Create(path).Close();
        }

        public void Rollback()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
