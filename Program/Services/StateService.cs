public class StateService
{
    private readonly List<State> _states;
    private readonly Dictionary<string, double> _averageSentimentsCache;

    public StateService(List<State> states)
    {
        _states = states;
        _averageSentimentsCache = new Dictionary<string, double>();
    }

    public string? GetClosestState(Tweet tweet)
    {
        double minDistance = double.MaxValue;
        string? closestState = null;

        foreach (var state in _states)
        {
            double distance = GeoService.HaversineDistance(
                tweet.Coordinates,
                state.Center);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestState = state.Code;
            }
        }

        return closestState;
    }

    public Dictionary<string, List<Tweet>> GroupTweetsByState(List<Tweet> tweets)
    {
        var result = new Dictionary<string, List<Tweet>>();
        
        foreach (var tweet in tweets)
        {
            string? stateCode = GetClosestState(tweet);
            if (stateCode == null) continue;
            
            if (!result.ContainsKey(stateCode))
                result[stateCode] = new List<Tweet>();
                
            result[stateCode].Add(tweet);
        }
        
        return result;
    }

	public Dictionary<string, double> CalculateAverageSentiments(
		Dictionary<string, List<Tweet>> tweetsByState)
	{
		var result = new Dictionary<string, double>();
		_averageSentimentsCache.Clear();

		foreach (var (stateCode, tweets) in tweetsByState)
		{
			var validTweets = tweets.Where(t => t.Weight.HasValue);
			
			if (!validTweets.Any())
				continue;

			double average = validTweets.Average(t => t.Weight!.Value);
			result[stateCode] = average;
			_averageSentimentsCache[stateCode] = average;
		}

		return result;
	}
}