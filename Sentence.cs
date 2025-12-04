using System.Xml.Serialization;

namespace GRSU_SE
{
    [Serializable]
    public class Sentence
    {
        [XmlElement("word", typeof(Word))]
        [XmlElement("punctuation", typeof(Punctuation))]
        public List<object> Tokens { get; set; } = new List<object>();

        public Sentence()
        { }

        public Sentence(string text)
        {
            Tokens = Parser.ParseSentenceTokens(text);
        }

        [XmlIgnore]
        public List<Word> Words => Tokens.OfType<Word>().ToList();

        [XmlIgnore]
        public List<Punctuation> Punctuations => Tokens.OfType<Punctuation>().ToList();

        [XmlIgnore]
        public int WordCount => Words.Count;

        [XmlIgnore]
        public bool IsQuestion => ToString().TrimEnd().EndsWith("?");

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