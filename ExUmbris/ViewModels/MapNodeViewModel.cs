using ExUmbris.Models;

namespace ExUmbris.ViewModels;

public sealed class MapNodeViewModel : ViewModelBase
{
	public MapNodeViewModel()
	{
		Name = "Unknown";
		Coordinates = new(0.0, 0.0);
		ConnectedNodes = [];
	}

	public int Id { get; init; }
	public string Name { get; set; }
	public MapCoordinates Coordinates { get; set; }
	public IReadOnlyList<MapNodeViewModel> ConnectedNodes { get; set; }
}
