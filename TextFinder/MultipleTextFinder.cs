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
            double grade;
            WordSet currTextWs;
            SearchResultEntry se;
            foreach (TaggedText ttxt in TextsToBeSearched)
            {
                currTextWs = new WordSet(ttxt.Text);
                WordSetPerTag.Add(ttxt.Tag, currTextWs);
                grade = CalculateGrade(currTextWs, searchWordSet);
                if (grade >= MinimumMatchGrade)
                {
                    se = new SearchResultEntry()
                    {
                        MatchGrade = grade
                    };

                    answ.Add(se);
                }
                
            }


            return answ;
        }

        /// <summary>
        /// Calculate the grade of equality for two word sets
        /// </summary>
        /// <param name="currTextWs"></param>
        /// <param name="searchWordSet"></param>
        /// <returns></returns>
        private double CalculateGrade(WordSet ws1, WordSet ws2)
        {
            int finds = 0;
            foreach (string w2 in ws2.Words)
            {
                if (ws1.Words.Contains(w2))
                    finds++;
            }

            return (double)finds / (double)ws2.Words.Count;
        }
    }
}
