namespace TestApp
{
    using Core;
    using Core.Tests.Fakes;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            var unit = new UnitOfWork();
            var oper = new MockTransactionUnit();
            oper.ID = "1";
            var oper2 = new MockTransactionUnit();
            oper2.ID = "2";
            var oper3 = new MockBadTransactionUnit();
            oper3.ID = "3";
            var a = Path.GetTempPath();

            using (var bo = unit.BeginTransaction()) {
                bo.RegisterOperation(oper);
                bo.RegisterOperation(oper2);
                bo.RegisterOperation(oper3);
                bo.Commit();
            }
        }
    }
}
