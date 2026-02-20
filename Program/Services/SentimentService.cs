using System.Text.RegularExpressions;
using System.Globalization;

public class SentimentService {
	private static readonly Regex CleanRegex = new(@"[^\w\s]", RegexOptions.Compiled);

	public Dictionary<string, double> LoadSentiments(string contentPath)
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

	public double? CalculateWeight(string text, Dictionary<string, double> sentiments)
	{
		double total = 0;
		int count = 0;

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
					count++;
					i += size;
					found = true;
					break;
				}
			}

			if (!found)
				i++;
		}

		if (count == 0)
			return null;

		return total / count;
	}
}