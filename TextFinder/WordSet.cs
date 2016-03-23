using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFinder
{
    /// <summary>
    /// class for storing an handling sets of words
    /// </summary>
    public class WordSet
    {
        public List<string> Words { private set; get; }
        public List<int> Positions { get; set; }

        public WordSet(string words)
        {
            char[] seps = new char[] { ' ', '\t', '\r', '\n', '.', ',', '"', '\'',
                ':', '!', ';', '-', '+', '*', '?', '(', ')', '{', '}', '&', '|', '<', '>',
                '[', '[', '=', '#','`'};
            //string[] parts = words.Split(seps, StringSplitOptions.RemoveEmptyEntries);

            string currw = "";
            int pos = 0;
            Words = new List<string>();
            Positions = new List<int>();
            foreach (char c in words)
            {
                if (seps.Contains(c))
                {
                    if (!string.IsNullOrEmpty(currw))
                    {
                        Words.Add(currw.ToUpper());
                        Positions.Add(pos);
                        pos += currw.Length;
                        currw = "";
                    }
                    else
                        pos++;
                }
                else
                {
                    currw += c;
                }
            }

            if(!string.IsNullOrEmpty(currw))
            {
                Words.Add(currw.ToUpper());
                Positions.Add(pos);
            }

            //Words = new List<string>();
            //foreach (string part in parts)
            //    Words.Add(part.ToUpper());
        }
    }
}
