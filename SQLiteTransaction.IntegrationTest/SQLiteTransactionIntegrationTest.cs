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
        private string pathToDataBase = "";

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            pathToDataBase = Path.GetTempPath() + "test.db";
            CreatDataBase(pathToDataBase);
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

        [Test]
        public void ConnectDatabase_ConnectToTheWrongName_RetrunFalse()
        {
            bool result = _sqLiteTransaction.ConnectDatabase("");
            Assert.IsFalse(result);
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
        public void AddSqliteCommand_AddComandToWrongDatabase_ReturnFalse()
        {
            _sqLiteTransaction.ConnectDatabase("");

            bool result = _sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('AddComand', 'WrongDatabase', 0, strftime('%s', '1993-10-10'));","");

            Assert.IsFalse(result);
            _sqLiteTransaction.Dispose();
        }

        [Test]
        public void Commit_CheckWriteData_ReturnTrue()
        {
            _sqLiteTransaction.ConnectDatabase(pathToDataBase);
            bool rezult = _sqLiteTransaction.AddSqliteCommand(
                 "INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('Commit', 'Check', 0, strftime('%s', '1993-10-10'));",
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
                            Assert.AreEqual("Commit", rdr["first_name"]);
                            Assert.AreEqual("Check", rdr["last_name"]);
                        }
                    }
                }
                connection.Close();
            }
        }

        [Test]
        //дорабатываю
        public void Commit_Work_a_few_SqLiteTransactions_ReturnTrue()
        {
            string toDataBase = Path.GetTempPath() + "OneTable.db";
            CreatDataBase(toDataBase);
            string toDataBaseWithError = Path.GetTempPath() + "TwoTable.db";
            CreatDataBase(toDataBaseWithError);
            
            SqLiteTransaction sqLiteTransactionGood = new SqLiteTransaction(toDataBase);
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(id , first_name, last_name, sex, birth_date) VALUES (1, 'Commit1', 'Check1', 0, strftime('%s', '1993-10-10'));",
                                                    "DELETE FROM person WHERE first_name = 'Commit1'");
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('Commit2', 'Check2', 0, strftime('%s', '1993-10-10'));",
                                                    "DELETE FROM person WHERE first_name = 'Commit2'");

            SqLiteTransaction sqLiteTransactiondWithError = new SqLiteTransaction(toDataBase);
            sqLiteTransactiondWithError.AddSqliteCommand("INSERT INTO person(id , first_name, last_name, sex, birth_date) VALUES (1, 'Commit3', 'Check3', 0, strftime('%s', '1993-10-10'));",
                                                    "DELETE FROM person WHERE first_name = 'Commit1'");
            sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('Commit5', 'Check5', 0, strftime('%s', '1993-10-10'));",
                                                    "DELETE FROM person WHERE first_name = 'Commit2'");

            SqLiteTransaction sqLiteTransactiondWithError2 = new SqLiteTransaction(toDataBase);
            sqLiteTransactiondWithError2.AddSqliteCommand("INSERT INTO person(id, first_name, last_name, sex, birth_date) VALUES (1, 'Commit4', 'Check4', 0, strftime('%s', '1993-10-10'));",
                                                    "DELETE FROM person WHERE first_name = 'Commit1'");

            sqLiteTransactionGood.Commit();
            sqLiteTransactiondWithError.Commit();
            sqLiteTransactiondWithError2.Commit();

            UnitOfWork unit = new UnitOfWork();
            using (var bussinesTransaction = unit.BeginTransaction())
            {
                bussinesTransaction.RegisterOperation(sqLiteTransactionGood);
                bussinesTransaction.RegisterOperation(sqLiteTransactiondWithError);
                bussinesTransaction.RegisterOperation(sqLiteTransactiondWithError2);
                bussinesTransaction.Commit();
            }

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", toDataBase)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'Commit1'", connection))
                {
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Assert.AreEqual("Commit", rdr["first_name"]);
                            Assert.AreEqual("Check", rdr["last_name"]);
                        }
                    }
                }
                connection.Close();
            }
            File.Delete(toDataBase);
            File.Delete(toDataBaseWithError);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            if (File.Exists(pathToDataBase))
            {
                _sqLiteTransaction.Dispose();
                File.Delete(pathToDataBase);
            }
        }
    }
}
