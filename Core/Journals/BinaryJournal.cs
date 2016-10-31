// -----------------------------------------------------------------------
// <copyright file="BinaryJournal.cs" company="Paragon Software Group">
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
    using System.Runtime.Serialization;

    using Core.Interfaces;
    using Core.Helpers;

    public class BinaryJournal : IJournal
    {
        public BinaryJournal()
        {
            this.name = Guid.NewGuid().ToString();
            File.WriteAllText(PathToFile, "");
        }

        public BinaryJournal(string journalName)
        {
            this.name = journalName;
        }

        public string PathToFile {
            get
            {
                return $"{FolderHelper.JournalsFolder}\\{this.name}.{format}";
            }
        }
        
        public void Add(ITransactionUnit operation)
        {
            if (operation == null)
                throw new NullReferenceException();

            if (!BinaryHelper.IsSerialisibleOperation(operation))
                throw new SerializationException($"Unit \"{operation.GetType().ToString()}\" isn't serialisible. Please, add attribute \"[Serializable]\" to this class.");

            this.operations.Add(operation);
            RewriteFile();
        }
        
        public void Remove(ITransactionUnit operation)
        {
            if (operation == null)
                throw new NullReferenceException();

            this.operations.Remove(operation);
            RewriteFile();
        }

        public List<ITransactionUnit> GetOperationsFromJournal()
        {
            return ReadFromFile();
        }
        
        public void Delete()
        {
            File.Delete(PathToFile);
        }

        private List<ITransactionUnit> ReadFromFile()
        {
            var binary = File.ReadAllText(PathToFile);

            return (string.IsNullOrEmpty(binary))
                ? new List<ITransactionUnit>()
                : BinaryHelper.FromBinary<List<ITransactionUnit>>(binary);
        }
        
        private void RewriteFile()
        {
            string binary = BinaryHelper.ToBinary(this.operations);
            File.WriteAllText(PathToFile, binary);
        }
        
        public void Dispose()
        {
            Delete();
        }
        
        private List<ITransactionUnit> operations = new List<ITransactionUnit>();
        private string name;
        private string format = "dat";
    }
}
