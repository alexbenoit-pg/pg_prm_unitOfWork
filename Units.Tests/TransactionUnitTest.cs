namespace Units.Tests
{
    using NUnit.Framework;
    using System.IO;
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class TransactionUnitTest
    {
        [Test]
        public void TransactionUnit_CreateInstance_IsNotNull()
        {
            var unitTest = new TransactionSimulationUnit();

            Assert.IsNotNull(unitTest);
        }

        [Test]
        public void TransactionUnit_GetOperationID_IsNotNull()
        {
            var unitTest = new TransactionSimulationUnit();
            var operationID = unitTest.GetOperationId();

            Assert.IsNotNull(operationID);
        }

        [Test]
        public void TransactionUnit_CheckCommite_IsTrue()
        {
            var unitTest = new TransactionSimulationUnit();
            unitTest.Commit();
            string testText = "Commit прошел успешно";
            string[] text = File.ReadAllLines(unitTest.path);

            Assert.IsTrue(text[text.Length-1] == testText);
        }



    }
}
