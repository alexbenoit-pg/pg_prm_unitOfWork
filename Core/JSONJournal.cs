// -----------------------------------------------------------------------
// <copyright file="Journal.cs" company="Paragon Software Group">
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

namespace Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;

    using Core.Interfaces;
    using Core.Helpers;

    public class JSONJournal : IJournal
    {
        public JSONJournal()
        {
            this.name = Guid.NewGuid().ToString();
            CreateJournalFile();
        }
        
        public string Path {
            get
            {
                return $"{FolderHelper.JournalsFolder}\\{this.name}.txt";
            }
        }

        public void Add(ITransactionUnit operation)
        {
            if (operation == null)
                throw new NullReferenceException();

            var serializibleOperation = new SerializibleOperation(operation);

            var jsonFormatter = 
                new DataContractJsonSerializer(typeof(SerializibleOperation));

            WriteToFile(serializibleOperation, jsonFormatter);
        }
        
        public void Delete(ITransactionUnit operation)
        {
            if (operation == null)
                throw new NullReferenceException();

            var jsonFormatter =
                new DataContractJsonSerializer(typeof(SerializibleOperation[]));

            var operationsInJournal = ReadJournalFile(jsonFormatter);
            operationsInJournal = operationsInJournal
                .Where(o => o.OperationID != operation.GetOperationId()).ToArray();

            ClearJournalFile();

            using (FileStream fs = new FileStream(Path, FileMode.Append))
            {
                jsonFormatter.WriteObject(fs, operationsInJournal);
            }
        }

        public void DeleteJournalFile() {
            File.Delete(Path);
        }

        private SerializibleOperation[] ReadJournalFile(DataContractJsonSerializer jsonFormatter)
        {
            SerializibleOperation[] result;
            
            using (FileStream fs = new FileStream(Path, FileMode.OpenOrCreate))
            {
                result = (SerializibleOperation[])jsonFormatter.ReadObject(fs);
            }

            return result;
        }

        private void WriteToFile(SerializibleOperation serializibleOperation, DataContractJsonSerializer jsonFormatter)
        {
            RemoveLastSymbols();

            using (FileStream fs = new FileStream(Path, FileMode.Append))
            {
                jsonFormatter.WriteObject(fs, serializibleOperation);
            }

            File.AppendAllText(Path, "," + endSymbols);
        }

        private void RemoveLastSymbols()
        {
            string file = string.Join("",File.ReadAllLines(Path));

            file = file.Remove(file.Length - 1, 1);

            File.WriteAllText(Path, file);
        }
        
        public void Dispose()
        {
        }
        
        private void CreateJournalFile()
        {
            File.WriteAllText(Path, $"{startSymbols}\n{endSymbols}");
        }

        private void ClearJournalFile()
        {
            File.WriteAllText(Path, "");
        }

        private readonly string name;
        private const string startSymbols = "[\n";
        private const string endSymbols = "\n]";

    }
}
