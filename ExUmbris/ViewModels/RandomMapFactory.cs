using ExUmbris.Models;
using GoldenAnvil.Utility.Logging;

namespace ExUmbris.ViewModels;

public interface IMapFactory
{
	IReadOnlyList<MapNodeViewModel> CreateMapNodes(Random rng, int count);
}

public sealed class RandomMapFactory : IMapFactory
{
	public IReadOnlyList<MapNodeViewModel> CreateMapNodes(Random rng, int count)
	{
		const double minSquareDistance = 0.2 * 0.2;
		const double maxSquareDistance = 0.5 * 0.5;
		const int maxAttempts = 10_000;

		int maxCount = (int) Math.Ceiling((4.0 * 0.75) / ((0.5 - 0.2) * (0.5 - 0.2)));
		if (count > maxCount)
		{
			Log.Warn($"Requested map node count {count} exceeds suggested maximum {maxCount}, reducing to maximum.");
			count = maxCount;
		}

		var nodes = new List<MapNodeViewModel>(count);
		if (count <=0)
			return nodes;

		nodes.Add(new MapNodeViewModel
			{
				Id = 1,
				Name = $"Location 1",
				Coordinates = new MapCoordinates(0.0,0.0),
			});

		for (int i = 1; i < count; i++)
		{
			MapCoordinates coordinates;
			int attempts = 0;
			bool isValid;
			do
			{
				attempts++;
				var x = rng.NextDouble() * 2.0 - 1.0;
				var y = rng.NextDouble() * 2.0 - 1.0;
				coordinates = new MapCoordinates(x, y);

				var isFarEnough = true;
				var isCloseEnough = false;
				foreach (var checkNode in nodes)
				{
					var squareDistance = coordinates.SquareDistanceTo(checkNode.Coordinates);
					if (squareDistance < minSquareDistance)
					{
						isFarEnough = false;
						break;
					}
					if (squareDistance <= maxSquareDistance)
					{
						isCloseEnough = true;
					}
				}

				isValid = isCloseEnough && isFarEnough;
			} while (!isValid && attempts < maxAttempts);

			if (attempts == maxAttempts)
			{
				Log.Warn($"Failed to place map node {i} after {maxAttempts} attempts, stopping early with {nodes.Count} nodes.");
				break;
			}

			var node = new MapNodeViewModel
			{
				Id = i + 1,
				Name = $"Location {i + 1}",
				Coordinates = coordinates,
			};
			nodes.Add(node);
		}

		for (var index1 = 0; index1 < nodes.Count; index1++)
		{
			var connected = new List<MapNodeViewModel>();
			for (var index2 = 0; index2 < nodes.Count; index2++)
			{
				if (index1 == index2)
					continue;
				var squareDistance = nodes[index1].Coordinates.SquareDistanceTo(nodes[index2].Coordinates);
				if (squareDistance <= maxSquareDistance)
					connected.Add(nodes[index2]);
			}
			nodes[index1].ConnectedNodes = connected;
		}

		return nodes;
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(RandomMapFactory));
}