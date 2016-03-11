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
            Text = txt.Replace("\r", ""); //cure doc from any \r
        }

        

        public List<DocLexElement> GetDocLexList()
        {
            List<DocLexElement> answ = new List<DocLexElement>();
            char[] textc = Text.ToCharArray();
            int currp = 0,
                max = textc.Length;
            char currc, nextc, thirdc;
            string currw = "";
            bool incode = false;
            int spcCt;
            int offset;

            while (currp < max)
            {
                ConsumeOneChar(textc, currp, out currc, out nextc, out thirdc, out spcCt);
                switch (currc)
                {
                    case ' ':
                        if (incode)
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.space));

                        break;

                    case '\t':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.tab, spcCount: spcCt));
                        break;

                   
                    case '|':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.cellstart, spcCount: spcCt));
                        break;

                    case '\n':
                        if (nextc == '\n' || nextc == '\x1A') //parabreak when another newline or document end follows
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.parabreak));
                            ConsumeOneChar(textc, currp++, out currc, out nextc, out thirdc, out spcCt);
                        }
                        else
                        {
                            if (nextc == '=' && HaveALineOfThese(textc, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, level:1, spcCount: 0));
                                currp += offset;
                            }
                            else if (nextc == '-' && HaveALineOfThese(textc, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, level: 2, spcCount: 0));
                                currp += offset;
                            }
                            else
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.linebreak));
                            }
                        }
                        break;

                    case '#':
                        int headlev = 0;
                        while (nextc == '#')
                        {
                            ConsumeOneChar(textc, currp++, out currc, out nextc, out thirdc, out spcCt);
                            if(nextc=='#')
                                headlev++;
                        }
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.hashes, level: headlev+1, spcCount:spcCt));
                        break;

                    case '-':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.minus, spcCount: spcCt));
                        break;

                    case '+':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.plus, spcCount: spcCt));
                        break;

                    case '*':
                        if (nextc != '*')
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.emphasize, spcCount: spcCt));
                        else if (thirdc != '*')
                        {
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.bold, spcCount: spcCt));
                        }
                        else
                        {
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.boldemphasize, spcCount: spcCt));
                        }
                        break;

                    case '_':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.underline, spcCount: spcCt));
                        break;

                    case '`':
                        if (nextc != '`')
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.codeinline, spcCount: spcCt));
                        }
                        else if (thirdc == '`')
                        {
                            incode = !incode;
                            currp += 2;
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.code));
                        }
                        else
                        {
                            currp++;
                            currw += "``";
                            if (nowords.Contains(thirdc))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw, spcCount: spcCt));
                                currw = "";
                            }
                        }
                        break;

                    case '>':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.greaterthan, spcCount: spcCt));
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

                            DocLexElement newl = new DocLexElement(DocLexElement.LexTypeEnum.todo, state: state, spcCount: spcCt);
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.Text = currc.ToString();
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.SpaceCountAtEnd = spcCt;
                            
                            answ.Add(newl);
                            
                        }
                        break;

                    case '<':
                        if (nowords.Contains(nextc))
                        {
                            currw += "&lt;";
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw, spcCount: spcCt));
                            currw = "";
                        }
                        else
                            currw += "&lt;";
                        break;

                    case '&':
                        if (nowords.Contains(nextc))
                        {
                            currw += "&amp;";
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw, spcCount: spcCt));
                            currw = "";
                        }
                        else
                            currw += "&amp;";
                        break;

                    default:
                        if (nowords.Contains(nextc))
                        {
                            currw += currc;
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw, spcCount: spcCt));
                            currw = "";
                        }
                        else if (string.IsNullOrEmpty(currw) && IsEnumStartChar(currc) && nextc == '.' && thirdc==' ')
                        {
                            DocLexElement newl = new DocLexElement(DocLexElement.LexTypeEnum.enumeration, currc.ToString() + nextc, spcCount: 1);
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc, out spcCt);
                            newl.SpaceCountAtEnd = spcCt;
                            answ.Add(newl);
                        } else
                            currw += currc;

                        break;
                }

                currp++;
            }

            if (!string.IsNullOrWhiteSpace(currw))
                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw));

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

        /// <summary>
        /// Take car of any digits to recognize a number.
        /// </summary>
        /// <param name="textc"></param>
        /// <param name="currp"></param>
        /// <param name="currc"></param>
        /// <param name="nextc"></param>
        /// <param name="thirdc"></param>
        /// <param name="spcCt"></param>
        private DocLexElement HandleDigits(char[] textc, int currp, out char currc, out char nextc, out char thirdc, out int spcCt, out int offset)
        {
            bool stop = false;
            string numbs = "";
            offset = 0;
            int i = currp;
            do
            {
                ConsumeOneChar(textc, i, out currc, out nextc, out thirdc, out spcCt);

                stop = !char.IsDigit(nextc) && nextc != '.';
                
                numbs += currc;
                offset++;
                i++;
            } while (i < textc.Length && !stop);

            return new DocLexElement(DocLexElement.LexTypeEnum.number, numbs, spcCount: spcCt);
        }

        private bool HaveALineOfThese(char[] textc, int currp, char currc, out int offset)
        {
            offset = 0;
            int idx = currp;
            while (idx<textc.Length && textc[idx] == currc)
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
        private void ConsumeOneChar(char[] textc, 
            int currp, 
            out char currc, 
            out char nextc, 
            out char thirdc, 
            out int spcCount)
        {
            currc = textc[currp];
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
        private char GetNextChar(char[] textc, int currp)
        {
            return (currp + 1) < Text.Length ? textc[currp + 1] : '\x1A';
        }
    }
}
