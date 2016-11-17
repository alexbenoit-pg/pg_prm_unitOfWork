// -----------------------------------------------------------------------
// <copyright file="SQLiteUnit.cs" company="Paragon Software Group">
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
    using System.Runtime.Serialization;
    using Core.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [DataContract]
    public class SQLiteUnit : ITransactionUnit
    {
        [DataMember(Order = 2)]
        private readonly List<string> rollbackCommands = new List<string>();
        private readonly List<string> commitCommands = new List<string>();
        
        [DataMember(Order = 1)]
        private string dataBasePath;
        private SQLiteConnection dataBaseConnection = null;
        private SQLiteCommand dataBaseCommand = null;
        private SQLiteTransaction dataBaseTransaction;
        
        public SQLiteUnit()
        {
        }

        public SQLiteUnit(string pathdatabase)
        {
            this.dataBasePath = pathdatabase;
        }

        [DataMember(Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitType Type
        {
            get
            {
                return UnitType.SQLiteUnit;
            }
        }

        public static string GetConnectionString(string pathDataBase)
        {
            return string.Format("Data Source={0};", pathDataBase);
        }

        public void Dispose()
        {
            if (this.dataBaseConnection != null)
            {
                this.dataBaseConnection.Close();
                this.dataBaseCommand.Dispose();
                this.dataBaseConnection.Dispose();
                this.dataBaseConnection = null;
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }
        
        public bool AddSqliteCommand(string sqlCommand, string rollbackCommand)
        {
            this.rollbackCommands.Add(rollbackCommand);
            this.commitCommands.Add(sqlCommand);
            return true;
        }

        public void Rollback()
        {
            string sqliteConnectionString = SQLiteUnit.GetConnectionString(this.dataBasePath);
            using (this.dataBaseConnection = new SQLiteConnection(sqliteConnectionString))
            {
                this.dataBaseConnection.Open();
                using (this.dataBaseTransaction = this.dataBaseConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(this.dataBaseConnection) { Transaction = this.dataBaseTransaction })
                    {
                        foreach (string line in this.rollbackCommands)
                        {
                            cmd.CommandText = line;
                            cmd.ExecuteNonQueryAsync();
                        }

                        this.dataBaseTransaction.Commit();
                    }
                }
            }
        }

        public void Commit()
        {
            this.ConnectDatabase(this.dataBasePath);
            foreach (var command in this.commitCommands)
            {
                this.dataBaseCommand.CommandText = command;
                try
                {
                    this.dataBaseCommand.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    this.Dispose();
                    throw exception;
                }
            }

            this.SqLiteCommit();
        }

        private bool ConnectDatabase(string pathDataBase)
        {
            try
            {
                if (File.Exists(pathDataBase))
                {
                    this.Dispose();
                    string sqliteConnectionString = SQLiteUnit.GetConnectionString(pathDataBase);
                    this.dataBaseConnection = new SQLiteConnection(sqliteConnectionString);
                    this.dataBaseCommand = this.dataBaseConnection.CreateCommand();
                    this.dataBaseConnection.Open();
                    this.dataBaseTransaction = this.dataBaseConnection.BeginTransaction();
                    this.dataBaseCommand.Transaction = this.dataBaseTransaction;
                    this.dataBasePath = pathDataBase;
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

        private void SqLiteCommit()
        {
            try
            {
                if (this.dataBaseConnection != null)
                {
                    this.dataBaseTransaction.Commit();
                }
            }
            catch (SQLiteException exception)
            {
                if (this.dataBaseTransaction != null)
                {
                    try
                    {
                        this.dataBaseTransaction.Rollback();
                        throw exception;
                    }
                    catch (SQLiteException secondException)
                    {
                        throw secondException;
                    }
                    finally
                    {
                        this.dataBaseTransaction.Dispose();
                    }
                }
            }
            finally
            {
                this.dataBaseCommand.Dispose();

                this.dataBaseTransaction.Dispose();

                if (this.dataBaseConnection != null)
                {
                    try
                    {
                        this.dataBaseConnection.Close();
                    }
                    catch (SQLiteException exception)
                    {
                        throw exception;
                    }
                    finally
                    {
                        this.dataBaseConnection.Dispose();
                        this.dataBaseConnection = null;
                    }
                }
            }
        }
    }
}
