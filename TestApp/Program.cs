namespace TestApp
{
    using Core;
    using Units;
    using Core.Tests.Fakes;
    using System.IO;

    class Program
    {
        static public string Root = @"C:\TestWoU\";
        static void Main(string[] args)
        {

            var unit = new UnitOfWork();

            //#region File_unit_create
            //var fu1 = new FileTransactionUnit();
            //fu1.CreateFile(Root+"\\1","1","txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu1);
            //    bo.Commit();
            //}
            //#endregion


            //#region File_unit_rename
            //var fu2 = new FileTransactionUnit();
            //fu2.Rename(Root + "\\2\\1.txt", "2.txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu2);
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


            //#region File_unit_writetext
            //var fu8 = new FileTransactionUnit();
            //fu8.WriteAllText(Root + "\\8\\1.txt", "azaaza");
            //fu8.AppendAllText(Root + "\\8\\2.txt", "azaaza");
            //fu8.Copy(Root + "\\8\\2.txt", Root + "\\8\\2_myCopy.txt", true);
            //fu8.Delete(Root + "\\8\\3.txt");
            //fu8.Move(Root + "\\8\\4.txt", Root + "\\8\\target\\4_move.txt");
            //fu8.Rename(Root + "\\8\\5.txt", "RENAMED.txt");
            //fu8.CreateFile(Root + "\\8", "CREATED", "txt");

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu8);
            //    bo.Commit();
            //}
            //#endregion

            //#region File_unit_writetext
            //var fu9 = new FileTransactionUnit();
            //fu9.WriteAllText(Root + "\\9\\1.txt", "azaaza");
            //fu9.AppendAllText(Root + "\\9\\2.txt", "azaaza");
            //fu9.Copy(Root + "\\9\\2.txt", Root + "\\9\\2_myCopy.txt", true);
            //fu9.Move(Root + "\\9\\4.txt", Root + "\\9\\target\\4_move.txt");
            //fu9.Delete(Root + "\\9\\3.txt");
            //fu9.Rename(Root + "\\9\\5.txt", "RENAMED.txt");
            //fu9.CreateFile(Root + "\\9", "CREATED", "txt");
            //fu9.Copy(Root + "\\9\\nine.txt", Root + "\\9\\12312312312_myCopy.txt", true);

            //using (var bo = unit.BeginTransaction())
            //{
            //    bo.RegisterOperation(fu9);
            //    bo.Commit();
            //}
            //#endregion

            #region SQL_unit_writetext
            var su1 = new SqLiteTransaction(Root + "\\testdb.db");
            su1.AddSqliteCommand(
                "INSERT INTO test(name) VALUES ('Fedor'));",
                "");

            using (var bo = unit.BeginTransaction())
            {
                bo.RegisterOperation(su1);
                bo.Commit();
            }
            #endregion

        }
    }
}