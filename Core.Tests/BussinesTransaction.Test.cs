// -----------------------------------------------------------------------
// <copyright file="BussinesTransactionTests.cs" company="Paragon Software Group">
// EXCEPT WHERE OTHERWISE STATED, THE INFORMATION AND SOURCE CODE CONTAINED 
// HEREIN AND IN RELATED FILES IS THE EXCLUSIVE PROPERTY OF PARAGON SOFTWARE
// GROUP COMPANY AND MAY NOT BE EXAMINED, DISTRIBUTED, DISCLOSED, OR REPRODUCED
// IN WHOLE OR IN PART WITHOUT EXPLICIT WRITTEN AUTHORIZATION FROM THE COMPANY.
// 
// Copyright (c) 1994-2016 Paragon Software Group, All rights reserved.
// 
// UNLESS OTHERWISE AGREED IN A WRITING SIGNED BY THE PARTIES, THIS SOFTWARE IS
// PROVIDED "AS-IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE, ALL OF WHICH ARE HEREBY DISCLAIMED. IN NO EVENT SHALL THE
// AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF NOT ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Tests
{
    using System.Linq;
    using NUnit.Framework;
    
    using Core.Tests.Fakes;
    
    /// <summary>
    /// Unit tests for BussinesTransaction Class
    /// </summary>
    public class BussinesTransactionTests
    {
        /// <summary>
        /// Test for a possibility of creation of an instans of a BussinesTransaction
        /// </summary>
        [Test]
        public void BussinesTransaction_Creation_IsNotNull()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction);
        }
        
        /// <summary>
        /// Test for a possibility of creation collection of Operations(transaction units)
        /// </summary>
        [Test]
        public void BussinesTransaction_CreateOperationCollection_IsNotNull()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction.Operations);
        }

        /// <summary>
        /// Test for a possibility of creation journal instance.
        /// </summary>
        [Test]
        public void BussinesTransaction_CreateJournalInBussinesTransaction_IsNotNull()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Assert
            Assert.IsNotNull(bussinesTransaction.Journal);
        }

        /// <summary>
        /// Test for a possibility of creation journal instance.
        /// </summary>
        [Test]
        public void BussinesTransaction_CreateCustomJournalInBussinesTransaction_IsNotNull()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var customJournal = new StabJournal();
            var bussinesTransaction = unitOfWork.BeginTransaction(customJournal);

            // Assert
            Assert.True(bussinesTransaction.Journal is StabJournal);
        }

        /// <summary>
        /// Test for a possibility of added operation to bussines transaction
        /// </summary>
        [Test]
        public void BussinesTransaction_AddOperation_IsTrue()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            var bussinesTransaction = unitOfWork.BeginTransaction();

            // Act
            var operation = new MockTransactionUnit();
            bussinesTransaction.RegisterOperation(operation);

            // Assert
            Assert.IsTrue(bussinesTransaction.Operations.Count > 0);
        }

        /// <summary>
        /// Test for a possibility of commit bussines transaction
        /// </summary>
        [Test]
        public void BussinesTransaction_CommitSomeOperaions_isTrue()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            BussinesTransaction bussinesTransaction;

            using (bussinesTransaction = unitOfWork.BeginTransaction())
            {
                // Act
                bussinesTransaction.Operations.Add(new MockTransactionUnit());
                bussinesTransaction.Operations.Add(new MockTransactionUnit());

                bussinesTransaction.Commit();

                // Assert
                Assert.IsTrue(bussinesTransaction.Operations.All(op =>
                {
                    var fakeUnit = (MockTransactionUnit)op;
                    return fakeUnit.IsCommit;
                }));
            }
        }

        /// <summary>
        /// Test for a possibility of rollback bad bussines transaction
        /// </summary>
        [Test]
        public void BussinesTransaction_RollbackAfterCrash_isTrue()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            BussinesTransaction bussinesTransaction;

            using (bussinesTransaction = unitOfWork.BeginTransaction())
            {
                // Act
                bussinesTransaction.Operations.Add(new MockTransactionUnit());
                bussinesTransaction.Operations.Add(new MockBadTransactionUnit());
                bussinesTransaction.Operations.Add(new MockTransactionUnit());

                bussinesTransaction.Commit();
                
                // Assert
                Assert.IsTrue(bussinesTransaction.Operations.All(op =>
                {
                    var fakeUnit = (IFakeTransactionUnit)op;
                    return !fakeUnit.IsCommit &&
                             fakeUnit.IsRollback;
                }));
            }
        }

        /// <summary>
        /// Test for a possibility of rollback bussines transaction
        /// </summary>
        [Test]
        public void BussinesTransaction_Rollback_isTrue()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            BussinesTransaction bussinesTransaction;

            using (bussinesTransaction = unitOfWork.BeginTransaction())
            {
                // Act
                bussinesTransaction.Operations.Add(new MockTransactionUnit());
                bussinesTransaction.Operations.Add(new MockTransactionUnit());

                bussinesTransaction.Rollback();

                // Assert
                Assert.IsTrue(bussinesTransaction.Operations.All(op =>
                {
                    var fakeUnit = (IFakeTransactionUnit)op;
                    return fakeUnit.IsRollback;
                }));
            }

        }
    }
}
