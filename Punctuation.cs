using System;
using System.Xml.Serialization;

namespace GRSU_SE
{
    [Serializable]
    public class Punctuation
    {
        [XmlText]
        public string Symbol { get; set; } // 🔧 теперь с set

        public Punctuation() { }

        public Punctuation(string symbol)
        {
            Symbol = symbol;
        }

        public override string ToString() => Symbol;
    }
}
