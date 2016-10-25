namespace Units
{
    using System;
    using System.IO;
    using System.Text;
    using Core.Interfaces;

    public class TransactionBadCommitUnit : ITransactionUnit
    {
        public string path = @"C:\Users\vuyan\Desktop\TestFile.txt";

        public TransactionBadCommitUnit()
        {
            using (FileStream fs = File.Create(path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");

                fs.Write(info, 0, info.Length);
            }
        }

        public void Commit()
        {
            File.AppendAllText(path, "\nCommit провален");
            throw new Exception();
        }

        public void Dispose()
        {
            File.AppendAllText(path, "\nDispose прошел успешно");
        }

        public string GetOperationId()
        {
            return Guid.NewGuid().ToString();
        }

        public void Rollback()
        {
            File.AppendAllText(path, "\nRollback прошел успешно");
        }

        public void Rollback(string operationId)
        {
            File.AppendAllText(path, $"\nRollback прошел успешно! Operation Id:{operationId}");
        }

        public void SetOperationId(string operationId)
        {
            throw new NotImplementedException();
        }
    }
}
