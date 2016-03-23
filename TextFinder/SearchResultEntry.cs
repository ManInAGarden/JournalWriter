using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFinder
{
    public class SearchResultEntry : IComparable
    {
        public double MatchGrade { get; set; }
        public object TagMark { get; set; }
        public List<string> MatchTexts { get; set; }
        public List<double> MatchGrades { get; set; }
        public List<int> MatchPositions { get; set; }

        public int CompareTo(object obj)
        {
            SearchResultEntry other = obj as SearchResultEntry;

            //Umkehrung damit absteigend sortiert wird
            return other.MatchGrade.CompareTo(this.MatchGrade);
        }
    }
}
