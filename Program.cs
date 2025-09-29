#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

class Program
{

    static Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "красн", Color.Red },
        { "ал", Color.Crimson },
        { "багр", Color.DarkRed },
        { "зелен", Color.Green },
        { "изумруд", Color.MediumSeaGreen },
        { "малахит", Color.MediumSeaGreen },
        { "син", Color.Blue },
        { "голуб", Color.LightBlue },
        { "лазур", Color.LightSkyBlue },
        { "ультрамарин", Color.Blue },
        { "желт", Color.Yellow },
        { "золот", Color.Gold },
        { "лимонн", Color.LemonChiffon },
        { "бел", Color.White },
        { "черн", Color.Black },
        { "сер", Color.Gray },
        { "фиолетов", Color.Purple },
        { "лилов", Color.Purple },
        { "оранжев", Color.Orange },
        { "коричнев", Color.Brown },
        { "розов", Color.Pink },
        { "бирюз", Color.Turquoise },
    };

    static (string Name, string FullPath)[] FindAllTxt()
    {
        string buildFolder = AppDomain.CurrentDomain.BaseDirectory;

        return Directory.GetFiles(buildFolder, "*.txt", SearchOption.AllDirectories)
                        .Select(path => (Path.GetFileNameWithoutExtension(path), path))
                        .ToArray();
    }

    static string GetText(string path)
    {
        string text = File.ReadAllText(path);
        return text;
    }

    static (List<string> coloredWords, List<Color> colors) FindColors(string text)
    {
        var colorsList = new List<Color>();
        var coloredWords = new List<string>();

        var pattern = @"^(" + string.Join("|", ColorMap.Keys.Select(Regex.Escape)) +
                      @")(?:еньк)?(ий|ый|ой|ая|ое|ую|ого|ые|их|им|овело|окурые)?$";
        var colorRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (Match wordMatch in Regex.Matches(text, @"\b[\p{IsCyrillic}a-zA-Z]+\b"))
        {
            var match = colorRegex.Match(wordMatch.Value);
            if (match.Success)
            {
                string key = match.Groups[1].Value;
                string wordLower = wordMatch.Value.ToLower();

                coloredWords.Add(wordLower);
                colorsList.Add(ColorMap[key]);

                Console.WriteLine(wordLower);
            }
        }

        return (coloredWords, colorsList);
    }


    static void DrawColors(List<Color> colors, string outputFile = "colors")
    {
        if (colors.Count == 0)
        {
            Console.WriteLine("Нет цветов для отрисовки.");
            return;
        }

        int squareSize = 50;
        int gridSize = (int)Math.Ceiling(Math.Sqrt(colors.Count));

        try
        {
            using Bitmap bmp = new Bitmap(gridSize * squareSize, gridSize * squareSize);
            using Graphics g = Graphics.FromImage(bmp);

            for (int i = 0; i < colors.Count; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                using var brush = new SolidBrush(colors[i]);
                g.FillRectangle(brush, col * squareSize, row * squareSize, squareSize, squareSize);

            }

            bmp.Save(outputFile + ".png", ImageFormat.Png);

            Console.WriteLine("Изображение сохранено успешно!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Произошла ошибка при сохранении: {e.Message}");
        }

    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Выберите файл, из которого хотите получить цвета: ");
        var files = FindAllTxt();
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {files[i].Name}");
        }

        int choice;
        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();


            if (int.TryParse(input, out choice) &&
                choice >= 1 && choice <= files.Length)
            {
                break;
            }

            Console.WriteLine("Некорректный ввод. Введите число от 1 до " + files.Length);
        }

        var selectedFile = files[choice - 1];

        Console.WriteLine($"Выбран файл: {selectedFile.Name}");

        string text = GetText(selectedFile.FullPath);
        var arrayColor = FindColors(text);
        DrawColors(arrayColor.Item2, selectedFile.Name);
    }
}
