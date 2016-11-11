namespace TestApp
{
    using Units;
    using Core;

    class Program
    {
        static public string Root = @"C:\TestWoU\";
        static void Main(string[] args)
        {
            Core.UnitOfWork unit = new Core.UnitOfWork();

            //#region File_unit_create
            //var fu1 = new FileTransactionUnit();
            //fu1.CreateFile(Root + "1\\1.txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu1);
            //    bo.Commit();
            //}
            //#endregion

            //#region File_unit_move
            //var fu3 = new FileTransactionUnit();
            //fu3.Move(Root + "\\3\\1.txt", Root + "\\3\\target\\1.txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu3);
            //    bo.Commit();
            //}
            //#endregion


            //#region File_unit_delete
            //var fu4 = new FileTransactionUnit();
            //fu4.Delete(Root + "\\4\\1.txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu4);
            //    bo.Commit();
            //}
            //#endregion


            //#region File_unit_copy
            //var fu5 = new FileTransactionUnit();
            //fu5.Copy(Root + "\\5\\1.txt", Root + "\\5\\2.txt", true);

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu5);
            //    bo.Commit();
            //}
            //#endregion

            //#region File_unit_appendtext
            //var fu6 = new FileTransactionUnit();
            //fu6.AppendAllText(Root + "\\6\\1.txt", "azaaza");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu6);
            //    bo.Commit();
            //}
            //#endregion

            //#region File_unit_writetext
            //var fu7 = new FileTransactionUnit();
            //fu7.WriteAllText(Root + "\\7\\1.txt", "azaaza");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu7);
            //    bo.Commit();
            //}
            //#endregion


            #region File_unit_Full
            var fu8 = new FileTransactionUnit();
            fu8.WriteAllText(Root + "8\\1.txt", "azaaza");
            fu8.AppendAllText(Root + "8\\2.txt", "azaaza");
            fu8.Copy(Root + "8\\2.txt", Root + "8\\2_myCopy.txt", false);
            fu8.Delete(Root + "8\\3.txt");
            fu8.Move(Root + "8\\4.txt", Root + "8\\target\\4_move.txt");
            fu8.CreateFile(Root + "8\\CREATED.txt");

            var fu8_1 = new FileTransactionUnit();
            fu8_1.Copy(Root + "\\8\\nine.txt", Root + "\\8\\12312312312_myCopy.txt", true);

            using (var bo = unit.BeginTransaction())
            {
                bo.RegisterOperation(fu8);
                bo.RegisterOperation(fu8_1);
                bo.Commit();
            }
            #endregion


            //#region File_unit_negative
            //var fu9 = new FileTransactionUnit();
            //fu9.WriteAllText(Root + "\\9\\1.txt", "azaaza");
            //fu9.AppendAllText(Root + "\\9\\2.txt", "azaaza");
            //fu9.Copy(Root + "\\9\\2.txt", Root + "\\9\\2_myCopy.txt", true);
            //fu9.Move(Root + "\\9\\4.txt", Root + "\\9\\target\\4_move.txt");
            //fu9.Delete(Root + "\\9\\3.txt");
            //fu9.CreateFile(Root + "9\\CREATED.txt");
            //fu9.Copy(Root + "\\9\\nine.txt", Root + "\\9\\12312312312_myCopy.txt", true);

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu9);
            //    bo.Commit();
            //}
            //#endregion


            //#region SQL_unit_write
            //var su10 = new SqLiteTransaction(Root + "\\testdb.db");
            //su10.AddSqliteCommand(
            //    "INSERT INTO test(name) VALUES ('Fedor');",
            //    "DELETE FROM test WHERE ID = 1;");
            //var su11 = new SqLiteTransaction(Root + "\\testdb.db");
            //su11.AddSqliteCommand(
            //    "INSERT INTO test(name) VALUES ('Dima');",
            //    "DELETE FROM test WHERE ID = 2;");
            //var su12 = new SqLiteTransaction(Root + "\\testdb.db");
            //su12.AddSqliteCommand(
            //    "INSERT INTO test(name) VALUES ('Igor');",
            //    "DELETE FROM test WHERE ID = 3;");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(su10);
            //    bo.RegisterOperation(su11);
            //    bo.RegisterOperation(su12);
            //    bo.Commit();
            //}
            //#endregion


            //#region SQL_unit_set
            //var su20 = new SqLiteTransaction(Root + "\\testdb.db");
            //su20.AddSqliteCommand(
            //    "UPDATE test2 set city = 'Spb' WHERE id = 1",
            //    "UPDATE test2 set city = 'Moscow' WHERE id = 1");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(su20);
            //    bo.Commit();
            //}
            //#endregion


            //#region SQL_unit_negative
            //var su30 = new SqLiteTransaction(Root + "\\testdb.db");
            //su30.AddSqliteCommand(
            //    "INSERT INTO test3(id, name) VALUES (3, 'Fedor');",
            //    "DELETE FROM test3 WHERE id = 3;");
            //var su31 = new SqLiteTransaction(Root + "\\testdb.db");
            //su31.AddSqliteCommand(
            //    "UPDATE test3 set name = 'Smith' WHERE id = 2",
            //    "UPDATE test3 set name = 'Phill' WHERE id = 2");
            //var su32 = new SqLiteTransaction(Root + "\\testdb.db");
            //su32.AddSqliteCommand(
            //    "INSERT INTO test3(id, name, city) VALUES (4, 'James', 'Moscow');",
            //    "DELETE FROM test3 WHERE ID = 4;");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(su30);
            //    bo.RegisterOperation(su31);
            //    bo.RegisterOperation(su32);
            //    bo.Commit();
            //}
            //#endregion


            //#region FULL_positive
            //var su40 = new SqLiteTransaction(Root + "\\testdb.db");
            //su40.AddSqliteCommand(
            //    "INSERT INTO test4(id, name) VALUES (2, 'Grag');",
            //    "DELETE FROM test WHERE ID = 2;");
            //var su41 = new SqLiteTransaction(Root + "\\testdb.db");
            //su41.AddSqliteCommand(
            //    "UPDATE test4 set name = 'pit' WHERE id = 1",
            //    "UPDATE test4 set name = 'max' WHERE id = 1");

            //var fu10 = new FileTransactionUnit();
            //fu10.WriteAllText(Root + "\\10\\1.txt", "azaaza");
            //fu10.AppendAllText(Root + "\\10\\2.txt", "azaaza");
            //fu10.Copy(Root + "\\10\\2.txt", Root + "\\10\\2_myCopy.txt", true);
            //fu10.Delete(Root + "\\10\\3.txt");
            //fu10.Move(Root + "\\10\\4.txt", Root + "\\10\\target\\4_move.txt");
            //fu10.CreateFile(Root + "\\10", "CREATED", "txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(su40);
            //    bo.RegisterOperation(su41);
            //    bo.RegisterOperation(fu10);
            //    bo.Commit();
            //}
            //#endregion


            //#region FULL_negative
            //var su50 = new SqLiteTransaction(Root + "\\testdb.db");
            //su50.AddSqliteCommand(
            //    "INSERT INTO test5(id, name) VALUES (3, 'Jake');",
            //    "DELETE FROM test5 WHERE ID = 3;");
            //var su51 = new SqLiteTransaction(Root + "\\testdb.db");
            //su51.AddSqliteCommand(
            //    "UPDATE test5 set name = 'Andre' WHERE id = 1",
            //    "UPDATE test5 set name = 'Jose' WHERE id = 1");

            //var fu11 = new FileTransactionUnit();
            //fu11.WriteAllText(Root + "\\11\\1.txt", "azaaza");
            //fu11.AppendAllText(Root + "\\11\\2.txt", "azaaza");
            //fu11.Copy(Root + "\\11\\2.txt", Root + "\\11\\2_myCopy.txt", true);
            //fu11.Delete(Root + "\\11\\3.txt");
            //fu11.Move(Root + "\\11\\4.txt", Root + "\\11\\target\\4_move.txt");
            //fu11.CreateFile(Root + "\\11", "CREATED", "txt");
            //fu11.Copy(Root + "\\11\\unknown_file.txt", Root + "\\11\\12312312312_myCopy.txt", true);

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(su50);
            //    bo.RegisterOperation(su51);
            //    bo.RegisterOperation(fu11);
            //    bo.Commit();
            //}
            //#endregion

            #region File_unit_Full
            var fu110 = new FileTransactionUnit();
            fu110.WriteAllText(Root + "12\\1.txt", "azaaza");
            var fu111 = new FileTransactionUnit();
            fu111.AppendAllText(Root + "12\\2.txt", "azaaza");
            var fu112 = new FileTransactionUnit();
            fu112.Copy(Root + "12\\2.txt", Root + "12\\2_myCopy.txt", false);
            var fu113 = new FileTransactionUnit();
            fu113.Delete(Root + "12\\3.txt");
            fu113.Move(Root + "12\\4.txt", Root + "12\\target\\4_move.txt");
            fu113.CreateFile(Root + "12\\CREATED.txt");

            using (var bo = unit.BeginTransaction())
            {
                bo.RegisterOperation(fu110);
                bo.RegisterOperation(fu111);
                bo.RegisterOperation(fu112);
                bo.RegisterOperation(fu113);
                bo.Commit();
            }
            #endregion
        }
    }
}