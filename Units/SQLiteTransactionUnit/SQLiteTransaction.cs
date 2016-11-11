namespace Units
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using Core.Interfaces;

    public class SqLiteTransaction : ITransactionUnit
    {
        public SQLiteConnection DbConnection = null;
        public SQLiteCommand DbCommand = null;
        private SQLiteTransaction dbTransaction;
        private readonly SqLiteJournal sqLiteJournal = new SqLiteJournal();
        private readonly List<string> rollbackCommands = new List<string>();
        private readonly List<string> commitCommands = new List<string>();
        private string operationId;
        private string databasePath;

        public SqLiteTransaction()
        {
            this.operationId = this.GetOperationId();
        }

        public SqLiteTransaction(string pathdatabase)
        {
            this.databasePath = pathdatabase;
            this.operationId = this.GetOperationId();
        }

        public void Dispose()
        {
            if (this.DbConnection != null)
            {
                this.DbConnection.Close();
                this.DbCommand.Dispose();
                this.DbConnection.Dispose();
                this.DbConnection = null;
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public bool ConnectDatabase(string pathdatabase)
        {
            try
            {
                if (File.Exists(pathdatabase))
                {
                    this.Dispose();
                    this.DbConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", pathdatabase));
                    this.DbCommand = this.DbConnection.CreateCommand();
                    this.DbConnection.Open();
                    this.dbTransaction = this.DbConnection.BeginTransaction();
                    this.DbCommand.Transaction = this.dbTransaction;
                    this.databasePath = pathdatabase;
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
            this.rollbackCommands.Add(rollbackCommand);
            this.commitCommands.Add(sqlCommand);
            return true;
        }

        public string GetOperationId()
        {
            if (this.operationId == null)
            {
                this.operationId = Guid.NewGuid().ToString();
            }

            return this.operationId;
        }

        public void SetOperationId(string setoperationId)
        {
            this.operationId = setoperationId;
        }

        public void Rollback(string operationId)
        {
            SqLiteJournal journal = new SqLiteJournal();
            journal.GetParameters(operationId);
            using (this.DbConnection = new SQLiteConnection(string.Format("Data Source={0}", journal.PathToDataBase)))
            {
                this.DbConnection.Open();
                using (this.dbTransaction = this.DbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(this.DbConnection) { Transaction = this.dbTransaction })
                    {
                        foreach (string line in journal.RollBackCommands)
                        {
                            cmd.CommandText = line;
                            cmd.ExecuteNonQueryAsync();
                        }

                        this.dbTransaction.Commit();
                        File.Delete(journal.PathToDataJournal);
                    }
                }
            }
        }

        public void Rollback()
        {
            Rollback(this.GetOperationId());
        }

        public void Commit()
        {
            ConnectDatabase(this.databasePath);
            foreach (var command in this.commitCommands)
            {
                this.DbCommand.CommandText = command;
                try
                {
                    this.DbCommand.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    this.Dispose();
                    throw exception;
                }
            }

            this.SqLiteCommit();
            this.sqLiteJournal.Write(this.databasePath, this.rollbackCommands, this.operationId);
        }

        private void SqLiteCommit()
        {
            try
            {
                if (this.DbConnection != null)
                {
                    this.dbTransaction.Commit();
                }
            }
            catch (SQLiteException exception)
            {
                if (this.dbTransaction != null)
                {
                    try
                    {
                        this.dbTransaction.Rollback();
                        throw exception;
                    }
                    catch (SQLiteException secondException)
                    {
                        throw secondException;
                    }
                    finally
                    {
                        this.dbTransaction.Dispose();
                    }
                }
            }
            finally
            {
                this.DbCommand.Dispose();

                this.dbTransaction.Dispose();

                if (this.DbConnection != null)
                {
                    try
                    {
                        this.DbConnection.Close();
                    }
                    catch (SQLiteException exception)
                    {
                        throw exception;
                    }
                    finally
                    {
                        this.DbConnection.Dispose();
                        this.DbConnection = null;
                    }
                }
            }
        }
    }
}
