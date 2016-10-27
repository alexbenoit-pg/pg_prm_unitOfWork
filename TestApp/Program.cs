namespace TestApp
{
    using Core;
    using Units;

    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\test";

            UnitOfWork FileManager = new UnitOfWork();
            FileTransactionUnit fileTransaction = new FileTransactionUnit();
            fileTransaction.Delete(@"C:\Users\vuyan\Desktop\TestUNIT\1.txt");
            fileTransaction.Move(@"C:\Users\vuyan\Desktop\TestUNIT\2.txt", @"C:\Users\vuyan\Desktop\TestUNIT\999.txt");




            FileTransactionUnit Katarsis = new FileTransactionUnit();
            Katarsis.Delete(@"C:\Users\vuyan\Desktop\TestUNIT\5.txt");
            //     FileTrax.Copy(@"C:\Users\vuyan\Desktop\TestUNIT\6.txt", "Sada\\", true);


            FileTransactionUnit Frustration = new FileTransactionUnit();
            Frustration.Copy(@"C:\Users\vuyan\Desktop\TestUNIT\512321312.txt", @"C:\Users\vuyan\Desktop\TestUNIT\CopyFile.txt", true);




            using (var fileTrn = FileManager.BeginTransaction())
            {
                fileTrn.RegisterOperation(fileTransaction);
                fileTrn.RegisterOperation(Katarsis);
                fileTrn.RegisterOperation(Frustration);
                fileTrn.Commit();
            }
        }
    }
}
