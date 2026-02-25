public static class GeoService
{
    private const double EarthRadius = 6371;

    public static double HaversineDistance(Coordinates a, Coordinates b)
    {
        double dLat = ToRadians(b.Latitude - a.Latitude);
        double dLon = ToRadians(b.Longitude - a.Longitude);

        double lat1 = ToRadians(a.Latitude);
        double lat2 = ToRadians(b.Latitude);

        double h =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

        return EarthRadius * c;
    }

    private static double ToRadians(double angle)
    {
        return angle * Math.PI / 180;
    }
}