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

    static string GetText(string path)
    {
        string text = File.ReadAllText(path + ".txt");
        return text;
    }

    static (List<string>, List<Color>) CountColors(string text)
    {
        var colors = new List<Color>();
        var coloredWords = new List<string>();

        var words = Regex.Matches(text, @"\b[\p{IsCyrillic}a-zA-Z]+\b");

        foreach (Match wordMatch in words)
        {
            string word = wordMatch.Value.ToLower();

            foreach (var kvp in ColorMap)
            {
                if (Regex.IsMatch(word, $@"^{kvp.Key}(?:еньк)?(ий|ый|ой|ая|ое|ую|ого|ые|их|им|овело)?$", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine(word);
                    coloredWords.Add(word);
                    colors.Add(kvp.Value);
                    break;
                }
            }
        }

        return (coloredWords, colors);
    }

    static void DrawColors(List<Color> colors, string outputFile = "colors")
    {
        if (colors.Count == 0)
        {
            Console.WriteLine("Нет цветов для отрисовки.");
            return;
        }

        int squareSize = 50;
        int columns = (int)Math.Ceiling(Math.Sqrt(colors.Count));
        int rows = (int)Math.Ceiling(colors.Count / (double)columns);

        int width = columns * squareSize;
        int height = rows * squareSize;

        using (Bitmap bmp = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);

            for (int i = 0; i < colors.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                Rectangle rect = new Rectangle(col * squareSize, row * squareSize, squareSize, squareSize);
                using (Brush brush = new SolidBrush(colors[i]))
                {
                    g.FillRectangle(brush, rect);
                }

            }

            bmp.Save(outputFile + ".png", ImageFormat.Png);
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string path = "Podarok";
        string text = GetText(path);

        var arrayColor = CountColors(text);
        DrawColors(arrayColor.Item2, path);
    }
}
