using System.Diagnostics;
using ExUmbris.Data;
using ExUmbris.Models;
using GoldenAnvil.Utility;
using GoldenAnvil.Utility.Logging;

namespace ExUmbris.ViewModels;

public interface IGameFactory
{
	IReadOnlyList<LocationViewModel> CreateLocations(Random rng, int count);
	IReadOnlyList<ActorViewModel> CreateActors(Random rng, int count, MapViewModel map, IReadOnlyList<Trait> traits);
}

public sealed class RandomGameFactory : IGameFactory
{
	const double c_minSystemSquareDistance = 0.25 * 0.25;
	const double c_maxSystemSquareDistance = 0.5 * 0.5;
	const int c_maxSystemPlacementAttempts = 10_000;
	const double c_minPlanetDistance = 0.03;
	const double c_maxPlanetDistance = 0.11;

	public IReadOnlyList<LocationViewModel> CreateLocations(Random rng, int systemCount)
	{
		if (systemCount <= 0)
			throw new ArgumentOutOfRangeException(nameof(systemCount), "System count must be positive.");

		int maxSystemCount = (int) Math.Ceiling((4.0 * 0.75) / ((0.5 - 0.2) * (0.5 - 0.2)));
		if (systemCount > maxSystemCount)
		{
			Log.Warn($"Requested location count {systemCount} exceeds suggested maximum {maxSystemCount}, reducing to maximum.");
			systemCount = maxSystemCount;
		}

		var systems = new List<LocationViewModel>();
		var locations = new List<LocationViewModel>();

		var nextLocationId = 1;

		// Create initial system
		var centerSystem = new LocationViewModel
		{
			Id = nextLocationId++,
			Kind = LocationKind.JumpStation,
			Name = NameHelper.GetRandomJumpStation(NameHelper.GetRandomSystem(rng), rng),
			Coordinates = new MapCoordinates(0.0, 0.0),
		};
		systems.Add(centerSystem);

		// Create remaining systems
		for (int i = 1; i < systemCount; i++)
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
				foreach (var checkSystem in systems)
				{
					var squareDistance = coordinates.SquareDistanceTo(checkSystem.Coordinates);
					if (squareDistance < c_minSystemSquareDistance)
					{
						isFarEnough = false;
						break;
					}
					if (squareDistance <= c_maxSystemSquareDistance)
					{
						isCloseEnough = true;
					}
				}

				isValid = isCloseEnough && isFarEnough;
			} while (!isValid && attempts < c_maxSystemPlacementAttempts);

			if (attempts == c_maxSystemPlacementAttempts)
			{
				Log.Warn($"Failed to place system {i} after {c_maxSystemPlacementAttempts} attempts, stopping early with {systems.Count} systems.");
				break;
			}

