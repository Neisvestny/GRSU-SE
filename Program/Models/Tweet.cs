using System.Text;

public class Tweet
{
	private static Dictionary<string, double>? _sentiments;

	public Coordinates Coordinates { get; }
	public DateTime Timestamp { get; }
	public string Text { get; }

	public double? Weight { get; private set; }

	private double? _cachedWeight;

	public Tweet(Coordinates coordinates, DateTime timestamp, string text)
	{
		Coordinates = coordinates;
		Timestamp = timestamp;
		Text = text;
	}

	public static void InitializeSentiments(Dictionary<string, double> sentiments)
	{
		_sentiments = sentiments;
	}

	public void CalculateWeight()
	{
		CalculateWeightOptimized();
	}

	public void CalculateWeightOptimized()
	{
		if (_cachedWeight.HasValue)
		{
			Weight = _cachedWeight;
			return;
		}

		if (_sentiments == null)
			throw new InvalidOperationException("Sentiments not initialized.");

		double total = 0;
		int count = 0;

		Span<char> buffer = stackalloc char[Text.Length];
		int len = 0;

		foreach (char c in Text)
		{
			if (char.IsLetterOrDigit(c) || c == ' ')
				buffer[len++] = char.ToLowerInvariant(c);
		}

		string cleaned = new string(buffer[..len]);

		var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		int i = 0;

		while (i < words.Length)
		{
			bool found = false;

			int remaining = words.Length - i;
			int maxGram = remaining >= 4 ? 4 : remaining;

			for (int size = maxGram; size >= 1; size--)
			{
				string phrase;

				if (size == 1)
					phrase = words[i];
				else if (size == 2)
					phrase = words[i] + " " + words[i + 1];
				else if (size == 3)
					phrase = words[i] + " " + words[i + 1] + " " + words[i + 2];
				else
					phrase = words[i] + " " + words[i + 1] + " " + words[i + 2] + " " + words[i + 3];

				if (_sentiments.TryGetValue(phrase, out double score))
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
		_cachedWeight = Weight;
	}

	public override string ToString()
	{
		return $"Tweet: {Text} | Weight: {Weight}";
	}
}