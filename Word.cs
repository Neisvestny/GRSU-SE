using System;
using System.Xml.Serialization;

namespace GRSU_SE
{
    [Serializable]
    public class Word
    {
        [XmlText]
        public string Value { get; set; } // 🔧 теперь с set

        public Word() { }

        public Word(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}
