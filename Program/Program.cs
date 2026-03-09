using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SkiaSharp;

class Program
{
    static async Task Main(string[] args)
    {
        var russianCulture = new CultureInfo("ru-RU");
        russianCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = russianCulture;
        Thread.CurrentThread.CurrentUICulture = russianCulture;

        Console.WriteLine("Загрузка данных...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var stateLoader = new StateLoaderService();
            List<State> states = stateLoader.LoadStates();
            Console.WriteLine($"Загружено {states.Count} штатов");

            var sentimentService = new SentimentService();
            var parallelParser = new ParallelTweetParserService(sentimentService);

            string dataPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "Data"
            ));

            var files = GetSortedTxtFiles(dataPath);
            string selectedFile = AskUserToChooseFile(files);

            Console.WriteLine($"Начинаем параллельный парсинг файла: {Path.GetFileName(selectedFile)}");

            List<Tweet> tweets = await parallelParser.ParseTweetsParallelAsync(selectedFile);

            Console.WriteLine($"Загружено {tweets.Count} твитов за {stopwatch.Elapsed.TotalSeconds:F2} сек");

            var optimizedStateService = new OptimizedStateService(states);
            await optimizedStateService.AssignTweetsParallelAsync(tweets);

            // освобождаем память
            tweets.Clear();
            tweets = null;

            // один проход вместо 3
            double min = double.MaxValue;
            double max = double.MinValue;
            bool hasValues = false;

            foreach (var state in states)
            {
                var avg = state.GetAverageSentiment();
                if (!avg.HasValue)
                    continue;

                hasValues = true;

                double value = avg.Value;

                if (value < min) min = value;
                if (value > max) max = value;
            }

            if (!hasValues)
            {
                Console.WriteLine("Нет данных для sentiment");
                return;
            }

            // быстрее чем обычный Parallel.ForEach
            Parallel.For(0, states.Count, i =>
            {
                var state = states[i];
                var avg = state.GetAverageSentiment();

                if (!avg.HasValue)
                    return;

                var color = SentimentColorService.GetColor(avg.Value, min, max);
                state.SetColor(color);
            });

            string mapOutputPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "Output",
                "us_states.png"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(mapOutputPath)!);

            var mapRenderer = new MapRendererService(states);
            mapRenderer.RenderMap(mapOutputPath);

            Console.WriteLine($"Карта создана: {mapOutputPath}");
            Console.WriteLine($"Общее время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} сек");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static List<string> GetSortedTxtFiles(string dataPath)
    {
        var files = Directory.GetFiles(dataPath, "*.txt");
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        return files.ToList();
    }

    private static string AskUserToChooseFile(List<string> files)
    {
        if (files.Count == 0)
            throw new Exception("No .txt files found in Data directory.");

        Console.WriteLine("Choose file:");

        for (int i = 0; i < files.Count; i++)
            Console.WriteLine($"{i + 1}) {Path.GetFileName(files[i])}");

        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int index) &&
                index >= 1 &&
                index <= files.Count)
            {
                return files[index - 1];
            }

            Console.WriteLine("Invalid selection, try again.");
        }
    }
}