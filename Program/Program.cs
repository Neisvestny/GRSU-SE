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
	}
}
