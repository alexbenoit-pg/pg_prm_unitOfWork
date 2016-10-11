using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;

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
    }
}
