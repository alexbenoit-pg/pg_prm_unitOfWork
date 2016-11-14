// -----------------------------------------------------------------------
// <copyright file="BadBussinesTransaction.cs" company="Paragon Software Group">
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

    using Core.Interfaces;
    using Core.Helpers;
    using Newtonsoft.Json;

    internal sealed class BadBussinesTransaction : IDisposable
    {
        private List<ITransactionUnit> operations;
        private List<ITransactionUnit> commitedOperations;
        private string journalPath;
        private JsonSerializerSettings jsonSettings;

        internal BadBussinesTransaction(string journalPath)
        {
            this.journalPath = journalPath;
            this.operations = JournalHelper.GetOperationsFromJournal<List<ITransactionUnit>>(this.journalPath, jsonSettings);
            this.operations.ForEach((op) => { this.commitedOperations.Add(op); });

            this.jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            this.RollbackAfterCrush();
        }

        public void Dispose()
        {
            File.Delete(this.journalPath);
            this.operations.Clear();
            this.operations = null;
            this.commitedOperations.Clear();
            this.commitedOperations = null;
        }

        private void RollbackAfterCrush()
        {
            foreach (var operation in this.operations)
            {
                operation.Rollback();

                this.commitedOperations.Remove(operation);
                JournalHelper.WriteOperationsToJournal(this.commitedOperations, this.jsonSettings, this.journalPath);
            }
        }
    }
}
