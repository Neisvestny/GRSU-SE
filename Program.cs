using GRSU_SE;
using System.Text;

internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string input = "Привет! Как твои дела? Это просто тест. Какой прекрасный день!";
        var text = new Text(input);

        Console.WriteLine("Исходный текст:");
        Console.WriteLine(text);
        Console.WriteLine();

        Console.WriteLine("1. Предложения в порядке возрастания количества слов:");
        foreach (var sentence in text.GetSentencesByWordCount())
        {
            Console.WriteLine($"{sentence}");
        }
        Console.WriteLine();

        Console.WriteLine("2. Предложения в порядке возрастания длины:");
        foreach (var sentence in text.GetSentencesByLength())
        {
            Console.WriteLine($"{sentence}");
        }
        Console.WriteLine();

        Console.WriteLine("3. Слова длиной 4 в вопросительных предложениях:");
        foreach (var word in text.FindWordsInQuestionsByLength(4))
        {
            Console.WriteLine(word);
        }
        Console.WriteLine();

        text.RemoveWordsByLengthStartingWithConsonant(4);
        Console.WriteLine(text);
        Console.WriteLine();

        Console.WriteLine("5. После замены слов длиной 5 во 2-м предложении на 'ЗАМЕНА':");
        text = new Text(input);
        text.ReplaceWordsInSentence(1, 5, "ЗАМЕНА");
        Console.WriteLine(text);
        Console.WriteLine();

        Console.WriteLine("6. После удаления стоп-слов:");
        text = new Text(input);
        text.RemoveStopWords("C:\\Programming\\Csh\\GRSU-SE\\content\\stopwords_ru.txt");
        Console.WriteLine(text);
        Console.WriteLine();

        Console.WriteLine("7. Экспорт в XML:");
        string result = text.ExportToXml(@"..\..\..\content\output.xml");
        Console.WriteLine(result);
    }
}