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
    /// Interaktionslogik für ShowSourceWindow.xaml
    /// </summary>
    public partial class ShowSourceWindow : Window
    {
        public ShowSourceWindow()
        {
            InitializeComponent();
        }


        public void Show(Window owner, string text)
        {
            this.Owner = owner;
            sourceTextBox.Text = text;

            this.Show();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
