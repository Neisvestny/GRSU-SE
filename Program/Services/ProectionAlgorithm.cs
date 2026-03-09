public static class ProectionAlgorithm
{
    public const double fi1 = (29.5 * Math.PI)/ 180.0;
    public const double fi2 = (45.5 * Math.PI)/ 180.0;
    public const double fi0 = (38.0 * Math.PI)/ 180.0;
    public const double l0 = (-96.0 * Math.PI)/ 180.0;

    public static double n
    {
        get
        {
            return (Math.Sin(fi1) + Math.Sin(fi2))/2.0;
        }
    }

    public static double C
    {
        get
        {
            return Math.Pow(Math.Cos(fi1), 2.0) + 2*n * Math.Sin(fi1);
        }
    }

    public static double p0
    {
        get
        {
            return Math.Sqrt(C - 2*n * Math.Sin(fi0))/n;  
        }
    }

    private static (double rangeX, double rangeY) CalculatingRanges(double minX, double minY, double maxX, double maxY)
    {
        double rangeX = maxX-minX;
        double rangeY = maxY-minY;
        return (rangeX, rangeY);
    }

    private static double CalculatingScales(int width, int height, int pad, double minX, double minY, double maxX, double maxY)
    {
        double scaleX = (width - 2*pad)/CalculatingRanges(minX, minY, maxX, maxY).Item1;
        double scaleY = (height - 2*pad)/CalculatingRanges(minX, minY, maxX, maxY).Item2;
        return double.Min(scaleX, scaleY);
    }

    private static (double offsetX, double offsetY) CalculateOffsets(int pad, int width, int height, 
        double minX, double minY, double maxX, double maxY)
    {
        double offsetX = pad + ((width - 2*pad) - (CalculatingRanges(minX, minY, maxX, maxY).Item1 * CalculatingScales(width, height, pad, minX, minY, maxX, maxY)))/2;
        double offsetY =  pad + ((height - 2*pad) - (CalculatingRanges(minX, minY, maxX, maxY).Item2 * CalculatingScales(width, height, pad, minX, minY, maxX, maxY)))/2;
        return (offsetX, offsetY);
    }

    public static (double pixelX, double pixelY) CalculatingPixels(int pad, int width, int height, double x, 
        double y, double minX, double minY, double maxX, double maxY)
    {
        double pixelX = CalculateOffsets(pad, width, height, minX, minY, maxX, maxY).Item1 + (x - minX) * CalculatingScales(width, height, pad, minX, minY, maxX, maxY);
        double pixelY = CalculateOffsets(pad, width, height, minX, minY, maxX, maxY).Item2 + (maxY - y) * CalculatingScales(width, height, pad, minX, minY, maxX, maxY);
        return (pixelX, pixelY);
    }
}