			var system = new LocationViewModel
			{
				Id = nextLocationId++,
				Kind = LocationKind.JumpStation,
				Name = NameHelper.GetRandomJumpStation(NameHelper.GetRandomSystem(rng), rng),
				Coordinates = coordinates,
			};
			systems.Add(system);
		}

		// Connect all systems
		for (var index1 = 0; index1 < systems.Count; index1++)
		{
			var connected = new List<LocationViewModel>();
			for (var index2 = 0; index2 < systems.Count; index2++)
			{
				if (index1 == index2)
					continue;
				var squareDistance = systems[index1].Coordinates.SquareDistanceTo(systems[index2].Coordinates);
				if (squareDistance <= c_maxSystemSquareDistance)
					connected.Add(systems[index2]);
			}
			systems[index1].ConnectedLocations = connected;
		}

		// Create system contents
		foreach (var system in systems)
		{
			var systemLocations = CreateSystem(system, rng, ref nextLocationId);
			locations.AddRange(systemLocations);
		}

		locations.AddRange(systems);

		return locations;
	}

	private IReadOnlyList<LocationViewModel> CreateSystem(LocationViewModel jumpStation, Random rng, ref int nextLocationId)
	{
		var planetCount = rng.NextRoll(3, 3);

		// Get angles to connected locations
		var origin = jumpStation.Coordinates;
		var connectedAngles = (jumpStation.ConnectedLocations ?? Array.Empty<LocationViewModel>())
			.Select(loc => Math.Atan2(loc.Coordinates.Y - origin.Y, loc.Coordinates.X - origin.X))
			.OrderBy(a => a)
			.ToList();

		// If no connected locations, just space planets evenly around the circle
		List<(double start, double end, int count)> gapAssignments = new();
		if (connectedAngles.Count == 0)
		{
			for (int i = 0; i < planetCount; i++)
				gapAssignments.Add((2 * Math.PI * i / planetCount, 2 * Math.PI * (i + 1) / planetCount, 1));
		}
		else
		{
			// Find gaps between sorted angles, including wrap-around
			var gaps = new List<(double start, double end)>();
			for (int i = 0; i < connectedAngles.Count; i++)
			{
				var start = connectedAngles[i];
				var end = connectedAngles[(i + 1) % connectedAngles.Count];
				if (end <= start)
					end += 2 * Math.PI;
				gaps.Add((start, end));
			}

			// Assign planets to gaps proportionally to gap size
			var gapSizes = gaps.Select(g => g.end - g.start).ToList();
			double totalGapSize = gapSizes.Sum();
			int assignedPlanets =0;
			for (int i =0; i < gaps.Count; i++)
			{
				// Calculate proportional count, rounding down
				double proportion = gapSizes[i] / totalGapSize;
				int count = (int)Math.Floor(proportion * planetCount);
				gapAssignments.Add((gaps[i].start, gaps[i].end, count));
				assignedPlanets += count;
			}
			// Distribute any remaining planets (due to rounding) to largest gaps
			int remaining = planetCount - assignedPlanets;
			if (remaining >0)
			{
				var sortedIndices = gapSizes
					.Select((size, idx) => (size, idx))
					.OrderByDescending(x => x.size)
					.Select(x => x.idx)
					.ToList();
				for (int i =0; i < remaining; i++)
				{
					var idx = sortedIndices[i % sortedIndices.Count];
					gapAssignments[idx] = (gapAssignments[idx].start, gapAssignments[idx].end, gapAssignments[idx].count +1);
				}
			}
		}

		// Place planets in each gap
		var locations = new List<LocationViewModel>();
		foreach (var (start, end, count) in gapAssignments)
		{
			for (int i = 0; i < count; i++)
			{
				// Evenly space within gap
				double angle = start + (end - start) * (i + 1) / (count + 1);
				double distance = c_minPlanetDistance + rng.NextDouble() * (c_maxPlanetDistance - c_minPlanetDistance);
				double x = origin.X + Math.Cos(angle) * distance;
				double y = origin.Y + Math.Sin(angle) * distance;
				var hub = new LocationViewModel
				{
					Id = nextLocationId++,
					Kind = LocationKind.PlanetaryHub,
					Name = NameHelper.GetRandomPlanet(rng),
					Coordinates = new MapCoordinates(x, y),
					ConnectedLocations = new List<LocationViewModel> { jumpStation },
				};
				locations.Add(hub);
			}
		}

		jumpStation.ConnectedLocations = jumpStation.ConnectedLocations!.Concat(locations).AsReadOnlyList();

		return locations;
	}

	public IReadOnlyList<ActorViewModel> CreateActors(Random rng, int count, MapViewModel map, IReadOnlyList<Trait> traits)
	{
		var actors = new List<ActorViewModel>(count);
		for (int i = 0; i < count; i++)
		{
			var gender = rng.NextRoll(1, 2) == 1 ? GenderKind.Male : GenderKind.Female;
			var (firstName, lastName) = NameHelper.GetRandomActor(gender, rng);
			var actor = new ActorViewModel
			{
				Id = i + 1,
				Gender = gender,
				Name = $"{firstName} {lastName}",
			};
			var locationIndex = rng.Next(map.Locations.Count);
			var location = map.Locations[locationIndex];
			actor.CurrentLocation = location;
			foreach (var trait in GetRandomActorTraits(rng, traits))
				actor.AddTrait(trait);
			actors.Add(actor);
		}

		return actors;
	}

	private IReadOnlyList<Trait> GetRandomActorTraits(Random rng, IReadOnlyList<Trait> allTraits)
	{
		Debug.Assert(allTraits.Count != 0);

		const double c_additionalTraitProbability = 0.1;

		var availableTraits = new List<Trait>(allTraits);
		var selectedTraits = new List<Trait>();
		var traitChance = 1.0;
		do
		{
			var trait = PickTrait();
			availableTraits.Remove(trait);
			selectedTraits.Add(trait);
			traitChance *= c_additionalTraitProbability;
		} while (rng.NextDouble() < traitChance && availableTraits.Count != 0);

		return selectedTraits;

		Trait PickTrait()
		{
			double totalFrequency = availableTraits.Sum(t => t.Frequency);
			double value = rng.NextDouble() * totalFrequency;
			foreach (var trait in availableTraits)
			{
				value -= trait.Frequency;
				if (value <= 0)
					return trait;
			}
			return availableTraits[^1];
		}
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(RandomGameFactory));
}