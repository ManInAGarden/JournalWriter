using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFinder
{
    public class SearchResultEntry
    {
        public double MatchGrade { get; set; }
        public object TagMark { get; set; }
        public List<string> MatchedTexts { get; set; }
        public List<double> MatchGrades { get; set; }
    }
}
