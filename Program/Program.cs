using System;
using System.IO;
using System.Globalization;
using System.Threading;

class Program
{
	static void Main(string[] args)
	{
		var russianCulture = new CultureInfo("ru-RU");
		russianCulture.NumberFormat.NumberDecimalSeparator = ".";
		Thread.CurrentThread.CurrentCulture = russianCulture;
		Thread.CurrentThread.CurrentUICulture = russianCulture;

		var sentimentService = new SentimentService();
		var parser = new TweetParserService(sentimentService);

		List<Tweet> tweets = parser.ReadTxtFileFromContent();

		var stateLoader = new StateLoaderService();
		List<State> states = stateLoader.LoadStates();

		var stateService = new StateService(states);

		var grouped = stateService.GroupTweetsByState(tweets);

		// foreach (var (stateCode, tweetsInState) in grouped)
		// {
		// 	Console.WriteLine($"{stateCode}: ");

		// 	foreach (var tweet in tweetsInState)
		// 	{
		// 		Console.WriteLine($"[{tweet.Timestamp}] {tweet.Text}");
		// 		Console.WriteLine($"Weight: {tweet.Weight}");
		// 		Console.WriteLine();
		// 	}

		// 	Console.WriteLine();
		// }

		// var averageTweetsWeight = stateService.CalculateAverageSentiments(grouped);
		// foreach (var (stateCode, stateWeight) in averageTweetsWeight) {
		// 	Console.WriteLine($"{stateCode}: {stateWeight}");
		// }
	}
}
