namespace Core.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Core;

    /// <summary>
    /// Unit tests for Journal Class
    /// </summary>
    [TestClass]
    public class JournalTests
    {
        [TestMethod]
        public void Test_Create_Journal()
        {
            // Arrange
            var journal = new Journal();

            // Assert
            Assert.IsNotNull(journal);
        }
    }
}
