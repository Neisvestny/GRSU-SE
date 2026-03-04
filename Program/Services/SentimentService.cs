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
}