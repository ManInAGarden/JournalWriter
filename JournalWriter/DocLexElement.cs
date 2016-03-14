using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JournalWriter
{
    public class DocLexElement
    {
        public enum LexTypeEnum {word, tab, space, nothing,
            linebreak, parabreak,
            hashes, headafter,
            bold, emphasize, boldemphasize, underline, codeinline,
            code, greaterthan,
            number, minus, plus, enumeration,
            cellstart, todo};

        static public List<int> JumpPositions { get; set; }

        public LexTypeEnum Type { get; set; }
        public string Text { get; set; }
        public int Level { get; set; }
        public int SpaceCountAtEnd { get; set; }
        public bool? State { get; set; }
        public int Position { get; set; }

        /// <summary>
        /// Contructor for lexical element with a level. Used for headings and list elements
        /// </summary>
        /// <param name="t">The type of the element</param>
        /// <param name="text">Text contents of the lexical element</param>
        /// <param name="level">Level of the elements</param>
        /// <param name="spcCount">Number of spaces following the element</param>
        /// <param name="posi">Position of the element in the text. Normally used for reverse finds</param>
        public DocLexElement(LexTypeEnum t, string text = "", int level=0, int spcCount=0, bool? state=null, int posi=0)
        {
            Type = t;
            Level = level;
            Text = text;
            SpaceCountAtEnd = spcCount;
            State = state;
            Position = CorrectPosi(posi);
        }

        /// <summary>
        /// Correct an element's position by calculating how many ommited chars
        /// (like \r) have to be added.
        /// </summary>
        /// <param name="posi">Position to be recalculated</param>
        /// <returns>The corrected position</returns>
        private int CorrectPosi(int posi)
        {
            int idx = 0;
            
            while (idx < JumpPositions.Count && posi >= JumpPositions[idx])
            {
                idx++;
                posi++; //this is correct! We have to go even further in the jump list for any single previously omitted char
            }

            return posi;
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
                case LexTypeEnum.todo:
                    answ += "(" + Position + "," + State + ")";
                    break;
            }
            return answ;
        }

    }
}
