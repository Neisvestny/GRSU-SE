using SkiaSharp;

public class State
{
	public string Code { get; }
	public Coordinates Center { get; }
	public SKColor Color { get; private set; } = new SKColor(191, 191, 191);
	public List<List<Coordinates>> Polygons { get; }

	private double _sumSentiment = 0;
	private int _countSentiment = 0;

	private readonly object _lock = new();

	public State(string code, Coordinates center, List<List<Coordinates>> polygons)
	{
		Code = code;
		Center = center;
		Polygons = polygons;
	}

	public void SetColor(SKColor color)
	{
		Color = color;
	}

	public void AddTweet(Tweet tweet)
	{
		if (!tweet.Weight.HasValue)
			return;

		lock (_lock)
		{
			_sumSentiment += tweet.Weight.Value;
			_countSentiment++;
		}
	}

	public double? GetAverageSentiment()
	{
		if (_countSentiment == 0)
			return null;

		return _sumSentiment / _countSentiment;
	}
}