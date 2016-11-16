// -----------------------------------------------------------------------
// <copyright file="CopyOperation.cs" company="Paragon Software Group">
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
    using FileTransactionManager.Heplers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    [DataContract]
    sealed class CopyOperation : SingleFileOperation
    {
        [DataMember]
        private readonly string sourceFileName;
        [DataMember]
        private readonly bool overwrite;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten, otherwise false.</param>
        public CopyOperation(string sourceFileName, string destFileName, bool overwrite)
            : base(destFileName)
        {
            this.sourceFileName = sourceFileName;
            this.overwrite = overwrite;
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileOperations Type
        {
            get
            {
                return FileOperations.Copy;
            }
        }

        public override void Execute()
        {
            if (File.Exists(path))
            {
                string temp = FileUtils.GetTempFileName(Path.GetExtension(path));
                File.Copy(path, temp);
                backupPath = temp;
            }
            
            File.Copy(sourceFileName, path, overwrite);
        }
    }
}
