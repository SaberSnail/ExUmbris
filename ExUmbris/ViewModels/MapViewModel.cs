using ExUmbris.Models;

namespace ExUmbris.ViewModels;

public sealed class MapViewModel : ViewModelBase
{
	public MapViewModel()
	{
		m_locations = [];
		m_zoom = 1.0;
		m_center = new MapCoordinates(0.0, 0.0);
	}

	public IReadOnlyList<LocationViewModel> Locations
	{
		get => VerifyAccess(m_locations);
		private set => SetPropertyField(value, ref m_locations);
	}

	public double Zoom
	{
		get => VerifyAccess(m_zoom);
		set => SetPropertyField(value, ref m_zoom);
	}

	public MapCoordinates Center
	{
		get => VerifyAccess(m_center);
		set => SetPropertyField(value, ref m_center);
	}

	public void Initialize(IGameFactory gameFactory, Random rng, int locationCount)
	{
		var locations = gameFactory.CreateLocations(rng, locationCount);
		Locations = locations;
	}

	public IReadOnlyList<LocationViewModel> GetShortestRoute(LocationViewModel sourceLocation, LocationViewModel destinationLocation)
	{
		var routeQueue = new PriorityQueue<LocationViewModel, double>();
		var cameFrom = new Dictionary<LocationViewModel, LocationViewModel?>();
		var costSoFar = new Dictionary<LocationViewModel, double>();

		routeQueue.Enqueue(sourceLocation, 0.0);
		cameFrom[sourceLocation] = null;
		costSoFar[sourceLocation] = 0.0;

		while (routeQueue.Count > 0)
		{
			var currentLocation = routeQueue.Dequeue();

			if (currentLocation == destinationLocation)
				break;

			foreach (var neighbor in currentLocation.ConnectedLocations)
			{
				var edgeCost = currentLocation.Coordinates.SquareDistanceTo(neighbor.Coordinates);
				var newCost = costSoFar[currentLocation] + edgeCost;
				if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
				{
					costSoFar[neighbor] = newCost;
					routeQueue.Enqueue(neighbor, newCost);
					cameFrom[neighbor] = currentLocation;
				}
			}
		}

		if (!cameFrom.ContainsKey(destinationLocation))
			return Array.Empty<LocationViewModel>();

		var path = new List<LocationViewModel>();
		for (var location = destinationLocation; location != null; location = cameFrom[location])
			path.Add(location);

		path.Reverse();
		return path;
	}

	private IReadOnlyList<LocationViewModel> m_locations;
	private double m_zoom;
	private MapCoordinates m_center;
}
