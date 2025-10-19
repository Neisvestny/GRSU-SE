using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GRSU_SE
{
    [Serializable]
    public class Sentence
    {
        [XmlElement("word", typeof(Word))]
        [XmlElement("punctuation", typeof(Punctuation))]
        public List<object> Tokens { get; set; } = new List<object>(); // 🔧 теперь с set
        public Sentence() { }

        public Sentence(string text)
        {
            var matches = Regex.Matches(text, @"\w+|[^\w\s]");
            foreach (Match match in matches)
            {
                if (Regex.IsMatch(match.Value, @"\w+"))
                    Tokens.Add(new Word(match.Value));
                else
                    Tokens.Add(new Punctuation(match.Value));
            }
        }
        [XmlIgnore]
        public List<Word> Words => Tokens.OfType<Word>().ToList();
        [XmlIgnore]
        public int WordCount => Words.Count;
        [XmlIgnore]
        public string RawText => ToString();
        [XmlIgnore]
        public bool IsQuestion => Tokens.OfType<Punctuation>().Any(p => p.Symbol == "?");

        public override string ToString()
        {
            string result = "";
            foreach (var token in Tokens)
            {
                if (token is Word word)
                    result += word.Value + " ";
                else if (token is Punctuation punct)
                    result = result.TrimEnd() + punct.Symbol + " ";
            }
            return result.Trim();
        }
    }
}
