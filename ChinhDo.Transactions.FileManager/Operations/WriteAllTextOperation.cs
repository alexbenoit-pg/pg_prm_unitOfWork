namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using ChinhDo.Transactions.Heplers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Creates a file, and writes the specified contents to it.
    /// </summary>
    [DataContract]
    sealed class WriteAllTextOperation : SingleFileOperation
    {
        [DataMember]
        private readonly string contents;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public WriteAllTextOperation(string path, string contents)
            : base(path)
        {
            this.contents = contents;
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileOperations Type
        {
            get
            {
                return FileOperations.WriteAllText;
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

            File.WriteAllText(path, contents);
        }
    }
}
