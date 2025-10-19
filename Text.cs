using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GRSU_SE
{
    [Serializable]
    [XmlRoot("text")]
    public class Text
    {
        [XmlElement("sentence")]
        public List<Sentence> Sentences { get; set; } = new List<Sentence>();

        public Text() { }

        public Text(string input)
        {
            var matches = Regex.Split(input, @"(?<=[.!?])\s+");
            foreach (var sentenceText in matches)
            {
                if (!string.IsNullOrWhiteSpace(sentenceText))
                    Sentences.Add(new Sentence(sentenceText.Trim()));
            }
        }

        // 1️⃣ Предложения по возрастанию количества слов
        public IEnumerable<Sentence> GetSentencesByWordCount()
        {
            return Sentences.OrderBy(s => s.WordCount);
        }

        // 2️⃣ Предложения по возрастанию длины текста
        public IEnumerable<Sentence> GetSentencesByLength()
        {
            return Sentences.OrderBy(s => s.RawText.Length);
        }

        // 3️⃣ Слова заданной длины в вопросительных предложениях (без повторов)
        public IEnumerable<string> FindWordsInQuestionsByLength(int length)
        {
            return Sentences
                .Where(s => s.IsQuestion)
                .SelectMany(s => s.Words)
                .Select(w => w.Value.ToLower())
                .Where(w => w.Length == length)
                .Distinct();
        }

        // 4️⃣ Удалить слова заданной длины, начинающиеся с согласной
        public void RemoveWordsByLengthStartingWithConsonant(int length)
        {
            string consonants = "бвгджзйклмнпрстфхцчшщbcdfghjklmnpqrstvwxyz";

            foreach (var sentence in Sentences)
            {
                // Удаляем из Tokens слова, которые начинаются с согласной и имеют заданную длину
                sentence.Tokens = sentence.Tokens
                    .Where(t =>
                        !(t is Word w &&
                          w.Value.Length == length &&
                          consonants.Contains(char.ToLower(w.Value[0]))))
                    .ToList();
            }
        }

        // 5️⃣ В предложении заменить слова заданной длины на подстроку
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

        // 6️⃣ Удалить стоп-слова (из файла)
        public void RemoveStopWords(string stopWordsFile)
        {
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

        // 7️⃣ Экспорт в XML
        public string ExportToXml(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Text));
                using (var writer = new StreamWriter(filePath))
                    serializer.Serialize(writer, this);

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
    }
}
