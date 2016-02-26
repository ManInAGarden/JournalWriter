using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JournalWriter
{
    public class MarkDownLexicalAnalyzer
    {
        char[] nowords = new char[] { ' ', '*', '\n', '\r', '\t', '#', '_', '\'' };
        
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

            while (currp < max)
            {
                ConsumeOneChar(textc, currp, out currc, out nextc, out thirdc);
                switch (currc)
                {
                    case ' ':
                        if(incode)
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.space));
                        break;
                    case '\t':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.tab));
                        break;
                    case '\n':
                        if (nextc == '\n' || nextc == '\x1A') //parabreak when another newline or document end follows
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.parabreak));
                            ConsumeOneChar(textc, currp++, out currc, out nextc, out thirdc);
                        }
                        else
                        {
                            int offset = 0;
                            if (nextc == '=' && HaveALineOfThese(textc, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, 1));
                                currp += offset;
                            }
                            else if (nextc == '-' && HaveALineOfThese(textc, currp + 1, nextc, out offset))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.headafter, 2));
                                currp += offset;
                            }
                            else
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.linebreak));
                        }
                        break;
                    case '#':
                        int headlev = 0;
                        while (nextc == '#')
                        {
                            ConsumeOneChar(textc, currp++, out currc, out nextc, out thirdc);
                            if(nextc=='#')
                                headlev++;
                        }
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.hashes, headlev+1));
                        break;
                    case '*':
                        if (nextc != '*')
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.emphasize));
                        else if (thirdc != '*')
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.bold));
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc);
                        }
                        else
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.boldemphasize));
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc);
                            ConsumeOneChar(textc, ++currp, out currc, out nextc, out thirdc);
                        }
                        break;
                    case '_':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.underline));
                        break;
                    case '`':
                        if (nextc != '`')
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.codeinline));
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
                            if(nowords.Contains(thirdc))
                            {
                                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw));
                                currw = "";
                            }
                        }
                        break;
                    case '>':
                        answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.greaterthan));
                        break;
                    default:
                        currw += currc;
                        if (nowords.Contains(nextc))
                        {
                            answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw));
                            currw = "";
                        }
                        break;
                }

                currp++;
            }

            if (!string.IsNullOrWhiteSpace(currw))
                answ.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, currw));

            return answ;
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

        private void ConsumeOneChar(char[] textc, int currp, out char currc, out char nextc, out char thirdc)
        {
            currc = textc[currp];
            nextc = GetNextChar(textc, currp);
            thirdc = GetNextChar(textc, currp + 1);
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
