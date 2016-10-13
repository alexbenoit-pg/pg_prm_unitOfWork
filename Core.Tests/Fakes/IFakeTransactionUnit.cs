using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;

namespace Core.Tests.Fakes
{
    interface IFakeTransactionUnit : ITransactionUnit
    {
        bool IsRollback { get; set; }
        bool IsCommit { get; set; }
    }
}
