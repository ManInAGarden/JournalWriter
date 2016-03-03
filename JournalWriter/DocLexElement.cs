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
            number, minus, plus, enumeration};

        public LexTypeEnum Type { get; set; }
        public string Text { get; set; }
        public int Level { get; set; }
        public int SpaceCountAtEnd { get; set; }

        /// <summary>
        /// Constructor for Lex-Element with a text content
        /// </summary>
        /// <param name="t">The type of the element</param>
        /// <param name="txt">The text content</param>
        /// <param name="spcCount">Number of spaces following the element</param>
        public DocLexElement(LexTypeEnum t, string txt, int spcCount = 0)
        {
            Type = t;
            Text = txt;
            SpaceCountAtEnd = spcCount;
        }


        /// <summary>
        /// Contructor for lexical element with a level. Used for headings and list elements
        /// </summary>
        /// <param name="t">The type of the element</param>
        /// <param name="level">Level of the elements</param>
        /// <param name="spcCount">Number of spaces following the element</param>
        public DocLexElement(LexTypeEnum t, int level=0, int spcCount=0)
        {
            Type = t;
            Level = level;
            SpaceCountAtEnd = spcCount;
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
