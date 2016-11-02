using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Runtime.InteropServices;
using Core.Interfaces;

namespace Units
{
    public class SqLiteTransaction : ITransactionUnit
    {
        public SQLiteConnection DbConnection = null;
        private SQLiteTransaction _dbTransaction;
        public SQLiteCommand DbCommand = null;
        private readonly SqLiteJournal _sqLiteJournal = new SqLiteJournal();
        private readonly List<string> _rollbackCommands = new List<string>();
        private readonly List<string> _commitCommands = new List<string>();
        private string _operationId;
        private string _databasePath;

        public SqLiteTransaction()
        {
            _operationId = GetOperationId();
        }

        public SqLiteTransaction(string databasePath)
        {
            _databasePath = databasePath;
            _operationId = GetOperationId();
        }

        public void Dispose()
        {
            if (DbConnection != null)
            {
                DbConnection.Close();
                DbCommand.Dispose();
                DbConnection.Dispose();
                DbConnection = null;
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
                    this.Dispose();
                    DbConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", databasePath));
                    DbCommand = DbConnection.CreateCommand();
                    DbConnection.Open();
                    _dbTransaction = DbConnection.BeginTransaction();
                    DbCommand.Transaction = _dbTransaction;
                    _databasePath = databasePath;
                    return true;
                }
                else
                {
                    throw new Exception("No such database file.");
                }
            }
            catch (SQLiteException exception)
            {
                throw exception;
            }
        }

        public bool AddSqliteCommand(string sqlCommand, string rollbackCommand)
        {
            this._rollbackCommands.Add(rollbackCommand);
            this._commitCommands.Add(sqlCommand);
            return true;
        }

        private void SqLiteCommit()
        {
            try
            {
                if (DbConnection != null)
                {
                    _dbTransaction.Commit();
                }
            }
            catch (SQLiteException exception)
            {
                if (_dbTransaction != null)
                {
                    try
                    {
                        _dbTransaction.Rollback();
                        throw exception;
                    }
                    catch (SQLiteException secondException)
                    {
                        throw secondException;
                    }
                    finally
                    {
                        _dbTransaction.Dispose();
                    }
                }
            }
            finally
            {
                DbCommand.Dispose();

                _dbTransaction.Dispose();

                if (DbConnection != null)
                {
                    try
                    {
                        DbConnection.Close();
                    }
                    catch (SQLiteException exception)
                    {
                        throw exception;
                    }
                    finally
                    {
                        DbConnection.Dispose();
                        DbConnection = null;
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

        public void SetOperationId(string operationId)
        {
            _operationId = operationId;
        }

        public void Rollback(string operationId)
        {
            SqLiteJournal journal = new SqLiteJournal();
            journal.GetParameters(operationId);
            using (DbConnection = new SQLiteConnection(string.Format("Data Source={0}", journal.PathToDataBase)))
            {
                DbConnection.Open();
                using (_dbTransaction = DbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(DbConnection) { Transaction = _dbTransaction })
                    {
                        foreach (string line in journal.RollBackCommands)
                        {
                            cmd.CommandText = line;
                            cmd.ExecuteNonQueryAsync();
                        }
                        _dbTransaction.Commit();
                        File.Delete(journal.PathToDataJournal);
                    }
                }
            }
        }

        public void Rollback()
        {
            Rollback(GetOperationId());
        }

        public void Commit()
        {
            ConnectDatabase(_databasePath);
            foreach (var command in _commitCommands)
            {
                DbCommand.CommandText = command;
                try
                {
                    DbCommand.ExecuteNonQuery();
                }
                catch (Exception e) {
                    Dispose();
                    throw e;
                }
            }
            SqLiteCommit();
            _sqLiteJournal.Write(_databasePath, _rollbackCommands, _operationId);
        }
    }
}
