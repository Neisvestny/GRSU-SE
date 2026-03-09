using System.Globalization;
using System.Text.RegularExpressions;

public class TweetParserService
{
    private static readonly Regex TweetRegex = new(
        @"\[(?<lat>-?\d+(?:\.\d+)?),\s*(?<lon>-?\d+(?:\.\d+)?)\]\s+_\s+(?<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+(?<text>.+)",
        RegexOptions.Compiled
    );

    private readonly SentimentService _sentimentService;
    private readonly string _dataPath;

    public TweetParserService(SentimentService sentimentService)
    {
        _sentimentService = sentimentService;

        _dataPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Data")
        );

        InitializeSentiments();
    }

    public List<Tweet> ReadTxtFileFromContent()
    {
        var files = GetSortedTxtFiles();
        string selectedFile = AskUserToChooseFile(files);
        return ParseTweets(selectedFile);
    }

    private void InitializeSentiments()
    {
        var sentiments = _sentimentService.LoadSentiments(_dataPath);
        Tweet.InitializeSentiments(sentiments);
    }

    private List<string> GetSortedTxtFiles()
    {
        return Directory
            .GetFiles(_dataPath, "*.txt")
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string AskUserToChooseFile(List<string> files)
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

            if (int.TryParse(input, out int index) && index >= 1 && index <= files.Count)
            {
                return files[index - 1];
            }

            Console.WriteLine("Invalid selection, try again.");
        }
    }

    private List<Tweet> ParseTweets(string filePath)
    {
        var tweets = new List<Tweet>();

        foreach (string line in File.ReadLines(filePath))
        {
            Match match = TweetRegex.Match(line);
            if (!match.Success)
                continue;

            double latitude = double.Parse(match.Groups["lat"].Value, CultureInfo.InvariantCulture);
            double longitude = double.Parse(
                match.Groups["lon"].Value,
                CultureInfo.InvariantCulture
            );
            DateTime timestamp = DateTime.Parse(
                match.Groups["date"].Value,
                CultureInfo.InvariantCulture
            );
            string text = match.Groups["text"].Value;

            var tweet = new Tweet(new Coordinates(latitude, longitude), timestamp, text);

            tweet.CalculateWeight();
            tweets.Add(tweet);
        }

        return tweets;
    }
}
