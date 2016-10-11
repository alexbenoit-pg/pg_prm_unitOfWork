using System.Collections.Generic;
using Core.Interfaces;

namespace Core
{
    public class UnitOfWork
    {
        public UnitOfWork()
        {
            this.journal = new Journal();
            this.operations = new List<ITransactionUnit>();
        }

        private Journal journal;
        public Journal Journal {
            get { return this.journal; }
        }

        private List<ITransactionUnit> operations;
        public List<ITransactionUnit> Operations
        {
            get { return this.operations; }
        }

        public BussinesTransaction BeginTransaction()
        {
            return new BussinesTransaction();
        }

        public void RegisterOperation(ITransactionUnit operation)
        {
            this.operations.Add(operation);
        }
    }
}
