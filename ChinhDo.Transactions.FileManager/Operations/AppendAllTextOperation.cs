namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Rollbackable operation which appends a string to an existing file, or creates the file if it doesn't exist.
    /// </summary>
    [DataContract]
    sealed class AppendAllTextOperation : SingleFileOperation
    {
        [DataMember]
        private readonly string contents;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to append the string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public AppendAllTextOperation(string path, string contents)
            : base(path)
        {
            this.contents = contents;
        }

        public override void Execute()
        {
            if (File.Exists(path))
            {
                string temp = FileUtils.GetTempFileName(Path.GetExtension(path));
                File.Copy(path, temp);
                backupPath = temp;
            }

            File.AppendAllText(path, contents);
        }
    }
}
