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
            fileTransaction.Delete(@"C:\Users\vuyan\Desktop\TestUNIT\2.txt");


            using (var fileTrn = FileManager.BeginTransaction())
            {
                fileTrn.RegisterOperation(fileTransaction);
                fileTrn.Commit();
            }

        }
    }
}
