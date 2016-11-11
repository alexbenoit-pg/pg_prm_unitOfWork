﻿// -----------------------------------------------------------------------
// <copyright file="BussinesTransaction.cs" company="Paragon Software Group">
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

namespace Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Core.Helpers;
    using Core.Interfaces;
    using Newtonsoft.Json;

    public sealed class BussinesTransaction : IDisposable
    {
        private List<ITransactionUnit> operations;
        private List<ITransactionUnit> commitedOperations;
        private string journalPath;
        private JsonSerializerSettings settings;

        internal BussinesTransaction()               
        {
            this.operations = new List<ITransactionUnit>();
            this.commitedOperations = new List<ITransactionUnit>();
            this.journalPath = FolderHelper.GetJournalPath(Guid.NewGuid().ToString());
            this.settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public void RegisterOperation(ITransactionUnit operation)
        {
            this.operations.Add(operation);
        }

        public void Commit()
        {
            try
            {
                File.Create(this.journalPath).Close();
                this.CommitEachOperation();
            }
            catch (Exception e)
            {
                this.Rollback();
            }
        }

        public void Dispose()
        {
            File.Delete(this.journalPath);
            this.operations.Clear();
            this.operations = null;
            this.commitedOperations.Clear();
            this.commitedOperations = null;
        }
        
        private void Rollback()
        {
            var json = File.ReadAllText(this.journalPath);
            this.operations = JsonConvert.DeserializeObject<List<ITransactionUnit>>(json, this.settings);
            this.commitedOperations = JsonConvert.DeserializeObject<List<ITransactionUnit>>(json, this.settings);

            foreach (var operation in this.operations)
            {
                operation.Rollback();
                this.commitedOperations.Remove(operation);
                string json2 = JsonConvert.SerializeObject(this.commitedOperations, Formatting.Indented, this.settings);
                File.WriteAllText(this.journalPath, json2);
            }
        }

        private void CommitEachOperation()
        {
            foreach (var operation in this.operations)
            {
                try
                {
                    operation.Commit();
                    this.commitedOperations.Add(operation);
                    string json = JsonConvert.SerializeObject(this.commitedOperations, Formatting.Indented, this.settings);
                    File.WriteAllText(this.journalPath, json);
                }
                catch (Exception e)
                {
                    this.commitedOperations.Remove(operation);
                    string json = JsonConvert.SerializeObject(this.commitedOperations, Formatting.Indented, this.settings);
                    File.WriteAllText(this.journalPath, json);
                    throw e;
                }
            }
        }
    }
}