using System;
using System.Collections.Generic;
using System.IO;

namespace Units
{
    internal class SqLiteJournal
    {
        private readonly string _pathToFolder = $"D:/Journal/";

        public void Dispose()
        {
            _pathToJournal = "";
            _pathToDb = "";
            _rollBackCommands.Clear();

            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public SqLiteJournal()
        { }

        public void GetParameters(string operationId)
        {
            _pathToJournal = _pathToFolder + operationId + ".txt";
            using (StreamReader sr = new StreamReader(_pathToJournal, System.Text.Encoding.Default))
            {
                _pathToDb = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    _rollBackCommands.Add(line);
                }
            }
        }

        public void Write(string databasePath, List<string> rollbackCommands, string operationId)
        {
            _pathToJournal = _pathToFolder + operationId + ".txt";
            using (StreamWriter streamWriter = File.AppendText(_pathToJournal))
            {
                streamWriter.WriteLine(databasePath);
                foreach (var command in rollbackCommands)
                {
                    streamWriter.WriteLineAsync(command);
                }
            }
        }

        private string _pathToJournal;
        public string PathToDataJournal => _pathToJournal;

        private string _pathToDb;
        public string PathToDataBase => _pathToDb;

        private readonly List<string> _rollBackCommands = new List<string>();
        public List<string> RollBackCommands => _rollBackCommands;
    }
}