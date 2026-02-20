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

		parser.ReadTxtFileFromContent();
	}
}
