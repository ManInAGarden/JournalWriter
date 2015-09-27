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
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void closeBU_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            versionLabel.Content = "V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            StreamReader sr = new StreamReader("JournalWriterHelpMarkup.txt");
            using (sr)
            {
                string txt = sr.ReadToEnd();
                MarkdownToXaml md2xaml = new MarkdownToXaml();
                aboutDocumentViewer.Document = md2xaml.GetDocument(this, txt);
            }
            
        }
    }
}
