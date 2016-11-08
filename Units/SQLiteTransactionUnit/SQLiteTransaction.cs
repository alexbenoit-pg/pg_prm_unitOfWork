namespace Units
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Runtime.Serialization;
    using Core.Interfaces;

    [DataContract]
    public class SqLiteTransaction : ITransactionUnit
    {
        public SQLiteConnection DbConnection = null;
        private SQLiteTransaction _dbTransaction;
        public SQLiteCommand DbCommand = null;
        private readonly List<string> _commitCommands = new List<string>();

        [DataMember]
        private string _databasePath;
        [DataMember]
        private readonly List<string> _rollbackCommands = new List<string>();

        public SqLiteTransaction()
        {
        }

        public SqLiteTransaction(string databasePath)
        {
            _databasePath = databasePath;
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
        
        public void Rollback()
        {
            using (DbConnection = new SQLiteConnection(string.Format("Data Source={0}", this._databasePath)))
            {
                DbConnection.Open();
                using (_dbTransaction = DbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(DbConnection) { Transaction = _dbTransaction })
                    {
                        foreach (string line in this._rollbackCommands)
                        {
                            cmd.CommandText = line;
                            cmd.ExecuteNonQueryAsync();
                        }
                        _dbTransaction.Commit();
                    }
                }
            }
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
                catch (Exception exception) {
                    Dispose();
                    throw exception;
                }
            }
            SqLiteCommit();
        }
    }
}
