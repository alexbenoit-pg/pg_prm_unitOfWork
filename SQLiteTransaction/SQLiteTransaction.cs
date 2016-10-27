using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Core.Interfaces;

namespace SQLiteTransaction
{
    public class SqLiteTransaction : ITransactionUnit
    {
        public SQLiteConnection _dbConnection = null;
        private System.Data.SQLite.SQLiteTransaction _dbTransaction;
        public SQLiteCommand _dbCommand = null;
        private readonly SqLiteJournal _sqLiteJournal = new SqLiteJournal();
        private readonly List<string> _rollbackCommands = new List<string>();
        private string _operationId;
        private string _databasePath;

        public SqLiteTransaction()
        {
            _operationId = GetOperationId();
        }

        public SqLiteTransaction(string databasePath)
        {
            ConnectDatabase(databasePath);
            _operationId = GetOperationId();
        }

        public void Dispose()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Close();
                _dbCommand.Dispose();
                _dbConnection.Dispose();
                _dbConnection = null;
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }
        
        public bool ConnectDatabase(string databasePath)
        {
            try
            {
                if (File.Exists(databasePath))
                {
                    _dbConnection = new SQLiteConnection(string.Format("Data Source={0}", databasePath));
                    _dbCommand = _dbConnection.CreateCommand();
                    _dbConnection.Open();
                    _dbTransaction = _dbConnection.BeginTransaction();
                    _dbCommand.Transaction = _dbTransaction;
                    _databasePath = databasePath;
                    return true;
                }
                else
                {
                    Console.WriteLine("no such file");
                    return false;
                }
            }
            catch (SQLiteException exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }
        }

        public bool AddSqliteCommand(string sqlCommand, string rollbackCommand)
        {
            if (_dbConnection != null)
            {
                this._rollbackCommands.Add(rollbackCommand);
                _dbCommand.CommandText = sqlCommand;
                _dbCommand.ExecuteNonQuery();
                return true;
            }
            Console.WriteLine("");
            return false;
        }

        private void SqLiteCommit()
        {
            try
            {
                if (_dbConnection != null)
                {
                    _dbTransaction.Commit();
                }
               
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
                _dbCommand.Dispose();

                _dbTransaction.Dispose();

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
                        _dbConnection = null;
                    }
                }
            }
        }

        public string GetOperationId()
        {
            if (_operationId == null)
            {
                _operationId = Guid.NewGuid().ToString();
            }
            return _operationId;
        }

        public void Rollback(string operationId)
        {
            SqLiteJournal journal = new SqLiteJournal();
            journal.GetParameters(operationId);
            using (_dbConnection = new SQLiteConnection(string.Format("Data Source={0}", journal.PathToDataBase)))
            {
                _dbConnection.Open();
                using (_dbTransaction = _dbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(_dbConnection) { Transaction = _dbTransaction })
                    {
                        foreach (string line in journal.RollBackCommands)
                        {
                            cmd.CommandText = line;
                            cmd.ExecuteNonQuery();
                        }
                        _dbTransaction.Commit();
                        File.Delete(journal.PathToDataBase);
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
            _sqLiteJournal.Write(_databasePath, _rollbackCommands, _operationId);
        }
    }
}
