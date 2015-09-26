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
        public string DocumentHeadLine2FontSize { set; get; }

        static private RegexOptions standardOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;

        /// <summary>
        /// Bold-ersetzung **blabla** -> <Bold>blabla</Bold>
        /// </summary>
        private Regex boldRex = new Regex(@"(?<start>(\s|\n|\?|\.|,|;|!))\*\*   # Zeilenschaltung oder Whitespace oder Worttrennzeichen mit Aufzeichnung gefolgt von 2 Strenchen
                                            (?<test>[^*]+)                      # Was zwischen den Sternchen ist
                                            \*\*(?<ende>(\s|\n|\?|\.|,|;|!))    # Gefolgt von 2 Sternchen und einer Zeilenschaltung oder einem Whitespace das wir aufzeichnen",
            standardOptions);

        private Regex italicRex = new Regex(@"(?<start>(\s|\n|\?|\.|,|;|!))\*(?<test>[^*]+)\*(?<ende>(\s|\n|\?|\.|,|;|!))", 
            standardOptions);

        private Regex singleunderRex = new Regex(@"(?<start>(\s|\n|\?|\.|,|;|!))_(?<test>[^_]+)_(?<ende>(\s|\n|\?|\.|,|;|!))", 
            standardOptions);

        /// <summary>
        /// Headlines
        /// </summary>
        private Regex headline1 = new Regex(@"^(?<test>.*)       # Zeilenanfang gefolgt von beliebigem Zeugs außer einem Zeilenende
                                              [\n][=]{3,}        # Zeilenende gefolgt von mindestens 3 Gleicheitszeichen
                                             $                   # Und dann noch ein Zeilenende", 
            standardOptions);



        private Regex headline2 = new Regex(@"^(?<test>.*)       # Zeilenanfang gefolgt von beliebigem Zeugs außer einem Zeilenende
                                              [\n][-]{3,}        # Zeilenende gefolgt von mindestens 3 Minuszeichen
                                             $                   # Und dann noch ein Zeilenende",
            standardOptions);

        private Regex bulletListRex = new Regex(@"(?<test>(^\*\s.+\n)+)  # Zeilenanfang gefolgt von einem Sternchen und einem Leerzeichen dann egal was",
            standardOptions);

        private Regex bulletRex = new Regex(@"^\*\s(?<test>.*)$", 
            standardOptions);

        private Regex quoteRex = new Regex(@"
            (?<test>                    # Wrap whole match in {test}
                (
                ^[ ]*&gt;[ ]?           # '>' at the start of a line
                .+\n                    # rest of the first line
                (.+\n)*                 # subsequent consecutive lines
                \n*                     # blanks
                )+
            )", standardOptions);

        private Regex codingRex = new Regex(@"
            (?<test>                    # Wrap whole match in {test}
                (
                ^([ ]{3}|\t)            # at least three spaces or a tab character at the start of a line
                .+\n                    # rest of the first line
                (.+\n)*                 # subsequent consecutive lines
                \n*                     
                )+
            )", standardOptions);


        private Regex paraRex = new Regex(@"
            (?<test>                    # Wrap whole match in {test}
                (
                ^(?!<Paragraph)+        # nothing in front of first line that does not start with <Paragraph>
                (.+\n)+                 # subsequent consecutive lines
                \n+                     
                )
            )", standardOptions);


        public string FormatDocument(string text)
        {
            
            const string start = "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""
                + " xmlns:sys=\"clr-namespace:System;assembly=mscorlib\""
                + " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\n";

            string answ = text.Replace("\r", "") + "\n\n";

            answ = answ.Replace("<", "&lt;");
            answ = answ.Replace(">", "&gt;");

            answ = bulletListRex.Replace(answ, "<List>\n${test}</List>");
            //answ = bulletRex.Replace(answ, "&#42; ${test}");
            answ = bulletRex.Replace(answ, string.Format("<ListItem><Paragraph FontSize=\"{0}\" FontFamily=\"{1}\">{2}</Paragraph></ListItem>",
                DocumentNormalFontSize,
                DocumentFontFamily,
                "${test}"));
            answ = boldRex.Replace(answ, "${start}<Bold>${test}</Bold>${ende}");
            answ = italicRex.Replace(answ, "${start}<Italic>${test}</Italic>${ende}");
            answ = singleunderRex.Replace(answ, "${start}<Underline>${test}</Underline>${ende}");

            answ = headline1.Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n", DocumentHeadline1FontSize, "${test}", DocumentFontFamily));
            answ = headline2.Replace(answ, string.Format("<Paragraph TextAlignment=\"Left\" FontSize=\"{0}\" FontFamily=\"{2}\" FontWeight=\"Bold\">{1}</Paragraph>\n\n", DocumentHeadLine2FontSize, "${test}", DocumentFontFamily));
            answ = quoteRex.Replace(answ, new MatchEvaluator(BlockQuoteEvaluator));
            answ = codingRex.Replace(answ, new MatchEvaluator(CodingEvaluator));
            
            //dies hier am Schluss - wandelt die normalen Absätze um
            answ = paraRex.Replace(answ, new MatchEvaluator(ParagraphEvaluator));

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

            return string.Format("<Paragraph xml:space=\"preserve\" TextAlignment=\"Left\" Margin=\"50,0,0,0\" FontSize=\"{1}\" FontFamily=\"Consolas\">{0}</Paragraph>\n\n", bq, DocumentNormalFontSize);
        }


        private string ParagraphEvaluator(Match match)
        {
            string bq = match.Groups["test"].Value;

            if (bq.StartsWith("<Paragraph"))
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
                DocumentHeadline1FontSize = "16pt";
            if (DocumentHeadLine2FontSize == null)
                DocumentHeadLine2FontSize = "14pt";

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
