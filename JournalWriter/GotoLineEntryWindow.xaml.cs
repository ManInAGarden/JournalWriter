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

namespace JournalWriter
{
    /// <summary>
    /// Interaktionslogik für GotoLineEntryWindow.xaml
    /// </summary>
    public partial class GotoLineEntryWindow : Window
    {
        /// <summary>
        /// Die Zeilennummer die angesteuert werden soll
        /// </summary>
        public int LineNumber { get; set; }

        public GotoLineEntryWindow()
        {
            InitializeComponent();
        }

        private void OKBu_Click(object sender, RoutedEventArgs e)
        {
            int lnum = 0;
            if (int.TryParse(LineNumberTB.Text, out lnum))
            {
                this.DialogResult = true;
                LineNumber = lnum;
                this.Close();
            }
            else
            {
                MessageBox.Show("Der eingegebene Text kann nicht als Zahl verwendet werden.");
            }
        }

        private void CancelBu_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
