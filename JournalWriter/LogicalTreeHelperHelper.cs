using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;

namespace JournalWriter
{
    /// <summary>
    /// Helper class for easy access to flow-documents and their hierachies.
    /// </summary>
    public class LogicalTreeHelperHelper
    {
        public static IEnumerable GetChildren(DependencyObject obj, Boolean AllChildrenInHierachy)
        {

            if (!AllChildrenInHierachy)
                return LogicalTreeHelper.GetChildren(obj);
            else
            {
                List<object> ReturnValues = new List<object>();

                RecursionReturnAllChildren(obj, ReturnValues);

                return ReturnValues;

            }

        }



        private static void RecursionReturnAllChildren(DependencyObject obj, List<object> returnValues)
        {

            foreach (object curChild in LogicalTreeHelper.GetChildren(obj))
            {
                returnValues.Add(curChild);

                if (curChild is DependencyObject)
                    RecursionReturnAllChildren((DependencyObject)curChild, returnValues);

            }
        }



        public static IEnumerable<ReturnType> GetChildren<ReturnType>(DependencyObject obj, Boolean AllChildrenInHierachy)
        {

            foreach (object child in GetChildren(obj, AllChildrenInHierachy))
                if (child is ReturnType)
                    yield return (ReturnType)child;

        }

    }
}
