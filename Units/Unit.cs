using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;

namespace Units
{

    public class Unit : ITransactionUnit
    {
       
        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetOperationId()
        {
            Guid g = Guid.NewGuid();


            return g.ToString();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public void Rollback(string operationId)
        {
            throw new NotImplementedException();
        }

        public void SetOperationId(string operationId)
        {
            throw new NotImplementedException();
        }
    }
}
