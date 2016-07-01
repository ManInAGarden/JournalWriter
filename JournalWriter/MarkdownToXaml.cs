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
        public string HeadingTextAlignment { get; set; }


        public MarkdownToXaml()
        {            
        }


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
            fdg.HeadingTextAlignment = HeadingTextAlignment;

            return fdg.ProduceDoc(lex);

        }


        public FlowDocument DebugDocument(Window parent, string text)
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
            fdg.HeadingTextAlignment = HeadingTextAlignment;

            FlowDocument doc = null;
            try
            {
                string xamlstr = fdg.DebugDoc(text, lex);
                doc = LoadXaml(xamlstr);
            }
            catch (Exception exc)
            {
                MessageBox.Show(parent, "Text kann nicht formatiert werden. Origaltext der Meldung:\n" + exc.Message);
            }

            return doc;
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
            if (TextAlignment == null)
                TextAlignment = "Left";
            if (HeadingTextAlignment == null)
                HeadingTextAlignment = "Left";

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

                MessageBox.Show(parent, "Text kann nicht formatiert werden. Originaltext der Meldung:\n" + exc.Message);
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
