using GRSU_SE;
using System;
using System.Text;

class Program
{
    static string contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\content");
    static int fileID;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string input = "Привет! Как твои дела? Это просто тест.";
        string filePath = GetTextFilePath();

        if (string.IsNullOrEmpty(filePath))
            return;

        //string input = File.ReadAllText(filePath, Encoding.UTF8);

        var text = new Text(input);

        Console.WriteLine("1. По количеству слов:");
        foreach (var s in text.GetSentencesByWordCount())
            Console.WriteLine(s);

        Console.WriteLine("\n2. По длине предложения:");
        foreach (var s in text.GetSentencesByLength())
            Console.WriteLine(s);

        Console.WriteLine("\n3. Слова длиной 4 в вопросительных предложениях:");
        foreach (var w in text.FindWordsInQuestionsByLength(4))
            Console.WriteLine(w);

        text.RemoveWordsByLengthStartingWithConsonant(4);
        Console.WriteLine("\n4. После удаления слов длиной 4, начинающихся с согласной:");
        Console.WriteLine(text);

        text.ReplaceWordsInSentence(1, 4, "замена");
        Console.WriteLine("\n5. После замены слов длиной 4 во 2-м предложении:");
        Console.WriteLine(text);

        text.RemoveStopWords(filePath);
        Console.WriteLine("\n6. После удаления стоп-слов в тексте:");
        Console.WriteLine(text);

        string result = text.ExportToXml(@"..\..\..\content\output.xml");
        Console.WriteLine("\n" + result);

        Console.WriteLine("\n\n" + text);
    }

    static string GetTextFilePath()
    {
        if (!Directory.Exists(contentPath))
        {
            Console.WriteLine($"Папка не найдена: {contentPath}");
            return string.Empty;
        }

        string[] textFiles = Directory.GetFiles(contentPath, "*.txt");

        if (textFiles.Length == 0)
        {
            Console.WriteLine("В папке нет текстовых файлов (.txt)");
            return string.Empty;
        }

        Console.WriteLine("Выберите файл для чтения:");
        for (int i = 0; i < textFiles.Length; i++)
            Console.WriteLine($"{i + 1}) {Path.GetFileName(textFiles[i])}");

        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out fileID) &&
                fileID >= 1 &&
                fileID <= textFiles.Length)
            {
                fileID--;
                break;
            }

            Console.WriteLine("Неверный ввод! Попробуйте снова.");
        }

        string selectedFile = textFiles[fileID];
        Console.WriteLine($"\nВы выбрали: {Path.GetFileName(selectedFile)}");
        return selectedFile;
    }
}
