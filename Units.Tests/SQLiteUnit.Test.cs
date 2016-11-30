// -----------------------------------------------------------------------
// <copyright file="SQLiteUnitTests.cs" company="Paragon Software Group">
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
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using Core.Exceptions;
    using NUnit.Framework;
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class SQLiteUnitTests
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
        
        private string[] Names => new string[]
        {
                this.FirstName1,
                this.FirstName2,
                this.FirstName3
        };

        private string[] LastNames => new string[]
        {
                this.LastName1,
                this.LastName2,
                this.LastName3
        };

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
        public void SQLiteUnit_OneCommandInUnit_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            string command =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}')";
            unit.AddSqliteCommand(command, string.Empty);

            // Act
            unit.Commit();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            var firstNameInDb = personsInDatabase[0][0];
            var lastNameInDb = personsInDatabase[0][1];
            Assert.AreEqual(this.FirstName1, firstNameInDb);
            Assert.AreEqual(this.LastName1, lastNameInDb);
        }

        [Test]
        public void SQLiteUnit_SeveralCommandsInUnit_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < this.Names.Length; i++)
            {
                string command =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({i}, '{this.Names[i]}', '{this.LastNames[i]}')";
                unit.AddSqliteCommand(command, string.Empty);
            }

            // Act
            unit.Commit();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            for (int i = 0; i < this.Names.Length; i++)
            {
                var firstNameInDb = personsInDatabase[i][0];
                var lastNameInDb = personsInDatabase[i][1];
                Assert.AreEqual(this.Names[i], firstNameInDb);
                Assert.AreEqual(this.LastNames[i], lastNameInDb);
            }
        }

        [Test]
        public void SQLiteUnit_BadCommandInUnitAndThrowException_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            string command =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}, FAKE_COLUMN) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}', 'FAKE FIELD')";
            unit.AddSqliteCommand(command, string.Empty);

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
        }
        
        [Test]
        public void SQLiteUnit_BadFieldInCommandAndThrowException_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            string badField = "String instead of integer value";
            string command =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({badField}, '{this.FirstName1}', '{this.LastName1}')";
            unit.AddSqliteCommand(command, string.Empty);

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
        }

        [Test]
        public void SQLiteUnit_RollbackUnitWithOneCommand_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            string command =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}')";
            string rollbackCommand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = 1";
            unit.AddSqliteCommand(command, rollbackCommand);

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            Assert.IsTrue(!personsInDatabase.Any());
        }

        [Test]
        public void SQLiteUnit_RollbackUnitWithSeveralCommands_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < this.Names.Length; i++)
            {
                int id = i + 1;
                string command =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({id}, '{Names[i]}', '{this.LastNames[i]}')";
                string rollbackComand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = {id}";
                unit.AddSqliteCommand(command, rollbackComand);
            }

            // Act
            unit.Commit();
            unit.Rollback();

            // Assert
            var personsInDatabase = this.GetInfOfDataBase();
            Assert.IsTrue(!personsInDatabase.Any());
        }

        [Test]
        public void SQLiteUnit_RunAndRollbackUnitWithOneCommandBadCommand_ReturnTrue()
        {
            // Arrange
            var unit = new SQLiteUnit(this.PathToDataBase);
            for (int i = 0; i < this.Names.Length; i++)
            {
                int id = i + 1;
                string command =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({id}, '{this.Names[i]}', '{this.LastNames[i]}')";
                string rollbackComand = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = {id}";

                if (i == 2)
                {
                    string badField = "String instead of integer value";
                    command =
                        $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                        + $"VALUES ({badField}, '{this.FirstName1}', '{this.LastName1}')";
                }

                unit.AddSqliteCommand(command, rollbackComand);
            }

            // Assert
            Assert.Throws<CommitException>(() => unit.Commit());
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
                            result.Add(new string[] { firstName, lastName });
                        }
                    }
                }

                connection.Close();
                return result;
            }
        }
    }
}
