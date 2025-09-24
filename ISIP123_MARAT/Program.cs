//практика 3

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;



class Program
{
    class StatText
    {
        public string Text { get; set; }
        public int WordCount { get; set; }
        public string ShortWord { get; set; }
        public string LongWord { get; set; }
        public int SentencesCount { get; set; }
        public int GlasCount { get; set; }
        public int SoglasCount { get; set; }
        public Dictionary <char, int> LetterFrequency { get; set; }
        public DateTime AnalysisDate { get; set; }
    }
}