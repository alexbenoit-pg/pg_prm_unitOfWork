using System.Collections.Generic;

namespace TestApp
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using Core;
    using NUnit.Framework;
    using Units;

    public class TestAppIntegrationTest
    {

        private static void Main()
        {

            string pathToSaveDirectory = Path.GetTempPath() + @"FileAndSQLiteTransaction\";
            string pathToDataBase = pathToSaveDirectory + "test.db";

            //Directory.CreateDirectory(pathToSaveDirectory);
            //pre(pathToSaveDirectory, pathToDataBase);

            UnitOfWork unit = new UnitOfWork(new UnitJsonJournal());

            //var sqliteTransactionFirst = new SQLiteUnit(pathToDataBase);
            //sqliteTransactionFirst.AddSqliteCommand(
            //    "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
            //    "DELETE FROM person WHERE first_name = 'Commit1'");

            //var sqliteTransactionSecond = new SQLiteUnit(pathToDataBase);
            //sqliteTransactionSecond.AddSqliteCommand(
            //    "UPDATE person set first_name = 'pit' WHERE id = 2",
            //    "UPDATE person set first_name = 'max' WHERE id = 2");

            var fileTransaction = new FileUnit();
            fileTransaction.CreateFile(pathToSaveDirectory + "CreateFileTest.txt");
            fileTransaction.Copy(pathToSaveDirectory + "copy.txt", pathToSaveDirectory + "copy_2.txt", true);
            fileTransaction.AppendAllText(pathToSaveDirectory + "append.txt", "\nAAAAAAAAA");
            fileTransaction.WriteAllText(pathToSaveDirectory + "write.txt", "AAAAAAAAA");
            fileTransaction.Delete(pathToSaveDirectory + "delete.txt");
            fileTransaction.Move(pathToSaveDirectory + "move.txt", pathToSaveDirectory + "Target\\moveble.txt");


            using (var bussinesTransaction = unit.BeginTransaction())
            {
                //bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
                //bussinesTransaction.ExecuteUnit(sqliteTransactionSecond);
                bussinesTransaction.ExecuteUnit(fileTransaction);
                bussinesTransaction.Commit();
            }

            Directory.Delete(pathToSaveDirectory, true);
        }

        private static void pre(string pathToSaveDirectory, string pathToDataBase) {
            if (File.Exists(pathToDataBase))
            {
                File.Delete(pathToDataBase);
            }
            CreatDataBase(pathToDataBase);

            string appendFile = pathToSaveDirectory + "append.txt";
            if (File.Exists(appendFile))
            {
                File.Delete(appendFile);
            }
            File.Create(appendFile).Close();
            File.AppendAllText(appendFile,"asd");
            
            string writeFile = pathToSaveDirectory + "write.txt";
            if (File.Exists(writeFile))
            {
                File.Delete(writeFile);
            }
            File.Create(writeFile).Close();
            File.AppendAllText(writeFile, "asd");

            string copyFile = pathToSaveDirectory + "copy.txt";
            if (File.Exists(copyFile))
            {
                File.Delete(copyFile);
            }
            File.Create(copyFile).Close();
            
            string deleteFile = pathToSaveDirectory + "delete.txt";
            if (File.Exists(deleteFile))
            {
                File.Delete(deleteFile);
            }
            File.Create(deleteFile).Close();

            string removeFile = pathToSaveDirectory + "move.txt";
            if (File.Exists(removeFile))
            {
                File.Delete(removeFile);
            }
            File.Create(removeFile).Close();
            Directory.CreateDirectory(pathToSaveDirectory+"Target");
        }

        private static void CreatDataBase(string pathDataBase)
        {
            string sqliteConnectionString = SQLiteUnit.GetConnectionString(pathDataBase);
            SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString);

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
    }

    //[TestFixture]
    //public class TestAppIntegrationTest
    //{
    //    private string pathToSaveDirectory = Path.GetTempPath() + @"FileAndSQLiteTransaction\";
    //    private string pathToDataBase;
    //    private UnitOfWork unit = new UnitOfWork();

    //    [OneTimeSetUp]
    //    public void TestFixtureSetup()
    //    {
    //        Directory.CreateDirectory(this.pathToSaveDirectory);
    //        this.pathToDataBase = this.pathToSaveDirectory + "test.db";
    //        this.CreatDataBase(this.pathToDataBase);
    //    }

    //    [OneTimeTearDown]
    //    public void TestFixtureTearDown()
    //    {
    //        GC.Collect();
    //        GC.SuppressFinalize(this);
    //        Directory.Delete(this.pathToSaveDirectory, true);
    //    }

    //    [Test]
    //    public void WorkFileAndSqliteTransactionUNits_PositiveWork_ReturnTrue()
    //    {
    //        var sqliteTransactionFirst = new SQLiteUnit(this.pathToDataBase);
    //        sqliteTransactionFirst.AddSqliteCommand(
    //            "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
    //            "DELETE FROM person WHERE first_name = 'Commit1'");

    //        var fileTransaction = new FileUnit();
    //        fileTransaction.CreateFile(this.pathToSaveDirectory + "CreateFileTest.txt");

    //        var sqliteTransactionSecond = new SQLiteUnit(this.pathToDataBase);
    //        sqliteTransactionSecond.AddSqliteCommand(
    //            "UPDATE person set first_name = 'pit' WHERE id = 2",
    //            "UPDATE person set first_name = 'max' WHERE id = 2");

    //        using (var bussinesTransaction = this.unit.BeginTransaction())
    //        {
    //            bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
    //            bussinesTransaction.ExecuteUnit(fileTransaction);
    //            bussinesTransaction.ExecuteUnit(sqliteTransactionSecond);
    //            bussinesTransaction.Commit();
    //        }

    //        string firstname = string.Empty;
    //        string lastname = string.Empty;
    //        GetInfoofDataBase(out firstname, out lastname);

    //        Assert.IsTrue(File.Exists(this.pathToSaveDirectory + "CreateFileTest.txt"));
    //        Assert.AreEqual("pit", firstname);
    //        Assert.AreEqual("Check1", lastname);
    //    }

    //    [Test]
    //    public void WorkFileAndSqliteTransactionUNits_NegativWork_ReturnTrue()
    //    {
    //        var sqliteTransactionFirst = new SQLiteUnit(this.pathToDataBase);
    //        sqliteTransactionFirst.AddSqliteCommand(
    //            "INSERT INTO person(id, first_name, last_name) VALUES (2, 'Commit1', 'Check1');",
    //            "DELETE FROM person WHERE first_name = 'Commit1'");

    //        var fileTransaction = new FileUnit();
    //        fileTransaction.CreateFile(this.pathToSaveDirectory + "CreateFileTest.txt");

    //        var sqliteTransactionSecond = new SQLiteUnit(this.pathToDataBase);
    //        sqliteTransactionSecond.AddSqliteCommand("UPDATE somethink set first_name = 'pit' WHERE id = 2", string.Empty);

    //        using (var bussinesTransaction = this.unit.BeginTransaction())
    //        {
    //            bussinesTransaction.ExecuteUnit(sqliteTransactionFirst);
    //            bussinesTransaction.ExecuteUnit(fileTransaction);
    //            bussinesTransaction.ExecuteUnit(sqliteTransactionSecond);
    //            bussinesTransaction.Commit();
    //        }

    //        string firstname = string.Empty;
    //        string lastname = string.Empty;
    //        GetInfoofDataBase(out firstname, out lastname);

    //        Assert.IsFalse(File.Exists(this.pathToSaveDirectory + "CreateFileTest.txt"));
    //        Assert.AreNotEqual("pit", firstname);
    //        Assert.AreNotEqual("Check1", lastname);
    //    }

    //    private void CreatDataBase(string pathDataBase)
    //    {
    //        string sqliteConnectionString = SQLiteUnit.GetConnectionString(pathDataBase);
    //        SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString);

    //        SQLiteCommand command = new SQLiteCommand("CREATE TABLE person("
    //                                                    + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
    //                                                    + "first_name TEXT, "
    //                                                    + "last_name TEXT);",
    //                                                    connection);
    //        connection.Open();
    //        command.ExecuteNonQuery();
    //        connection.Close();
    //        connection.Dispose();
    //    }

    //    private void GetInfoofDataBase(out string firstname , out string lastname)
    //    {
    //        firstname = string.Empty;
    //        lastname = string.Empty;

    //        string sqliteConnectionString = SQLiteUnit.GetConnectionString(this.pathToDataBase);
    //        using (SQLiteConnection connection = new SQLiteConnection(sqliteConnectionString))
    //        {
    //            connection.Open();
    //            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM person  WHERE first_name = 'pit'", connection))
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
    //    }

    //    private static void Main()
    //    {
    //    }
    //}


}