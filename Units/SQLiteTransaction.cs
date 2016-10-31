// -----------------------------------------------------------------------
// <copyright file="SqLiteTransaction.cs" company="Paragon Software Group">
// EXCEPT WHERE OTHERWISE STATED, THE INFORMATION AND SOURCE CODE CONTAINED 
// HEREIN AND IN RELATED FILES IS THE EXCLUSIVE PROPERTY OF PARAGON SOFTWARE
// GROUP COMPANY AND MAY NOT BE EXAMINED, DISTRIBUTED, DISCLOSED, OR REPRODUCED
// IN WHOLE OR IN PART WITHOUT EXPLICIT WRITTEN AUTHORIZATION FROM THE COMPANY.
// 
// Copyright (c) 1994-2016 Paragon Software Group, All rights reserved.
// 
// UNLESS OTHERWISE AGREED IN A WRITING SIGNED BY THE PARTIES, THIS SOFTWARE IS
// PROVIDED "AS-IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE, ALL OF WHICH ARE HEREBY DISCLAIMED. IN NO EVENT SHALL THE
// AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF NOT ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------

namespace Units
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using Core.Interfaces;

    [Serializable]
    public class SqLiteTransaction : ITransactionUnit
    {
        public SQLiteConnection DbConnection = null;
        private System.Data.SQLite.SQLiteTransaction _dbTransaction;
        public SQLiteCommand DbCommand = null;
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
                    DbConnection = new SQLiteConnection(string.Format("Data Source={0}", databasePath));
                    DbCommand = DbConnection.CreateCommand();
                    DbConnection.Open();
                    _dbTransaction = DbConnection.BeginTransaction();
                    DbCommand.Transaction = _dbTransaction;
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
            if (DbConnection != null)
            {
                this._rollbackCommands.Add(rollbackCommand);
                DbCommand.CommandText = sqlCommand;
                DbCommand.ExecuteNonQuery();
                return true;
            }
            Console.WriteLine("");
            return false;
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

                        Console.WriteLine("Closing connection failed.");
                        Console.WriteLine("Error: {0}", exception.ToString());

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
