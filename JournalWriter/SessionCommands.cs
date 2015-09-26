﻿using System;
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
        public static RoutedUICommand ShowTabs{get; private set;}

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

            ShowSource = new RoutedUICommand("_Quelltext (Flow Document) anzeigen", "ShowSource", typeof(SessionCommands)
              );

            ShowTabs = new RoutedUICommand("_TABs Anzeige ein/aus", "ShowTabs", typeof(SessionCommands),
                new InputGestureCollection  
                { 
                    new KeyGesture(Key.T, ModifierKeys.Control, "Ctr+T")
                }
              );

        }   
        
    }
}
