using System.Text.Json;

public class StateLoaderService
{
    public List<State> LoadStates()
    {
        string dataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Data"
        );

        dataPath = Path.GetFullPath(dataPath);
        string filePath = Path.Combine(dataPath, "states.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("states.json not found", filePath);

        string json = File.ReadAllText(filePath);

        using JsonDocument document = JsonDocument.Parse(json);

        var states = new List<State>();

        foreach (var stateProperty in document.RootElement.EnumerateObject())
        {
            string code = stateProperty.Name;
            var points = new List<Coordinates>();

            ExtractPoints(stateProperty.Value, points);

            if (points.Count == 0)
                throw new Exception($"State {code} has no coordinates");

            double avgLat = points.Average(p => p.Latitude);
            double avgLon = points.Average(p => p.Longitude);

            var center = new Coordinates(avgLat, avgLon);
            states.Add(new State(code, center));

            // Console.WriteLine(states.Last());
        }

        return states;
    }

    private void ExtractPoints(JsonElement element, List<Coordinates> points)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return;

        if (element.GetArrayLength() == 2 &&
            element[0].ValueKind == JsonValueKind.Number &&
            element[1].ValueKind == JsonValueKind.Number)
        {
            double lon = element[0].GetDouble();
            double lat = element[1].GetDouble();

            points.Add(new Coordinates(lat, lon));
            return;
        }

        foreach (var child in element.EnumerateArray())
        {
            ExtractPoints(child, points);
        }
    }
}
