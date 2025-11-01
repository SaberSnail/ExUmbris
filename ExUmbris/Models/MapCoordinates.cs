namespace ExUmbris.Models;

public sealed class MapCoordinates
{
	public MapCoordinates(double x, double y)
	{
		X = x;
		Y = y;
	}

	public double X { get; }
	public double Y { get; }

	public double SquareDistanceTo(MapCoordinates that)
	{
		double dx = that.X - X;
		double dy = that.Y - Y;
		return dx * dx + dy * dy;
	}
}
