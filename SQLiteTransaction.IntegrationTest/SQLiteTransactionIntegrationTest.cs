using System;
using System.Data.SQLite;
using System.IO;
using Core;
using NUnit.Framework;
using Units;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;


namespace SQLiteTransaction.IntegrationTest
{
    [TestFixture]
    public class SqLiteTransactionIntegrationTest
    {
        private readonly SqLiteTransaction _sqLiteTransaction = new SqLiteTransaction();
        private string pathToDataBase = Path.GetTempPath() + "test.db";
        private string toDataBase = Path.GetTempPath() + "Work_a_few_SqLiteTransactions.db";

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            if (File.Exists(pathToDataBase))
                File.Delete(pathToDataBase);

            if (File.Exists(toDataBase))
                File.Delete(toDataBase);

            CreatDataBase(pathToDataBase);
            CreatDataBase(toDataBase);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            if (File.Exists(pathToDataBase))
            {
                _sqLiteTransaction.Dispose();
                File.Delete(pathToDataBase);
            }

            if (File.Exists(toDataBase))
                File.Delete(toDataBase);
        }

        [Test]
        public void ConnectDatabase_ConnectToTheWrongName_RetrunFalse()
        {
            var exception = Assert.Catch<Exception>(() => _sqLiteTransaction.ConnectDatabase(""));
            StringAssert.Contains("No such database file.", exception.Message);
        }

        [Test]
        public void ConnectDatabase_ConnectToTheDataBase_RetrunTrue()
        {
            bool result = _sqLiteTransaction.ConnectDatabase(pathToDataBase);
            Assert.IsTrue(result);
            Assert.AreNotEqual(null, _sqLiteTransaction.DbConnection);
            Assert.AreNotEqual(null, _sqLiteTransaction.DbCommand);
            _sqLiteTransaction.Dispose();
        }

        [Test]
        public void AddSqliteCommand_AddComandToDataBase_ReturnTrue()
        {
            _sqLiteTransaction.ConnectDatabase(pathToDataBase);
            bool result = _sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('AddCommand', 'TrueDataBase', 0, strftime('%s', '1993-10-10'));","");

            Assert.IsTrue(result);
            _sqLiteTransaction.Dispose();
        }

        [Test]
        public void Commit_CheckWriteData_ReturnTrue()
        {
            string firstname = "";
            string lastname = "";

            _sqLiteTransaction.ConnectDatabase(pathToDataBase);
            bool rezult = _sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name) VALUES ('Commit', 'Check');",
                 "DELETE FROM person WHERE first_name = 'Commit'");
            _sqLiteTransaction.Commit();
            Assert.IsTrue(rezult);

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", pathToDataBase)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'Commit'", connection))
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

            Assert.IsTrue(rezult);
            Assert.AreEqual("Commit", firstname);
            Assert.AreEqual("Check", lastname);
        }

        [Test]
        public void Commit_Work_a_few_SqLiteTransactions_ReturnTrue()
        {
            string firstname = "";
            string lastname = "";

            SqLiteTransaction sqLiteTransactionGood = new SqLiteTransaction(toDataBase);
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit1', 'Check1');","DELETE FROM person WHERE first_name = 'Commit1'");
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(first_name, last_name) VALUES ('Commit2', 'Check2');","");

            SqLiteTransaction sqLiteTransactiondWithError = new SqLiteTransaction(toDataBase);
            sqLiteTransactiondWithError.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit3', 'Check3');","");

            UnitOfWork unit = new UnitOfWork();
            using (var bussinesTransaction = unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqLiteTransactionGood);
                bussinesTransaction.RegisterOperation(sqLiteTransactiondWithError);
                bussinesTransaction.Commit();
            }

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", toDataBase)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'Commit2'", connection))
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
            }

            Assert.AreEqual("Commit2", firstname);
            Assert.AreEqual("Check2", lastname);
        }

        private void CreatDataBase(string pathToDataBase)
        {
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", pathToDataBase));
            SQLiteCommand command = new SQLiteCommand("CREATE TABLE person("
                                                        + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                        + "first_name TEXT, "
                                                        + "last_name TEXT, "
                                                        + "sex INTEGER, "
                                                        + "birth_date INTEGER);",
                                                        connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();
        }
    }
}
