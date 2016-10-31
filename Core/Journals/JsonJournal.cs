// -----------------------------------------------------------------------
// <copyright file="JsonJournal.cs" company="Paragon Software Group">
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

namespace Core.Journals
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Core.Interfaces;
    using Core.Helpers;

    public class JsonJournal : IJournal
    {
        public JsonJournal()
        {
            this.name = Guid.NewGuid().ToString();
            CreateFile();
        }

        public JsonJournal(string journalName)
        {
            this.name = journalName;
        }

        public string PathToFile {
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

            WriteToFile(serializibleOperation);
        }
        
        public void Remove(ITransactionUnit operation)
        {
            if (operation == null)
                throw new NullReferenceException();

            var operationsInJournal = ReadFromFile()
                .Where(o => o.OperationID != operation.GetOperationId())
                .ToArray();
            
            WriteToFile(operationsInJournal);
        }

        public List<ITransactionUnit> GetOperationsFromJournal()
        {
            var result = new List<ITransactionUnit>();
            var operationsInJson = ReadFromFile();
            foreach (var op in operationsInJson)
            {
                ITransactionUnit unit = (ITransactionUnit)Activator.CreateInstance(
                    op.TransactionUnitAssembly,
                    op.TransactionUnitName);
                unit.SetOperationId(op.OperationID);
                result.Add(unit);
            }

            return result;
        }

        public void Delete()
        {
            File.Delete(PathToFile);
        }

        private SerializibleOperation[] ReadFromFile()
        {
            var json = File.ReadAllText(PathToFile);
            return JsonHelper.FromJson<SerializibleOperation[]>(json);
        }

        private void WriteToFile(SerializibleOperation serializibleOperation)
        {
            RemoveLastSymbol();
            var json = JsonHelper.ToJson(serializibleOperation);
            File.AppendAllText(PathToFile, $"{json},{endSymbols}");
        }

        private void WriteToFile(SerializibleOperation[] serializibleOperations)
        {
            var json = JsonHelper.ToJson(serializibleOperations);
            File.WriteAllText(PathToFile, json);
        }

        private void RemoveLastSymbol()
        {
            string file = string.Join("",File.ReadAllLines(PathToFile));
            file = file.Remove(file.Length - 1, 1);
            File.WriteAllText(PathToFile, file);
        }
        
        private void CreateFile()
        {
            File.WriteAllText(PathToFile, $"{startSymbols}\n{endSymbols}");
        }
        
        public void Dispose()
        {
            Delete();
        }

        private string name;
        private const string startSymbols = "[\n";
        private const string endSymbols = "\n]";
    }
}
