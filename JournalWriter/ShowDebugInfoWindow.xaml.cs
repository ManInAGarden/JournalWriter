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
    /// Interaktionslogik für ShowDebugInfoWindow.xaml
    /// </summary>
    public partial class ShowDebugInfoWindow : Window
    {
        public ShowDebugInfoWindow()
        {
            InitializeComponent();
        }

        internal void Show(MainWindow mainWindow, FlowDocument fdoc)
        {
            this.Owner = mainWindow;

            flowdoDebugFDV.Document = fdoc;
            Show();
        }
    }
}
