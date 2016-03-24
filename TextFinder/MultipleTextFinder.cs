using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFinder
{
    /// <summary>
    /// a class to support text search by calculating similiarities
    /// </summary>
    public class MultipleTextFinder
    {

        /// <summary>
        /// The text we try to find (my consist of many words)
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// a list of texts to be searched
        /// </summary>
        public List<TaggedText> TextsToBeSearched { get; set; }

        public Dictionary<object, WordSet> WordSetPerTag { get; set; }

        /// <summary>
        /// minmum match grade to be applied in the search
        /// </summary>
        public double MinimumMatchGrade { get; set; }


        public List<SearchResultEntry> DoSearch(string searchTxt)
        {
            SearchText = searchTxt;
            WordSet searchWordSet = new WordSet(searchTxt);
            List<SearchResultEntry> answ = new List<SearchResultEntry>();
            WordSetPerTag = new Dictionary<object, WordSet>();

            WordSet currTextWs;
            SearchResultEntry se;
            foreach (TaggedText ttxt in TextsToBeSearched)
            {
                currTextWs = new WordSet(ttxt.Text);
                WordSetPerTag.Add(ttxt.Tag, currTextWs);
                se = CalculateGrade(currTextWs, searchWordSet);
                se.TagMark = ttxt.Tag;
                if(se.MatchGrade>=MinimumMatchGrade)
                    answ.Add(se);
            }

            answ.Sort();

            return answ;
        }

        /// <summary>
        /// Calculate the grade of equality for two word sets
        /// </summary>
        /// <param name="currTextWs"></param>
        /// <param name="searchWordSet"></param>
        /// <returns>A search result entry containing information about the found elements</returns>
        private SearchResultEntry CalculateGrade(WordSet ws1, WordSet ws2)
        {
            SearchResultEntry answ = new SearchResultEntry();

            answ.MatchTexts = new List<string>();
            answ.MatchGrades = new List<double>();
            answ.MatchPositions = new List<int>();
            double gradeSum = 0.0;
            int minldist, currldist, minidx;
            int matchIdx;
            string w1;
            double currGrade;

            foreach (string w2 in ws2.Words)
            {
                matchIdx = ws1.Words.IndexOf(w2);
                if (matchIdx>=0)
                {
                    answ.MatchTexts.Add(w2);
                    answ.MatchGrades.Add(1.0);
                    answ.MatchPositions.Add(ws1.Positions[matchIdx]);
                    gradeSum += 1.0;
                }
                else
                {
                    minldist = int.MaxValue;
                    minidx = 0;
                    for (int i=0; i<ws1.Words.Count; i++)
                    {
                        w1 = ws1.Words[i];
                        currldist = LevenshteinDistance(w1, w2);
                        if (currldist < minldist)
                        {
                            minldist = currldist;
                            minidx = i;
                        }
                    }

                    currGrade = (double)(w2.Length - minldist) / w2.Length;

                    if (currGrade >= MinimumMatchGrade)
                    {
                        answ.MatchTexts.Add(w2);
                        answ.MatchGrades.Add(currGrade);
                        answ.MatchPositions.Add(ws1.Positions[minidx]);

                        gradeSum += currGrade;
                    }
                }
            }

            answ.MatchGrade = gradeSum / (double)ws2.Words.Count;
            
            return answ;
        }


        /// <summary>
        /// Calculate the levenshtein distance (stolen from wikibooks)
        /// see, https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance#C.23
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                                distance[previousRow, j] + 1,
                                distance[currentRow, j - 1] + 1),
                                distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }

    }
}
