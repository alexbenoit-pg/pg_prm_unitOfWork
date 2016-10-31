using System;
using System.IO;

namespace ChinhDo.Transactions
{
    static class FileUtils
    {
        public static readonly string tempFolder = Path.Combine(Path.GetTempPath(), "UnitOfWorkFileTransaction");
        public static readonly string journalFolder = Path.Combine(tempFolder, "Journals");

        /// <summary>
        /// Ensures that the folder that contains the temporary files exists.
        /// </summary>
        public static void EnsureTempFolderExists()
        {
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
        }

        /// <summary>
        /// Ensures that the folder that contains the temporary files exists.
        /// </summary>
        public static void EnsureJournalFolder()
        {
            if (!Directory.Exists(journalFolder))
            {
                Directory.CreateDirectory(journalFolder);
            }
        }


        /// <summary>
        /// Returns a unique temporary file name.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetTempFileName(string extension)
        {
            Guid g = Guid.NewGuid();
            string retVal = Path.Combine(tempFolder, g.ToString().Substring(0, 8)) + extension;

            return retVal;
        }
    }
}
