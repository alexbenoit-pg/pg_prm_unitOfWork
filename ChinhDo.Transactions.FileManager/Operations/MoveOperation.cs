namespace ChinhDo.Transactions.Operations
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using ChinhDo.Transactions.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Rollbackable operation which moves a file to a new location.
    /// </summary>
    [DataContract]
    sealed class MoveOperation : IRollbackableOperation
    {
        [DataMember]
        private readonly string sourceFileName;
        [DataMember]
        private readonly string destFileName;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move.</param>
        /// <param name="destFileName">The new path for the file.</param>
        public MoveOperation(string sourceFileName, string destFileName)
        {
            this.sourceFileName = sourceFileName;
            this.destFileName = destFileName;
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileOperations Type
        {
            get
            {
                return FileOperations.Move;
            }
        }

        public void Execute()
        {
            File.Move(sourceFileName, destFileName);
        }

        public void Rollback()
        {
            File.Move(destFileName, sourceFileName);
        }
    }
}
