namespace FileTransaction.IntegrationTest
{
    using System;
    using System.IO;
    using Core;
    using NUnit.Framework;
    using Units;

    [TestFixture]
    public class FileTransactionIntagrationTest
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
        public void CreateFile_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);
            unit.Commit();
            unit.Dispose();
           
            Assert.IsTrue(File.Exists(this.CreateFilePath));
        }

        [Test]
        public void RollbackCreateFile_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.CreateFile(this.CreateFilePath);
            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            Assert.IsFalse(File.Exists(this.CreateFilePath));
        }

        [Test]
        public void Move_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.Move(this.MoveFilePath, this.MovebleFilePath);
            unit.Commit();
            unit.Dispose();

            Assert.IsTrue(File.Exists(this.MovebleFilePath) 
                            && !File.Exists(this.MoveFilePath));
        }

        [Test]
        public void RollbackMove_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.Move(this.MoveFilePath, this.MovebleFilePath);
            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            Assert.IsTrue(!File.Exists(this.MovebleFilePath)
                            && File.Exists(this.MoveFilePath));
        }

        [Test]
        public void Delete_ReturnFalse()
        {
            var unit = new FileUnit();
            unit.Delete(this.DeleteFilePath);
            unit.Commit();
            unit.Dispose();

            Assert.IsFalse(File.Exists(this.DeleteFilePath));
        }

        [Test]
        public void RollbackDelete_ReturnFalse()
        {
            var unit = new FileUnit();
            unit.Delete(this.DeleteFilePath);
            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            Assert.IsTrue(File.Exists(this.DeleteFilePath));
        }

        [Test]
        public void Copy_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            unit.Commit();
            unit.Dispose();

            Assert.IsTrue(File.Exists(this.CopyFilePath)
                && File.Exists(this.CopybleFilePath));
        }

        [Test]
        public void RollbackCopy_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            Assert.IsTrue(File.Exists(this.CopyFilePath)
                && !File.Exists(this.CopybleFilePath));
        }

        [Test]
        public void AppendAllText_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);

            unit.Commit();
            unit.Dispose();
            
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content + this.AddedContent);
        }

        [Test]
        public void RollbackAppendAllText_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.AppendAllText(this.AppendFilePath, this.AddedContent);

            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
        }

        [Test]
        public void WriteAllText_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);

            unit.Commit();
            unit.Dispose();

            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.AddedContent);
        }

        [Test]
        public void RollbackWriteAllText_ReturnTrue()
        {
            var unit = new FileUnit();
            unit.WriteAllText(this.WriteFilePath, this.AddedContent);

            unit.Commit();
            unit.Rollback();
            unit.Dispose();

            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
        }
    }
}
