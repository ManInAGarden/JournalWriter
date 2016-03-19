using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFinder
{
    public class SearchResultEntry
    {
        public string FoundText { get; set; }
        public double MatchGrade { get; set; }
        public int FoundPosition { get; set; }
    }
}
