using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xaml;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace JournalWriter
{
    public class MarkdownToXaml
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


        public Dictionary<string, Regex> Regexes { get; set; }
       
        static private RegexOptions standardOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;
        

        public MarkdownToXaml()
        {
            //Regexes = new Dictionary<string, Regex>();
            //AddTheRexes(Regexes);
        }


        /// <summary>
        /// Add all regular expressions to the dict of regular expressions for later use
        /// </summary>
        /// <param name="regexes">Dictionary to be filled</param>
        //private void AddTheRexes(Dictionary<string, Regex> rxs)
        //{
        //    rxs.Clear();

        //    //bold text
        //    AddRex(rxs, "boldRex",
        //        @"(?<start>(\s|\n|\?|\.|,|;|!))\*\*",   //Zeilenschaltung oder Whitespace oder Worttrennzeichen mit Aufzeichnung gefolgt von 2 Strenchen
        //        @"(?<test>[^*]+)",                      //Was zwischen den Sternchen ist
        //        @"\*\*(?<ende>(\s|\n|\?|\.|,|;|!))");   //gefolgt von 2 Sternchen und einer Zeilenschaltung oder einem Whitespace das wir aufzeichnen"

        //    //italic text
        //    AddRex(rxs, "italicRex", @"(?<start>(\s|\n|\?|\.|,|;|!))\*(?<test>[^*]+)\*(?<ende>(\s|\n|\?|\.|,|;|!))");

        //    //single underline text
        //    AddRex(rxs, "singleUnderlineRex", @"(?<start>(\s|\n|\?|\.|,|;|!))_(?<test>[^_]+)_(?<ende>(\s|\n|\?|\.|,|;|!))");


        //    //first level # title
        //    AddRex(rxs, "headline1", @"^\#+\s+(?<test>.*)[\n]");
        //    //second level alternative title
        //    AddRex(rxs, "alt-headline1", @"^(?<test>.*)",   //Zeilenanfang gefolgt von beliebigem Zeugs außer einem Zeilenende
        //                             @"[\n][=]{3,}",        //Zeilenende gefolgt von mindestens 3 Gleicheitszeichen
        //                             @"$");                 //Und dann noch ein Zeilenende"

        //    //second level ##-Titles
        //    AddRex(rxs, "headline2", @"^\#{2}\s+(?<test>.*)[\n]");
        //    //second level alternative title
        //    AddRex(rxs, "alt-headline2", @"^(?<test>.*)",   //Zeilenanfang gefolgt von beliebigem Zeugs außer einem Zeilenende
        //                             @"[\n][-]{3,}",        //Zeilenende gefolgt von mindestens 3 Minuszeichen
        //                             @"$");                 //Und dann noch ein Zeilenende

        //    //und alle weiteren Titelebenen die aber nur mit der #-Methode definiert sind
        //    AddRex(rxs, "headline3", @"^\#{3}\s+(?<test>.*)[\n]");
        //    AddRex(rxs, "headline4", @"^\#{4}\s+(?<test>.*)[\n]");
        //    AddRex(rxs, "headline5", @"^\#{5}\s+(?<test>.*)[\n]");

        //    //bullet list
        //    AddRex(rxs, "bulletListRex", @"(?<test>(^\s*\*\s.+\n)+)");  //Zeilenanfang gefolgt von mindestens einem Listenelement dann egal was
        //    AddRex(rxs, "bulletRex", @"^\s*\*\s(?<test>.*)$");          //Zeilenanfang gefolgt von beliebigen Leerzeichen und einem Stern und einem Leerzeichen
        //                                                                //bullet list
        //    //bullet list with circles
        //    AddRex(rxs, "circBulletListRex", @"(?<test>(^\s*[o]\s.+\n)+)");  //Zeilenanfang gefolgt von mindestens einem Listenelement dann egal was
        //    AddRex(rxs, "circBulletRex", @"^\s*[o]\s(?<test>.*)$");          //Zeilenanfang gefolgt von beliebigen Leerzeichen und einem kleinen o und einem Leerzeichen

        //    //numbered list
        //    AddRex(rxs, "numberedListRex", 
        //        @"(?<test>",                            //wwap everything into test
        //        @"(^[\s]*[0-9]+\.\s.*\n)+",             //definition for single list entry
        //        @")");                                  //closing braces for test capture
        //    AddRex(rxs, "numberedRex", @"^[\s]*[0-9]+\.\s(?<test>.*)$");   //Zeilenanfang gefolgt von beliebigen Leerzeichen und einem Stern und einem Leerzeichen

        //    //numbered list with letters
        //    AddRex(rxs, "letteredListRex",
        //        @"(?<test>",                            //wwap everything into test
        //        @"(^[\s]*[a-z]+\.\s.*\n)+",             //definition for single list entry
        //        @")");                                  //closing braces for test capture
        //    AddRex(rxs, "letteredRex", @"^[\s]*[a-z]+\.\s(?<test>.*)$");   //Zeilenanfang gefolgt von beliebigen Leerzeichen und einem Stern und einem Leerzeichen


        //    //numbered list with capital letters
        //    AddRex(rxs, "capitalLetteredListRex",
        //        @"(?<test>",                            //wwap everything into test
        //        @"(^[\s]*[A-Z]+\.\s.*\n)+",             //definition for single list entry
        //        @")");                                  //closing braces for test capture
        //    AddRex(rxs, "capitalLetteredRex", @"^[\s]*[A-Z]+\.\s(?<test>.*)$");   //Zeilenanfang gefolgt von beliebigen Leerzeichen und einem Stern und einem Leerzeichen


        //    //quotations
        //    AddRex(rxs, "quoteRex", @"(?<test>",            //Wrap whole match in {test}
        //        @"(",
        //        @"^[ ]*&gt;[ ]?",                           //'>' at the start of a line
        //        @".+\n",                                    //rest of the first line
        //        @"(.+\n)*",                                 //subsequent consecutive lines
        //        @"\n*",                                     //blanks
        //        @")+)");

        //    //coding starting and ending with a line of exactly three backticks (`)
        //    AddRex(rxs, "backtickCodingRex",
        //        @"^```[\n]+",
        //        @"(?<test>(.+\n)*)",
        //        @"```$");

        //    //programm code with TAB method
        //    AddRex(rxs, "codingRex", @"(?<test>",           //Wrap whole match in {test}
        //        @"(",
        //        @"^([ ]{3}|\t)",                            //at exactly three spaces or a tab character at the start of a line
        //        @".+\n",                                    //rest of the first line
        //        @"(.+\n)*",                                 //subsequent consecutive lines
        //        @"\n *",
        //        @")+)");

        //    //paragraphs
        //    AddRex(rxs, "paraRex", 
        //        @"(?<test>",                                //Wrap whole match in {test}
        //            @"(",                                        //opening brace for wrap
        //               @"^(?!<Paragraph)+",                         //nothing in front of first line that does not start with <Paragraph>
        //               @"(.+\n)+",                                  //subsequent consecutive lines
        //               @"\n+",                                      //at least one newline
        //            @")",                                        //closing brace for wrap
        //        @")");                                      //closing brace for <test> contents
        //}


        //private void AddRex(Dictionary<string, Regex> rx, string name, params string[] parts)
        //{
        //    string complete = "";

        //    foreach (string part in parts)
        //        complete += part;

        //    rx.Add(name, new Regex(complete, standardOptions));
        //}

        /// <summary>
        /// Das FlowDocument aus dem Markup-Text aufbauen
        /// </summary>
        /// <param name="text">Text in Markup-Form</param>
        /// <returns>Test als FlowDocument formatiert</returns>
        public string FormatDocument(string text)
        {
            MarkDownLexicalAnalyzer ana = new MarkDownLexicalAnalyzer(text + "\n\n");

            List<DocLexElement> lex = ana.GetDocLexList();
           


            FlowDocGenerator fdg = new FlowDocGenerator();
            fdg.DocumentFontFamily = DocumentFontFamily;
            fdg.DocumentNormalFontSize = DocumentNormalFontSize;
            fdg.DocumentHeadline1FontSize = DocumentHeadline1FontSize;
            fdg.DocumentHeadline2FontSize = DocumentHeadline2FontSize;
            fdg.DocumentHeadline3FontSize = DocumentHeadline3FontSize;
            fdg.DocumentHeadline4FontSize = DocumentHeadline4FontSize;
            fdg.DocumentHeadline5FontSize = DocumentHeadline5FontSize;
            fdg.CodingFontSize = CodingFontSize;
            fdg.CodingFontFamily = CodingFontFamily;
            fdg.TextAlignment = TextAlignment;

            return fdg.ProduceDoc(lex);

        }



        /// <summary>
        /// Das FlowDocument aus dem Markup-Text aufbauen
        /// </summary>
        /// <param name="text">Text in Markup-Form</param>
        /// <returns>Test als FlowDocument formatiert</returns>
        public string OldFormatDocument(string text)
        {
            
            const string start = "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""
                + " xmlns:sys=\"clr-namespace:System;assembly=mscorlib\""
                + " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\n";

            //Vorarbeiten
            string answ = text.Replace("\r", "") + "\n\n";
            answ = answ.Replace("<", "&lt;");
            answ = answ.Replace(">", "&gt;");

            //Programmcode im Text
            answ = Regexes["backtickCodingRex"].Replace(answ, new MatchEvaluator(CodingEvaluator));
            answ = Regexes["codingRex"].Replace(answ, new MatchEvaluator(CodingEvaluator));

            //Listen erkennen
            //answ = bulletRex.Replace(answ, "&#42; ${test}"); alt und nicht mehr gebraucht. Ersetzt durch die beiden nächsten regexps
            answ = Regexes["bulletListRex"].Replace(answ, string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                DocumentNormalFontSize,
                DocumentFontFamily,
                TextMarkerStyle.Disc,
                "${test}"));
            answ = Regexes["bulletRex"].Replace(answ, "<ListItem><Paragraph>${test}</Paragraph></ListItem>");

            answ = Regexes["circBulletListRex"].Replace(answ, string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                DocumentNormalFontSize,
                DocumentFontFamily,
                TextMarkerStyle.Circle,
                "${test}"));
            answ = Regexes["circBulletRex"].Replace(answ, "<ListItem><Paragraph>${test}</Paragraph></ListItem>");

            answ = Regexes["numberedListRex"].Replace(answ, string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
                DocumentNormalFontSize,
                DocumentFontFamily,
                TextMarkerStyle.Decimal,
                "${test}"));
            answ = Regexes["numberedRex"].Replace(answ, "<ListItem><Paragraph>${test}</Paragraph></ListItem>");

            answ = Regexes["letteredListRex"].Replace(answ, string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
              DocumentNormalFontSize,
              DocumentFontFamily,
              TextMarkerStyle.LowerLatin,
              "${test}"));
            answ = Regexes["letteredRex"].Replace(answ, "<ListItem><Paragraph>${test}</Paragraph></ListItem>");

            answ = Regexes["capitalLetteredListRex"].Replace(answ, string.Format("\n<List FontSize=\"{0}\" FontFamily=\"{1}\" MarkerStyle=\"{2}\">\n{3}</List>\n",
             DocumentNormalFontSize,
             DocumentFontFamily,
             TextMarkerStyle.UpperLatin,
             "${test}"));
            answ = Regexes["capitalLetteredRex"].Replace(answ, "<ListItem><Paragraph>${test}</Paragraph></ListItem>");

            //fette, kursive und unterstrichene Textteile
            answ = Regexes["boldRex"].Replace(answ, "${start}<Bold>${test}</Bold>${ende}");
            answ = Regexes["italicRex"].Replace(answ, "${start}<Italic>${test}</Italic>${ende}");
            answ = Regexes["singleUnderlineRex"].Replace(answ, "${start}<Underline>${test}</Underline>${ende}");

            //Titel
            answ = Regexes["headline5"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
               DocumentHeadline5FontSize,
               "${test}",
               DocumentFontFamily));
            answ = Regexes["headline4"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
               DocumentHeadline4FontSize,
               "${test}",
               DocumentFontFamily));
            answ = Regexes["headline3"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
               DocumentHeadline3FontSize,
               "${test}",
               DocumentFontFamily));
            answ = Regexes["headline2"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
               DocumentHeadline2FontSize,
               "${test}",
               DocumentFontFamily));
            answ = Regexes["headline1"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                DocumentHeadline1FontSize,
                "${test}",
                DocumentFontFamily));
            answ = Regexes["alt-headline1"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                DocumentHeadline1FontSize,
                "${test}",
                DocumentFontFamily));
            answ = Regexes["alt-headline2"].Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n",
                DocumentHeadline2FontSize,
                "${test}",
                DocumentFontFamily));



            //Zitate
            answ = Regexes["quoteRex"].Replace(answ, new MatchEvaluator(BlockQuoteEvaluator));


            //dies hier am Schluss - wandelt die normalen Absätze um
            answ = Regexes["paraRex"].Replace(answ, new MatchEvaluator(ParagraphEvaluator));

            return start + answ + "</FlowDocument>";
        }

        private string BlockQuoteEvaluator(Match match)
        {
            string bq = match.Groups["test"].Value;

            bq = Regex.Replace(bq, @"^[ ]*&gt;[ ]?", "", RegexOptions.Multiline);       // trim one level of quoting
            bq = Regex.Replace(bq, @"^[ ]+$", "", RegexOptions.Multiline);           // trim whitespace-only lines

            bq = Regex.Replace(bq, @"\n\n", "", RegexOptions.Multiline);
            bq = Regex.Replace(bq, @"\n", "<LineBreak />", RegexOptions.Multiline);

            return string.Format("<Paragraph Margin=\"50,0,30,0\" FontSize=\"{1}\"  FontFamily=\"{2}\" FontStyle=\"Italic\">{0}</Paragraph>\n\n", 
                bq, 
                DocumentNormalFontSize,
                DocumentFontFamily);
        }


        private string CodingEvaluator(Match match)
        {
            string bq = match.Groups["test"].Value;

            bq = Regex.Replace(bq, @"^([ ]{3,}|\t{1})", "", RegexOptions.Multiline);    // trim one level of tabs or spaces
            bq = Regex.Replace(bq, @"^[ ]+$", "", RegexOptions.Multiline);           // trim whitespace-only lines
            bq = Regex.Replace(bq, @"\n\n", "", RegexOptions.Multiline);
            bq = Regex.Replace(bq, @"\n", "<LineBreak />", RegexOptions.Multiline);

            bq = Regex.Replace(bq, @"#", "&#35;", RegexOptions.Multiline); //Dies hier muss als erste Zeichenersetzung gemacht werden!!!!
            bq = Regex.Replace(bq, @"\*", "&#42;", RegexOptions.Multiline);
            bq = Regex.Replace(bq, @"_", "&#95;", RegexOptions.Multiline);
            return string.Format("<Paragraph xml:space=\"preserve\" TextAlignment=\"Left\" Margin=\"50,0,0,0\" FontSize=\"{1}\" FontFamily=\"Consolas\">{0}</Paragraph>\n\n", bq, DocumentNormalFontSize);
        }


        private string ParagraphEvaluator(Match match)
        {
            string bq = match.Groups["test"].Value;

            if (bq.StartsWith("<Paragraph") || bq.StartsWith("<List"))
                return bq;
            
            bq = Regex.Replace(bq, @"^[ ]+$", "", RegexOptions.Multiline);           // trim whitespace-only lines
            bq = Regex.Replace(bq, @"\n\n", "", RegexOptions.Multiline);
            bq = Regex.Replace(bq, @"\n", "<LineBreak />", RegexOptions.Multiline);

            return string.Format("<Paragraph FontSize=\"{1}\" FontFamily=\"{2}\">{0}</Paragraph>\n\n", 
                bq, 
                DocumentNormalFontSize,
                DocumentFontFamily);
        } 

        /// <summary>
        /// Get a FlowDocument from a text formatted with markdown elements
        /// </summary>
        /// <param name="text">The markdown text</param>
        /// <returns>The flow document</returns>
        public FlowDocument GetDocument(Window parent, string text)
        {
            if (DocumentFontFamily == null)
                DocumentFontFamily = "Times New Roman";
            if(DocumentNormalFontSize == null)
                DocumentNormalFontSize = "12pt";
            if (DocumentHeadline1FontSize == null)
                DocumentHeadline1FontSize = "26pt";
            if (DocumentHeadline2FontSize == null)
                DocumentHeadline2FontSize = "24pt";
            if (DocumentHeadline3FontSize == null)
                DocumentHeadline3FontSize = "22pt";
            if (DocumentHeadline4FontSize == null)
                DocumentHeadline4FontSize = "20pt";
            if (DocumentHeadline5FontSize == null)
                DocumentHeadline5FontSize = "18pt";
            if (CodingFontFamily == null)
                CodingFontFamily = "Lucida Sans";
            if (CodingFontSize == null)
                CodingFontSize = "12";

            FlowDocument doc = null;
            try
            {
                string xamlStr = FormatDocument(text);

                doc = LoadXaml(xamlStr);
            }
            catch (Exception exc)
            {
                Paragraph paragraph = new Paragraph();

                paragraph.Inlines.Add(text);

                doc = new FlowDocument(paragraph);

                MessageBox.Show(parent, "Text kann nicht formatiert werden. Origaltext der Meldung:\n" + exc.Message);
            }

            return doc;
        }


      

       
        private FlowDocument LoadXaml(string text)
        {
            return System.Windows.Markup.XamlReader.Parse(text) as FlowDocument;
        }

        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }
    }
}
