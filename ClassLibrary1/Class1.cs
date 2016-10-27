using System;
using System.Data.SQLite;
using Core.Interfaces;

namespace SQLiteTransaction
{
    public class SqLiteTransaction : ITransactionUnit
    {
        public SqLiteTransaction(string databasePath)
        {
            ConnectDatabase(databasePath);
            _databasePath = databasePath;
            _operationId = GetOperationId();
        }

        public void Dispose()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }

        private SQLiteConnection _dbConnection = null;
        private System.Data.SQLite.SQLiteTransaction _dbTransaction = null;
        private SQLiteCommand _dbCommand = null;
        private SqLiteJournal sqLiteJournal = new SqLiteJournal();
        private string _rollbackCommand = "";
        private string _operationId;
        private string _databasePath;
        //private SqLiteJournal sqLiteJournal = new SqLiteJournal(Operationid);

        public bool ConnectDatabase(string databasePath)
        {
            try
            {
                _dbConnection = new SQLiteConnection(string.Format("Data Source={0}", databasePath));
                _dbCommand = _dbConnection.CreateCommand();
                _dbTransaction = _dbConnection.BeginTransaction();
                _dbCommand.Transaction = _dbTransaction;
                return true;

            }
            catch (SQLiteException exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }
        }


        public bool AddSqliteCommand(string sqlCommand, string rollbackCommand)
        {

            if (_dbConnection == null)
            {
                this._rollbackCommand += rollbackCommand;
                _dbCommand.CommandText = sqlCommand;
                _dbCommand.ExecuteNonQuery();
                return true;
            }
            return false;
        }

        private void SqLiteCommit()
        {
            try
            {
                _dbTransaction.Commit();
            }
            catch (SQLiteException exception)
            {
                Console.WriteLine("Error: {0}", exception.Message);
                if (_dbTransaction != null)
                {
                    try
                    {
                        _dbTransaction.Rollback();
                    }
                    catch (SQLiteException secondException)
                    {
                        Console.WriteLine("Transaction rollback failed.");
                        Console.WriteLine("Error: {0}", secondException.Message);
                    }
                    finally
                    {
                        _dbTransaction.Dispose();
                    }
                }
            }
            finally
            {
                _dbCommand?.Dispose();

                _dbTransaction?.Dispose();

                if (_dbConnection != null)
                {
                    try
                    {
                        _dbConnection.Close();

                    }
                    catch (SQLiteException exception)
                    {

                        Console.WriteLine("Closing connection failed.");
                        Console.WriteLine("Error: {0}", exception.ToString());

                    }
                    finally
                    {
                        _dbConnection.Dispose();
                    }
                }
            }
        }

        public string GetOperationId()
        {
            return Guid.NewGuid().ToString();
        }

        public void Rollback(string operationId)
        {
            SqLiteJournal journal = new SqLiteJournal();
            journal.GetParameters(operationId);
            using (_dbConnection = new SQLiteConnection(string.Format("Data Source={0}", journal.PathToDataBase)))
            {
                using (_dbTransaction = _dbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(_dbConnection) { Transaction = _dbTransaction })
                    {
                        //foreach (string line in new LineReader(() => new StringReader(journal.RollBackCommand)))
                        //
                        cmd.CommandText = journal.RollBackCommand;
                        cmd.ExecuteNonQuery();
                        //}

                        _dbTransaction.Commit();
                    }

                }
            }
        }

        public void Rollback()
        {
            _dbTransaction.Rollback();
        }

        public void Commit()
        {
            SqLiteCommit();
            sqLiteJournal.Write(_databasePath, _rollbackCommand, _operationId);
        }
    }
}
