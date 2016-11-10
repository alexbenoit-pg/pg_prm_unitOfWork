// -----------------------------------------------------------------------
// <copyright file="SqLiteJournal.cs" company="Paragon Software Group">
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
    using System.IO;

    internal class SqLiteJournal
    {
        private readonly string pathToFolder = Path.Combine(
            Path.GetTempPath(), 
            "UnitOfWorkSQLiteTransaction");
        public SqLiteJournal()
        {
            this.EnsureExistJournalFolder();
        }

        public void Dispose()
        {
            this.pathToJournal = string.Empty;
            this.pathToDb = string.Empty;
            this.rollBackCommands.Clear();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
        
        public void GetParameters(string operationId)
        {
            this.pathToJournal = Path.Combine(this.pathToFolder, operationId + ".txt");
            using (StreamReader sr = new StreamReader(this.pathToJournal, System.Text.Encoding.Default))
            {
                this.pathToDb = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    this.rollBackCommands.Add(line);
                }
            }
        }

        public void Write(string databasePath, List<string> rollbackCommands, string operationId)
        {
            this.pathToJournal = Path.Combine(this.pathToFolder, operationId + ".txt");
            using (StreamWriter streamWriter = File.AppendText(this.pathToJournal))
            {
                streamWriter.WriteLine(databasePath);
                foreach (var command in rollbackCommands)
                {
                    streamWriter.WriteLineAsync(command);
                }
            }
        }

        private void EnsureExistJournalFolder()
        {
            if (!Directory.Exists(this.pathToFolder))
                Directory.CreateDirectory(this.pathToFolder);
        }

        private string pathToJournal;
        public string PathToDataJournal => this.pathToJournal;

        private string pathToDb;
        public string PathToDataBase => this.pathToDb;

        private readonly List<string> rollBackCommands = new List<string>();
        public List<string> RollBackCommands => this.rollBackCommands;
    }
}