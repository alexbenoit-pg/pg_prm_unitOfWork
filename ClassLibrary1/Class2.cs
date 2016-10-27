using System.Data;
using System.IO;
using Core.Interfaces;

namespace SQLiteTransaction
{
    public class SqLiteJournal : IJournal
    {
        private string pathToFolder = $"C:folder/itd/itomu/podobnoe/";

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public SqLiteJournal()
        {

        }

        public void GetParameters(string operationID)
        {
            _pathToJournal = pathToFolder + operationID;
            using (StreamReader sr = new StreamReader(_pathToJournal, System.Text.Encoding.Default))
            {
                _pathToDB = sr.ReadLine();
                _rollBackCommand = sr.ReadLine();
            }
        }

        public bool Write(string _databasePath, string _rollbackCommand, string operationID)
        {
            _pathToJournal = pathToFolder + operationID;
            using (StreamWriter streamWriter = new StreamWriter(_pathToJournal, false, System.Text.Encoding.Default))
            {
                streamWriter.WriteLine(_databasePath);
                streamWriter.WriteLine(_rollbackCommand);
            }
            return false;
        }

        private string _pathToJournal;
        public string PathToDataJournal => _pathToJournal;

        private string _pathToDB;
        public string PathToDataBase => _pathToDB;

        private string _rollBackCommand;
        public string RollBackCommand => _rollBackCommand;
    }
}