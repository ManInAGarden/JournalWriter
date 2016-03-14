using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JournalWriter
{
    public class MarkDownLexicalAnalyzer
    {
        char[] nowords = new char[] { ' ', '*', '\n', '\r', '\t', '#', '_', '`',
            '-', '+', '>', '|', '['};
        char[] enumletters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
        public string Text { get; set; }

       

        public MarkDownLexicalAnalyzer(string txt)
        {
            //cure doc from any \r and store the positions of 
            //ommited chars for late rcorrection of text positions
            //for the lexical elements

            //Text = txt.Replace("\r", ""); 
            DocLexElement.JumpPositions = new List<int>();
            Text = "";
            char currc;
            for (int i = 0; i < txt.Length; i++)
            {
                currc = txt[i];
                if (currc != '\r')
                    Text += currc;
                else
                    DocLexElement.JumpPositions.Add(i);
            }

        }
    

        public List<DocLexElement> GetDocLexList()
        {
            List<DocLexElement> answ = new List<DocLexElement>();
            string txt = Text;
            int currp = 0,
                max = txt.Length;
            char currc, nextc, thirdc;
            string currw = "";
            bool incode = false;
            int spcCt = 0;
            int offset;

            while (currp < max)
            {
                ConsumeOneChar(txt, currp, out currc, out nextc, out thirdc, out spcCt);
                switch (currc)
                {
                    case ' ':
                        if (incode)
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.space, posi: currp));
                        }
                        break;

                    case '\t':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.tab, spcCount: spcCt, posi: currp));
                        break;
 
                    case '|':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.cellstart, spcCount: spcCt, posi: currp));
                        break;

                    case '\n':
                        if (nextc == '\n' || nextc == '\x1A') //parabreak when another newline or document end follows
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.parabreak, posi: currp));
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                        }
                        else
                        {
                            if (nextc == '=' && HaveALineOfThese(txt, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, level:1, spcCount: 0, posi: currp));
                                currp += offset;
                            }
                            else if (nextc == '-' && HaveALineOfThese(txt, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, level: 2, spcCount: 0, posi: currp));
                                currp += offset;
                            }
                            else
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.linebreak, posi: currp));
                            }
                        }
                        break;

                    case '#':
                        int headlev = 0;
                        while (nextc == '#')
                        {
                            ConsumeOneChar(txt, currp++, out currc, out nextc, out thirdc, out spcCt);
                            if (nextc=='#')
                                headlev++;
                        }
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.hashes, level: headlev+1, spcCount:spcCt, posi: currp-headlev));
                        break;

                    case '-':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.minus, spcCount: spcCt, posi: currp));
                        break;

                    case '+':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.plus, spcCount: spcCt, posi: currp));
                        break;

                    case '*':
                        if (nextc != '*')
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.emphasize, spcCount: spcCt, posi: currp));
                        else if (thirdc != '*')
                        {
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.bold, spcCount: spcCt, posi: currp-1));
                        }
                        else
                        {
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.boldemphasize, spcCount: spcCt, posi: currp-2));
                        }
                        break;

                    case '_':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.underline, spcCount: spcCt, posi: currp));
                        break;

                    case '`':
                        if (nextc != '`')
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.codeinline, spcCount: spcCt, posi: currp));
                        }
                        else if (thirdc == '`')
                        {
                            incode = !incode;
                            currp += 2;
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.code, posi: currp));
                        }
                        else
                        {
                            currp++;
                            currw += "``";
                            if (nowords.Contains(thirdc))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw, spcCount: spcCt, posi: currp));
                                currw = "";
                            }
                        }
                        break;

                    case '>':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.greaterthan, spcCount: spcCt, posi: currp));
                        break;

                    case '[':
                        if (nextc != ']' && thirdc == ']')
                        {
                            bool? state;
                            if (nextc == ' ')
                                state = false;
                            else if (char.ToLower(nextc) == 'o')
                                state = null;
                            else
                                state = true;

                            DocLexElement newl = new DocLexElement(DocLexElement.LexTypeEnum.todo,
                                state: state,
                                posi: currp,
                                spcCount: spcCt);
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.Text = currc.ToString();
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.SpaceCountAtEnd = spcCt;

                            answ.Add(newl);

                        }
                        else
                            currw += '[';

                        break;

                    case '<':
                        if (nowords.Contains(nextc))
                        {
                            currw += "&lt;";
                            answ.Add(GetWordLexElement(ref currw, spcCt, currp));
                            currw = "";
                        }
                        else
                            currw += "&lt;";
                        break;

                    case '&':
                        if (nowords.Contains(nextc))
                        {
                            currw += "&amp;";
                            answ.Add(GetWordLexElement(ref currw, spcCt, currp));
                        }
                        else
                            currw += "&amp;";
                        break;

                    default:
                        if (nowords.Contains(nextc))
                        {
                            currw += currc;
                            answ.Add(GetWordLexElement(ref currw, spcCt, currp));
                        }
                        else if (string.IsNullOrEmpty(currw) && IsEnumStartChar(currc) && nextc == '.' && thirdc==' ')
                        {
                            DocLexElement newl = new DocLexElement(DocLexElement.LexTypeEnum.enumeration, 
                                currc.ToString() + nextc, 
                                spcCount: 1, 
                                posi: currp);
                            ConsumeOneChar(txt, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.SpaceCountAtEnd = spcCt;
                            answ.Add(newl);
                        } else
                            currw += currc;

                        break;
                }

                currp++;
            }

            if (!string.IsNullOrWhiteSpace(currw))
                answ.Add(GetWordLexElement(ref currw, spcCt, currp));

            return answ;
        }

        private DocLexElement GetWordLexElement(ref string currw, int spcCt, int currp)
        {
            int additchars = CalcAdditChars(currw);
            DocLexElement answ = new DocLexElement(DocLexElement.LexTypeEnum.word,
                text: currw,
                spcCount: spcCt,
                posi: currp - additchars - currw.Length + 1);

            currw = "";

            return answ;
        }

        private int CalcAdditChars(string currw)
        {
            int answ = 0;
            string[] searchfors = new string[] { "&amp;", "&gt;", "&lt;" };

            foreach (string searchfor in searchfors)
            {
                answ += CountOccurences(currw, searchfor) * (searchfor.Length-1);
            }

            return answ;
        }

        private int CountOccurences(string tststr, string sfor)
        {
            int answ = 0;

            if (sfor.Length < tststr.Length)
                return answ;

            int idx = tststr.IndexOf(sfor);

            if (idx >= 0)
                answ = 1 + CountOccurences(tststr.Substring(idx+tststr.Length), sfor);

            return answ;
        }


        /// <summary>
        /// checks wether the given character is a valid
        /// start of an enumetation
        /// </summary>
        /// <param name="currc">Chaarcter to be checked</param>
        /// <returns></returns>
        private bool IsEnumStartChar(char currc)
        {
            return char.IsDigit(currc) || enumletters.Contains(char.ToLower(currc));
        }


        private bool HaveALineOfThese(string textc, int currp, char currc, out int offset)
        {
            offset = 0;
            int idx = currp;
            while (idx < textc.Length && textc[idx] == currc)
                idx++;

            if (idx > textc.Length)
            {
                return false;
            }

            if (textc[idx] == '\n' || textc[idx] == '\r')
            {
                if (idx - currp >= 3)
                {
                    offset = idx - currp;
                    return true;
                }
            }

            return false;
        }



        /// <summary>
        /// consume one character from the input text
        /// </summary>
        /// <param name="textc">The input text as an array of chars</param>
        /// <param name="currp">A pointer to the currently processed position in the input text</param>
        /// <param name="currc">returns the current char</param>
        /// <param name="nextc">returns the character following the current character</param>
        /// <param name="thirdc">returns the character following the character after the current character</param>
        /// <param name="spcCount"></param>
        private void ConsumeOneChar(string textc,
            int currp,
            out char currc,
            out char nextc,
            out char thirdc,
            out int spcCount)
        {
            currc = currp < Text.Length ? textc[currp] : '\x1A';
            nextc = GetNextChar(textc, currp);
            thirdc = GetNextChar(textc, currp + 1);
            spcCount = 0;
            int i = currp + 1;

            while ((i < textc.Length) && (textc[i++] == ' '))
            {
                spcCount++;
            }
        }


        /// <summary>
        /// Get Next char as a preview, return EOF ('\x1A') when no next char exists at end of array
        /// </summary>
        /// <param name="textc">char array to be used</param>
        /// <param name="currp">current position</param>
        /// <returns></returns>
        private char GetNextChar(string textc, int currp)
        {
            return (currp + 1) < Text.Length ? textc[currp + 1] : '\x1A';
        }
    }
}
