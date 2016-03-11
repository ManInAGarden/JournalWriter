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
            code, greaterthan,
            number, minus, plus, enumeration,
            cellstart, todo};

        public LexTypeEnum Type { get; set; }
        public string Text { get; set; }
        public int Level { get; set; }
        public int SpaceCountAtEnd { get; set; }
        public bool? State { get; set; }


        /// <summary>
        /// Contructor for lexical element with a level. Used for headings and list elements
        /// </summary>
        /// <param name="t">The type of the element</param>
        /// <param name="text">Text contents of the lexical element</param>
        /// <param name="level">Level of the elements</param>
        /// <param name="spcCount">Number of spaces following the element</param>
        public DocLexElement(LexTypeEnum t, string text = "", int level=0, int spcCount=0, bool? state=null)
        {
            Type = t;
            Level = level;
            Text = text;
            SpaceCountAtEnd = spcCount;
            State = state;
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
