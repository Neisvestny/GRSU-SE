using System;

public class Tweet
{
    public Coordinates Coordinates { get; }
    public DateTime Timestamp { get; }
    public string Text { get; }
    public double? Weight { get; }

    public Tweet(Coordinates coordinates, DateTime timestamp, string text, double? weight)
    {
        Coordinates = coordinates;
        Timestamp = timestamp;
        Text = text;
        Weight = weight;
    }

	public override string ToString()
	{
		return $"Tweet\nText: {Text}\nLat: {Coordinates.Latitude}\nLon: {Coordinates.Longitude}\nTime: {Timestamp}\nWeight: {(Weight.HasValue ? Weight.Value : "None")}\n";
	}
}