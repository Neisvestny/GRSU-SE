public class StateService
{
    private readonly List<State> _states;

    public StateService(List<State> states)
    {
        _states = states;
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

		foreach (var (stateCode, tweets) in tweetsByState)
		{
			var sentiments = tweets
				.Where(t => t.Weight is double)
				.Select(t => t.Weight!.Value);

			if (!sentiments.Any())
				continue;

			result[stateCode] = sentiments.Average();
		}

		return result;
	}
}