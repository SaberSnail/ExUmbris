namespace ExUmbris.ViewModels;

public sealed class MapViewModel : ViewModelBase
{
	public MapViewModel()
	{
		m_mapNodes = [];
	}

	public void Initialize(IMapFactory mapFactory, Random rng, int nodeCount)
	{
		var nodes = mapFactory.CreateMapNodes(rng, nodeCount);
		MapNodes = nodes;
	}

	public IReadOnlyList<MapNodeViewModel> MapNodes {
		get => VerifyAccess(m_mapNodes);
		private set => SetPropertyField(value, ref m_mapNodes);
	}

	public IReadOnlyList<MapNodeViewModel> GetShortestRoute(MapNodeViewModel sourceNode, MapNodeViewModel destinationNode)
	{
		var routeQueue = new PriorityQueue<MapNodeViewModel, double>();
		var cameFrom = new Dictionary<MapNodeViewModel, MapNodeViewModel?>();
		var costSoFar = new Dictionary<MapNodeViewModel, double>();

		routeQueue.Enqueue(sourceNode, 0.0);
		cameFrom[sourceNode] = null;
		costSoFar[sourceNode] = 0.0;

		while (routeQueue.Count > 0)
		{
			var currentNode = routeQueue.Dequeue();

			if (currentNode == destinationNode)
				break;

			foreach (var neighbor in currentNode.ConnectedNodes)
			{
				var edgeCost = currentNode.Coordinates.SquareDistanceTo(neighbor.Coordinates);
				var newCost = costSoFar[currentNode] + edgeCost;
				if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
				{
					costSoFar[neighbor] = newCost;
					routeQueue.Enqueue(neighbor, newCost);
					cameFrom[neighbor] = currentNode;
				}
			}
		}

		if (!cameFrom.ContainsKey(destinationNode))
			return Array.Empty<MapNodeViewModel>();

		var path = new List<MapNodeViewModel>();
		for (var node = destinationNode; node != null; node = cameFrom[node])
			path.Add(node);

		path.Reverse();
		return path;
	}

	private IReadOnlyList<MapNodeViewModel> m_mapNodes;
}
