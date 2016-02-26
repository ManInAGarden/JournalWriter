using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JournalWriter
{
    public class DocLexElement
    {
        public enum LexTypeEnum {word, tab, space,
            linebreak, parabreak,
            hashes, headafter,
            bold, emphasize, boldemphasize, underline, codeinline,
            code, greaterthan};

        public LexTypeEnum Type { get; set; }
        public string Text { get; set; }
        public int Level { get; set; }

        public DocLexElement(LexTypeEnum t, string txt)
        {
            Type = t;
            Text = txt;
        }

        public DocLexElement(LexTypeEnum t)
        {
            Type = t;
        }

        public DocLexElement(LexTypeEnum t, int lvl)
        {
            Type = t;
            Level = lvl;
        }

        public override string ToString()
        {
            string answ = Type.ToString();

            switch (Type)
            {
                case LexTypeEnum.word:
                    answ += "(" + Text + ")";
                    break;
                case LexTypeEnum.hashes:
                    answ += "(" + Level + ")";
                    break;
                case LexTypeEnum.headafter:
                    answ += "(" + Level + ")";
                    break;
            }
            return answ;
        }

    }
}
