namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using ChinhDo.Transactions.Heplers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Rollbackable operation which deletes a file. An exception is not thrown if the file does not exist.
    /// </summary>
    [DataContract]
    sealed class DeleteFileOperation : SingleFileOperation
    {
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to be deleted.</param>
        public DeleteFileOperation(string path)
            : base(path)
        {
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileOperations Type
        {
            get
            {
                return FileOperations.Delete;
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

            File.Delete(path);
        }
    }
}
