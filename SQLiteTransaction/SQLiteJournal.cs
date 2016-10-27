using System;
using System.Collections.Generic;
using System.IO;
using Core.Interfaces;

namespace SQLiteTransaction
{
    public class SqLiteJournal : IJournal
    {
        private string _pathToFolder = $"D:/Journal/";

        public void Dispose()
        {
            _pathToJournal = "";
            _pathToDB = "";
            _rollBackCommands.Clear();

            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public SqLiteJournal()
        {}

        public void GetParameters(string operationID)
        {
            _pathToJournal = _pathToFolder + operationID + ".txt";
            using (StreamReader sr = new StreamReader(_pathToJournal, System.Text.Encoding.Default))
            {
                _pathToDB = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    _rollBackCommands.Add(line);
                }
            }
        }

        public void Write(string _databasePath, List<string> _rollbackCommands, string operationID)
        {
            _pathToJournal = _pathToFolder + operationID + ".txt";
          //  using (StreamWriter streamWriter = new StreamWriter(_pathToJournal,false, System.Text.Encoding.Default))
          
            using (StreamWriter streamWriter = File.AppendText(_pathToJournal))
            {
                streamWriter.WriteLine(_databasePath);
                foreach (var command in _rollbackCommands)
                {
                    streamWriter.WriteLineAsync(command);
                }
            }
        }

        private string _pathToJournal;
        public string PathToDataJournal => _pathToJournal;

        private string _pathToDB;
        public string PathToDataBase => _pathToDB;

        private List<string> _rollBackCommands = new List<string>();
        public List<string> RollBackCommands => _rollBackCommands;
    }
}