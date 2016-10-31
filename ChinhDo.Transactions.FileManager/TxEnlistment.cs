namespace ChinhDo.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Transactions;
    using ChinhDo.Transactions.Heplers;
    using ChinhDo.Transactions.Interfaces;

    /// <summary>Provides two-phase commits/rollbacks/etc for a single <see cref="Transaction"/>.</summary>
    [Serializable]
    sealed class TxEnlistment : IEnlistmentNotification
    {
        private List<IRollbackableOperation> _journal = new List<IRollbackableOperation>();
        private string OperationID;

        /// <summary>Initializes a new instance of the <see cref="TxEnlistment"/> class.</summary>
        /// <param name="tx">The Transaction.</param>
        public TxEnlistment(Transaction tx, string operationID)
        {
            tx.EnlistVolatile(this, EnlistmentOptions.None);
            this.OperationID = operationID;
        }

        public TxEnlistment(string JournalId)
        {
            this.OperationID = JournalId;
        }

        /// <summary>
        /// Enlists <paramref name="operation"/> in its journal, so it will be committed or rolled
        /// together with the other enlisted operations.
        /// </summary>
        /// <param name="operation"></param>
        public void EnlistOperation(IRollbackableOperation operation)
        {
            operation.Execute();
            _journal.Add(operation);
        }

        public void Commit(Enlistment enlistment)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, _journal);

                File.WriteAllBytes(FileUtils.journalFolder + "\\" + OperationID, stream.ToArray());
            }

            DisposeJournal();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Rollback(enlistment);
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
                for (int i = _journal.Count - 1; i >= 0; i--)
                {
                    _journal[i].Rollback();
                }

                DisposeJournal();
            }
            catch (Exception e)
            {
                throw new TransactionException("Failed to roll back.", e);
            }

            enlistment.Done();
        }

        public void RollbackAfterCrash()
        {
            byte[] arr = File.ReadAllBytes(FileUtils.journalFolder + "\\" + OperationID);

            using (var stream = new MemoryStream(arr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                _journal = (List<IRollbackableOperation>)new BinaryFormatter().Deserialize(stream);
            }

            try
            {
                // Roll back journal items in reverse order
                for (int i = _journal.Count - 1; i >= 0; i--)
                {
                    _journal[i].Rollback();
                }

                DisposeJournal();
            }
            catch (Exception e)
            {
                throw new TransactionException("Failed to roll back.", e);
            }
        }

        private void DisposeJournal()
        {
            IDisposable disposable;
            for (int i = _journal.Count - 1; i >= 0; i--)
            {
                disposable = _journal[i] as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                _journal.RemoveAt(i);
            }
        }
    }
}