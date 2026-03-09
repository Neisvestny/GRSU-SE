using SkiaSharp;

public static class SentimentColorService
{
    public static SKColor GetColor(double value, double min, double max)
    {
        double n = (value - min) / (max - min);
        if (n < 0.5)
        {
            return new SKColor(255, (byte)(255 * (1 - n)), (byte)(255 * (1 - n)));
        }
        else
        {
            return new SKColor((byte)(255 * (1 - n)), 255, (byte)(255 * (1 - n)));
        }
    }
}
