using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JournalWriter
{
    public class WordPositionInfo : IComparable
    {
        public int Index { get; set; }
        public string Text { get; set; }

        public int CompareTo(object obj)
        {
            return Index.CompareTo((obj as WordPositionInfo).Index);
        }
    }
}
