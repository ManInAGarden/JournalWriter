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

        public WordSet(string words)
        {
            string[] parts = words.Split(new char[] { ' ', '\t', '\r', '\n', '.', ',', '"', '\'',
                ':', '!', ';', '-', '+', '*', '?', '(', ')', '{', '}', '&', '|', '<', '>',
                '[', '[', '=', '#','`'}, 
                StringSplitOptions.RemoveEmptyEntries);

            Words = new List<string>();
            foreach (string part in parts)
                Words.Add(part.ToUpper());
        }
    }
}
