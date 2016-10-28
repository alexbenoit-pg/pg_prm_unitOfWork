using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;

namespace Core.Journals
{
    internal static class JournalsFactory
    {
        internal static IJournal GetJournal(JournalTypes type)
        {
            switch (type)
            {
                case JournalTypes.JSON:
                    return new JsonJournal();
                case JournalTypes.Binary:
                    return new BinaryJournal();
                default:
                    return new JsonJournal();
            }
        }
    }
}
