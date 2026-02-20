public class Coordinates
{
    private double latitude;
    private double longitude;

    public Coordinates() {}
    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude
    {
        get => latitude;
        set => latitude = value;
    }

    public double Longitude
    {
        get => longitude;
        set => longitude = value;
    }
}
