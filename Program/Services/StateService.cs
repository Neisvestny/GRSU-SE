public class StateService
{
    private readonly List<State> _states;
    private readonly Dictionary<string, double> _averageSentimentsCache = new();

    public StateService(IEnumerable<State> states)
    {
        _states = states?.ToList() ?? throw new ArgumentNullException(nameof(states));
    }

    public State? GetClosestState(Tweet tweet)
    {
        if (tweet == null)
            throw new ArgumentNullException(nameof(tweet));

        double minDistance = double.MaxValue;
        State? closestState = null;

        foreach (var state in _states)
        {
            double distance = GeoService.HaversineDistance(
                tweet.Coordinates,
                state.Center);

            if (distance >= minDistance)
                continue;

            minDistance = distance;
            closestState = state;
        }

        return closestState;
    }

    public Dictionary<string, List<Tweet>> GroupTweetsByState(IEnumerable<Tweet> tweets)
    {
        if (tweets == null)
            throw new ArgumentNullException(nameof(tweets));

        var result = new Dictionary<string, List<Tweet>>();

        foreach (var tweet in tweets)
        {
            var state = GetClosestState(tweet);
            if (state is null)
                continue;

            var stateCode = state.Code;

            if (!result.TryGetValue(stateCode, out var list))
            {
                list = new List<Tweet>();
                result[stateCode] = list;
            }

            list.Add(tweet);
        }

        return result;
    }

    public IReadOnlyDictionary<string, double> CalculateAverageSentiments(
        IReadOnlyDictionary<string, List<Tweet>> tweetsByState)
    {
        if (tweetsByState == null)
            throw new ArgumentNullException(nameof(tweetsByState));

        _averageSentimentsCache.Clear();

        foreach (var (stateCode, tweets) in tweetsByState)
        {
            double sum = 0;
            int count = 0;

            foreach (var tweet in tweets)
            {
                if (!tweet.Weight.HasValue) 
                    continue;

                sum += tweet.Weight.Value;
                count++;
            }

            if (count == 0) 
                continue;

            _averageSentimentsCache[stateCode] = sum / count;
        }

        return _averageSentimentsCache;
    }

    public void AssignTweets(IEnumerable<Tweet> tweets)
    {
        foreach (var tweet in tweets)
        {
            var state = GetClosestState(tweet);
            state?.AddTweet(tweet);
        }
    }
}