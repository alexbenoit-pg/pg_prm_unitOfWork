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
        private string pathToSaveDirectory = Path.GetTempPath() + @"TestFileTransaction\";
       
        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            Directory.CreateDirectory(this.pathToSaveDirectory);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(this.pathToSaveDirectory,true);
        }

        [Test]
        public void FileTransactionUnit_CreateFile_ReturnTrue()
        {
            var filetransaction = new FileUnit();
            filetransaction.CreateFile(this.pathToSaveDirectory + "CreateFileTest.txt");
            filetransaction.Commit();
            filetransaction.Dispose();
           
            Assert.IsTrue(File.Exists(this.pathToSaveDirectory + "CreateFileTest.txt"));
        }

        [Test]
        public void FileTransactionUnit_Move_ReturnTrue()
        {
            Directory.CreateDirectory(this.pathToSaveDirectory + "MoveTestFrom");
            Directory.CreateDirectory(this.pathToSaveDirectory + "MoveTestWhere");

            using (File.Create(this.pathToSaveDirectory + "\\MoveTestFrom\\" + "FileMoveTest.txt"))
            {
            }

            var filetransaction = new FileUnit();
            filetransaction.Move(this.pathToSaveDirectory + "MoveTestFrom\\FileMoveTest.txt", this.pathToSaveDirectory + "MoveTestWhere\\FileMoveTest.txt");
            filetransaction.Commit();
            filetransaction.Dispose();

            Assert.IsTrue(File.Exists(this.pathToSaveDirectory + "MoveTestWhere\\FileMoveTest.txt"));
        }

        [Test]
        public void FileTransactionUnit_Delete_ReturnFalse()
        {
            using (File.Create(this.pathToSaveDirectory + "TestFileDelete.txt"))
            {
            }

            var filetransaction = new FileUnit();
            filetransaction.Delete(this.pathToSaveDirectory + "TestFileDelete.txt");

            filetransaction.Commit();
            filetransaction.Dispose();

            Assert.IsFalse(File.Exists(this.pathToSaveDirectory + "TestFileDelete.txt"));
        }

        [Test]
        public void FileTransactionUnit_Copy_ReturnTrue()
        {
            Directory.CreateDirectory(this.pathToSaveDirectory + "CopyTestFrom");
            Directory.CreateDirectory(this.pathToSaveDirectory + "CopyTestWhere");

            using (File.Create(this.pathToSaveDirectory + "CopyTestFrom" + "\\FileCopyTestFrom.txt"))
            {
            }
            
            var filetransaction = new FileUnit();
            filetransaction.Copy(this.pathToSaveDirectory + "\\CopyTestFrom\\FileCopyTestFrom.txt", this.pathToSaveDirectory + "\\CopyTestWhere\\FileCopyTestWhere.txt", true);

            filetransaction.Commit();
            filetransaction.Dispose();

            Assert.IsTrue(File.Exists(this.pathToSaveDirectory + "CopyTestWhere\\FileCopyTestWhere.txt"));
        }

        [Test]
        public void FileTransactionUnit_AppendAllText_ReturnTrue()
        {
            var filetransaction = new FileUnit();

            using (File.Create(this.pathToSaveDirectory + "\\FileAppendAllTextFrom.txt"))
            {
            }

            filetransaction.AppendAllText(this.pathToSaveDirectory + "\\FileAppendAllTextFrom.txt", "Test Append Text");

            filetransaction.Commit();
            filetransaction.Dispose();

            string readedstring = string.Empty;
            using (StreamReader streamReader = new StreamReader(this.pathToSaveDirectory + "\\FileAppendAllTextFrom.txt", System.Text.Encoding.Default))
            {
                while ((readedstring = streamReader.ReadLine()) != null)
                {
                    if (readedstring == "Test Append Text")
                    {
                        break;
                    }
                }
            }

            Assert.AreEqual("Test Append Text", readedstring);
        }

        [Test]
        public void FileTransactionUnit_WriteAllText_ReturnTrue()
        {
            var filetransaction = new FileUnit();
            filetransaction.WriteAllText(this.pathToSaveDirectory + "\\FileWriteAllTextFrom.txt", "Test Write Text");

            filetransaction.Commit();
            filetransaction.Dispose();

            string readedstring = string.Empty;
            using (StreamReader sr = new StreamReader(this.pathToSaveDirectory + "\\FileWriteAllTextFrom.txt", System.Text.Encoding.Default))
            {
                while ((readedstring = sr.ReadLine()) != null)
                {
                    if (readedstring == "Test Write Text")
                    {
                        break;
                    }
                }
            }

            Assert.AreEqual("Test Write Text", readedstring);
        }
    }
}
