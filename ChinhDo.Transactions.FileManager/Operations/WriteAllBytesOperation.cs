namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using ChinhDo.Transactions.Heplers;

    /// <summary>
    /// Creates a file, and writes the specified contents to it.
    /// </summary>
    [DataContract]
    sealed class WriteAllBytesOperation : SingleFileOperation
    {
        [DataMember]
        private readonly byte[] contents;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public WriteAllBytesOperation(string path, byte[] contents)
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

            File.WriteAllBytes(path, contents);
        }
    }
}