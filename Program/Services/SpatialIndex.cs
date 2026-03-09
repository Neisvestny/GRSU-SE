public class SpatialIndex
{
    private readonly List<State> _states;
    private readonly Dictionary<State, BoundingBox> _boxes;

    public SpatialIndex(List<State> states)
    {
        _states = states;
        _boxes = new(states.Count);

        foreach (var state in states)
        {
            _boxes[state] = CalculateBoundingBox(state);
        }
    }

    public State? FindClosestState(Coordinates point)
    {
        State? bestState = null;
        double bestDistance = double.MaxValue;

        foreach (var state in _states)
        {
            var box = _boxes[state];

            if (!box.Contains(point))
                continue;

            double distance = GeoService.HaversineDistance(point, state.Center);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestState = state;
            }
        }

        if (bestState != null)
            return bestState;

        foreach (var state in _states)
        {
            double distance = GeoService.HaversineDistance(point, state.Center);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestState = state;
            }
        }

        return bestState;
    }

    private BoundingBox CalculateBoundingBox(State state)
    {
        double minLat = double.MaxValue;
        double maxLat = double.MinValue;
        double minLon = double.MaxValue;
        double maxLon = double.MinValue;

        foreach (var polygon in state.Polygons)
        {
            foreach (var coord in polygon)
            {
                if (coord.Latitude < minLat) minLat = coord.Latitude;
                if (coord.Latitude > maxLat) maxLat = coord.Latitude;
                if (coord.Longitude < minLon) minLon = coord.Longitude;
                if (coord.Longitude > maxLon) maxLon = coord.Longitude;
            }
        }

        return new BoundingBox(minLat, maxLat, minLon, maxLon);
    }

    private class BoundingBox
    {
        public double MinLat { get; }
        public double MaxLat { get; }
        public double MinLon { get; }
        public double MaxLon { get; }

        public BoundingBox(double minLat, double maxLat, double minLon, double maxLon)
        {
            MinLat = minLat;
            MaxLat = maxLat;
            MinLon = minLon;
            MaxLon = maxLon;
        }

        public bool Contains(Coordinates p)
        {
            return p.Latitude >= MinLat &&
                   p.Latitude <= MaxLat &&
                   p.Longitude >= MinLon &&
                   p.Longitude <= MaxLon;
        }
    }
}