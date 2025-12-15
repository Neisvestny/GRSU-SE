using System.Xml.Serialization;

[Serializable]
public class Punctuation
{
    [XmlText]
    public string Symbol { get; set; }

    public Punctuation()
    { }

    public Punctuation(string symbol)
    {
        Symbol = symbol;
    }

    public bool IsQuestion()
    {
        return Symbol == "?" ? true : false;
    }

    public bool IsExclamation()
    {
        return Symbol == "!";
    }

    public bool IsDot()
    {
        return Symbol == ".";
    }

    public bool IsComma()
    {
        return Symbol == ",";
    }

    public bool IsPaired()
    {
        string paired = "()[]{}«»\"'";
        return paired.Contains(Symbol);
    }

    public override string ToString() => Symbol;
}