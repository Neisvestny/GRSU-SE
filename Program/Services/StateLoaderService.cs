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

			var rings = new List<List<Coordinates>>();

			ExtractRings(stateProperty.Value, rings);

			if (rings.Count == 0)
				throw new Exception($"State {code} has no rings");

			var allPoints = rings.SelectMany(r => r).ToList();

			double avgLat = allPoints.Average(p => p.Latitude);
			double avgLon = allPoints.Average(p => p.Longitude);

			var center = new Coordinates(avgLat, avgLon);

			states.Add(new State(code, center, rings));
		}

		return states;
	}

	private void ExtractRings(JsonElement element, List<List<Coordinates>> rings)
	{
		if (element.ValueKind != JsonValueKind.Array)
			return;

		if (element.GetArrayLength() > 0 &&
			element[0].ValueKind == JsonValueKind.Array &&
			element[0].GetArrayLength() == 2 &&
			element[0][0].ValueKind == JsonValueKind.Number)
		{
			var ring = new List<Coordinates>();

			foreach (var coord in element.EnumerateArray())
			{
				double lon = coord[0].GetDouble();
				double lat = coord[1].GetDouble();

				ring.Add(new Coordinates(lat, lon));
			}

			rings.Add(ring);
			return;
		}

		foreach (var child in element.EnumerateArray())
		{
			ExtractRings(child, rings);
		}
	}
}
