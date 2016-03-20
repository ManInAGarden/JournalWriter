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
                if(se.MatchGrade>MinimumMatchGrade)
                    answ.Add(se);
            }


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

            answ.MatchedTexts = new List<string>();
            answ.MatchGrades = new List<double>();
            double gradeSum = 0.0;

            foreach (string w2 in ws2.Words)
            {
                if (ws1.Words.Contains(w2))
                {
                    answ.MatchedTexts.Add(w2);
                    answ.MatchGrades.Add(1.0);
                    gradeSum += 1.0;
                }
            }

            answ.MatchGrade = gradeSum / (double)ws2.Words.Count;
            
            return answ;
        }
    }
}
