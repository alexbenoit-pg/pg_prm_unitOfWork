// -----------------------------------------------------------------------
// <copyright file="FileUnitTests.cs" company="Paragon Software Group">
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

namespace Units.Tests
{
    using System;
    using System.IO;
    using Core.Exceptions;
    using NUnit.Framework;
    using Units;

    [TestFixture]
    public class FileUnitTests
    {
        private static string PathToSaveDirectory => $"{Path.GetTempPath()}FileAndSQLiteTransaction\\";

        private string TargetDirectory => $"{PathToSaveDirectory}TargetFolder\\";

        private string AppendFilePath => $"{PathToSaveDirectory}append.txt";

        private string WriteFilePath => $"{PathToSaveDirectory}write.txt";

        private string Content => "Text for test.";

        private string CopyFilePath => $"{PathToSaveDirectory}copy.txt";

        private string DeleteFilePath => $"{PathToSaveDirectory}delete.txt";

        private string MoveFilePath => $"{PathToSaveDirectory}move.txt";

        private string CreateFilePath => $"{PathToSaveDirectory}CreateFileTest.txt";

        private string MovebleFilePath => $"{TargetDirectory}moveble.txt";

        private string CopybleFilePath => $"{PathToSaveDirectory}copy_2.txt";

        private string AddedContent => "\n=====\nThis text was added\n=====\n";

        [SetUp]
        public void TestFixtureSetup()
        {
            if (Directory.Exists(PathToSaveDirectory))
            {
                Directory.Delete(PathToSaveDirectory, true);
            }

            Directory.CreateDirectory(PathToSaveDirectory);
            File.Create(this.AppendFilePath).Close();
            File.AppendAllText(this.AppendFilePath, this.Content);
            File.Create(this.WriteFilePath).Close();
            File.AppendAllText(this.WriteFilePath, this.Content);
            File.Create(this.CopyFilePath).Close();
            File.Create(this.DeleteFilePath).Close();
            File.Create(this.MoveFilePath).Close();
            Directory.CreateDirectory(this.TargetDirectory);
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(PathToSaveDirectory, true);
        }

        [Test]
        public void FileUnit_CreateFile_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);

            // Act
            unit.Commit();

            // Assert
            Assert.IsTrue(File.Exists(this.CreateFilePath));
        }

        [Test]
        public void FileUnit_RollbackCreateFile_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            Assert.IsFalse(File.Exists(this.CreateFilePath));
        }
        
        [Test]
        public void FileUnit_CreateFileInNonExistFolder_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.CreateFile(Path.Combine(PathToSaveDirectory, "UnrealFolder","File.txt"));

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
        }

        [Test]
        public void FileUnit_Move_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Move(this.MoveFilePath, this.MovebleFilePath);

            // Act
            unit.Commit();

            // Assert
            Assert.IsTrue(File.Exists(this.MovebleFilePath) 
                            && !File.Exists(this.MoveFilePath));
        }

        [Test]
        public void FileUnit_RollbackMove_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Move(this.MoveFilePath, this.MovebleFilePath);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            Assert.IsTrue(!File.Exists(this.MovebleFilePath)
                            && File.Exists(this.MoveFilePath));
        }

        [Test]
        public void FileUnit_Delete_ReturnFalse()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Delete(this.DeleteFilePath);

            // Act
            unit.Commit();

            // Assert
            Assert.IsFalse(File.Exists(this.DeleteFilePath));
        }

        [Test]
        public void FileUnit_RollbackDelete_ReturnFalse()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Delete(this.DeleteFilePath);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
        }

        [Test]
        public void FileUnit_Copy_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);

            // Act
            unit.Commit();

            // Assert
            Assert.IsTrue(File.Exists(this.CopyFilePath)
                && File.Exists(this.CopybleFilePath));
        }

        [Test]
        public void FileUnit_RollbackCopy_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            Assert.IsTrue(File.Exists(this.CopyFilePath)
                && !File.Exists(this.CopybleFilePath));
        }

        [Test]
        public void FileUnit_AppendAllText_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);

            // Act
            unit.Commit();

            // Assert
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content + this.AddedContent);
        }

        [Test]
        public void FileUnit_RollbackAppendAllText_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
        }

        [Test]
        public void FileUnit_WriteAllText_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);

            // Act
            unit.Commit();

            // Assert
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.AddedContent);
        }

        [Test]
        public void FileUnit_RollbackWriteAllText_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
        }

        [Test]
        public void FileUnit_BadOperationAfterGoodOperationsAndRollback_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);
            unit.Delete(this.DeleteFilePath);
            unit.Move(this.MoveFilePath, this.MovebleFilePath);
            // Плохая операция
            unit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);
            
            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
        }

        [Test]
        public void FileUnit_BadOperationBetweenGoodOperationsAndRollback_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);
            // Плохая операция
            unit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);
            unit.Delete(this.DeleteFilePath);
            unit.Move(this.MoveFilePath, this.MovebleFilePath);
            unit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
        }
        
        [Test]
        public void FileUnit_BadOperationBeforeGoodOperationsAndRollback_ReturnTrue()
        {
            // Arrange
            var unit = new FileUnit();
            // Плохая операция
            unit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);
            unit.CreateFile(this.CreateFilePath);
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);
            unit.Delete(this.DeleteFilePath);
            unit.Move(this.MoveFilePath, this.MovebleFilePath);
            unit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
        }

    }
}
