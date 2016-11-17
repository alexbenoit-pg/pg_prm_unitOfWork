// -----------------------------------------------------------------------
// <copyright file="TxEnlistment.cs" company="Paragon Software Group">
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

namespace FileTransactionManager
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Transactions;
    using FileTransactionManager.Interfaces;
    using Newtonsoft.Json;

    /// <summary>Provides two-phase commits/rollbacks/etc for a single <see cref="Transaction"/>.</summary>
    [DataContract]
    internal sealed class TxEnlistment : IEnlistmentNotification
    {
        [DataMember]
        private List<IRollbackableOperation> journal = new List<IRollbackableOperation>();

        /// <summary>Initializes a new instance of the <see cref="TxEnlistment"/> class.</summary>
        /// <param name="tx">The Transaction.</param>
        public TxEnlistment(Transaction tx)
        {
            tx.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public TxEnlistment()
        {
        }

        /// <summary>
        /// Enlists <paramref name="operation"/> in its journal, so it will be committed or rolled
        /// together with the other enlisted operations.
        /// </summary>
        /// <param name="operation"></param>
        public void EnlistOperation(IRollbackableOperation operation)
        {
            operation.Execute();
            this.journal.Add(operation);
        }
        
        public void Commit(Enlistment enlistment)
        {
            this.DisposeJournal();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            this.Rollback(enlistment);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        /// <summary>Notifies an enlisted object that a transaction is being rolled back (aborted).</summary>
        /// <param name="enlistment">A <see cref="T:System.Transactions.Enlistment"></see> object used to send a response to the transaction manager.</param>
        /// <remarks>This is typically called on a different thread from the transaction thread.</remarks>
        public void Rollback(Enlistment enlistment)
        {
            try
            {
                // Roll back journal items in reverse order
                for (int i = this.journal.Count - 1; i >= 0; i--)
                {
                    this.journal[i].Rollback();
                }

                this.DisposeJournal();
            }
            catch (Exception e)
            {
                throw new TransactionException("Failed to roll back.", e);
            }

            enlistment.Done();
        }

        public void RollbackAfterCrash(List<IRollbackableOperation> journal)
        {
            this.journal = journal;

            try
            {
                // Roll back journal items in reverse order
                for (int i = this.journal.Count - 1; i >= 0; i--)
                {
                    this.journal[i].Rollback();
                }

                this.DisposeJournal();
            }
            catch (Exception e)
            {
                throw new TransactionException("Failed to roll back.", e);
            }
        }

        internal List<IRollbackableOperation> GetJournal()
        {
            return this.journal;
        }

        private void DisposeJournal()
        {
            IDisposable disposable;
            for (int i = this.journal.Count - 1; i >= 0; i--)
            {
                disposable = this.journal[i] as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                this.journal.RemoveAt(i);
            }
        }
    }
}