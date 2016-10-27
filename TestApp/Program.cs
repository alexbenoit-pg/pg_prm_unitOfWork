namespace TestApp
{
    using Core;
    using Core.Tests.Fakes;
    using System.IO;
    using ChinhDo.Transactions;
    using System.Transactions;
    using System;

    class Program
    {
        //static void Main(string[] args)
        //{
        //    //var unit = new UnitOfWork();

        //    //var oper = new MockTransactionUnit();
        //    //oper.ID = "1";
        //    //var oper2 = new MockTransactionUnit();
        //    //oper2.ID = "2";
        //    //var oper3 = new MockBadTransactionUnit();
        //    //oper3.ID = "3";
        //    //var a = Path.GetTempPath();

        //    //using (var bo = unit.BeginTransaction()) {
        //    //    bo.RegisterOperation(oper);
        //    //    bo.RegisterOperation(oper2);
        //    //    //bo.RegisterOperation(oper3);
        //    //    bo.Commit();


        //    }
        //}

        private int _numTempFiles;
        private IFileManager _target;

        public void DeleteFile()
        {
            string path = @"C:\test";

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    _target.CreateFile(path,"test","txt");
                    _target.CreateFile(path, "1", "h");
                    _target.CreateFile(path, "2", "c");
                    _target.Copy(path + "\\3.h", path + "\\4.h", true);

                    scope.Complete();
                }
                catch (Exception e)
                {
                    scope.Dispose();
                }
            }
        }

        public void TestInitialize()
        {
            _target = new TxFileManager();
            _numTempFiles = Directory.GetFiles(Path.Combine(Path.GetTempPath(), "UnitOfWorkFileTransaction")).Length;
        }


        static void Main(string[] args)
        {
            Program k = new Program();
            k.TestInitialize();
            //k.CanCopyAndRollback();
            k.DeleteFile();

        }
    }
}
