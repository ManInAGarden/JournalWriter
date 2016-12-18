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
using System.Windows.Threading;
using TextFinder;
using System.Reflection;
using Markdown;

namespace JournalWriter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DateTime CurrentDate = DateTime.Now;
        public TreeViewItem CurrentTiToFill = null;
        private bool m_haveChange = false;
        private DispatcherTimer clockDT = new DispatcherTimer(),
            autoSaveDT = new DispatcherTimer(),
            wordCountDT = new DispatcherTimer();
        MarkdownToXaml mdwn = new MarkdownToXaml();
       


        public bool HaveChange {
            private set
            {         
                if(value==true)
                    changeIndicator.Visibility = System.Windows.Visibility.Visible;
                else
                    changeIndicator.Visibility = System.Windows.Visibility.Collapsed;

                m_haveChange = value;
            }
            get { return m_haveChange; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        void clockDT_Tick(object sender, EventArgs e)
        {
            dateTimeStatusbarItem.Content = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }


        /// <summary>
        /// Wird aufgerufen wenn das Fenster geladen ist, alle Controls sind jetzt da
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeMyself();

            FetchFromFile();

            AddCurrentDay();
            dateTreeView.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));

            JumpToNode(dateTreeView, "D_" + DateTime.Now.ToString("yyyyMMdd"));
            CreateAllTimers();
            StartAllTimers();
        }


        private void CreateAllTimers()
        {
            clockDT.Tick += new EventHandler(clockDT_Tick);
            clockDT.Interval = new TimeSpan(0, 1, 0);

            autoSaveDT.Tick += new EventHandler(autoSaveDT_Tick);
            autoSaveDT.Interval = new TimeSpan(0, 10, 0);

            wordCountDT.Tick += new EventHandler(wordCountDT_Tick);
            wordCountDT.Interval = new TimeSpan(0, 0, 5);
        }


        private void StartAllTimers()
        {
            clockDT.Start();
            autoSaveDT.Start();
            wordCountDT.Start();
        }

        private void StopAllTimers()
        {
            clockDT.Stop();
            autoSaveDT.Stop();
            wordCountDT.Stop();
        }


        private void wordCountDT_Tick(object sender, EventArgs e)
        {
            if (!firstTB.IsVisible)
                return;

            int wc = CountWords(firstTB.Text);
            wordCountStatusBarItem.Content = wc.ToString();
        }


        /// <summary>
        /// Fügt den aktuellen Tag zum TreeView hinzu falls dieser darin noch nicht vorhanden ist.
        /// </summary>
        private void AddCurrentDay()
        {
            DateTime currDay = DateTime.Now.Date;
            string currDayName = "D_" + currDay.ToString("yyyymmdd");
            object o = dateTreeView.FindName(currDayName);

            if (o == null)
                AddDayItemToTree(currDay, "");
        }

        private TreeViewItem AddDayItemToTree(DateTime currDay, string itemText)
        {
            string findName = "Y_" + currDay.ToString("yyyy");
            TreeViewItem yti = FindTreeViewItem(dateTreeView, findName);
            TreeViewItem mti, dti;
            string refinedText = itemText;

            if (string.IsNullOrWhiteSpace(itemText))
            {
                string dstr = currDay.ToString("dddd dd.MM.yyyy");
                refinedText =  dstr + "\n" + "\r\n\r\n".PadLeft(dstr.Length + 4, '=');
            }

            if (yti == null)
            {
                yti = AddYearItemToTree(currDay, "");
                mti = AddMonthItemToTree(yti, currDay, "");
                dti = AddDayItemToTree(mti, currDay, refinedText);
            }
            else{
                findName = "M_" + currDay.ToString("yyyyMM");
                mti = FindTreeViewItem(yti, findName);

                if (mti == null)
                {
                    mti = AddMonthItemToTree(yti, currDay, "");
                    dti = AddDayItemToTree(mti, currDay, refinedText);
                }
                else
                {
                    findName = "D_" + currDay.ToString("yyyyMMdd");
                    dti = FindTreeViewItem(mti, findName);

                    if (dti == null)
                    {
                        dti = AddDayItemToTree(mti, currDay, refinedText);
                    }
                    
                }
            }

            return dti;
        }

        private TreeViewItem AddDayItemToTree(TreeViewItem parentTi, DateTime currDay, string itemText)
        {
            TreeViewItem ti = new TreeViewItem();
            ti.Name = "D_" + currDay.ToString("yyyyMMdd");
            ti.Tag = itemText;
            ti.Header = currDay.ToString("dd");
            ti.Foreground = parentTi.Foreground;
            ti.FontFamily = parentTi.FontFamily;
            ti.FontSize = parentTi.FontSize;
            parentTi.Items.Add(ti);

            return ti;
        }

        private TreeViewItem AddMonthItemToTree(TreeViewItem parentTI, DateTime currDay, string itemText)
        {
            TreeViewItem monthTI = new TreeViewItem();
            monthTI.Name = "M_" + currDay.ToString("yyyyMM");
            monthTI.Header = currDay.ToString("MM");
            monthTI.Foreground = parentTI.Foreground;
            monthTI.FontFamily = parentTI.FontFamily;
            monthTI.FontSize = parentTI.FontSize;
            monthTI.Tag = itemText;

            parentTI.Items.Add(monthTI);

            return monthTI;
        }

        private TreeViewItem AddYearItemToTree(DateTime currDay, string itemText)
        {
            TreeViewItem yearTI = new TreeViewItem();
            yearTI.Name = "Y_" + currDay.ToString("yyyy");
            yearTI.Header = currDay.Year.ToString();
            yearTI.Tag = itemText;
            yearTI.Foreground = dateTreeView.Foreground;
            yearTI.FontFamily = dateTreeView.FontFamily;
            yearTI.FontSize = dateTreeView.FontSize;
            dateTreeView.Items.Add(yearTI);

            return yearTI;
        }


        /// <summary>
        /// Expand a TreeView to a specific node
        /// </summary>
        /// <param name="TreeViewItem">Searching will begin from this TreeViewItem</param>
        /// <param name="NodeName">the name of the target node</param>
        bool JumpToNode(TreeViewItem tvi, string NodeName)
        {
            bool jumped = false;

            if (tvi.Name == NodeName)
            {
                tvi.IsExpanded = true;
                tvi.BringIntoView();
                tvi.IsSelected = true;
                return true;
            }
            else
                tvi.IsExpanded = false;

            if (tvi.HasItems)
            {
                foreach (var item in tvi.Items)
                {
                    TreeViewItem temp = item as TreeViewItem;
                    jumped = JumpToNode(temp, NodeName);

                    if (jumped)
                        break;
                }
            }

            return jumped;
        }


        void JumpToNode(TreeView tv, string itemName)
        {
            
            foreach (TreeViewItem ti in tv.Items)
            {
                if (JumpToNode(ti, itemName))
                    break;
            }
        }


        TreeViewItem FindTreeViewItem(TreeView tv, string tviName)
        {
            TreeViewItem answ = null;

            foreach (TreeViewItem tvi in tv.Items)
            {
                answ = FindTreeViewItem(tvi, tviName);
                
                if (answ != null)
                    break;
            }

            return answ;
        }

        /// <summary>
        /// Finde ein TreeView Item nach dessen Namen rekursiv
        /// </summary>
        /// <param name="tvi">mit diesem wird die Suche gestartet</param>
        /// <param name="tviName">Der Name nach dem gesucht wird</param>
        TreeViewItem FindTreeViewItem(TreeViewItem tvi, string tviName)
        {
            TreeViewItem answ = null;

            if (tvi.Name == tviName)
            {
                return tvi;
            }
            

            if (tvi.HasItems)
            {
                foreach (TreeViewItem item in tvi.Items)
                {
                    if (item.Name == tviName)
                        answ = item;
                    else
                        answ = FindTreeViewItem(item, tviName);

                    if (answ != null)
                        break;

                }
            }

            return answ;
        }

        void autoSaveDT_Tick(object sender, EventArgs e)
        {
            if (HaveChange)
            {
                if (firstTB.IsFocused && CurrentTiToFill != null)
                {
                    CurrentTiToFill.Tag = RemoveTabReplacements(firstTB.Text);
                }

                HaveChange = !WriteToFile(dateTreeView);
            }
        }

        /// <summary>
        /// Kann GotoLine ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GotoLine_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = firstTB.IsVisible;
        }

        /// <summary>
        /// Gehezu Zeile ausführen, also die Box zur Erfassung der Zeilennummer öffnen usw.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GotoLine_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GotoLineEntryWindow goew = new GotoLineEntryWindow();
            goew.Owner = this;

            if (goew.ShowDialog()==true)
            {
                int linenum = goew.LineNumber;

                firstTB.CaretIndex = CalcStartOfLineIdx(firstTB, linenum);
            }
        }


        /// <summary>
        /// Can we execute? Produce a debug Info for the markdown elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugMarkdown_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !firstTB.IsVisible;
        }

        /// <summary>
        /// Produce a debug Info for the markdown elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugMarkdown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeViewItem selo = dateTreeView.SelectedItem as TreeViewItem;

            if ((selo == null) || (selo.Tag == null) || !(selo.Tag is string))
                return;

            try
            {
                FlowDocument fdoc = mdwn.DebugDocument(selo.Tag as string);

                ShowDebugInfoWindow dbgw = new ShowDebugInfoWindow();
                dbgw.Show(this, fdoc);
            }
            catch (DocFormatException dfxc)
            {
                MessageBox.Show(this, "Text kann nicht formatiert werden. Origaltext der Meldung:\n" + dfxc.Message);
            }
        }


        /// <summary>
        /// Can we execute? Produce a debug Info for the markdown elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobalSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !firstTB.IsVisible && !globalSearchTB.IsVisible;
        }

        /// <summary>
        /// Start a global search (inclding the whole journal an not just the current day)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobalSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            globalSearchSBI.Visibility = Visibility.Visible;
            globalSearchTB.IsEnabled = true;
        }

        private void globalSearchTB_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                object focelem = Keyboard.Focus(globalSearchTB);
                globalSearchTB.SelectAll();
            }
        }

        /// <summary>
        /// Den Caret Index brechnen der dem Beginn der übergebenen
        /// Zeilennummer entspricht
        /// </summary>
        /// <param name="firstTB">Die Textbox mit dem Text</param>
        /// <param name="linenum">Die Zeilennummer</param>
        /// <returns></returns>
        private int CalcStartOfLineIdx(TextBox firstTB, int linenum)
        {
            int lc = 1,
                idx = 0;

            foreach (char c in firstTB.Text)
            {
                switch (c)
                {
                    case '\n':
                        lc++;
                        break;
                }

                idx++;

                if (lc >= linenum)
                    break;
            }

            if (idx > firstTB.Text.Length-1)
                idx = firstTB.Text.Length-1;

            return idx;
        }

        /// <summary>
        /// Kann ShowTabs ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTabs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = firstTB.IsVisible;
        }

         /// <summary>
        /// Beenden ausführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTabs_Executed(object sender, ExecutedRoutedEventArgs e)
        {    
            firstTB.Text = ToggleTabs(firstTB.Text);   
        }

        private string ToggleTabs(string ins)
        {
            bool tabsAreThere = ins.IndexOf('\t') >= 0;

            if (tabsAreThere)
                return ReplaceTabs(ins);
            else
                return RemoveTabReplacements(ins);
        }

        private string RemoveTabReplacements(string ins)
        {
            return ins.Replace("[TAB  ]", "\t");
        }

        private string ReplaceTabs(string ins)
        {
            return ins.Replace("\t", "[TAB  ]");
        }

        /// <summary>
        /// Kann Beenden ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Beenden ausführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (firstTB.IsFocused && CurrentTiToFill != null)
            {
                CurrentTiToFill.Tag = firstTB.Text;
            }

            if (HaveChange)
                WriteToFile(dateTreeView);

            this.Close();
        }


        /// <summary>
        /// Kann "Quelltext anzeigen" ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowSource_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        /// <summary>
        /// "Quelltext anzeigen" ausführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowSource_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeViewItem selo = dateTreeView.SelectedItem as TreeViewItem;

            if ((selo == null) || (selo.Tag==null) || !(selo.Tag is string))
                return;

            string txt = mdwn.FormatDocument(selo.Tag as string);

            ShowSourceWindow ssw = new ShowSourceWindow();

            ssw.Show(this, txt);
        }


        /// <summary>
        /// Kann Über Journal-Writer ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Über Journal-Writer ausführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.Owner = this;
            aw.Show();
        }


        /// <summary>
        /// Kann Beenden ausgeführt werden?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_haveChange;
        }

        /// <summary>
        /// Beenden ausführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAllToFile();
        }

        private void SaveAllToFile()
        {
            if (firstTB.IsFocused && CurrentTiToFill != null)
            {
                CurrentTiToFill.Tag = RemoveTabReplacements(firstTB.Text);
            }

            HaveChange = !WriteToFile(dateTreeView);
        }


        /// <summary>
        /// Schreibt alle Daten in eine Datei
        /// </summary>
        /// <returns>true wenn das Schreiben erfolgreich war</returns>
        private bool WriteToFile(TreeView tv)
        {
            bool answ = false;
            string fileName;

            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.JournalPath))
                fileName = Properties.Settings.Default.JournalPath
                    + System.IO.Path.DirectorySeparatorChar.ToString()
                    + Properties.Settings.Default.FileName;
            else
                fileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
                    + System.IO.Path.DirectorySeparatorChar.ToString()
                    + Properties.Settings.Default.FileName;

            try
                {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.CloseOutput = true;
                settings.Indent = true;

                StreamWriter sw = new StreamWriter(fileName);
                XmlWriter xmlw = XmlWriter.Create(sw, settings);

                using (xmlw)
                {
                    xmlw.WriteStartDocument();
                    xmlw.WriteComment("JournalWriter Journalsicherung");

                    xmlw.WriteStartElement("JournalYears");
                    foreach (TreeViewItem ti in tv.Items)
                    {
                        WriteToWriter(xmlw, ti);
                    }

                    xmlw.WriteEndElement();
                    xmlw.WriteEndDocument();
                }

                answ = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Beim Sichern des Journals in die Journaldatei ist ein Fehler aufgetreten. Text der Originalmeldung:"
                    + exc.Message);
            }

            return answ;
        }

        private void WriteToWriter(XmlWriter xmlw, TreeViewItem pti)
        {
            string subElementName;
            string elementName = GetElementName(pti.Name, out subElementName);

            xmlw.WriteStartElement(elementName);
            xmlw.WriteAttributeString("Name", pti.Name);
            xmlw.WriteAttributeString("Text", pti.Tag as string);
            xmlw.WriteEndElement();

            if (pti.HasItems)
            {
                xmlw.WriteStartElement(subElementName);
                foreach (TreeViewItem ti in pti.Items)
                {
                    WriteToWriter(xmlw, ti);
                }
                xmlw.WriteEndElement();
            }
        }

        private string GetElementName(string name, out string subName)
        {
            string elementName = null;

            if (name.StartsWith("D_"))
            {
                elementName = "JournalDay";
                subName = null;
            }
            else if (name.StartsWith("M_"))
            {
                elementName = "JournalMonth";
                subName = "JournalDays";
            }
            else if (name.StartsWith("Y_"))
            {
                elementName = "JournalYear";
                subName = "JournalMonths";
            }
            else
                throw new ApplicationException("Unbekannter Typ mit Namen <" + name + "> kann nicht geschrieben werden.");

            return elementName;
        }


        private bool FetchFromFile()
        {
            string fileName;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.JournalPath))
                fileName = Properties.Settings.Default.JournalPath
                    + System.IO.Path.DirectorySeparatorChar.ToString()
                    + Properties.Settings.Default.FileName;
            else
                fileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
                    + System.IO.Path.DirectorySeparatorChar.ToString()
                    + Properties.Settings.Default.FileName;


            bool answ = false;

            DateTime currDt = DateTime.MinValue;
            string text = null;

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CloseInput = true;

                StreamReader sr = new StreamReader(fileName);
                XmlReader xmlr = XmlReader.Create(sr, settings);
                string refinedDateText;
                int currYear, currMonth, currDay;

                using (xmlr)
                {
                    while (xmlr.Read())
                    {
                        switch (xmlr.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (xmlr.Name == "JournalDay")
                                {
                                    currDt = DateTime.MinValue;
                                    text = null;

                                    for (int attInd = 0; attInd < xmlr.AttributeCount; attInd++)
                                    {
                                        xmlr.MoveToAttribute(attInd);
                                        switch (xmlr.Name)
                                        {
                                            case "Name":
                                                refinedDateText = xmlr.Value.Substring(2);
                                                currYear = Int32.Parse(refinedDateText.Substring(0, 4));
                                                currMonth = Int32.Parse(refinedDateText.Substring(4, 2));
                                                currDay = Int32.Parse(refinedDateText.Substring(6, 2));
                                                currDt = new DateTime(currYear, currMonth, currDay);
                                                break;
                                            case "Text":
                                                text = xmlr.Value;
                                                break;
                                        }       
                                    }

                                    AddDayItemToTree(currDt, text);

                                    xmlr.MoveToElement();
                                }
                                break;
                        }
                    }

                    answ = true;
                }
            }
            catch (Exception)
            {
                answ = false;
            }

            return answ;
        }

        private void InitializeMyself()
        {
            dateTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(dateTreeView_SelectedItemChanged);

            dateTimeStatusbarItem.Content = CurrentDate.ToString("dd.MM.yyyy HH:mm");

            mdwn.DocumentFontFamily = Properties.Settings.Default.ReaderFont;
            mdwn.DocumentNormalFontSize = Properties.Settings.Default.ReaderFontSize.ToString();
            mdwn.DocumentHeadline1FontSize = Properties.Settings.Default.ReaderHeadline1FontSize.ToString();
            mdwn.DocumentHeadline2FontSize = Properties.Settings.Default.ReaderHeadline2FontSize.ToString();
            mdwn.DocumentHeadline3FontSize = Properties.Settings.Default.ReaderHeadline3FontSize.ToString();
            mdwn.DocumentHeadline4FontSize = Properties.Settings.Default.ReaderHeadline4FontSize.ToString();
            mdwn.DocumentHeadline5FontSize = Properties.Settings.Default.ReaderHeadline5FontSize.ToString();
            mdwn.CodingFontFamily = Properties.Settings.Default.ReaderCodingFont;
            mdwn.CodingFontSize = Properties.Settings.Default.ReaderCodingFontSize.ToString();
            mdwn.TextAlignment = Properties.Settings.Default.ReaderTextAlignment;
            mdwn.HeadingTextAlignment = Properties.Settings.Default.ReaderHeadlineTextAlignment;
        }

      

        /// <summary>
        /// Evetn-Handler: Das selektierte Element im TreeView hat sich geändert.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dateTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selItem = dateTreeView.SelectedItem as TreeViewItem;

            if (selItem == null)
                return;

            if (!selItem.Name.StartsWith("D_"))
                return;

            CurrentTiToFill = selItem;

            string text = selItem.Tag as string;

            if (text == null)
                text = "";

            try
            {
                firstDV.Document = mdwn.GetDocument(text);
            }
            catch (DocFormatException dfxc)
            {
                MessageBox.Show(this, "Text kann nicht formatiert werden. Origaltext der Meldung:\n" + dfxc.Message);
            }

            SetEventsOnDocument(firstDV.Document);

            firstDV.Visibility = System.Windows.Visibility.Visible;
            firstTB.Visibility = System.Windows.Visibility.Hidden;

            int wcount = CountWords(text);

            wordCountStatusBarItem.Content = wcount.ToString();
        }


        private void SetEventsOnDocument(FlowDocument fdoc)
        {
            foreach (object obi in LogicalTreeHelperHelper.GetChildren<object>(fdoc, true))
            {
                if (obi is Run)
                {
                    (obi as Run).MouseDown += new MouseButtonEventHandler(fdoc_MouseDown);
                } else if (obi is CheckBox)
                {
                    (obi as CheckBox).Checked += FlowDoCB_Checked;
                    (obi as CheckBox).Unchecked += FlowDoCB_Unchecked;
                    (obi as CheckBox).Indeterminate += FlowDoCB_Indeterminate;
                }
                
            }

        }

        private void FlowDoCB_Indeterminate(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            WriteCBToText(cb.Tag as string, "o");
        }

        private void FlowDoCB_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            WriteCBToText(cb.Tag as string, " ");
        }

        private void FlowDoCB_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            WriteCBToText(cb.Tag as string, "x");
        }

        /// <summary>
        /// Write the contents of a combo box to the text
        /// </summary>
        /// <param name="poss">String containig a number which is the position of the todo list markdown element</param>
        /// <param name="todovalue">The value to store in the middle of []</param>
        private void WriteCBToText(string poss, string todovalue)
        {
            int pos = 0;

            if (int.TryParse(poss, out pos))
            {
                string fulltxt = CurrentTiToFill.Tag as string;
                string txt = fulltxt.Substring(0, pos+1) + todovalue + fulltxt.Substring(pos + 2);
                CurrentTiToFill.Tag = txt;
                HaveChange = true;
            }
        }

        void fdoc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentTiToFill == null)
                return;

            if(!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            Run srun = sender as Run;
            Point mousePosition = Mouse.GetPosition(firstDV);

            if (srun == null) return;

            TextPointer tpt = GetPositionFromPoint(srun, mousePosition);

            if (tpt != null)
            {
                TextRange beginning = new TextRange(firstDV.Document.ContentStart, tpt);
                int inspos = GetRawTextCharacterPosition(CurrentTiToFill.Tag as string, 
                    beginning.Text.Length, 
                    CountReturns(beginning.Text));

                ShowTextBox(inspos);

            }

            e.Handled = true;
        }

        private int CountReturns(string instr)
        {
            int ct = 0;
            foreach (char c in instr)
            {
                if (c == '\n' || c == '\r')
                    ct++;
            }

            return ct;
        }

        private int GetRawTextCharacterPosition(string instr, int maxrealc, int incrcorr)
        {
            int nettoPos = 0, bruttoPos = 0, oldNettoPos=0;
            bool hadspace = false;
            bool hadbreak = false;
            int starct = 0;
            int starctdir = 1;
            char headsign = 'X';
            string counted = "", notcounted = "";

            foreach (char c in instr)
            {
                oldNettoPos = nettoPos;

                switch (c)
                {
                    case '\r':
                        starctdir = 1;
                        starct = 0;
                        hadspace = false;
                        hadbreak = false;
                        headsign = 'X';
                        break;
                    case '\n':
                        starctdir = 1;
                        starct = 0;
                        hadspace = false;
                        hadbreak = true;
                        headsign = 'X';
                        break;
                    case '=':
                        if (hadbreak)
                            headsign = c;
                        else if (headsign!=c)
                            nettoPos++;

                        starctdir = -1;
                        hadspace = false;
                        hadbreak = false;

                        break;
                    case '-':
                        if(hadbreak)
                            headsign = c;
                        else if (headsign!=c)
                            nettoPos++;

                        starctdir = -1;
                        hadspace = false;
                        hadbreak = false;
                        break;
                    case '*':
                        if (!hadspace && (starct == 0))
                        {
                            nettoPos++;
                        }
                        else if (!hadspace && (starct > 0))
                        {
                            starct += starctdir;
                        }
                        else if (hadspace)
                        {
                            starctdir = 1;
                            starct = 1;
                        }
                        
                        hadspace = false;
                        hadbreak = false;
                        headsign = 'X';
                        break;
                    case '_':
                        if (!hadspace)
                            nettoPos++;

                        starctdir = -1;
                        hadspace = false;
                        hadbreak = false;
                        headsign = 'X';
                        break;

                    case ' ':
                        starctdir = -1;
                        hadspace = true;
                        hadbreak = false;
                        headsign = 'X';
                        nettoPos++;
                        break;

                    case '\t':
                        if (!hadbreak)
                        {
                            hadspace = true;
                            nettoPos++;
                        }

                        starctdir = -1;
                        hadbreak = false;
                        headsign = 'X';
                        break;

                    case '>':
                        if (!hadbreak)
                        {
                            nettoPos++;
                        }

                        starctdir = -1;
                        hadspace = false;
                        hadbreak = false;
                        headsign = 'X';
                        break;

                    default:
                        starctdir = -1;
                        hadspace = false;
                        hadbreak = false;
                        headsign = 'X';
                        nettoPos++;
                        break;
                }

                bruttoPos++;

                if (oldNettoPos != nettoPos)
                    counted += c;
                else
                    notcounted += c;

                if (nettoPos >= (maxrealc-incrcorr))
                    break;
            }

            return bruttoPos + 1;
        }


        public static TextPointer GetPositionFromPoint(/* this */ Run _this, Point searchForPoint)
        {

            TextPointer ptrCurCharacter = _this.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
            TextPointer ptrNextCharacter = ptrCurCharacter.GetNextInsertionPosition(LogicalDirection.Forward);

            while (ptrNextCharacter != null)
            {
                Rect characterInsertionPointRectangle = ptrCurCharacter.GetCharacterRect(LogicalDirection.Forward);
                Rect nextCharacterInsertionPointRectangle = ptrNextCharacter.GetCharacterRect(LogicalDirection.Backward);

                if (searchForPoint.X >= characterInsertionPointRectangle.X
                    && searchForPoint.X <= nextCharacterInsertionPointRectangle.X
                    && searchForPoint.Y >= characterInsertionPointRectangle.Top
                    && searchForPoint.Y <= characterInsertionPointRectangle.Bottom)
                {
                    return ptrCurCharacter;
                }

                ptrCurCharacter = ptrCurCharacter.GetNextInsertionPosition(LogicalDirection.Forward);
                ptrNextCharacter = ptrNextCharacter.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            return null;
        }




        /// <summary>
        /// Die Anzahl der Wörter zurückgeben
        /// </summary>
        /// <param name="text">Text der die zu zählenden Wörter enthält</param>
        /// <returns>Anzahl Wörter im Text</returns>
        private int CountWords(string text)
        {
            int answ = 0;
            bool countMe = false;

            foreach (char c in text)
            {
                if ((char.IsWhiteSpace(c)) )
                {
                    if (countMe)
                    {
                        answ++;
                        countMe = false;
                    }
                }
                else
                    countMe = true;
            }

            if (answ > 0)
                answ++;

            return answ;
        }


        private void ShowTextBox()
        {
            ShowTextBox(-1);
        }

        private void ShowTextBox(int caretPos)
        {
            if (CurrentTiToFill == null)
                return;

            firstTB.Text = CurrentTiToFill.Tag as string;
            firstDV.Visibility = System.Windows.Visibility.Hidden;
            firstTB.Visibility = System.Windows.Visibility.Visible;
            firstTB.Focus();

            if (caretPos < 0)
            {
                firstTB.CaretIndex = firstTB.Text.Length;
            }
            else
            {
                firstTB.CaretIndex = caretPos;
            }
        }

        private void firstDV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowTextBox();
        }

        private void firstTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentTiToFill == null)
                return;

            CurrentTiToFill.Tag = RemoveTabReplacements(firstTB.Text);

            editModeStatusbarItem.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Handle key presses to register changes with the text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void firstTB_KeyUp(object sender, KeyEventArgs e)
        {

            if (HaveChange)
                return;

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                return;

            if (!(((e.Key==Key.S) || (e.Key == Key.T)) && ((Keyboard.Modifiers & ModifierKeys.Control)==ModifierKeys.Control)))
                HaveChange = true;

        }



        private void dateTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            const int maxY = 5;
            Point pt = e.GetPosition(dateTreeView);

            if (pt.Y < maxY)
                mainMenu.Visibility = Visibility.Visible;
            else
                mainMenu.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// show the user that we are in edit mode for the selected day's text now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void firstTB_GotFocus(object sender, RoutedEventArgs e)
        {
            editModeStatusbarItem.Visibility = Visibility.Visible;
        }


        private void editModeStatusbarItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void MarkdownHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MarkdownHelpWindow mdh = new MarkdownHelpWindow();
            mdh.Owner = this;
            mdh.Show();
        }

        /// <summary>
        /// We`re always in the mood to help
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarkdownHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void SetFileLocation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog opfd = new Microsoft.Win32.OpenFileDialog();
            opfd.AddExtension = true;
            opfd.CheckPathExists = true;
            opfd.DefaultExt = "xml";
            opfd.Multiselect = false;
            opfd.Filter = "SJournal-Dateien (*.xml) |*.xml| Alle Dateien (*.*) |*.*";
            opfd.InitialDirectory = Properties.Settings.Default.JournalPath;
            opfd.Title = "Dateiauswahl für die Journal Datei";
            opfd.CheckFileExists = true;

            StopAllTimers();

            if (HaveChange)
                SaveAllToFile();

            if (opfd.ShowDialog() == true)
            {
                Properties.Settings.Default.JournalPath = System.IO.Path.GetDirectoryName(opfd.FileName);
                Properties.Settings.Default.FileName = System.IO.Path.GetFileName(opfd.FileName);
                dateTreeView.Items.Clear();
                CurrentTiToFill = null;
                FetchFromFile();

                AddCurrentDay();
                dateTreeView.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));

                JumpToNode(dateTreeView, "D_" + DateTime.Now.ToString("yyyyMMdd"));

                Properties.Settings.Default.Save();
            }

            StartAllTimers();
        }

        /// <summary>
        /// We can only change the file location when we are not currently editing a day's text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetFileLocation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !firstTB.IsVisible;
        }

        private void xBU_Click(object sender, RoutedEventArgs e)
        {
            CloseDownSearchElements();
        }

        /// <summary>
        /// Clear the screen from all search elements produced by a preceding global search
        /// </summary>
        private void CloseDownSearchElements()
        {
            globalSearchSBI.Visibility = Visibility.Collapsed;
            globalSearchResultsLB.Visibility = Visibility.Collapsed;
        }

        private void xBU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseDownSearchElements();
            else if (e.Key == Key.Return)
                DoSearch(globalSearchTB.Text);
        }

        /// <summary>
        /// Start a global search with the given text
        /// </summary>
        /// <param name="text"></param>
        private void DoSearch(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            MultipleTextFinder mtf = new MultipleTextFinder() { MinimumMatchGrade=0.6 };

            mtf.TextsToBeSearched = GetAllJournalTexts();
            List<SearchResultEntry> results = mtf.DoSearch(text);
            globalSearchResultsLB.Items.Clear();

            if (results.Count>0)
            {
                
                foreach (SearchResultEntry sre in results)
                {
                    globalSearchResultsLB.Items.Add(GetSearchTextBlock(sre));
                }

                globalSearchResultsLB.IsEnabled = true;
            }
            else
            {
                TextBlock tb = new TextBlock() { TextWrapping = TextWrapping.Wrap};
                tb.Inlines.Add(new Italic(new Run("Es konnten keine Textstellen für die Suchbegriffe gefunden werden.")));

                globalSearchResultsLB.Items.Add(tb);
                globalSearchResultsLB.IsEnabled = false;
            }

            globalSearchResultsLB.MaxHeight = dateTreeView.ActualHeight / 2;
            globalSearchResultsLB.Visibility = Visibility.Visible;
        }

        private TextBlock GetSearchTextBlock(SearchResultEntry sre)
        {
            TextBlock answ = new TextBlock() { TextWrapping = TextWrapping.Wrap, Tag=sre };
            answ.Inlines.Add(new Bold(new Run(((DateTime)sre.TagMark).ToString("dd.MM.yyyy"))));
            answ.Inlines.Add(" - " + (sre.MatchGrade * 100).ToString("0.") + "%\n");
            
            int minpos = sre.MatchPositions.Min();

            if (minpos - 10 >= 0)
                minpos -= 10;
            else
                minpos = 0;

            string displayedPart = GetPart((DateTime)sre.TagMark, minpos, 40);
            string upperCopy = displayedPart.ToUpper();

            //answ.Inlines.Add(displayedPart + "\n");
            int partidx = 0;
            List<WordPositionInfo> idxes = new List<WordPositionInfo>();
            foreach (string word in sre.CounterMatchTexts)
            {
                partidx = upperCopy.IndexOf(word);
                if (partidx >= 0)
                {
                    idxes.Add(new WordPositionInfo() { Index = partidx, Text = word });
                }
            }

            idxes.Sort();
            int lastidx = 0;
            foreach (WordPositionInfo idx in idxes)
            {
                if (idx.Index > lastidx)
                    answ.Inlines.Add(displayedPart.Substring(lastidx, (idx.Index - lastidx)));

                answ.Inlines.Add(new Bold(new Run(displayedPart.Substring(idx.Index, idx.Text.Length))));
                lastidx = idx.Index + idx.Text.Length;
            }

            if (lastidx < displayedPart.Length)
                answ.Inlines.Add(displayedPart.Substring(lastidx));

            return answ;
        }

        private string GetPart(DateTime day, int minpos, int v)
        {
            string name = "D_" + day.ToString("yyyyMMdd");
            string answ;

            TreeViewItem it = FindTreeViewItem(dateTreeView, name);

            if (it != null)
            {
                string txt = it.Tag as string;
                if (txt.Length > minpos + 40)
                {
                    answ = txt.Substring(minpos, 40);
                }
                else
                    answ = txt.Substring(minpos);
            }
            else
                throw new ApplicationException("Häh!");

            answ = answ.Replace("\r", "");
            answ = answ.Replace('\n', ' ');
            answ = answ.Replace('\t', ' ');
            return answ;
        }

        private List<TaggedText> GetAllJournalTexts()
        {
            List<TaggedText> answ = new List<TaggedText>();

            
            foreach (TreeViewItem item in dateTreeView.Items)
            {
                AddItemTexts(answ, item);
            }

            return answ;
        }

        private void AddItemTexts(List<TaggedText> answ, TreeViewItem paritem)
        {
            TaggedText curtt;
            DateTime currtag;

            if (paritem.Name.StartsWith("D_"))
            {
                if (paritem.Tag != null)
                {
                    if (DateTime.TryParseExact(paritem.Name.Substring(2),
                        "yyyyMMdd", 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, 
                        out currtag))
                    {
                        curtt = new TaggedText() { Text = paritem.Tag as string, Tag = currtag };
                        answ.Add(curtt);
                    }
                }
            }

            foreach (TreeViewItem childitem in paritem.Items)
            {
                AddItemTexts(answ, childitem);
            }
        }

        /// <summary>
        /// a search result was selected in the list of global search results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void globalSearchResultsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock tbl = globalSearchResultsLB.SelectedItem as TextBlock;

            if (tbl == null)
                return;

            SearchResultEntry sre = tbl.Tag as SearchResultEntry;

            if (sre == null)
                return;

            if (sre.TagMark == null)
                return;

            DateTime day = (DateTime)sre.TagMark;

            TreeViewItem searchedTvi = FindTreeViewItem(dateTreeView, "D_" + day.ToString("yyyyMMdd"));
            if (searchedTvi == null)
                return;

            searchedTvi.IsSelected = true;
            ExpandUp(searchedTvi);
            SelectTextInFlowDoc(firstDV.Document, sre.CounterMatchTexts);
        }


        private void SelectTextInFlowDoc(FlowDocument newDocument, List<string> searches)
        {
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase;
            string textRun;
            int indexInRun;

            foreach (string search in searches)
            {
                for (TextPointer position = newDocument.ContentStart;
                    position != null && position.CompareTo(newDocument.ContentEnd) <= 0;
                    position = position.GetNextContextPosition(LogicalDirection.Forward))
                {
                    if (position.CompareTo(newDocument.ContentEnd) == 0)
                    {
                        break;
                    }

                    textRun = position.GetTextInRun(LogicalDirection.Forward);
                    indexInRun = textRun.IndexOf(search, stringComparison);
                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        if (position != null)
                        {
                            TextPointer nextPointer = position.GetPositionAtOffset(search.Length);
                            TextRange textRange = new TextRange(position, nextPointer);
                            textRange.ApplyPropertyValue(TextElement.BackgroundProperty,
                                          new SolidColorBrush(Colors.SkyBlue));
                        }
                    }
                }
            }
        }

        //private void FindTextInDocReader(FlowDocumentReader docViewer, string searchs)
        //{
        //    docViewer.Find();
        //    object findToolbar = docViewer.Template.FindName("PART_FindToolBarHost", docViewer);

        //    // Set Text
        //    FieldInfo findTextBoxFieldInfo = findToolbar.GetType().GetField("FindTextBox", BindingFlags.NonPublic | BindingFlags.Instance);
        //    TextBox findTextBox = (TextBox)findTextBoxFieldInfo.GetValue(findToolbar);
        //    findTextBox.Text = searchs;

        //    // Raise ClickEvent for Highlighting
        //    FieldInfo findNextButtonFieldInfo = findToolbar.GetType().GetField("FindNextButton", BindingFlags.NonPublic | BindingFlags.Instance);
        //    Button findNextButton = (Button)findNextButtonFieldInfo.GetValue(findToolbar);
        //    findNextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        //}


        /// <summary>
        /// Expand the given item and all the items up to the root item
        /// </summary>
        /// <param name="startFromTvi">The tree view item to start the expansion from</param>
        private void ExpandUp(TreeViewItem startFromTvi)
        {
            if (startFromTvi == null)
                return;

            startFromTvi.IsExpanded = true;

            if (startFromTvi.Parent != null)
                ExpandUp(startFromTvi.Parent as TreeViewItem);
        }


        private void PrintDay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                FlowDocument fd = CloneFlowDocument(firstDV.Document);
                fd.PageHeight = pd.PrintableAreaHeight;
                fd.PageWidth = pd.PrintableAreaWidth;
                fd.PagePadding = new Thickness(50);
                fd.ColumnGap = 0;
                fd.ColumnWidth = pd.PrintableAreaWidth;

                IDocumentPaginatorSource dps = fd;
                pd.PrintDocument(dps.DocumentPaginator, "flow doc");
            }
        }

        private void PrintDay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = firstDV.IsVisible;
        }



        /// <summary>
        /// Creates a new flow document as a clone of another
        /// </summary>
        /// <param name="from">a flow document to be cloned</param>
        /// <returns>a flow document as a clone of the given flow document</returns>
        public FlowDocument CloneFlowDocument(FlowDocument from)
        {
            FlowDocument to = new FlowDocument();
            TextRange rangefrom = new TextRange(from.ContentStart, from.ContentEnd);
            MemoryStream mems = new MemoryStream();

            System.Windows.Markup.XamlWriter.Save(rangefrom, mems);
            rangefrom.Save(mems, DataFormats.XamlPackage);

            TextRange rangeto = new TextRange(to.ContentEnd, to.ContentEnd);
            rangeto.Load(mems, DataFormats.XamlPackage);

            return to;
        }

        

        private void wordCountStatusBarItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Wenn wir nicht gerade editieren, dann lassen wir einfach die alte Zahl stehen
            //denn die wurde ja bei der Selektion des TV-Items erneuert.
            if (firstTB.IsVisible)
            {
                int wc = 0;
                wc = CountWords(firstTB.Text);
                wordCountStatusBarItem.Content = wc.ToString();
            }
        }

        /// <summary>
        /// Der Anwender dreht im Textfenster am Mausrad. Den Font vergrößern bzw verkleinern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void firstTB_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            e.Handled = true;

            if ((e.Delta > 0) && (firstTB.FontSize<32))
                firstTB.FontSize += 1.0;
            else if (e.Delta < 0 && (firstTB.FontSize > 8))
                firstTB.FontSize -= 1.0;
                  
        }

        
    }
}
