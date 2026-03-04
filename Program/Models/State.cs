public class State
{
	public string Code { get; }
	public Coordinates Center { get; }
	public string Color { get; } = String.Empty; 

	public State(string code, Coordinates center)
	{
		Code = code;
		Center = center;
	}

	public override string ToString()
	{
		return $"State\nCode: {Code}\nLat: {Center.Latitude:F6}\nLon: {Center.Longitude:F6}\n";
	}
}