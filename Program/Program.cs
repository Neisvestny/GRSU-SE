using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

class Program
{
	static readonly Regex CleanRegex = new(@"[^\w\s]", RegexOptions.Compiled);
	static readonly Regex TweetRegex = new(@"\[(?<lat>-?\d+(?:\.\d+)?),\s*(?<lon>-?\d+(?:\.\d+)?)\]\s+_\s+(?<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+(?<text>.+)",
        RegexOptions.Compiled);

	static double CalculateWeight(string text, Dictionary<string, double> sentiments)
	{
		double total = 0;

		string cleaned = CleanRegex.Replace(text, "");
		string[] words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		int maxGram = 4;
		int i = 0;

		while (i < words.Length)
		{
			bool found = false;

			for (int size = maxGram; size >= 1; size--)
			{
				if (i + size > words.Length)
					continue;

				string phrase = string.Join(" ", words, i, size);

				if (sentiments.TryGetValue(phrase, out double score))
				{
					total += score;
					i += size;
					found = true;
					break;
				}
			}

			if (!found)
				i++;
		}

		return total;
	}

	static Dictionary<string, double> LoadSentiments(string contentPath)
	{
		string filePath = Path.Combine(contentPath, "sentiments.csv");

		if (!File.Exists(filePath))
		{
			Console.WriteLine("sentiments.csv not found!");
			return new Dictionary<string, double>();
		}

		var sentiments = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
		
		foreach (string line in File.ReadLines(filePath))
		{
			if (string.IsNullOrWhiteSpace(line))
				continue;

			string[] parts = line.Split(',', 2);

			if (parts.Length != 2)
				continue;

			string word = parts[0].Trim();
			double value = double.Parse(parts[1], CultureInfo.InvariantCulture);

			sentiments[word] = value;
		}

		return sentiments;
	}

	static void ReadTxtFileFromContent()
	{
		string contentPath = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"..", "..", "..", "..", "Data"
		);

		contentPath = Path.GetFullPath(contentPath);

		var sentiments = LoadSentiments(contentPath);
		Console.WriteLine($"Loaded {sentiments.Count}");

		string[] txtFiles = Directory.GetFiles(contentPath, "*.txt");

		Console.WriteLine("Choose file: ");
		for (int i = 0; i < txtFiles.Length; i++)
			Console.WriteLine($"{i + 1}) {Path.GetFileName(txtFiles[i])}");

		Console.Write(">>> ");
		if (!int.TryParse(Console.ReadLine(), out int fileIndex) ||
			fileIndex < 1 || fileIndex > txtFiles.Length)
		{
			Console.WriteLine("Invalid selection");
			return;
		}

		fileIndex--;

		foreach (string line in File.ReadLines(txtFiles[fileIndex]))
		{
			Match match = TweetRegex.Match(line);

			if (!match.Success)
				continue;

			double latitude = double.Parse(match.Groups["lat"].Value, CultureInfo.InvariantCulture);
			double longitude = double.Parse(match.Groups["lon"].Value, CultureInfo.InvariantCulture);
			DateTime timestamp = DateTime.Parse(match.Groups["date"].Value, CultureInfo.InvariantCulture);
			string text = match.Groups["text"].Value;


			double weight = CalculateWeight(text, sentiments);
			var tweet = new Tweet(
				new Coordinates(latitude, longitude),
				timestamp,
				text,
				weight
			);

			Console.WriteLine("Tweet");
			Console.WriteLine($"Text: {tweet.Text}");
			Console.WriteLine($"Lat: {tweet.Coordinates.Latitude}");
			Console.WriteLine($"Lon: {tweet.Coordinates.Longitude}");
			Console.WriteLine($"Time: {tweet.Timestamp}");
			Console.WriteLine($"Weight: {tweet.Weight}");
			Console.WriteLine();
		}
	}

	static void Main(string[] args)
	{
		var russianCulture = new CultureInfo("ru-RU");
		russianCulture.NumberFormat.NumberDecimalSeparator = ".";

		Thread.CurrentThread.CurrentCulture = russianCulture;
		Thread.CurrentThread.CurrentUICulture = russianCulture;

		ReadTxtFileFromContent();
	}
}
