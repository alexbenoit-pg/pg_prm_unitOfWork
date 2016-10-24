using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Units;

namespace Units.Tests
{
    [TestFixture]
    public class TransactionUnitTest
    {
        [Test]
        public void Test_Create_Unit()
        {
            var unitTest = new Unit();
                
            Assert.IsNotNull(unitTest);
        }

        [Test]
        public void Test_GetOperationID()
        {
            var unitTest = new Unit();

            var opID = unitTest.GetOperationId();

          
            Assert.IsNotNull(opID);
        }

    }
}
