using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Units;
using Assert = NUnit.Framework.Assert;

namespace SQLiteTransaction.UnitTest
{
    [TestFixture]
    public class SQLiteTransactionUnitTest
    {
        [Test]
        public void ConnectDatabase_IsValidPathToDataBase_ReturnsTrue()
        {
            //Arrage
            FileUnit transaction = new FileUnit();
            //Act
            //Assert 
            Assert.IsNotNull(transaction);
        }
    }
}
