using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JournalWriter
{
    public class SessionCommands
    {
        public static RoutedUICommand Exit { get; private set; }
        public static RoutedUICommand Save { get; private set; }
        public static RoutedUICommand About { get; private set; }
        public static RoutedUICommand ShowSource { get; private set; }
        public static RoutedUICommand ShowTabs { get; private set; }
        public static RoutedUICommand GotoLine { get; private set; }
        public static RoutedUICommand MarkdownHelp { get; private set; }
        public static RoutedUICommand SetFileLocation { get; private set; }
        public static RoutedUICommand DebugMarkdown { get; private set; }
        public static RoutedUICommand GlobalSearch { get; private set; }
        public static RoutedUICommand PrintDay { get; private set; }
        public static RoutedUICommand CodeBlock { get; private set; }
        public static RoutedUICommand CodeInline { get; private set; }
        public static RoutedUICommand Bold {get; private set;}
        public static RoutedUICommand Italics { get; private set; }
        public static RoutedUICommand Underline { get; private set; }

        static SessionCommands()
        {

            Exit = new RoutedUICommand("_Beenden", "Exit", typeof(SessionCommands),
                new InputGestureCollection  
                { 
                    new KeyGesture(Key.Q, ModifierKeys.Control, "Strg+Q")
                }
              );

            Save = new RoutedUICommand("_Speichern", "Save", typeof(SessionCommands),
                new InputGestureCollection  
                { 
                    new KeyGesture(Key.S, ModifierKeys.Control, "Strg+S")
                }
              );

            About = new RoutedUICommand("_Über Journal Writer", "About", typeof(SessionCommands)
              );

            MarkdownHelp = new RoutedUICommand("_Markdown Hilfe", "MarkdownHelp", typeof(SessionCommands)
              );

            ShowSource = new RoutedUICommand("_Quelltext (Flow Document) anzeigen", "ShowSource", typeof(SessionCommands)
              );

            ShowTabs = new RoutedUICommand("_TABs Anzeige ein/aus", "ShowTabs", typeof(SessionCommands),
                new InputGestureCollection  
                { 
                    new KeyGesture(Key.T, ModifierKeys.Control, "Ctr+T")
                }
              );

            GotoLine = new RoutedUICommand("_Gehezu Zeile", "GotoLine", typeof(SessionCommands),
                new InputGestureCollection
                {
                    new KeyGesture(Key.G, ModifierKeys.Alt, "Alt+G")
                }
              );

            SetFileLocation = new RoutedUICommand("_JournalDatei", "SetFileLocation", typeof(SessionCommands));

            DebugMarkdown = new RoutedUICommand("_JournalDatei", "DebugMarkDown", typeof(SessionCommands));

            GlobalSearch = new RoutedUICommand("_Global Suchen", "GlobalSearch", typeof(SessionCommands),
               new InputGestureCollection
               {
                    new KeyGesture(Key.F, ModifierKeys.Control, "Strg+F")
               }
             );

            PrintDay = new RoutedUICommand("_Tag drucken", "PrintDay", typeof(SessionCommands),
               new InputGestureCollection
               {
                    new KeyGesture(Key.P, ModifierKeys.Control, "Strg+P")
               }
             );

            CodeBlock = new RoutedUICommand("_Code-Block", "CodeBlock", typeof(SessionCommands),
               new InputGestureCollection
               {
                    new KeyGesture(Key.C, ModifierKeys.Alt, "Alt+C")
               }
             );

            CodeInline = new RoutedUICommand("_Inlinecode", "CodeInline", typeof(SessionCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift, "Shift+Ctlr+C")
              }
            );

            Bold = new RoutedUICommand("_Fett", "Bold", typeof(SessionCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift, "Shift+Ctrl+F")
              }
            );

            Italics = new RoutedUICommand("_Kursiv", "Italics", typeof(SessionCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.K, ModifierKeys.Control | ModifierKeys.Shift, "Shift+Ctlr+K")
              }
            );

            Underline = new RoutedUICommand("_Unterstrichen", "Underline", typeof(SessionCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.U, ModifierKeys.Control | ModifierKeys.Shift, "Shift+Ctlr+U")
              }
            );
        }   
        
    }
}
