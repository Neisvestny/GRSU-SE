using System.Text.RegularExpressions;
using System.Globalization;

public class TweetParserService {
	private static readonly Regex TweetRegex = new(@"\[(?<lat>-?\d+(?:\.\d+)?),\s*(?<lon>-?\d+(?:\.\d+)?)\]\s+_\s+(?<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+(?<text>.+)", RegexOptions.Compiled);
	private readonly SentimentService _sentimentService;

    public TweetParserService(SentimentService sentimentService)
    {
        _sentimentService = sentimentService;
    }

	public List<Tweet> ReadTxtFileFromContent()
	{
		var tweets = new List<Tweet>();
		string contentPath = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"..", "..", "..", "..", "Data"
		);

		contentPath = Path.GetFullPath(contentPath);

		var sentiments = _sentimentService.LoadSentiments(contentPath);
		// Console.WriteLine($"Loaded {sentiments.Count} sentiments");
		Tweet.InitializeSentiments(sentiments);

		string[] txtFiles = Directory.GetFiles(contentPath, "*.txt");

		Console.WriteLine("Choose file: ");
		for (int i = 0; i < txtFiles.Length; i++)
			Console.WriteLine($"{i + 1}) {Path.GetFileName(txtFiles[i])}");
		
		int fileIndex;
		while (true)
		{
			Console.Write(">>> ");
			string? input = Console.ReadLine();

			if (int.TryParse(input, out fileIndex) &&
				fileIndex >= 1 &&
				fileIndex <= txtFiles.Length)
			{
				fileIndex--;
				break;
			}
			Console.WriteLine("Invalid selection, try again");
		}

		foreach (string line in File.ReadLines(txtFiles[fileIndex]))
		{
			Match match = TweetRegex.Match(line);

			if (!match.Success)
				continue;

			double latitude = double.Parse(match.Groups["lat"].Value, CultureInfo.InvariantCulture);
			double longitude = double.Parse(match.Groups["lon"].Value, CultureInfo.InvariantCulture);
			DateTime timestamp = DateTime.Parse(match.Groups["date"].Value, CultureInfo.InvariantCulture);
			string text = match.Groups["text"].Value;

			var tweet = new Tweet(
				new Coordinates(latitude, longitude),
				timestamp,
				text
			);

			tweet.CalculateWeight();

			tweets.Add(tweet);
			// Console.WriteLine(tweet);
		}

		return tweets;
	}
}