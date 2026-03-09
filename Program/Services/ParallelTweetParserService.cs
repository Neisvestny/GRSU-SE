using System.Collections.Concurrent;
using System.Globalization;

public class ParallelTweetParserService
{
	private readonly SentimentService _sentimentService;
	private readonly string _dataPath;

	private const int BatchSize = 50000;

	public ParallelTweetParserService(SentimentService sentimentService)
	{
		_sentimentService = sentimentService;

		_dataPath = Path.GetFullPath(Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"..", "..", "..", "..", "Data"
		));

		var sentiments = _sentimentService.LoadSentiments(_dataPath);
		if (sentiments != null)
			Tweet.InitializeSentiments(sentiments);
	}

	public async Task<List<Tweet>> ParseTweetsParallelAsync(string filePath)
	{
		var result = new List<Tweet>(200_000);
		var mergeLock = new object();

		using var reader = new StreamReader(filePath);
		var buffer = new List<string>(BatchSize);

		while (true)
		{
			buffer.Clear();

			for (int i = 0; i < BatchSize; i++)
			{
				var line = await reader.ReadLineAsync();
				if (line == null)
					break;

				buffer.Add(line);
			}

			if (buffer.Count == 0)
				break;

			Parallel.ForEach(
				Partitioner.Create(0, buffer.Count),
				new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
				() => new List<Tweet>(1024), // thread-local list
				(range, _, local) =>
				{
					for (int i = range.Item1; i < range.Item2; i++)
					{
						if (TryParseTweet(buffer[i], out var tweet))
						{
							tweet.CalculateWeight();
							local.Add(tweet);
						}
					}
					return local;
				},
				local =>
				{
					lock (mergeLock)
					{
						result.AddRange(local);
					}
				}
			);
		}

		return result;
	}

	private static bool TryParseTweet(string line, out Tweet tweet)
	{
		tweet = null!;

		ReadOnlySpan<char> span = line.AsSpan();

		// формат: [lat, lon] _ yyyy-MM-dd HH:mm:ss text

		if (span.Length < 30 || span[0] != '[')
			return false;

		int comma = span.IndexOf(',');
		if (comma < 0) return false;

		int close = span.IndexOf(']');
		if (close < 0) return false;

		var latSpan = span.Slice(1, comma - 1).Trim();
		var lonSpan = span.Slice(comma + 1, close - comma - 1).Trim();

		if (!double.TryParse(latSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
			return false;

		if (!double.TryParse(lonSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
			return false;

		int underscore = span.IndexOf('_');
		if (underscore < 0) return false;

		int dateStart = underscore + 2;

		if (span.Length < dateStart + 19)
			return false;

		var dateSpan = span.Slice(dateStart, 19);

		if (!DateTime.TryParse(dateSpan, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
			return false;

		int textStart = dateStart + 20;
		if (textStart >= span.Length)
			return false;

		string text = span.Slice(textStart).ToString();

		tweet = new Tweet(
			new Coordinates(lat, lon),
			time,
			text
		);

		return true;
	}
}