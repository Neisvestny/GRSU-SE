public struct Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(Coordinates other)
    {
        return GeoService.HaversineDistance(this, other);
    }
}
