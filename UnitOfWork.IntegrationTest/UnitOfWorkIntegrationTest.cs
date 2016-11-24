// -----------------------------------------------------------------------
// <copyright file="UnitOfWorkIntegrationTest.cs" company="Paragon Software Group">
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

namespace TestApp
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using Core;
    using NUnit.Framework;
    using Units;
    using Units.SQLiteTransactionUnit;

    [TestFixture]
    public class UnitOfWorkIntegrationTest
    {
        private readonly UnitOfWork unit = new UnitOfWork(new UnitJsonJournal(), false);

        private static string PathToSaveDirectory => $"{Path.GetTempPath()}FileAndSQLiteTransaction\\";

        private string PathToDataBase => $"{PathToSaveDirectory}test.db";

        private string DbTableName => "Person";

        private string DbFieldFirstName => "FirstName";

        private string DbFieldLastName => "LastName";

        private string DbFieldId => "Id";

        private string DbRowId => "1";

        private string FirstName => "Max";

        private string LastName => "Doerty";

        private string NewFirstName => "Pit";

        private string TargetDirectory => $"{PathToSaveDirectory}TargetFolder\\";

        private string AppendFilePath => $"{PathToSaveDirectory}append.txt"; 

        private string WriteFilePath => $"{PathToSaveDirectory}write.txt"; 

        private string Content => "Text for test.";

        private string CopyFilePath => $"{PathToSaveDirectory}copy.txt"; 

        private string DeleteFilePath => $"{PathToSaveDirectory}delete.txt"; 

        private string MoveFilePath => $"{PathToSaveDirectory}move.txt"; 

        private string CreateFilePath => $"{PathToSaveDirectory}CreateFileTest.txt"; 

        private string MovebleFilePath => $"{TargetDirectory}moveble.txt"; 

        private string CopybleFilePath => $"{PathToSaveDirectory}copy_2.txt"; 

        private string AddedContent => "\n=====\nThis text was added\n=====\n";
        
        public static void Main()
        { }

        [SetUp]
        public void TestFixtureSetup()
        {
            if (Directory.Exists(PathToSaveDirectory))
            {
                Directory.Delete(PathToSaveDirectory, true);
            }

            Directory.CreateDirectory(PathToSaveDirectory);
            this.CreatDataBase(this.PathToDataBase);
            File.Create(this.AppendFilePath).Close();
            File.AppendAllText(this.AppendFilePath, this.Content);
            File.Create(this.WriteFilePath).Close();
            File.AppendAllText(this.WriteFilePath, this.Content);
            File.Create(this.CopyFilePath).Close();
            File.Create(this.DeleteFilePath).Close();
            File.Create(this.MoveFilePath).Close();
            Directory.CreateDirectory(this.TargetDirectory);
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            Directory.Delete(PathToSaveDirectory, true);
        }

        [Test]
        public void AllFunctionallityPositiveTest_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand = 
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({this.DbRowId}, '{this.FirstName}', '{this.LastName}')";
            string firstSqlRollback = $"DELETE FROM {this.DbTableName} WHERE {this.DbFieldId} = {this.DbRowId}";
            sqliteTransactionFirst.AddSqliteCommand(
                firstSqlCommand,
                firstSqlRollback);

            var fileTransaction = new FileUnit();
            fileTransaction.CreateFile(this.CreateFilePath);
            fileTransaction.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            fileTransaction.AppendAllText(this.AppendFilePath, this.AddedContent);
            fileTransaction.WriteAllText(this.WriteFilePath, this.AddedContent);
            fileTransaction.Delete(this.DeleteFilePath);
            fileTransaction.Move(this.MoveFilePath, this.MovebleFilePath);

            var sqliteTransactionSecond = new SQLiteUnit(this.PathToDataBase);
            string secondSqlCommand = 
                $"UPDATE {this.DbTableName} set {this.DbFieldFirstName} = "
                + $"'{this.NewFirstName}' WHERE {this.DbFieldId} = {this.DbRowId}";
            string secondSqlRollback = 
                $"UPDATE {this.DbTableName} set {this.DbFieldFirstName} = "
                + $"'{this.LastName}' WHERE {this.DbFieldId} = {this.DbRowId}";
            sqliteTransactionSecond.AddSqliteCommand(
                secondSqlCommand,
                secondSqlRollback);

            // Act
            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
                bussinesTransaction.ExecuteUnit(fileTransaction);
                bussinesTransaction.ExecuteUnit(sqliteTransactionSecond);
                bussinesTransaction.Commit();
            }

            string firstNameInDb = string.Empty;
            string lastNameInDb = string.Empty;
            this.GetInfOfDataBase(out firstNameInDb, out lastNameInDb);

            // Assert
            Assert.IsTrue(File.Exists(this.CreateFilePath));
            Assert.IsTrue(File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content + this.AddedContent);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.AddedContent);
            Assert.IsFalse(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(File.Exists(this.MovebleFilePath) && !File.Exists(this.MoveFilePath));
            Assert.AreEqual(this.NewFirstName, firstNameInDb);
            Assert.AreEqual(this.LastName, lastNameInDb);
        }

        [Test]
        public void BadUnitAfterGoodUnits_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({this.DbRowId}, '{this.FirstName}', '{this.LastName}')";
            string firstSqlRollback = $"DELETE FROM {this.DbTableName} WHERE {this.DbFieldId} = {this.DbRowId}";
            sqliteTransactionFirst.AddSqliteCommand(
                firstSqlCommand,
                firstSqlRollback);

            var fileTransaction = new FileUnit();
            fileTransaction.CreateFile(this.CreateFilePath);
            fileTransaction.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            fileTransaction.AppendAllText(this.AppendFilePath, this.AddedContent);
            fileTransaction.WriteAllText(this.WriteFilePath, this.AddedContent);
            fileTransaction.Delete(this.DeleteFilePath);
            fileTransaction.Move(this.MoveFilePath, this.MovebleFilePath);
            
            var badFileUnit = new FileUnit();
            badFileUnit.Copy(this.DeleteFilePath, this.CopybleFilePath, true);
            
            // Act
            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
                bussinesTransaction.ExecuteUnit(fileTransaction);
                bussinesTransaction.ExecuteUnit(badFileUnit);
                bussinesTransaction.Commit();
            }

            string firstNameInDb = string.Empty;
            string lastNameInDb = string.Empty;
            this.GetInfOfDataBase(out firstNameInDb, out lastNameInDb);

            // Assert
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
            Assert.AreEqual(string.Empty, firstNameInDb);
            Assert.AreEqual(string.Empty, lastNameInDb);
        }

        [Test]
        public void BadUnitBetweenGoodUnits_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({this.DbRowId}, '{this.FirstName}', '{this.LastName}')";
            string firstSqlRollback = $"DELETE FROM {this.DbTableName} WHERE {this.DbFieldId} = {this.DbRowId}";
            sqliteTransactionFirst.AddSqliteCommand(
                firstSqlCommand,
                firstSqlRollback);

            var fileTransaction = new FileUnit();
            fileTransaction.CreateFile(this.CreateFilePath);
            fileTransaction.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            fileTransaction.AppendAllText(this.AppendFilePath, this.AddedContent);
            fileTransaction.WriteAllText(this.WriteFilePath, this.AddedContent);
            fileTransaction.Delete(this.DeleteFilePath);
            fileTransaction.Move(this.MoveFilePath, this.MovebleFilePath);

            var badFileUnit = new FileUnit();
            badFileUnit.Copy(this.CopybleFilePath, this.CopybleFilePath, true);

            // Act
            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
                bussinesTransaction.ExecuteUnit(badFileUnit);
                bussinesTransaction.ExecuteUnit(fileTransaction);
                bussinesTransaction.Commit();
            }

            string firstNameInDb = string.Empty;
            string lastNameInDb = string.Empty;
            this.GetInfOfDataBase(out firstNameInDb, out lastNameInDb);

            // Assert
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
            Assert.AreEqual(string.Empty, firstNameInDb);
            Assert.AreEqual(string.Empty, lastNameInDb);
        }
        
        [Test]
        public void ForgottenToMakeCommit_ReturnTrue()
        {
            // Arrange
            var sqliteTransactionFirst = new SQLiteUnit(this.PathToDataBase);
            string firstSqlCommand =
                $"INSERT INTO {this.DbTableName}({this.DbFieldId}, {this.DbFieldFirstName}, {this.DbFieldLastName}) "
                + $"VALUES ({DbRowId}, '{FirstName}', '{LastName}')";
            string firstSqlRollback = $"DELETE FROM {this.DbTableName} WHERE {DbFieldId} = {DbRowId}";
            sqliteTransactionFirst.AddSqliteCommand(
                firstSqlCommand,
                firstSqlRollback);

            var fileTransaction = new FileUnit();
            fileTransaction.CreateFile(this.CreateFilePath);
            fileTransaction.Copy(this.CopyFilePath, this.CopybleFilePath, true);
            fileTransaction.AppendAllText(this.AppendFilePath, this.AddedContent);
            fileTransaction.WriteAllText(this.WriteFilePath, this.AddedContent);
            fileTransaction.Delete(this.DeleteFilePath);
            fileTransaction.Move(this.MoveFilePath, this.MovebleFilePath);

            var sqliteTransactionSecond = new SQLiteUnit(this.PathToDataBase);
            string secondSqlCommand =
                $"UPDATE {this.DbTableName} set {this.DbFieldFirstName} = "
                + $"'{this.NewFirstName}' WHERE {this.DbFieldId} = {this.DbRowId}";
            string secondSqlRollback =
                $"UPDATE {this.DbTableName} set {this.DbFieldFirstName} = "
                + $"'{LastName}' WHERE {this.DbFieldId} = {this.DbRowId}";
            sqliteTransactionSecond.AddSqliteCommand(
                secondSqlCommand,
                secondSqlRollback);

            // Act
            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
                bussinesTransaction.ExecuteUnit(fileTransaction);
                bussinesTransaction.ExecuteUnit(sqliteTransactionSecond);
            }

            string firstNameInDb = string.Empty;
            string lastNameInDb = string.Empty;
            this.GetInfOfDataBase(out firstNameInDb, out lastNameInDb);

            // Assert
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
            Assert.AreEqual(string.Empty, firstNameInDb);
            Assert.AreEqual(string.Empty, lastNameInDb);
        }

        [Test]
        public void OnlyBadUnit_ReturnTrue()
        {
            // Arrange
            var badFileUnit = new FileUnit();
            badFileUnit.Copy(this.CopybleFilePath, this.CopybleFilePath, true);

            // Act
            using (var bussinesTransaction = this.unit.BeginTransaction())
            {
                bussinesTransaction.ExecuteUnit(badFileUnit);
                bussinesTransaction.Commit();
            }

            string firstNameInDb = string.Empty;
            string lastNameInDb = string.Empty;
            this.GetInfOfDataBase(out firstNameInDb, out lastNameInDb);

            // Assert
            Assert.IsFalse(File.Exists(this.CreateFilePath));
            Assert.IsTrue(!File.Exists(this.CopybleFilePath) && File.Exists(this.CopyFilePath));
            string textInAppendFile = File.ReadAllText(this.AppendFilePath);
            Assert.AreEqual(textInAppendFile, this.Content);
            string textInWriteFile = File.ReadAllText(this.WriteFilePath);
            Assert.AreEqual(textInWriteFile, this.Content);
            Assert.IsTrue(File.Exists(this.DeleteFilePath));
            Assert.IsTrue(!File.Exists(this.MovebleFilePath) && File.Exists(this.MoveFilePath));
            Assert.AreEqual(string.Empty, firstNameInDb);
            Assert.AreEqual(string.Empty, lastNameInDb);
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
            connection.Close();
            connection.Dispose();
        }

        private void GetInfOfDataBase(out string firstName, out string lastName)
        {
            firstName = string.Empty;
            lastName = string.Empty;

            string connectionString = SQLiteManager.GetConnectionString(this.PathToDataBase);
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(
                    $"SELECT * FROM person WHERE {DbFieldId} = {DbRowId}", 
                    connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            firstName = reader[$"{DbFieldFirstName}"].ToString();
                            lastName = reader[$"{DbFieldLastName}"].ToString();
                        }
                    }
                }

                connection.Close();
            }
        }
    }
}