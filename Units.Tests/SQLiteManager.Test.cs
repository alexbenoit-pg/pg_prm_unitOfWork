// -----------------------------------------------------------------------
// <copyright file="SQLiteManagerTests.cs" company="Paragon Software Group">
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
    using NUnit.Framework;
    using Units.SQLiteTransactionUnit;
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class SQLiteManagerTests
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
        public void SQLiteUnit_OneCommand_ReturnTrue()
        {
            //Arrange
             string command =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES (1, '{this.FirstName1}', '{this.LastName1}')";
            //Act
            SQLiteManager.ExecuteCommandsInTransaction(
                this.PathToDataBase,
                new List<string> { command });

            //Assert
            var personsInDatabase = this.GetInfOfDataBase();
            var FirstNameinDb = personsInDatabase[0][0];
            var LastNameinDb = personsInDatabase[0][1];
            Assert.AreEqual(this.FirstName1, FirstNameinDb);
            Assert.AreEqual(this.LastName1, LastNameinDb);
        }

        [Test]
        public void SQLiteUnit_SeveralCommands_ReturnTrue()
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
            var commands = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                string command =
                    $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                    + $"VALUES ({i}, '{names[i]}', '{lastNames[i]}')";
                commands.Add(command);
            }

            // Act
            SQLiteManager.ExecuteCommandsInTransaction(
                this.PathToDataBase,
                commands);

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
