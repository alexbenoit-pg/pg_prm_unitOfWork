namespace SQLiteTransaction.IntegrationTest
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using Core;
    using NUnit.Framework;
    using Units;
    using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;

    [TestFixture]
    public class SqLiteTransactionIntegrationTest
    {
        private readonly SqLiteTransaction sqLiteTransaction = new SqLiteTransaction();
        private string pathToDataBase = Path.GetTempPath() + "test.db";
        private string toDataBase = Path.GetTempPath() + "Work_a_fewsqLiteTransactions.db";

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            if (File.Exists(this.pathToDataBase))
            {
                File.Delete(this.pathToDataBase);
            }

            if (File.Exists(this.toDataBase))
            {
                File.Delete(this.toDataBase);
            }

            this.CreatDataBase(this.pathToDataBase);
            this.CreatDataBase(this.toDataBase);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            if (File.Exists(this.pathToDataBase))
            {
                this.sqLiteTransaction.Dispose();
                File.Delete(this.pathToDataBase);
            }

            if (File.Exists(this.toDataBase))
            {
                File.Delete(this.toDataBase);
            }
        }

        [Test]
        public void ConnectDatabase_ConnectToTheWrongName_Throw()
        {
            var exception = Assert.Catch<Exception>(() => this.sqLiteTransaction.ConnectDatabase(string.Empty));
            StringAssert.Contains("No such database file.", exception.Message);
        }

        [Test]
        public void ConnectDatabase_ConnectToTheDataBase_RetrunTrue()
        {
            bool result = this.sqLiteTransaction.ConnectDatabase(this.pathToDataBase);
            Assert.IsTrue(result);
            Assert.AreNotEqual(null, this.sqLiteTransaction.dbConnection);
            Assert.AreNotEqual(null, this.sqLiteTransaction.dbCommand);
            this.sqLiteTransaction.Dispose();
        }

        [Test]
        public void AddSqliteCommand_AddComandToDataBase_ReturnTrue()
        {
            this.sqLiteTransaction.ConnectDatabase(this.pathToDataBase);
            bool result = this.sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('AddCommand', 'TrueDataBase', 0, strftime('%s', '1993-10-10'));", string.Empty);

            Assert.IsTrue(result);
            this.sqLiteTransaction.Dispose();
        }

        [Test]
        public void Commit_CheckWriteData_ReturnTrue()
        {
            string firstname = string.Empty;
            string lastname = string.Empty;

            this.sqLiteTransaction.ConnectDatabase(this.pathToDataBase);
            bool rezult = this.sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name) VALUES ('Commit', 'Check');",
                 "DELETE FROM person WHERE first_name = 'Commit'");
            this.sqLiteTransaction.Commit();
            Assert.IsTrue(rezult);

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", this.pathToDataBase)))
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
        public void Commit_Work_a_fewsqLiteTransactions_ReturnTrue()
        {
            string firstname = string.Empty;
            string lastname = string.Empty;
            SqLiteTransaction sqLiteTransactionGood = new SqLiteTransaction(this.toDataBase);
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit1', 'Check1');", "DELETE FROM person WHERE first_name = 'Commit1'");
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(first_name, last_name) VALUES ('Commit2', 'Check2');", string.Empty);

            SqLiteTransaction sqLiteTransactiondWithError = new SqLiteTransaction(this.toDataBase);
            sqLiteTransactiondWithError.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit3', 'Check3');", string.Empty);

            UnitOfWork unit = new UnitOfWork();
            using (var bussinesTransaction = unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqLiteTransactionGood);
                bussinesTransaction.RegisterOperation(sqLiteTransactiondWithError);
                bussinesTransaction.Commit();
            }

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", this.toDataBase)))
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
