using System.Xml.Serialization;

[Serializable]
public class Word
{
    [XmlText]
    public string Value { get; set; }

    public Word()
    { }

    public Word(string value)
    {
        Value = value.Trim();
    }

    public string IsUpperCase()
    {
        return Value?.ToUpper() ?? string.Empty;
    }

    public string IsLowerCase()
    {
        return Value?.ToLower() ?? string.Empty;
    }

    public bool EqualsIgnoreCase(Word other)
    {
        if (other == null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public int VowelCount()
    {
        if (string.IsNullOrEmpty(Value))
            return 0;

        string vowels = "аеёиоуыэюяaeiou";
        return Value.ToLower().Count(c => vowels.Contains(c));
    }

    public int ConsonantCount()
    {
        if (string.IsNullOrEmpty(Value))
            return 0;

        string consonants = "бвгджзйклмнпрстфхцчшщbcdfghjklmnpqrstvwxyz";
        return Value.ToLower().Count(c => consonants.Contains(c));
    }

    public override string ToString() => Value;
}