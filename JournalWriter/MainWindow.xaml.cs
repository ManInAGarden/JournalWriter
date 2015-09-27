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
            autoSaveDT = new DispatcherTimer();
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

            clockDT.Tick += new EventHandler(clockDT_Tick);
            clockDT.Interval = new TimeSpan(0, 1, 0);
            clockDT.Start();

            autoSaveDT.Tick += new EventHandler(autoSaveDT_Tick);
            autoSaveDT.Interval = new TimeSpan(0, 10, 0);
            autoSaveDT.Start();
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
            //string fileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            //    + System.IO.Path.DirectorySeparatorChar.ToString()
            //    + Properties.Settings.Default.FileName;
            string fileName = Properties.Settings.Default.JournalPath
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
            //string fileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) 
            //    + System.IO.Path.DirectorySeparatorChar.ToString() 
            //    + Properties.Settings.Default.FileName;

            string fileName = Properties.Settings.Default.JournalPath
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
            mdwn.DocumentHeadLine2FontSize = Properties.Settings.Default.ReadlerHeadline2FontSize.ToString();

        }

      


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

            firstDV.Document = mdwn.GetDocument(this, text);

            SetEventsOnDocument(firstDV.Document);

            firstDV.Visibility = System.Windows.Visibility.Visible;
            firstTB.Visibility = System.Windows.Visibility.Hidden;

            int wcount = CountWords(text);

            wordCountStatusBarItem.Content = wcount.ToString();
        }

        private void SetEventsOnDocument(FlowDocument fdoc)
        {
            foreach (Run curRun in LogicalTreeHelperHelper.GetChildren<Run>(fdoc, true))
                curRun.MouseDown += new MouseButtonEventHandler(fdoc_MouseDown);

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
                int inspos = GetRawTextCharacterPosition(CurrentTiToFill.Tag as string, beginning.Text.Length, CountReturns(beginning.Text));

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





        private int CountWords(string text)
        {
            int answ = 0;
            bool countMe = false;

            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
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
                //firstTB.ScrollToEnd();
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

        private void firstTB_KeyUp(object sender, KeyEventArgs e)
        {
            if (HaveChange)
                return;

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                return;

            if (!(((e.Key==Key.S) || (e.Key == Key.T)) && ((Keyboard.Modifiers & ModifierKeys.Control)==ModifierKeys.Control)))
                HaveChange = true;

            //if (((e.Key == Key.T) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)))
            //{
            //    firstTB.Text = ToggleTabs(firstTB.Text);
            //}
        }



        private void dateTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            const int maxY = 5;
            Point pt = e.GetPosition(dateTreeView);

            if (pt.Y < maxY)
                mainMenu.Visibility = System.Windows.Visibility.Visible;
            else
                mainMenu.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void firstTB_GotFocus(object sender, RoutedEventArgs e)
        {
            editModeStatusbarItem.Visibility = System.Windows.Visibility.Visible;
        }

        private void editModeStatusbarItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

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
