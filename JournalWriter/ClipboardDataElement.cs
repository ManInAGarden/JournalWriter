using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JournalWriter
{
    public enum ClipTypeEnum { Line, Block };

    [Serializable]
    public class ClipboardDataElement
    {
        public static string Format = "SJournalClipboardDataElement";

        public ClipTypeEnum  ClipType { get; set; }

        public string Text { get; set; }

        public ClipboardDataElement(ClipTypeEnum clipt, string txt)
        {
            ClipType = clipt;
            Text = txt;
        }
    }
}
