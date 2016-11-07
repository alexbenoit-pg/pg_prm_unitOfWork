using System;
using NUnit.Framework;
using System.IO;
using Core;
using Units;

namespace FileTransaction.IntegrationTest
{
    [TestFixture]
    public class FileTransactionIntagrationTest
    {
        private string _pathToSaveDirectory = Path.GetTempPath() + @"TestFileTransaction\";
        private UnitOfWork _unit = new UnitOfWork();

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            Directory.CreateDirectory(_pathToSaveDirectory);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(_pathToSaveDirectory,true);
        }

        [Test]
        public void FileTransactionUnit_CreateFile_ReturnTrue()
        {
            var filetransaction = new FileTransactionUnit();
            filetransaction.CreateFile(_pathToSaveDirectory + "CreateFileTest.txt");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }

            Assert.IsTrue(File.Exists(_pathToSaveDirectory + "CreateFileTest.txt"));
        }

        [Test]
        public void FileTransactionUnit_Move_ReturnTrue()
        {
            Directory.CreateDirectory(_pathToSaveDirectory + "MoveTestFrom");
            Directory.CreateDirectory(_pathToSaveDirectory + "MoveTestWhere");

            using (File.Create(_pathToSaveDirectory + "\\MoveTestFrom\\" + "FileMoveTest.txt")){}

            var filetransaction = new FileTransactionUnit();
            filetransaction.Move(_pathToSaveDirectory + "MoveTestFrom\\FileMoveTest.txt", _pathToSaveDirectory + "MoveTestWhere\\FileMoveTest.txt");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }
           
            Assert.IsTrue(File.Exists(_pathToSaveDirectory + "MoveTestWhere\\FileMoveTest.txt"));
        }

        [Test]
        public void FileTransactionUnit_Delete_ReturnFalse()
        {
            using (File.Create(_pathToSaveDirectory + "TestFileDelete.txt")){}

            var filetransaction = new FileTransactionUnit();
            filetransaction.Delete(_pathToSaveDirectory + "TestFileDelete.txt");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }
          
            Assert.IsFalse(File.Exists(_pathToSaveDirectory + "TestFileDelete.txt"));
        }

        [Test]
        public void FileTransactionUnit_Copy_ReturnTrue()
        {
            Directory.CreateDirectory(_pathToSaveDirectory + "CopyTestFrom");
            Directory.CreateDirectory(_pathToSaveDirectory + "CopyTestWhere");

            using (File.Create(_pathToSaveDirectory + "CopyTestFrom" + "\\FileCopyTestFrom.txt")){}
            
            var filetransaction = new FileTransactionUnit();
            filetransaction.Copy(_pathToSaveDirectory + "\\CopyTestFrom\\FileCopyTestFrom.txt", _pathToSaveDirectory + "\\CopyTestWhere\\FileCopyTestWhere.txt", true);

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }
          
            Assert.IsTrue(File.Exists(_pathToSaveDirectory + "CopyTestWhere\\FileCopyTestWhere.txt"));
        }

        [Test]
        public void FileTransactionUnit_AppendAllText_ReturnTrue()
        {
            var filetransaction = new FileTransactionUnit();

            using (File.Create(_pathToSaveDirectory + "\\FileAppendAllTextFrom.txt")) { }

            filetransaction.AppendAllText(_pathToSaveDirectory + "\\FileAppendAllTextFrom.txt", "Test Append Text");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }

            string readedstring = "";
            using (StreamReader sr = new StreamReader(_pathToSaveDirectory + "\\FileAppendAllTextFrom.txt", System.Text.Encoding.Default))
            {
                while ((readedstring = sr.ReadLine()) != null)
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
            var filetransaction = new FileTransactionUnit();
            filetransaction.WriteAllText(_pathToSaveDirectory + "\\FileWriteAllTextFrom.txt", "Test Write Text");
           
            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(filetransaction);
                bussinesTransaction.Commit();
            }

            string readedstring = "";
            using (StreamReader sr = new StreamReader(_pathToSaveDirectory + "\\FileWriteAllTextFrom.txt", System.Text.Encoding.Default))
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
