using System.Collections.Concurrent;

public class OptimizedStateService
{
    private readonly List<State> _states;
    private readonly SpatialIndex _spatialIndex;

    public OptimizedStateService(IEnumerable<State> states)
    {
        _states = states?.ToList() ?? throw new ArgumentNullException(nameof(states));
        _spatialIndex = new SpatialIndex(_states);
    }

    public State? GetClosestStateOptimized(Tweet tweet)
    {
        return _spatialIndex.FindClosestState(tweet.Coordinates);
    }

    public async Task AssignTweetsParallelAsync(IEnumerable<Tweet> tweets)
    {
        var tweetsList = tweets as List<Tweet> ?? tweets.ToList();
        var partitioner = Partitioner.Create(0, tweetsList.Count);

        await Task.Run(() =>
        {
            Parallel.ForEach(
                partitioner,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var tweet = tweetsList[i];
                        var state = _spatialIndex.FindClosestState(tweet.Coordinates);
                        state?.AddTweet(tweet);
                    }
                }
            );
        });
    }
}
