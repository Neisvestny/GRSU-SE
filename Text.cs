using System.Xml.Serialization;

[Serializable]
[XmlRoot()]
public class Text
{
    [XmlElement()]
    public List<Sentence> Sentences { get; set; } = new List<Sentence>();

    public Text()
    { }

    public Text(string input)
    {
        Sentences = Parser.ParseText(input).Sentences;
    }

    public IEnumerable<Sentence> GetSentencesByWordCount()
    {
        return Sentences.OrderBy(s => s.WordCount);
    }

    public IEnumerable<Sentence> GetSentencesByLength()
    {
        return Sentences.OrderBy(s => s.ToString().Length);
    }

    public IEnumerable<string> FindWordsInQuestionsByLength(int length)
    {
        return Sentences
            .Where(s => s.IsQuestion)
            .SelectMany(s => s.Words)
            .Select(w => w.Value.ToLower())
            .Where(w => w.Length == length)
            .Distinct();
    }

    public void RemoveWordsByLengthStartingWithConsonant(int length)
    {
        string consonants = "бвгджзйклмнпрстфхцчшщbcdfghjklmnpqrstvwxyz";

        foreach (var sentence in Sentences)
        {
            sentence.Tokens = sentence.Tokens
                .Where(t => !(t is Word w &&
                             w.Value.Length == length &&
                             consonants.Contains(char.ToLower(w.Value[0]))))
                .ToList();
        }
    }

    public void ReplaceWordsInSentence(int sentenceIndex, int wordLength, string replacement)
    {
        if (sentenceIndex < 0 || sentenceIndex >= Sentences.Count)
            throw new ArgumentOutOfRangeException(nameof(sentenceIndex));

        var sentence = Sentences[sentenceIndex];

        for (int i = 0; i < sentence.Tokens.Count; i++)
        {
            if (sentence.Tokens[i] is Word w && w.Value.Length == wordLength)
                sentence.Tokens[i] = new Word(replacement);
        }
    }

    public void RemoveStopWords(string stopWordsFile)
    {
        if (!File.Exists(stopWordsFile))
            return;

        var stopWords = File.ReadAllLines(stopWordsFile)
            .Select(w => w.Trim().ToLower())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToHashSet();

        foreach (var sentence in Sentences)
        {
            sentence.Tokens.RemoveAll(t =>
                t is Word w && stopWords.Contains(w.Value.ToLower()));
        }
    }

    public string ExportToXml(string filePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var serializer = new XmlSerializer(typeof(Text));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, this);
            }

            return $"Текст успешно экспортирован в XML-файл: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Ошибка при экспорте в XML: {ex.Message}";
        }
    }

    public override string ToString()
    {
        return string.Join(" ", Sentences.Select(s => s.ToString()));
    }

    public string GetStatistics()
    {
        int totalWords = Sentences.Sum(s => s.WordCount);
        int totalChars = ToString().Length;
        int questionSentences = Sentences.Count(s => s.IsQuestion);

        return $"Предложений: {Sentences.Count}, Слов: {totalWords}, Символов: {totalChars}\n" +
               $"Вопросительных предложений: {questionSentences}";
    }
}