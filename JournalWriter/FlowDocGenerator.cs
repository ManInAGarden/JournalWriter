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
        public string TextAlignment { get; set; }


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
                        subtxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        if (inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            subtxt += GetTextAndSpaces("*", lex.SpaceCountAtEnd);
                        }
                        else
                        {
                            if (AtBeginningOfLine(lexes, i-1) && lex.SpaceCountAtEnd>0) //do we have a list element here
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
                                subtxt += GetInlineFormat(lex, ref inparmode, lexes, i);
                            }
                        }
                        break;

                    case DocLexElement.LexTypeEnum.cellstart:
                        if (!AtBeginningOfLine(lexes, i - 1))
                            subtxt += GetTextAndSpaces("|", lex.SpaceCountAtEnd);
                        else
                        {
                            answ += string.Format("\n<Table>{0}</Table>\n",
                                GetTableText(lexes, i-1, out offset));

                            i += offset;
                        }
                        break;

                    case DocLexElement.LexTypeEnum.minus:
                        if (AtBeginningOfLine(lexes, i - 1) && lex.SpaceCountAtEnd > 0) //do we have a list element here
                        {
                            answ += string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                                        DocumentNormalFontSize,
                                        DocumentFontFamily,
                                        TextMarkerStyle.None,
                                        GetListElementsText(lexes, i, '-', out offset));
                            i += offset;
                        }
                        else //no we don't
                        {
                            subtxt += GetTextAndSpaces("-", lex.SpaceCountAtEnd);
                        }
                        break;

                    case DocLexElement.LexTypeEnum.plus:
                        if (AtBeginningOfLine(lexes, i - 1) && lex.SpaceCountAtEnd > 0) //do we have a list element here
                        {
                            answ += string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                                        DocumentNormalFontSize,
                                        DocumentFontFamily,
                                        TextMarkerStyle.Box,
                                        GetListElementsText(lexes, i, '+', out offset));
                            i += offset;
                        }
                        else //no we don't
                        {
                            subtxt += GetTextAndSpaces("+", lex.SpaceCountAtEnd);
                        }
                        break;

                    case DocLexElement.LexTypeEnum.enumeration:
                        //Enumerations must start at the beginning of a line
                        if (AtBeginningOfLine(lexes, i-1))
                        {
                            TextMarkerStyle tms = GetTmsFrom(lex.Text);
                            answ += string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                                            DocumentNormalFontSize,
                                            DocumentFontFamily,
                                            tms,
                                            GetNumberedListElementsText(lexes, i, tms, out offset));
                            i += offset;
                        }
                        else
                        {
                            subtxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        }
                        break;

                    case DocLexElement.LexTypeEnum.codeinline:
                        if (!inparmode.HasFlag(InParamodeEnum.inlinecode))
                        {
                            inparmode |= InParamodeEnum.inlinecode;
                            subtxt += string.Format("<Span  xml:space=\"preserve\" FontFamily=\"{0}\">",
                                CodingFontFamily);
                        }
                        else
                        {
                            subtxt += "</Span>" + GetStringOf(" ", lex.SpaceCountAtEnd);
                            inparmode &= ~InParamodeEnum.inlinecode;
                        }
                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        subtxt += GetInlineFormat(lex, ref inparmode, lexes, i);
                        break;

                    case DocLexElement.LexTypeEnum.boldemphasize:
                        subtxt += GetInlineFormat(lex, ref inparmode, lexes, i);
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        subtxt += GetInlineFormat(lex, ref inparmode, lexes, i);
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:
                        subtxt += "<LineBreak />";
                        break;

                    case DocLexElement.LexTypeEnum.headafter:
                        if (!string.IsNullOrWhiteSpace(subtxt))
                        {
                            
                            answ += string.Format("<Paragraph TextAlignment=\"{3}\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                                GetFontSizeFromHeadLevel(lex.Level),
                                subtxt,
                                DocumentFontFamily,
                                TextAlignment);
                        }
                        subtxt = "";
                        break;

                    case DocLexElement.LexTypeEnum.hashes:
                        if (AtBeginningOfLine(lexes, i-1))
                        {
                            subtxt = GetHashesContent(lexes, i-1, out offset);
                            if (!string.IsNullOrWhiteSpace(subtxt))
                            {

                                answ += string.Format("<Paragraph TextAlignment=\"{3}\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                                    GetFontSizeFromHeadLevel(lex.Level),
                                    subtxt,
                                    DocumentFontFamily,
                                    TextAlignment);
                            }
                            subtxt = "";
                            i += offset;
                        }
                        else
                        {
                            subtxt += GetTextAndSpaces("#", lex.SpaceCountAtEnd);
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
                        if (AtBeginningOfLine(lexes, i-1))
                        {
                            answ += GetQuotationText(lexes, i, out offset);
                            i += offset;
                        }
                        else
                            subtxt += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);

                        break;
                }
            }

            return start + answ + "</FlowDocument>";
        }

        /// <summary>
        /// Create the flow doc code for a table created out of lexical definitions from a
        /// startposition up to a pragraph end
        /// </summary>
        /// <param name="lexes"></param>
        /// <param name="startpos"></param>
        /// <param name="offset"></param>
        /// <returns>Code for table represented by the lexcal elements in lexes</returns>
        private string GetTableText(List<DocLexElement> lexes, int startpos, out int offset)
        {
            string celltxt = "",
                rowtxt = "";
            string currAlignment = null;
            string answ = "";
            offset = 0;
            DocLexElement lex;
            bool stop = false;
            int colnum = 0;
            int rownum = 0;
            List<string> colalignments = new List<string>();
            List<int> maxcollengths = new List<int>();
            InParamodeEnum ipm = InParamodeEnum.none;
           
            for (int i = startpos; !stop && (i < lexes.Count); i++)
            {
                lex = lexes[i];
                switch (lex.Type)
                {
                    case DocLexElement.LexTypeEnum.parabreak:
                        answ += AddTableLine(ref rowtxt, ref rownum, ref colnum);
                        answ += "</TableRowGroup>";
                        stop = true;
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:

                        if (rownum == 0)
                        {
                            answ += "<TableRowGroup>";
                        }

                        answ += AddTableLine(ref rowtxt, ref rownum, ref colnum);
                        break;

                    case DocLexElement.LexTypeEnum.cellstart:
                        if (colnum > 0)
                        {
                            if (rownum == 0)
                            {
                                rowtxt += string.Format("<TableCell BorderThickness=\"0,0,0,1\" BorderBrush=\"SkyBlue\"><Paragraph FontSize=\"{3}\" FontFamily=\"{2}\" TextAlignment=\"{1}\">{0}</Paragraph></TableCell>",
                                    celltxt,
                                    "####COLALIGN####" + (colnum - 1) + "#",
                                    DocumentFontFamily,
                                    DocumentNormalFontSize);

                                DoMaxColWidth(maxcollengths, colnum - 1, celltxt.Length);
                            }
                            else if (rownum == 1)
                            {
                                currAlignment = GetAlignment(celltxt);
                                colalignments.Add(currAlignment);
                            }
                            else
                            {
                                rowtxt += string.Format("<TableCell><Paragraph FontSize=\"{3}\" FontFamily=\"{2}\" TextAlignment=\"{1}\">{0}</Paragraph></TableCell>",
                                    celltxt,
                                    "####COLALIGN####" + (colnum - 1) + "#",
                                    DocumentFontFamily,
                                    DocumentNormalFontSize);

                                DoMaxColWidth(maxcollengths, colnum - 1, celltxt.Length);
                            }
                        }
                        colnum++;
                        celltxt = "";
                        break;

                    case DocLexElement.LexTypeEnum.word:
                        celltxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.greaterthan:
                        celltxt += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        celltxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        celltxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        celltxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                    case DocLexElement.LexTypeEnum.minus:
                        celltxt += GetTextAndSpaces("-", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.plus:
                        celltxt += GetTextAndSpaces("+", lex.SpaceCountAtEnd);
                        break;

                }

                offset++;

            }

            colnum = 0;
            foreach (string colali in colalignments)
            {
                answ = answ.Replace("####COLALIGN####" + colnum++ + "#", colali);
            }

            return string.Format("{0}\n{1}", GetColumSection(maxcollengths), answ);
        }

        /// <summary>
        /// Create the col definitions with percentage rated width of the flow document table
        /// to be placed at the top of the table definition
        /// </summary>
        /// <param name="maxcollengths">A list of maximum col lengths</param>
        /// <returns>A string containing the col definitions</returns>
        private object GetColumSection(List<int> maxcollengths)
        {
            string colspecs = "";

            foreach (int maxl in maxcollengths)
            {
                colspecs += string.Format("<TableColumn Width=\"{0}*\"/>", maxl);
            }

            return string.Format("<Table.Columns>{0}</Table.Columns>", colspecs);
        }

        /// <summary>
        /// Dynamically fill up an initially empty list of maximum columns widths for the table
        /// </summary>
        /// <param name="maxcollengths">The list to be built</param>
        /// <param name="colnum">The current column index</param>
        /// <param name="length">The length of a text filled into one of the lines of the column</param>
        private void DoMaxColWidth(List<int> maxcollengths, int colnum, int length)
        {
            while (colnum >= maxcollengths.Count)
            {
                maxcollengths.Add(0);
            }

            if (length > maxcollengths[colnum])
                maxcollengths[colnum] = length;
        }

        /// <summary>
        /// Returns a row definition for a table filled with a given rowtext (the cell defs)
        /// </summary>
        /// <param name="rowtxt">The cell definitions already in flow document syntax</param>
        /// <param name="rownum">The current rownum, is incremented internally</param>
        /// <param name="colnum">The current colnum, is reset to 0 internally</param>
        /// <returns></returns>
        private string AddTableLine(ref string rowtxt, ref int rownum, ref int colnum)
        {
            string row = string.Format("<TableRow>{0}</TableRow>\n",
                                          rowtxt);

            rownum++;
            colnum = 0;
            rowtxt = "";

            return row;
        }

        /// <summary>
        /// Get the alignment for the current column. |:---... or ...----:| or |:---....---:| is acceptet.
        /// </summary>
        /// <param name="celltext">Celltext</param>
        /// <returns>Alignment for the current cell as a string Right, Left, or Center. Standard alignment when no alignment info can be dervive from celltext</returns>
        private string GetAlignment(string celltext)
        {
            string answ;
            if (celltext.StartsWith(":") && celltext.EndsWith(":"))
                answ = "Center";
            else if (celltext.StartsWith(":"))
                answ = "Left";
            else if (celltext.EndsWith(":"))
                answ = "Right";
            else
                answ = TextAlignment;

            return answ;
        }

        /// <summary>
        /// Gets a text marker style from the given text. When it starts with a digit
        /// the marker style ist Decimal, if its a capital letter the marker style is
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private TextMarkerStyle GetTmsFrom(string text)
        {
            TextMarkerStyle answ = TextMarkerStyle.UpperRoman;

            if (text.Length < 1)
                throw new ApplicationException("Enumeration ohne Index-Text");

            if (char.IsDigit(text[0]))
                answ = TextMarkerStyle.Decimal;

            if (char.IsLetter(text[0]))
            {
                if (char.ToUpper(text[0]) == text[0])
                    answ = TextMarkerStyle.UpperLatin;
                else if(char.ToLower(text[0])==text[0])
                    answ = TextMarkerStyle.LowerLatin;
            }


            return answ;
        }


        /// <summary>
        /// Process an inline formatting element like bold, emphasize, underline
        /// </summary>
        /// <param name="lex"></param>
        /// <param name="inparmode"></param>
        /// <param name="lexes"></param>
        /// <param name="currPos"></param>
        /// <returns></returns>
        private string GetInlineFormat(DocLexElement lex, ref InParamodeEnum inparmode, List<DocLexElement> lexes, int currPos)
        {
            string answ = "";
            InParamodeEnum thisflag;
            string rawtext;
            string onflow, outflow;
            switch (lex.Type)
            {
                case DocLexElement.LexTypeEnum.underline:
                    rawtext = "_";
                    thisflag = InParamodeEnum.underline;
                    onflow = "<Underline>";
                    outflow = "</Underline>";
                    break;
                case DocLexElement.LexTypeEnum.bold:
                    rawtext = "**";
                    thisflag = InParamodeEnum.bold;
                    onflow = "<Bold>";
                    outflow = "</Bold>";
                    break;
                case DocLexElement.LexTypeEnum.boldemphasize:
                    rawtext = "***";
                    thisflag = InParamodeEnum.bold | InParamodeEnum.emphasize;
                    onflow = "<Bold><Italic>";
                    outflow = "</Italic></Bold>";
                    break;
                case DocLexElement.LexTypeEnum.emphasize:
                    rawtext = "*";
                    thisflag = InParamodeEnum.emphasize;
                    onflow = "<Italic>";
                    outflow = "</Italic>";
                    break;
                default:
                    throw new ApplicationException("Unknown inline style");
            }


            if (inparmode.HasFlag(InParamodeEnum.inlinecode))
            {
                answ = GetTextAndSpaces("_", lex.SpaceCountAtEnd);
            }
            else
            {
                if (!inparmode.HasFlag(thisflag))
                {
                    if (CanBeFoundBefore(lexes,
                        currPos,
                        lex.Type,
                        DocLexElement.LexTypeEnum.parabreak))
                    {
                        inparmode |= thisflag;
                        answ = GetTextAndSpaces(onflow, lex.SpaceCountAtEnd);
                    }
                    else
                        answ = GetTextAndSpaces(rawtext, lex.SpaceCountAtEnd);
                }
                else
                {
                    answ = GetTextAndSpaces(outflow, lex.SpaceCountAtEnd);
                    inparmode &= ~thisflag;
                }
            }

            return answ;
        }

        /// <summary>
        /// Checks wether a lexical element can be found before another lexical element occurs
        /// </summary>
        /// <param name="lexes">List of lexical elements to be searched</param>
        /// <param name="pos">Position from where on the search shall be processed</param>
        /// <param name="searchElement">Element to search in lexes</param>
        /// <param name="endElement"></param>
        /// <returns>true when the searched element was found before the end element occurs</returns>
        private bool CanBeFoundBefore(List<DocLexElement> lexes, 
            int pos,
            DocLexElement.LexTypeEnum searchElement, DocLexElement.LexTypeEnum endElement)
        {
            int i = pos;
            bool found = false;
            while (i < lexes.Count && !found && lexes[i].Type != endElement)
            {
                found = lexes[i].Type == searchElement;
                i++;
            }

            return found;
        }

        /// <summary>
        /// Test wether the current lex is the first one on a new line
        /// </summary>
        /// <param name="lexes">List of lexical elements</param>
        /// <param name="i">Current position</param>
        /// <returns></returns>
        private bool AtBeginningOfLine(List<DocLexElement> lexes, int i)
        {
            if (i <= 0)
                return true;

            return lexes[i - 1].Type == DocLexElement.LexTypeEnum.linebreak || lexes[i - 1].Type == DocLexElement.LexTypeEnum.parabreak;
        }



        /// <summary>
        /// Fetch the floaw document of all the numbered list elements
        /// </summary>
        /// <param name="lexes">List of lexical elements to partly process</param>
        /// <param name="pos">Position in the lexes list where to start processing</param>
        /// <param name="tms">The list marker style derived from the first element of the enumerated list</param>
        /// <param name="offset">The offset for the porition to continue processing in the calling process</param>
        /// <returns>FLow document xaml of the list elements</returns>
        private string GetNumberedListElementsText(List<DocLexElement> lexes, int pos, TextMarkerStyle tms, out int offset)
        {
            string subtxt = "";
            string answ = "";
            offset = 0;
            DocLexElement lex;
            bool stop = false;
            InParamodeEnum ipm = InParamodeEnum.none;
            char listc;

            switch (tms)
            {
                case TextMarkerStyle.Decimal:
                    listc = '0';
                    break;
                case TextMarkerStyle.LowerLatin:
                    listc = 'a';
                    break;
                case TextMarkerStyle.UpperLatin:
                    listc = 'A';
                    break;
                case TextMarkerStyle.UpperRoman:
                    listc = 'I';
                    break;
                default:
                    throw new ApplicationException("Unknown list marker style");
            }

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
                            answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.word:
                        subtxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.cellstart:
                        subtxt += GetTextAndSpaces("|", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.greaterthan:
                        subtxt += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;
                        
                    case DocLexElement.LexTypeEnum.underline:
                        subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                }

                offset++;

            }

            //handle the end of doc with this
            if (!string.IsNullOrEmpty(subtxt))
                answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);

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
            InParamodeEnum ipm = InParamodeEnum.none;

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
                            answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.linebreak:
                        if (!IsListElementStart(lexes, i + 1, listc))
                            stop = true;
                        else
                        {
                            answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);
                            subtxt = "";
                        }
                        break;

                    case DocLexElement.LexTypeEnum.word:
                        subtxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.number:
                        subtxt += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.cellstart:
                        subtxt += GetTextAndSpaces("|", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.greaterthan:
                        subtxt += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);
                        break;

                    case DocLexElement.LexTypeEnum.emphasize:
                        if(!string.IsNullOrEmpty(subtxt))
                            subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);

                        break;

                    case DocLexElement.LexTypeEnum.boldemphasize:
                        if (!string.IsNullOrEmpty(subtxt))
                            subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);

                        break;

                    case DocLexElement.LexTypeEnum.bold:
                        subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                    case DocLexElement.LexTypeEnum.underline:
                        subtxt += GetInlineFormat(lex, ref ipm, lexes, i + 1);
                        break;

                }

                offset++;

            }

            //handle the end of doc with this
            if(!string.IsNullOrEmpty(subtxt))
                answ += string.Format("<ListItem><Paragraph TextAlignment=\"{1}\">{0}</Paragraph></ListItem>",
                                        subtxt,
                                        TextAlignment);

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
                case DocLexElement.LexTypeEnum.plus:
                    tstc = '+';
                    break;
                case DocLexElement.LexTypeEnum.minus:
                    tstc = '-';
                    break;
                case DocLexElement.LexTypeEnum.word:
                    tstc = lexes[pos].Text.First();
                    break;
                case DocLexElement.LexTypeEnum.enumeration:
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
                case '+':
                    answ = tstc == testfor;
                    break;
                case '-':
                    answ = tstc == testfor;
                    break;
                case '0':
                    answ = char.IsDigit(tstc);
                    break;
                case 'a':
                    answ = char.IsLetter(tstc) && (char.ToLower(tstc) == tstc);
                    break;
                case 'A':
                    answ = char.IsLetter(tstc) && (char.ToUpper(tstc) == tstc);
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
                        answ += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.greaterthan:
                        answ += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.plus:
                        answ += GetTextAndSpaces("+", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.minus:
                        answ += GetTextAndSpaces("-", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.emphasize:
                        answ += GetTextAndSpaces("*", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.hashes:
                        answ += GetTextAndSpaces(GetStringOf("#", lex.Level), lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.bold:
                        if (!inbold)
                        {
                            answ += "<Bold>";
                            inbold = true;
                        }
                        else
                        {
                            answ += GetTextAndSpaces("</Bold>", lex.SpaceCountAtEnd);
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
                            answ += GetTextAndSpaces("</Underline>", lex.SpaceCountAtEnd);
                            inunderline = false;
                        }
                        break;
                }

                offset++;

                if (stop)
                    break;
            }

            if (!string.IsNullOrEmpty(answ))
                return string.Format("<Paragraph Margin=\"50,0,30,0\" FontSize=\"{1}\" FontFamily=\"{2}\" FontStyle=\"Italic\" TextAlignment=\"{3}\">{0}</Paragraph>",
                    answ,
                    DocumentNormalFontSize,
                    DocumentFontFamily,
                    TextAlignment);
            else
                return "";
        }

        private string GetTextAndSpaces(string text, int ct)
        {
            return text + GetStringOf(" ", ct);
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
                        answ += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.linebreak:
                        answ += "\n";
                        break;
                    case DocLexElement.LexTypeEnum.cellstart:
                        answ += GetTextAndSpaces("|", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.codeinline:
                        answ += GetTextAndSpaces("`", lex.SpaceCountAtEnd);
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
                    case DocLexElement.LexTypeEnum.greaterthan:
                        answ += "&gt;";
                        break;
                    case DocLexElement.LexTypeEnum.minus:
                        answ += "-";
                        break;
                    case DocLexElement.LexTypeEnum.plus:
                        answ += "+";
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
                return string.Format("<Paragraph TextAlignment=\"{3}\" FontSize=\"{0}\" FontFamily=\"{2}\">{1}</Paragraph>\n\n",
                    DocumentNormalFontSize,
                    paratxt,
                    DocumentFontFamily,
                    TextAlignment);

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
            for(int i=start+1; i<lexes.Count; i++)
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
                    case DocLexElement.LexTypeEnum.cellstart:
                        answ += "|";
                        break;
                    case DocLexElement.LexTypeEnum.minus:
                        answ += GetTextAndSpaces("-", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.emphasize:
                        answ += GetTextAndSpaces("*", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.bold:
                        answ += GetTextAndSpaces("**", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.boldemphasize:
                        answ += GetTextAndSpaces("***", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.underline:
                        answ += GetTextAndSpaces("_", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.codeinline:
                        answ += GetTextAndSpaces("`", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.plus:
                        answ += GetTextAndSpaces("+", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.greaterthan:
                        answ += GetTextAndSpaces("&gt;", lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.hashes:
                        answ += GetTextAndSpaces(GetStringOf("#", lex.Level), lex.SpaceCountAtEnd);
                        break;
                    case DocLexElement.LexTypeEnum.number:
                        answ += GetTextAndSpaces(lex.Text, lex.SpaceCountAtEnd);
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
