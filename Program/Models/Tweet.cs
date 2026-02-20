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
}