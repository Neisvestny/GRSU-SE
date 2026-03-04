using System;
using System.Text.RegularExpressions;

public class Tweet
{
	private static Dictionary<string, double> _globalSentiments;
	private static readonly Regex CleanRegex = new(@"[^\w\s]", RegexOptions.Compiled);

	public Coordinates Coordinates { get; }
	public DateTime Timestamp { get; }
	public string Text { get; }
	public double? Weight { get; private set; }

	public Tweet(Coordinates coordinates, DateTime timestamp, string text)
	{
		Coordinates = coordinates;
		Timestamp = timestamp;
		Text = text;
	}

	public static void InitializeSentiments(Dictionary<string, double> sentiments)
	{
		_globalSentiments = sentiments;
	}

	public void CalculateWeight()
	{
		if (_globalSentiments == null)
			throw new InvalidOperationException("Sentiments not initialized. Call InitializeSentiments first.");

		double total = 0;
		int count = 0;

		string cleaned = CleanRegex.Replace(Text, "");
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

				if (_globalSentiments.TryGetValue(phrase, out double score))
				{
					total += score;
					count++;
					i += size;
					found = true;
					break;
				}
			}

			if (!found)
				i++;
		}

		Weight = count > 0 ? total / count : null;
	}

	public override string ToString()
	{
		return $"Tweet\nText: {Text}\nLat: {Coordinates.Latitude}\nLon: {Coordinates.Longitude}\nTime: {Timestamp}\nWeight: {(Weight.HasValue ? Weight.Value : "None")}\n";
	}
}