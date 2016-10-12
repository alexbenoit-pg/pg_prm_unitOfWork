using System.Collections.Generic;
using Core.Interfaces;

namespace Core
{
    public class UnitOfWork
    {
        public UnitOfWork()
        {
            this.journal = new Journal();
        }

        private Journal journal;
        public Journal Journal {
            get { return this.journal; }
        }
        
        public BussinesTransaction BeginTransaction()
        {
            return new BussinesTransaction();
        }
    }
}
