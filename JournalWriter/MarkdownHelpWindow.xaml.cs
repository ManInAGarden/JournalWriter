using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;

namespace JournalWriter
{
    /// <summary>
    /// Interaktionslogik für AboutWindow.xaml
    /// </summary>
    public partial class MarkdownHelpWindow : Window
    {
        public MarkdownHelpWindow()
        {
            InitializeComponent();
        }

        private void closeBU_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader("JournalWriterHelpMarkup.txt");
            using (sr)
            {
                string txt = sr.ReadToEnd();
                MarkdownToXaml md2xaml = new MarkdownToXaml();
                SetDocProps(md2xaml);
                aboutDocumentViewer.Document = md2xaml.GetDocument(this, txt);
            }
            
        }

        private void SetDocProps(MarkdownToXaml md2xaml)
        {
            md2xaml.CodingFontFamily = Properties.Settings.Default.ReaderCodingFont;
            md2xaml.CodingFontSize = Properties.Settings.Default.ReaderCodingFontSize.ToString();
            md2xaml.DocumentFontFamily = Properties.Settings.Default.ReaderFont;
            md2xaml.DocumentNormalFontSize = Properties.Settings.Default.ReaderFontSize.ToString();
            md2xaml.DocumentHeadline1FontSize = Properties.Settings.Default.ReaderHeadline1FontSize.ToString();
            md2xaml.DocumentHeadline2FontSize = Properties.Settings.Default.ReaderHeadline2FontSize.ToString();
            md2xaml.DocumentHeadline3FontSize = Properties.Settings.Default.ReaderHeadline3FontSize.ToString();
            md2xaml.DocumentHeadline4FontSize = Properties.Settings.Default.ReaderHeadline4FontSize.ToString();
            md2xaml.DocumentHeadline5FontSize = Properties.Settings.Default.ReaderHeadline5FontSize.ToString();
        }
    }
}
