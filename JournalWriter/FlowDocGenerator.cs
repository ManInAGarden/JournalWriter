using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JournalWriter
{
    public class FlowDocGenerator
    {
        public string DocumentFontFamily { set; get; }
        public string DocumentNormalFontSize { set; get; }
        public string DocumentHeadline1FontSize { set; get; }
        public string DocumentHeadline2FontSize { set; get; }
        public string DocumentHeadline3FontSize { set; get; }
        public string DocumentHeadline4FontSize { set; get; }
        public string DocumentHeadline5FontSize { set; get; }
        public string CodingFontSize { set; get; }
        public string CodingFontFamily { set; get; }


        /// <summary>
        /// Das FlowDocument erzeugen aus dem Ergebnis der lexikalischen Analyse
        /// </summary>
        /// <param name="lexes">Die Liste lexikalischer Konstruktionen</param>
        /// <returns>Ein String, der das FlowDocument erzeugen kann</returns>
        public string ProduceDoc(List<DocLexElement> lexes)
        {
            const string start = "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""
                + " xmlns:sys=\"clr-namespace:System;assembly=mscorlib\""
                + " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\n";

            bool initalic = false,
                inbold = false,
                inunderline = false,
                inboldemphasize = false,
                incode = false;

            string answ = "";
            string subtxt = "";
            DocLexElement lex;
            int i = 0;
            while (i<lexes.Count)
            {
                lex = lexes[i++];

                switch (lex.Type)
                {
                    case DocLexElement.LexTypeEnum.word:
                        subtxt += lex.Text + " ";
                        break;
                    case DocLexElement.LexTypeEnum.emphasize:
                        if (!initalic)
                        {
                            initalic = true;
                            subtxt += "<Italic>";
                        }
                        else
                        {
                            subtxt += "</Italic> ";
                            initalic = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.codeinline:
                        if (!incode)
                        {
                            incode = true;
                            subtxt += "<Span FontFamily=\"Lucida Sans\">";
                        }
                        else
                        {
                            subtxt += "</Span> ";
                            incode = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.bold:
                        if (!inbold)
                        {
                            inbold = true;
                            subtxt += "<Bold>";
                        }
                        else
                        {
                            subtxt += "</Bold> ";
                            inbold = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.boldemphasize:
                        if (!inboldemphasize)
                        {
                            inboldemphasize = true;
                            subtxt += "<Bold><Italic>";
                        }
                        else
                        {
                            subtxt += "</Italic></Bold> ";
                            inboldemphasize = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.underline:
                        if (!inunderline)
                        {
                            inunderline = true;
                            subtxt += "<Underline>";
                        }
                        else
                        {
                            subtxt += "</Underline> ";
                            inunderline = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.linebreak:
                        subtxt += "<LineBreak />";
                        break;
                    case DocLexElement.LexTypeEnum.headafter:
                        if (!string.IsNullOrWhiteSpace(subtxt))
                        {
                            
                            answ += string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                                GetFontSizeFromHeadLevel(lex.Level),
                                subtxt,
                                DocumentFontFamily);
                        }
                        subtxt = "";
                        break;

                    case DocLexElement.LexTypeEnum.hashes:
                        int offset;
                        if (string.IsNullOrEmpty(subtxt))
                        {
                            subtxt = GetHashesContent(lexes, i, out offset);
                            if (!string.IsNullOrWhiteSpace(subtxt))
                            {

                                answ += string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                                    GetFontSizeFromHeadLevel(lex.Level),
                                    subtxt,
                                    DocumentFontFamily);
                            }
                            subtxt = "";
                            i += offset;
                        }
                        else
                        {
                            subtxt += "#";
                        }
                        
                        break;
                    case DocLexElement.LexTypeEnum.parabreak:
                        answ += EndCurrentParagraph(subtxt);
                        subtxt = "";
                        break;

                    case DocLexElement.LexTypeEnum.code:
                        answ += EndCurrentParagraph(subtxt);
                        subtxt = "";
                        answ += GetCodeText(lexes, ++i, out offset);
                        i += offset;
                        break;

                    case DocLexElement.LexTypeEnum.greaterthan:
                        if (string.IsNullOrEmpty(subtxt))
                        {
                            answ += GetQuotationText(lexes, i, out offset);
                            i += offset;
                        }
                        else
                            subtxt += " >";
                        break;
                }
            }


            return start + answ + "</FlowDocument>";
        }

        private string GetQuotationText(List<DocLexElement> lexes, int pos, out int offset)
        {
            string answ = "";
            offset = 0;
            DocLexElement lex;
            bool stop = false;
            bool inbold = false,
                inunderline = false;
            for (int i = pos; i < lexes.Count; i++)
            {
                lex = lexes[i];
                switch(lex.Type)
                {
                    case DocLexElement.LexTypeEnum.parabreak:
                        stop = true;
                        break;
                    case DocLexElement.LexTypeEnum.word:
                        answ += lex.Text + " ";
                        break;
                    case DocLexElement.LexTypeEnum.greaterthan:
                        answ += ">";
                        break;
                    case DocLexElement.LexTypeEnum.emphasize:
                        answ += "*";
                        break;
                    case DocLexElement.LexTypeEnum.bold:
                        if (!inbold)
                        {
                            answ += "<Bold>";
                            inbold = true;
                        }
                        else
                        {
                            answ += "</Bold>";
                            inbold = false;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.underline:
                        if(!inunderline)
                        {
                            answ += "<Underline>";
                            inunderline = true;
                        }
                        else
                        {
                            answ += "</Underline>";
                            inunderline = false;
                        }
                        break;
                }

                offset++;

                if (stop)
                    break;
            }

            if (!string.IsNullOrEmpty(answ))
                return string.Format("<Paragraph Margin=\"50,0,30,0\" FontSize=\"{1}\"  FontFamily=\"{2}\" FontStyle=\"Italic\">{0}</Paragraph>",
                    answ,
                    DocumentNormalFontSize,
                    DocumentFontFamily);
            else
                return "";
        }


        /// <summary>
        /// Get all the coding from a coding blog
        /// </summary>
        /// <param name="lexes"></param>
        /// <returns></returns>
        private string GetCodeText(List<DocLexElement> lexes, int currp, out int offset)
        {
            string answ = "";
            DocLexElement lex;
            bool stop = false;
            offset = 0;
            for(int i=currp; i<lexes.Count; i++)
            {
                lex = lexes[i];
                switch(lex.Type)
                {
                    case DocLexElement.LexTypeEnum.word:
                        answ += lex.Text;
                        break;
                    case DocLexElement.LexTypeEnum.linebreak:
                        answ += "\n";
                        break;
                    case DocLexElement.LexTypeEnum.codeinline:
                        answ += "'";
                        break;
                    case DocLexElement.LexTypeEnum.emphasize:
                        answ += "*";
                        break;
                    case DocLexElement.LexTypeEnum.bold:
                        answ += "**";
                        break;
                    case DocLexElement.LexTypeEnum.boldemphasize:
                        answ += "***";
                        break;
                    case DocLexElement.LexTypeEnum.space:
                        answ += " ";
                        break;
                    case DocLexElement.LexTypeEnum.hashes:
                        answ += GetStringOf("#", lex.Level);
                        break;
                    case DocLexElement.LexTypeEnum.headafter:
                        switch (lex.Level)
                        {
                            case 1:
                                answ += "\n===";
                                break;
                            case 2:
                                answ += "\n---";
                                break;
                        }
                        break;
                    case DocLexElement.LexTypeEnum.greaterthan:
                        answ += ">";
                        break;
                    case DocLexElement.LexTypeEnum.parabreak:
                        answ += "\n\n";
                        break;
                    case DocLexElement.LexTypeEnum.tab:
                        answ += "\t";
                        break;
                    case DocLexElement.LexTypeEnum.code:
                        stop = true;
                        break;
                }

                offset++;

                if (stop)
                    break;
            }

            if (!string.IsNullOrEmpty(answ))
                return string.Format("<Paragraph xml:space=\"preserve\" TextAlignment=\"Left\" Margin=\"50,0,0,0\" FontSize=\"{1}\" FontFamily=\"{2}\">{0}</Paragraph>",
                    answ,
                    CodingFontSize,
                    CodingFontFamily);
            else
                return "";
                
        }


        /// <summary>
        /// Produce a string by repeating a string a given number of times
        /// </summary>
        /// <param name="source">The string to be repeated</param>
        /// <param name="repeats">Number of repeats</param>
        /// <returns></returns>
        private string GetStringOf(string source, int repeats)
        {
            string answ = "";

            for (int i = 0; i < repeats; i++)
                answ += source;

            return answ;
        }


        /// <summary>
        /// Produces a paragraph if any text is in paratext
        /// </summary>
        /// <param name="paratxt"></param>
        /// <returns></returns>
        private string EndCurrentParagraph(string paratxt)
        {
            if (!string.IsNullOrWhiteSpace(paratxt))
                return string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\">{1}</Paragraph>\n\n",
                    DocumentNormalFontSize,
                    paratxt,
                    DocumentFontFamily);

            return "";
        }


        /// <summary>
        /// fetch the FlowDoc Content of the hashes i.e. everthing til the next parabreak
        /// </summary>
        /// <param name="lexes">List of lexical elements</param>
        /// <param name="start">Start of processing</param>
        /// <param name="offset">Number of processed (used up) elements</param>
        /// <returns></returns>
        private string GetHashesContent(List<DocLexElement> lexes, int start, out int offset)
        {
            offset = 0;

            string answ = "";
            bool stop = false;

            DocLexElement lex;
            for(int i=start; i<lexes.Count; i++)
            {
                lex = lexes[i];
                switch(lex.Type)
                {
                    case DocLexElement.LexTypeEnum.parabreak:
                        stop = true;
                        break;
                    case DocLexElement.LexTypeEnum.word:
                        answ += lex.Text + " ";
                        break;
                    case DocLexElement.LexTypeEnum.linebreak:
                        answ += "<LineBreak />";
                        break;
                }

                offset++;

                if (stop)
                    break;
            }

            return answ;
        }

        /// <summary>
        /// Finde das Ende des aktuellen Absatzes.
        /// </summary>
        /// <param name="lexes">List der Lexikalischen Elemente</param>
        /// <param name="start">Hier beginnt die Suche</param>
        /// <returns></returns>
        private int FindNextParabreakIndex(List<DocLexElement> lexes, int start)
        {
            
            for (int k = start; start < lexes.Count; k++)
            {
                if (lexes[k].Type == DocLexElement.LexTypeEnum.parabreak)
                    return k;
            }

            return lexes.Count-1;
        }

        /// <summary>
        /// Set the font size from the heading level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GetFontSizeFromHeadLevel(int level)
        {
            string answ;

            switch (level)
            {
                case 1:
                    answ = DocumentHeadline1FontSize;
                    break;
                case 2:
                    answ = DocumentHeadline2FontSize;
                    break;
                case 3:
                    answ = DocumentHeadline3FontSize;
                    break;
                case 4:
                    answ = DocumentHeadline4FontSize;
                    break;
                case 5:
                    answ = DocumentHeadline5FontSize;
                    break;
                default:
                    answ = DocumentNormalFontSize;
                    break;
            }

            return answ;
        }
    }
}
