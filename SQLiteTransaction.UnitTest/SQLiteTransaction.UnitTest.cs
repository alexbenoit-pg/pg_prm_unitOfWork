using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
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
            SqLiteTransaction transaction = new SqLiteTransaction();
            //Act
            bool result = transaction.ConnectDatabase("");
            //Assert 
            Assert.IsFalse(result);
        }
    }
}
