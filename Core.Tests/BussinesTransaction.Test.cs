using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using Core.Tests.Fakes;

namespace Core.Tests
{
    /// <summary>
    /// Unit tests for BussinesTransaction Class
    /// </summary>
    [TestClass]
    public class BussinesTransactionTests
    {
        [TestMethod]
        public void Test_Create_BussinesTransaction()
        {
            // Arrange
            var bussinesTransaction = new BussinesTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction);
        }

        [TestMethod]
        public void Test_Create_Operation_collection()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction.Operations);
        }

        [TestMethod]
        public void Test_Add_Operation()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Act
            var operation = new FakeTransactionUnit();
            bussinesTransaction.RegisterOperation(operation);

            // Assert
            Assert.IsTrue(bussinesTransaction.Operations.Count > 0);
        }
    }
}
