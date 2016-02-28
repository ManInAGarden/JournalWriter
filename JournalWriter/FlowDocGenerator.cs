using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace JournalWriter
{
    [Flags]
    public enum InParamodeEnum { none=0, bold=1, emphasize=2, underline=4, inlinecode=8, unlist=16, inorderedlist=32}

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

            InParamodeEnum inparmode = InParamodeEnum.none;

            string answ = "";
            string subtxt = "";
            DocLexElement lex;
            int i = 0;
            int offset = 0;

            while (i<lexes.Count)
            {
                lex = lexes[i++];

                switch (lex.Type)
                {
                    case DocLexElement.LexTypeEnum.word:
                        subtxt += lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        if (inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            subtxt += "*" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(subtxt) && lex.SpaceCountAtEnd>0) //do we have a list element here
                            {
                                answ += string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                                            DocumentNormalFontSize,
                                            DocumentFontFamily,
                                            TextMarkerStyle.Disc,
                                            GetListElementsText(lexes, i, '*', out offset));
                                i += offset;
                            }
                            else //no we don't
                            {
                                if (!inparmode.HasFlag(InParamodeEnum.emphasize))
                                {
                                    inparmode |= InParamodeEnum.emphasize;
                                    subtxt += "<Italic>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                                }
                                else
                                {
                                    subtxt += "</Italic>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                                    inparmode &= ~InParamodeEnum.emphasize;
                                }
                            }
                        }
                        break;

                    case DocLexElement.LexTypeEnum.number:
                        //are we at a start of a line or paragraph
                        //AND the numbe rist followed by at least one space
                        //THEN we have a numbered list to be processed,
                        //ELSE we have a simple number in the middle of a pragraph or something else
                        if (string.IsNullOrEmpty(subtxt) && lex.SpaceCountAtEnd > 0 && lex.Text.EndsWith("."))
                        {
                            answ += string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                                            DocumentNormalFontSize,
                                            DocumentFontFamily,
                                            TextMarkerStyle.Decimal,
                                            GetNumberedListElementsText(lexes, i, '0', out offset));
                            i += offset;
                        }
                        else
                        {
                            subtxt += lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        }
                        break;

                    case DocLexElement.LexTypeEnum.codeinline:
                        if (!inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            inparmode |= InParamodeEnum.inlinecode;
                            subtxt += "<Span  xml:space=\"preserve\" FontFamily=\"Lucida Sans\">";
                        }
                        else
                        {
                            subtxt += "</Span>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inparmode &= ~InParamodeEnum.inlinecode;
                        }
                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        if (inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            subtxt += "**" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        }
                        else
                        {
                            if (!inparmode.HasFlag(InParamodeEnum.bold))
                            {
                                inparmode |= InParamodeEnum.bold;
                                subtxt += "<Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd); ;
                            }
                            else
                            {
                                subtxt += "</Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                                inparmode &= ~InParamodeEnum.bold;
                            }
                        }
                        break;

                    case DocLexElement.LexTypeEnum.boldemphasize:
                        if (inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            subtxt += "***" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        }
                        else
                        {
                            if (!inparmode.HasFlag(InParamodeEnum.bold | InParamodeEnum.emphasize))
                            {
                                inparmode |= InParamodeEnum.bold | InParamodeEnum.emphasize;
                                subtxt += "<Bold><Italic>" + GetStringOf(" ", lex.SpaceCountAtEnd); ;
                            }
                            else
                            {
                                subtxt += "</Italic></Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                                inparmode &= ~InParamodeEnum.bold & ~InParamodeEnum.emphasize;
                            }
                        }
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        if (inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            subtxt += "_" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        }
                        else
                        {
                            if (!inparmode.HasFlag(InParamodeEnum.underline))
                            {
                                inparmode |= InParamodeEnum.underline;
                                subtxt += "<Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd); ;
                            }
                            else
                            {
                                subtxt += "</Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                                inparmode &= ~InParamodeEnum.underline;
                            }
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
                            subtxt += " >" + GetStringOf(" ", lex.SpaceCountAtEnd);

                        break;
                }
            }

            return start + answ + "</FlowDocument>";
        }



        /// <summary>
        /// Fetch the floaw document of all the numbered list elements
        /// </summary>
        /// <param name="lexes">List of lexical elements to partly process</param>
        /// <param name="pos">Position in the lexes list where to start processing</param>
        /// <param name="listc">The list marker character that was foun in the first element</param>
        /// <param name="offset">The offset for the porition to continue processing in the calling process</param>
        /// <returns>FLow document xaml of the list elements</returns>
        private string GetNumberedListElementsText(List<DocLexElement> lexes, int pos, char listc, out int offset)
        {
            string subtxt = "";
            string answ = "";
            offset = 0;
            DocLexElement lex;
            bool stop = false;
            bool inbold = false,
                inunderline = false,
                initalic = false;

            for (int i = pos; !stop && (i < lexes.Count); i++)
            {
                lex = lexes[i];
                switch (lex.Type)
                {
                    case DocLexElement.LexTypeEnum.parabreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.word:
                        subtxt += lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;


                    case DocLexElement.LexTypeEnum.greaterthan:
                        subtxt += ">" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        if (!initalic)
                        {
                            subtxt += "<Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            initalic = true;
                        }
                        else
                        {
                            subtxt += "</Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            initalic = false;
                        }

                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        if (!inbold)
                        {
                            subtxt += "<Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inbold = true;
                        }
                        else
                        {
                            subtxt += "</Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inbold = false;
                        }
                        break;

                    case DocLexElement.LexTypeEnum.number:
                        if(!string.IsNullOrEmpty(subtxt) || !lex.Text.EndsWith("."))
                            subtxt += lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        if (!inunderline)
                        {
                            subtxt += "<Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inunderline = true;
                        }
                        else
                        {
                            subtxt += "</Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inunderline = false;
                        }
                        break;

                }

                offset++;

            }

            //handle the end of doc with this
            if (!string.IsNullOrEmpty(subtxt))
                answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);

            return answ;
        }


        /// <summary>
        /// Fetch the flow doc content of a list element
        /// </summary>
        /// <param name="lexes">List of lexical elements to partly process</param>
        /// <param name="pos">Position in the lexes list where to start processing</param>
        /// <param name="offset">The offset for the porition to continue processing in the calling process</param>
        /// <returns>FLow document xaml of the list elements</returns>
        private string GetListElementsText(List<DocLexElement> lexes, int pos, char listc, out int offset)
        {
            string subtxt = "";
            string answ = "";
            offset = 0;
            DocLexElement lex;
            bool stop = false;
            bool inbold = false,
                inunderline = false;

            for (int i = pos; !stop && (i < lexes.Count); i++)
            {
                lex = lexes[i];
                switch (lex.Type)
                {
                    case DocLexElement.LexTypeEnum.parabreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.word:
                        subtxt += lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.number:
                        subtxt = lex.Text + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.greaterthan:
                        subtxt += ">" + GetStringOf(" ", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        if(!string.IsNullOrEmpty(subtxt))
                            subtxt += "*" + GetStringOf(" ", lex.SpaceCountAtEnd); 

                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        if (!inbold)
                        {
                            subtxt += "<Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inbold = true;
                        }
                        else
                        {
                            subtxt += "</Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inbold = false;
                        }
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        if (!inunderline)
                        {
                            subtxt += "<Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inunderline = true;
                        }
                        else
                        {
                            subtxt += "</Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd); 
                            inunderline = false;
                        }
                        break;

                }

                offset++;

            }

            //handle the end of doc with this
            if(!string.IsNullOrEmpty(subtxt))
                answ += string.Format("<ListItem><Paragraph>{0}</Paragraph></ListItem>",
                                        subtxt);

            return answ;
        }


        /// <summary>
        /// check if the next lex element is or contains a valid list element start
        /// </summary>
        /// <param name="lexes">List of the lexical elements</param>
        /// <param name="pos">position where the test should be applied</param>
        /// <param name="testfor">Start element to test for</param>
        /// <returns></returns>
        private bool IsListElementStart(List<DocLexElement> lexes, int pos, char testfor)
        {
            if (pos >= lexes.Count)
                return false;
            char tstc;
            switch (lexes[pos].Type)
            {
                case DocLexElement.LexTypeEnum.emphasize:
                    tstc = '*';
                    break;
                case DocLexElement.LexTypeEnum.word:
                    tstc = lexes[pos].Text.First();
                    break;
                case DocLexElement.LexTypeEnum.number:
                    tstc = lexes[pos].Text.First();
                    break;
                default:
                    return false;
            }

            bool answ = false;

            switch (testfor)
            {
                case '*':
                    answ = tstc == testfor;
                    break;
                case '0':
                    answ = char.IsDigit(tstc);
                    break;
            }

            return answ;
        }


        /// <summary>
        /// Get the contents of a quotation (>......)
        /// </summary>
        /// <param name="lexes">The list of lexical elements delivered by the lexical analyser</param>
        /// <param name="pos">Current position, where reading of contents should start</param>
        /// <param name="offset">returns the offset i.e where the caller should procees to process lexical elements</param>
        /// <returns>The flow document for the quotation</returns>
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
                            answ += "</Bold>" + GetStringOf(" ", lex.SpaceCountAtEnd);
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
                            answ += "</Underline>" + GetStringOf(" ", lex.SpaceCountAtEnd);
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
                    case DocLexElement.LexTypeEnum.number:
                        answ += lex.Text;
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
        /// Set the font size from the heading level. Note, font sizes can be setup in
        /// JournalWriter.exe.config and are transferred to properties
        /// of this method's class.
        /// </summary>
        /// <param name="level">heading level for the font</param>
        /// <returns>The font size for the given level of a heading</returns>
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
