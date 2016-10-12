using System.Collections.Generic;
using Core.Interfaces;

namespace Core
{
    public class BussinesTransaction
    {
        public BussinesTransaction()
        {
            this.operations = new List<ITransactionUnit>();
        }

        private List<ITransactionUnit> operations;
        public List<ITransactionUnit> Operations
        {
            get { return this.operations; }
        }

        public void RegisterOperation(ITransactionUnit operation)
        {
            this.operations.Add(operation);
        }
    }
}
