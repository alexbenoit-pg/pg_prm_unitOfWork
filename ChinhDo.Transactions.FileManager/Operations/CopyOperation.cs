namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

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
