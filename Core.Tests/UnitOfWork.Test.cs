using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using Core.Tests.Fakes;

namespace Core.Tests
{
    /// <summary>
    /// Unit tests for UnitOfWork Class
    /// </summary>
    [TestClass]
    public class UnitOfWorkTests
    {
        [TestMethod]
        public void Test_Create_UnitOfWork()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Assert
            Assert.IsNotNull(unitOfWork);
        }

        [TestMethod]
        public void Test_Get_BussinesTransaction()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act 
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction);
        }

        [TestMethod]
        public void Test_Create_journal_in_UnitOfWork()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            
            // Assert
            Assert.IsNotNull(unitOfWork.Journal);
        }
    }


}
