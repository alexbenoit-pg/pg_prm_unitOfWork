namespace TestApp
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using Core;
    using NUnit.Framework;
    using Units;

    [TestFixture]
    public class TestAppIntegrationTest
    {
        private string pathToSaveDirectory = Path.GetTempPath() + @"FileAndSQLiteTransaction\";
        private string pathToDataBase;
        private UnitOfWork unit = new UnitOfWork();

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            Directory.CreateDirectory(this.pathToSaveDirectory);
            this.pathToDataBase = this.pathToSaveDirectory + "test.db";
            this.CreatDataBase(this.pathToDataBase);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(this.pathToSaveDirectory, true);
        }

        [Test]
        public void WorkFileAndSqliteTransactionUNits_PositiveWork_ReturnTrue()
        {
            var sqliteTransactionFirst = new SqLiteTransaction(this.pathToDataBase);
            sqliteTransactionFirst.AddSqliteCommand(
                "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
                "DELETE FROM person WHERE first_name = 'Commit1'");

            var fileTransaction = new FileTransactionUnit();
            fileTransaction.CreateFile(this.pathToSaveDirectory + "CreateFileTest.txt");

            var sqliteTransactionSecond = new SqLiteTransaction(this.pathToDataBase);
            sqliteTransactionSecond.AddSqliteCommand(
                "UPDATE person set first_name = 'pit' WHERE id = 2",
                "UPDATE person set first_name = 'max' WHERE id = 2");

            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqliteTransactionFirst);
                bussinesTransaction.RegisterOperation(fileTransaction);
                bussinesTransaction.RegisterOperation(sqliteTransactionSecond);
                bussinesTransaction.Commit();
            }

            string firstname = string.Empty;
            string lastname = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", this.pathToDataBase)))
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

            Assert.IsTrue(File.Exists(this.pathToSaveDirectory + "CreateFileTest.txt"));
            Assert.AreEqual("pit", firstname);
            Assert.AreEqual("Check1", lastname);
        }

        [Test]
        public void WorkFileAndSqliteTransactionUNits_NegativWork_ReturnTrue()
        {
            var sqliteTransactionFirst = new SqLiteTransaction(this.pathToDataBase);
            sqliteTransactionFirst.AddSqliteCommand(
                "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
                "DELETE FROM person WHERE first_name = 'Commit1'");

            var fileTransaction = new FileTransactionUnit();
            fileTransaction.CreateFile(this.pathToSaveDirectory + "CreateFileTest.txt");

            var sqliteTransactionSecond = new SqLiteTransaction(this.pathToDataBase);
            sqliteTransactionSecond.AddSqliteCommand("UPDATE somethink set first_name = 'pit' WHERE id = 2", string.Empty);

            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqliteTransactionFirst);
                bussinesTransaction.RegisterOperation(fileTransaction);
                bussinesTransaction.RegisterOperation(sqliteTransactionSecond);
                bussinesTransaction.Commit();
            }

            string firstname = string.Empty;
            string lastname = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", this.pathToDataBase)))
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

            Assert.IsFalse(File.Exists(this.pathToSaveDirectory + "CreateFileTest.txt"));
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

        private void GetInfoofDataBase()
        {
            
        }

        private static void Main()
        {
        }
    }
}