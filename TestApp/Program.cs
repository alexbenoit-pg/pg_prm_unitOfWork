using System;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace TestApp
{
    using Core;
    using NUnit.Framework;
    using Units;
    using Core.Tests.Fakes;
    using System.IO;

    [TestFixture]
    public class TestAppIntegrationTest
    {
        private string _pathToSaveDirectory = Path.GetTempPath() + @"FileAndSQLiteTransaction\";
        private string pathToDataBase;
        private UnitOfWork _unit = new UnitOfWork();

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            Directory.CreateDirectory(_pathToSaveDirectory);
            pathToDataBase = _pathToSaveDirectory + "test.db";
            CreatDataBase(pathToDataBase);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(_pathToSaveDirectory, true);
        }

        [Test]
        public void WorkFileAndSqliteTransactionUNits_PositiveWork_ReturnTrue()
        {
            var sqliteTransactionFirst = new SqLiteTransaction(pathToDataBase);
            sqliteTransactionFirst.AddSqliteCommand(
                "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
                "DELETE FROM person WHERE first_name = 'Commit1'");

            var fileTransaction = new FileTransactionUnit();
            fileTransaction.CreateFile(_pathToSaveDirectory + "CreateFileTest.txt");

            var sqliteTransactionSecond = new SqLiteTransaction(pathToDataBase);
            sqliteTransactionSecond.AddSqliteCommand(
                "UPDATE person set first_name = 'pit' WHERE id = 2",
                "UPDATE person set first_name = 'max' WHERE id = 2");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqliteTransactionFirst);
                bussinesTransaction.RegisterOperation(fileTransaction);
                bussinesTransaction.RegisterOperation(sqliteTransactionSecond);
                bussinesTransaction.Commit();
            }

            #region SqliteCheck
            string firstname = "";
            string lastname = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", pathToDataBase)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'pit'", connection))
                {
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            firstname = rdr["first_name"].ToString();
                            lastname = rdr["last_name"].ToString();
                        }
                    }
                }
                connection.Close();
            }
            #endregion

            Assert.IsTrue(File.Exists(_pathToSaveDirectory + "CreateFileTest.txt"));
            Assert.AreEqual("pit", firstname);
            Assert.AreEqual("Check1", lastname);
        }

        [Test]
        public void WorkFileAndSqliteTransactionUNits_NegativWork_ReturnTrue()
        {
            var sqliteTransactionFirst = new SqLiteTransaction(pathToDataBase);
            sqliteTransactionFirst.AddSqliteCommand(
                "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
                "DELETE FROM person WHERE first_name = 'Commit1'");

            var fileTransaction = new FileTransactionUnit();
            fileTransaction.CreateFile(_pathToSaveDirectory + "CreateFileTest.txt");

            var sqliteTransactionSecond = new SqLiteTransaction(pathToDataBase);
            sqliteTransactionSecond.AddSqliteCommand(
                "UPDATE somethink set first_name = 'pit' WHERE id = 2","");

            using (var bussinesTransaction = _unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqliteTransactionFirst);
                bussinesTransaction.RegisterOperation(fileTransaction);
                bussinesTransaction.RegisterOperation(sqliteTransactionSecond);
                bussinesTransaction.Commit();
            }

            #region SqliteCheck
            string firstname = "";
            string lastname = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", pathToDataBase)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'pit'", connection))
                {
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            firstname = rdr["first_name"].ToString();
                            lastname = rdr["last_name"].ToString();
                        }
                    }
                }
                connection.Close();
            }
            #endregion

            Assert.IsFalse(File.Exists(_pathToSaveDirectory + "CreateFileTest.txt"));
            Assert.AreNotEqual("pit", firstname);
            Assert.AreNotEqual("Check1", lastname);
        }

        private void CreatDataBase(string pathDataBase)
        {
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", pathDataBase));
            SQLiteCommand command = new SQLiteCommand("CREATE TABLE person("
                                                        + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                        + "first_name TEXT, "
                                                        + "last_name TEXT);",
                                                        connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();
        }

        static void Main()
        {
            
        }
    }
}