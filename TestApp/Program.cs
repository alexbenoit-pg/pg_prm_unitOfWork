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


            FileTransactionUnit Frustration = new FileTransactionUnit();
            Frustration.Copy(@"C:\Users\vuyan\Desktop\TestUNIT\999.txt", @"C:\Users\vuyan\Desktop\TestUNIT\123456.txt", true);
            
            //FAILOp
            FileTransactionUnit Sublimation = new FileTransactionUnit();
            Sublimation.Copy("dasdasdas", "dsadas", true);
            
            using (var fileTrn = FileManager.BeginTransaction())
            {
                fileTrn.RegisterOperation(fileTransaction);
                fileTrn.RegisterOperation(Katarsis);
                fileTrn.RegisterOperation(Frustration);
              //  fileTrn.RegisterOperation(Sublimation);
                fileTrn.Commit();
            }
        }
    }
}
