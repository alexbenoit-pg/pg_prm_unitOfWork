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

using System;

namespace Units
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Core.Exceptions;
    using Core.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Units.SQLiteTransactionUnit;

    [DataContract]
    public class SQLiteUnit : ITransactionUnit
    {
        [DataMember(Order = 2)] private readonly List<string> rollbackCommands = new List<string>();
        private readonly List<string> commitCommands = new List<string>();
        [DataMember(Order = 1)] private string dataBasePath;

        public SQLiteUnit(string dataBasePath)
        {
            this.dataBasePath = dataBasePath;
        }

        [DataMember(Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitType Type => UnitType.SQLiteUnit;

        public void AddSqliteCommand(string sqlCommand, string rollbackCommand)
        {
            this.rollbackCommands.Add(rollbackCommand);
            this.commitCommands.Add(sqlCommand);
        }

        public void Rollback()
        {
            try
            {
                SQLiteManager.ExecuteCommandsInTransaction(
                    this.dataBasePath, 
                    this.rollbackCommands);
            }
            catch (Exception e)
            {
                throw new RollbackException(e.Message);
            }
        }

        public void Commit()
        {
            try
            {
                SQLiteManager.ExecuteCommandsInTransaction(
                    this.dataBasePath, 
                    this.commitCommands);
            }
            catch (Exception e)
            {
                throw new CommitException(e.Message);
            }
            }

        public void Dispose()
        {
        }
    }
}
