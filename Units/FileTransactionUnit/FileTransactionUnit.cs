// -----------------------------------------------------------------------
// <copyright file="FileTransactionUnit.cs" company="Paragon Software Group">
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
    using System.Transactions;


    using ChinhDo.Transactions;
    using Core.Interfaces;
    using Core.Helpers;

    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract]
    public class FileTransactionUnit : ITransactionUnit
    {
        private TxFileManager target;
        private List<FileOperations> operations;
        private Dictionary<int, object[]> parametersForOperations;

        [DataMember]
        private string jsonJournal;

        public FileTransactionUnit()
        {
            this.target = new TxFileManager();
            this.operations = new List<FileOperations>();
            this.parametersForOperations = new Dictionary<int, object[]>();
            this.jsonJournal = "";
        }
        
        public void Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    ExecuteEachOperation();
                    scope.Complete();
                    this.jsonJournal = this.target.GetJsonJournal();
                }
                catch (Exception e)
                {
                    scope.Dispose();
                    throw e;
                }
            }
        }

        public void Dispose()
        {

        }
        
        public void Rollback()
        {
            target.RollbackAfterCrash(this.jsonJournal);
        }
        
        private void ExecuteEachOperation()
        {
            for (int i = 0; i < operations.Count; i++)
            {
                this.target.UniverseRun(
                    this.operations[i],
                    this.parametersForOperations[i]);
            }
        }

        #region File operations

        public void AppendAllText(string path, string contents)
        {
            this.operations.Add(FileOperations.AppendAllText);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { path, contents }
                );
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            this.operations.Add(FileOperations.Copy);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { sourceFileName, destFileName, overwrite }
                );
        }

        public void CreateFile(string pathToFile)
        {
            this.operations.Add(FileOperations.CreateFile);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { pathToFile }
                );
        }

        public void Delete(string path)
        {
            this.operations.Add(FileOperations.Delete);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { path }
                );
        }

        public void Move(string srcFileName, string destFileName)
        {
            this.operations.Add(FileOperations.Move);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { srcFileName, destFileName }
                );
        }

        public void WriteAllText(string path, string contents)
        {
            this.operations.Add(FileOperations.WriteAllText);
            this.parametersForOperations.Add(
                this.operations.Count - 1,
                new object[] { path, contents }
                );
        }

        #endregion
    }
}
