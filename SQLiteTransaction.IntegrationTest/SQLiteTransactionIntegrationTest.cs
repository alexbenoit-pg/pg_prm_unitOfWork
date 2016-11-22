using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Units.SQLiteTransactionUnit;

namespace SQLiteTransaction.IntegrationTest
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using NUnit.Framework;
    using Units;

    [TestFixture]
    public class SqLiteTransactionIntegrationTest
    {
        private static string PathToSaveDirectory => $"{Path.GetTempPath()}FileAndSQLiteTransaction\\";

        private string PathToDataBase => $"{PathToSaveDirectory}test.db";

        private string DbTableName => "Person";

        private string DbFieldFirstName => "FirstName";

        private string DbFieldLastName => "LastName";

        private string DbFieldId => "Id";

        private string FirstName1 => "Max";

        private string LastName1 => "Doerty";

        private string FirstName2 => "Pit";

        private string LastName2 => "Grakham";

        private string FirstName3 => "Jhon";

        private string LastName3 => "Snow";

        [SetUp]
        public void TestFixtureSetup()
        {
            if (Directory.Exists(PathToSaveDirectory))
            {
                Directory.Delete(PathToSaveDirectory, true);
            }

            Directory.CreateDirectory(PathToSaveDirectory);
            this.CreatDataBase(this.PathToDataBase);
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(PathToSaveDirectory, true);
        }

        [Test]
        public void OneCommandInUnit_ReturnTrue()
        {
            //Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}')";
            sqliteTransactionFirst.AddSqliteCommand(firstSqlCommand, string.Empty);

            //Act
            sqliteTransactionFirst.Commit();

            //Assert
            var personsInDatabase = this.GetInfOfDataBase();
            var FirstNameinDb = personsInDatabase[0][0];
            var LastNameinDb = personsInDatabase[0][1];
            Assert.AreEqual(this.FirstName1, FirstNameinDb);
            Assert.AreEqual(this.LastName1, LastNameinDb);
        }

        [Test]
        public void SeveralCommandsInUnit_ReturnTrue()
        {
            var names = new string[]
            {
                this.FirstName1,
                this.FirstName2,
                this.FirstName3
            };
            var lastNames = new string[]
            {
                this.LastName1,
                this.LastName2,
                this.LastName3
            };
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < names.Length; i++)
            {
                string sqlCommand =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({i}, '{names[i]}', '{lastNames[i]}')";
                sqliteTransactionFirst.AddSqliteCommand(sqlCommand, string.Empty);
            }

            // Act
            sqliteTransactionFirst.Commit();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            for (int i = 0; i < names.Length; i++)
            {
                var firstNameinDb = personsInDatabase[i][0];
                var lastNameinDb = personsInDatabase[i][1];
                Assert.AreEqual(names[i], firstNameinDb);
                Assert.AreEqual(lastNames[i], lastNameinDb);
            }

        }

        [Test]
        public void BadCommandInUnitAndThrowException_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}, FAKE_COLUMN) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}', 'FAKE FIELD')";
            sqliteTransactionFirst.AddSqliteCommand(firstSqlCommand, string.Empty);
            
            //Assert
            Assert.Throws<SQLiteException>(() => sqliteTransactionFirst.Commit());
        }


        [Test]
        public void BadFieldInCommandAndThrowException_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string badField = "String instead of integer value";
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({badField}, '{this.FirstName1}', '{this.LastName1}')";
            sqliteTransactionFirst.AddSqliteCommand(firstSqlCommand, string.Empty);

            //Assert
            Assert.Throws<SQLiteException>(() => sqliteTransactionFirst.Commit());
        }

        [Test]
        public void RollbackUnitWithOneCommand_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}')";
            string rollbackCommand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = 1";
            sqliteTransactionFirst.AddSqliteCommand(firstSqlCommand, rollbackCommand);

            // Act
            sqliteTransactionFirst.Commit();
            sqliteTransactionFirst.Rollback();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            Assert.IsTrue(!personsInDatabase.Any());
        }
        
        [Test]
        public void RollbackUnitWithSeveralCommands_ReturnTrue()
        {
            // Arrange
            var names = new string[]
            {
                this.FirstName1,
                this.FirstName2,
                this.FirstName3
            };
            var lastNames = new string[]
            {
                this.LastName1,
                this.LastName2,
                this.LastName3
            };
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < names.Length; i++)
            {
                int id = i + 1;
                string sqlCommand =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({id}, '{names[i]}', '{lastNames[i]}')";
                string rollbackComand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = {id}";
                sqliteTransactionFirst.AddSqliteCommand(sqlCommand, rollbackComand);
            }

            // Act
            sqliteTransactionFirst.Commit();
            sqliteTransactionFirst.Rollback();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            Assert.IsTrue(!personsInDatabase.Any());
        }

        [Test]
        public void RunAndRollbackUnitWithOneCommandBadCommand_ReturnTrue()
        {
            // Arrange
            var names = new string[]
            {
                this.FirstName1,
                this.FirstName2,
                this.FirstName3
            };
            var lastNames = new string[]
            {
                this.LastName1,
                this.LastName2,
                this.LastName3
            };
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < names.Length; i++)
            {
                int id = i + 1;
                string sqlCommand =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({id}, '{names[i]}', '{lastNames[i]}')";
                string rollbackComand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = {id}";

                if (i == 2)
                {
                    string badField = "String instead of integer value";
                    sqlCommand =
                        $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                        + $"VALUES ({badField}, '{this.FirstName1}', '{this.LastName1}')";
                }
                sqliteTransactionFirst.AddSqliteCommand(sqlCommand, rollbackComand);
            }

            // Assert
            Assert.Throws<SQLiteException>(() => sqliteTransactionFirst.Commit());
            var personsInDatabase = this.GetInfOfDataBase();
            Assert.IsTrue(!personsInDatabase.Any());
        }

        private void CreatDataBase(string pathDataBase)
        {
            string sqliteConnectionString = SQLiteManager.GetConnectionString(pathDataBase);
            SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString);

            SQLiteCommand command = new SQLiteCommand(
                $"CREATE TABLE {DbTableName}("
                    + $"{DbFieldId} INTEGER, "
                    + $"{DbFieldFirstName} TEXT, "
                    + $"{DbFieldLastName} TEXT);",
                connection);

            connection.Open();
            command.ExecuteNonQuery();
            command.Dispose();
            connection.Close();
        }

        private List<string[]> GetInfOfDataBase()
        {
            var result = new List<string[]>();

            string connectionString = SQLiteManager.GetConnectionString(this.PathToDataBase);
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(
                    $"SELECT * FROM person",
                    connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var firstName = reader[$"{DbFieldFirstName}"].ToString();
                            var lastName = reader[$"{DbFieldLastName}"].ToString();
                            result.Add(new string[] {firstName, lastName});
                        }
                    }
                }

                connection.Close();
                return result;
            }

        }
    }

    //[TestFixture]
    //public class SqLiteTransactionIntegrationTest
    //{
    //    //private readonly SqLiteTransaction sqLiteTransaction;
    //    private string pathToDataBase = Path.GetTempPath() + "test.db";
    //    private string toDataBase = Path.GetTempPath() + "Work_a_fewsqLiteTransactions.db";

    //    [OneTimeSetUp]
    //    public void TestFixtureSetup()
    //    {
    //        if (File.Exists(this.pathToDataBase))
    //        {
    //            File.Delete(this.pathToDataBase);
    //        }

    //        if (File.Exists(this.toDataBase))
    //        {
    //            File.Delete(this.toDataBase);
    //        }

    //        this.CreatDataBase(this.pathToDataBase);
    //        this.CreatDataBase(this.toDataBase);
    //    }

    //    [OneTimeTearDown]
    //    public void TestFixtureTearDown()
    //    {
    //        if (File.Exists(this.pathToDataBase))
    //        {
    //            File.Delete(this.pathToDataBase);
    //        }

    //        if (File.Exists(this.toDataBase))
    //        {
    //            File.Delete(this.toDataBase);
    //        }
    //    }

    //    [Test]
    //    public void Constructor_ConnectToTheWrongName_Throw()
    //    {
    //        SQLiteUnit sqLiteTransaction = new SQLiteUnit(string.Empty);
    //        var exception = Assert.Catch<Exception>(() => sqLiteTransaction.Commit());
    //        StringAssert.Contains("No such database file.", exception.Message);
    //        sqLiteTransaction.Dispose();
    //    }

    //    [Test]
    //    public void AddSqliteCommand_AddComandToDataBase_ReturnTrue()
    //    {
    //        SQLiteUnit sqLiteTransaction = new SQLiteUnit(this.pathToDataBase);
    //        bool result = sqLiteTransaction.AddSqliteCommand(
    //             "INSERT INTO person(first_name, last_name, sex, birth_date) VALUES ('AddCommand', 'TrueDataBase', 0, strftime('%s', '1993-10-10'));", string.Empty);

    //        Assert.IsTrue(result);
    //        sqLiteTransaction.Dispose();
    //    }

    //    [Test]
    //    public void Commit_CheckWriteData_ReturnTrue()
    //    {
    //        string firstname = string.Empty;
    //        string lastname = string.Empty;

    //        SQLiteUnit sqLiteTransaction = new SQLiteUnit(this.pathToDataBase);
    //        bool rezult = sqLiteTransaction.AddSqliteCommand(
    //             "INSERT INTO person(first_name, last_name) VALUES ('Commit', 'Check');",
    //             "DELETE FROM person WHERE first_name = 'Commit'");
    //        sqLiteTransaction.Commit();
    //        Assert.IsTrue(rezult);
    //        string sqliteConnectionString = SQLiteUnit.GetConnectionString(this.pathToDataBase);
    //        using (SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString))
    //        {
    //            connection.Open();
    //            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'Commit'", connection))
    //            {
    //                using (SQLiteDataReader rdr = command.ExecuteReader())
    //                {
    //                    while (rdr.Read())
    //                    {
    //                        firstname = rdr["first_name"].ToString();
    //                        lastname = rdr["last_name"].ToString();
    //                    }
    //                }
    //            }

    //            connection.Close();
    //        }

    //        Assert.IsTrue(rezult);
    //        Assert.AreEqual("Commit", firstname);
    //        Assert.AreEqual("Check", lastname);
    //    }

    //    //[Test]
    //    //public void Commit_Work_a_fewsqLiteTransactions_ReturnTrue()
    //    //{
    //    //    string firstname = string.Empty;
    //    //    string lastname = string.Empty;
    //    //    SQLiteUnit sqLiteTransactionGood = new SQLiteUnit(this.toDataBase);
    //    //    sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit1', 'Check1');", "DELETE FROM person WHERE first_name = 'Commit1'");
    //    //    sqLiteTransactionGood.AddSqliteCommand("INSERT INTO person(first_name, last_name) VALUES ('Commit2', 'Check2');", string.Empty);

    //    //    SQLiteUnit sqLiteTransactiondWithError = new SQLiteUnit(this.toDataBase);
    //    //    sqLiteTransactiondWithError.AddSqliteCommand("INSERT INTO person(id, first_name, last_name) VALUES (1, 'Commit3', 'Check3');", string.Empty);

    //    //    UnitOfWork unit = new UnitOfWork();
    //    //    using (var bussinesTransaction = unit.BeginTransaction())
    //    //    {
    //    //        bussinesTransaction.ExecuteUnit(sqLiteTransactionGood);
    //    //        bussinesTransaction.ExecuteUnit(sqLiteTransactiondWithError);
    //    //        bussinesTransaction.Commit();
    //    //    }

    //    //    string sqliteConnectionString = SQLiteUnit.GetConnectionString(this.toDataBase);
    //    //    using (SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString))
    //    //    {
    //    //        connection.Open();
    //    //        using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'Commit2'", connection))
    //    //        {
    //    //            using (SQLiteDataReader rdr = command.ExecuteReader())
    //    //            {
    //    //                while (rdr.Read())
    //    //                {
    //    //                    firstname = rdr["first_name"].ToString();
    //    //                    lastname = rdr["last_name"].ToString();
    //    //                }
    //    //            }
    //    //        }
    //    //    }

    //    //    Assert.AreEqual("Commit2", firstname);
    //    //    Assert.AreEqual("Check2", lastname);
    //    //}

    //    private void CreatDataBase(string pathToDataBase)
    //    {
    //        string sqliteConnectionString = SQLiteUnit.GetConnectionString(pathToDataBase);
    //        SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString);
    //        SQLiteCommand command = new SQLiteCommand("CREATE TABLE person("
    //                                                    + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
    //                                                    + "first_name TEXT, "
    //                                                    + "last_name TEXT, "
    //                                                    + "sex INTEGER, "
    //                                                    + "birth_date INTEGER);",
    //                                                    connection);
    //        connection.Open();
    //        command.ExecuteNonQuery();
    //        connection.Close();
    //        connection.Dispose();
    //    }
    //}
}
