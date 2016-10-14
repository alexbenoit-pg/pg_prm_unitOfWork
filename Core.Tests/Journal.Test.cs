﻿// -----------------------------------------------------------------------
// <copyright file="JournalTests.cs" company="Paragon Software Group">
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
    using System;
    using Core;
    using Core.Tests.Fakes;
    using NUnit.Framework;

    /// <summary>
    /// Unit tests for Journal Class
    /// </summary>
    [TestFixture]
    public class JournalTests
    {

        /// <summary>
        /// Test for a possibility of creation of an instans of a Journal
        /// </summary>
        [Test]
        public void Journal_CreateIstance_IsNotNull()
        {
            // Arrange
            var journal = new JSONJournal();

            // Assert
            Assert.IsNotNull(journal);
        }

        [Test]
        public void Journal_AddOperation_WithoutException()
        {
            // Arrange
            var journal = new JSONJournal();
            var operation = new MockTransactionUnit();

            // Assert
            Assert.That(() => journal.Add(operation),
                Throws.Nothing);
        }
        
        [Test]
        public void Journal_DeleteOperation_WithoutException()
        {
            // Arrange
            var journal = new JSONJournal();
            var operation = new MockTransactionUnit(); 

            // Assert
            Assert.That(() => journal.Delete(operation),
                Throws.Nothing);
        }
    }
}