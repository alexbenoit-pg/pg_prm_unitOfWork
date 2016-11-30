// -----------------------------------------------------------------------
// <copyright file="FileUnit.cs" company="Paragon Software Group">
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

namespace Units
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Transactions;
    using Core;
    using Core.Exceptions;
    using Core.Interfaces;
    using FileTransactionManager;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [DataContract]
    public class FileUnit : ITransactionUnit
    {
        [DataMember(Order = 1)]
        private TxFileManager target;
        private List<FileOperations> operations;
        private Dictionary<int, object[]> paramsForOperations;

        public FileUnit()
        {
            this.target = new TxFileManager();
            this.target.TempFolder = Path.Combine(
                UnitOfWork.GetJournalsFolder(), 
                "FileUnit");
            this.operations = new List<FileOperations>();
            this.paramsForOperations = new Dictionary<int, object[]>();
        }

        [DataMember(Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitType Type
        {
            get
            {
                return UnitType.FileUnit;
            }
        }

        public void Commit()
        {
            this.target.TempFolder = Path.Combine(
                UnitOfWork.GetJournalsFolder(),
                "FileUnit",
                Guid.NewGuid().ToString());

            using (var scope = new TransactionScope())
            {
                try
                {
                    this.ExecuteEachOperation();
                    scope.Complete();
                }
                catch (Exception e)
                {
                    scope.Dispose();
                    throw new CommitException(e.Message);
                }
            }
        }
        
        public void Rollback()
        {
            this.target.Rollback();
        }

        public void Dispose()
        {
            this.target.Dispose();
        }

        #region File operations

        public void AppendAllText(string path, string contents)
        {
            this.operations.Add(FileOperations.AppendAllText);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { path, contents });
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            this.operations.Add(FileOperations.Copy);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { sourceFileName, destFileName, overwrite });
        }

        public void CreateFile(string pathToFile)
        {
            this.operations.Add(FileOperations.CreateFile);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { pathToFile });
        }

        public void Delete(string path)
        {
            this.operations.Add(FileOperations.Delete);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { path });
        }

        public void Move(string srcFileName, string destFileName)
        {
            this.operations.Add(FileOperations.Move);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { srcFileName, destFileName });
        }

        public void WriteAllText(string path, string contents)
        {
            this.operations.Add(FileOperations.WriteAllText);
            this.paramsForOperations.Add(
                this.operations.Count - 1,
                new object[] { path, contents });
        }

        #endregion
        
        private void ExecuteEachOperation()
        {
            for (int i = 0; i < this.operations.Count; i++)
            {
                this.target.UniverseRun(
                    this.operations[i],
                    this.paramsForOperations[i]);
            }
        }
    }
}
