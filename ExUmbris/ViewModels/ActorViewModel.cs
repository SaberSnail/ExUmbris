using GoldenAnvil.Utility;
using GoldenAnvil.Utility.Logging;
using ExUmbris.Models;

namespace ExUmbris.ViewModels;

public sealed class ActorViewModel : ViewModelBase
{
	public ActorViewModel()
	{
		Name = "Unknown Actor";
		m_currentRoute = [];
		m_traits = new List<Trait>();
		m_modifiedAttributes = new List<AttributeModifier>();
	}

	public int Id { get; init; }

	public GenderKind Gender { get; init; }

	public string Name { get; set; }

	public IReadOnlyList<Trait> Traits => m_traits;

	public double TotalAttributeMagnitude => m_traits.SelectMany(t => t.Modifiers).Sum(e => e.Modifier);

	public IReadOnlyList<AttributeModifier> ModifiedAttributes => m_modifiedAttributes;

	public double GetAttributeModifier(AttributeKind attribute) =>
		m_modifiedAttributes.FirstOrDefault(x => x.Attribute == attribute)?.Modifier ?? 0.0;

	public LocationViewModel? CurrentLocation
	{
		get => VerifyAccess(m_currentLocation);
		set
		{
			if (CurrentLocation == value)
				return;

			using var _ = ScopedPropertyChange(nameof(CurrentLocation));

			if (m_currentLocation is not null)
				m_currentLocation.RemoveActor(this);
			m_currentLocation = value;
			if (m_currentLocation is not null)
				m_currentLocation.AddActor(this);
		}
	}

	public IReadOnlyList<LocationViewModel> CurrentRoute => VerifyAccess(m_currentRoute);

	public void ProcessTurn(Random rng, MapViewModel map)
	{
		if (CurrentLocation is null)
			return;

		if (m_currentRoute.Count == 0)
		{
			if (rng.NextDouble() < c_moveProbability)
			{
				m_currentRoute.AddRange(SelectBestTargetRoute(rng, map));
				Log.Info($"{Name} has selected a new route from {CurrentLocation!.Name} to {CurrentRoute[^1].Name}.");
			}
		}

		if (m_currentRoute.Count > 1)
		{
			var nextLocation = m_currentRoute[1];
			Log.Info($"{Name} has moves from {CurrentLocation!.Name} to {nextLocation.Name}.");
			CurrentLocation = nextLocation;
			if (m_currentRoute.Count == 2)
				m_currentRoute.Clear();
			else
				m_currentRoute.RemoveAt(0);
		}
	}

	public void AddTrait(Trait trait)
	{
		m_traits.Add(trait);
		UpdateModifiedAttributes();
	}

	private IReadOnlyList<LocationViewModel> SelectBestTargetRoute(Random rng, MapViewModel map)
	{
		if (CurrentLocation is null)
			return [];

		var candidateLocations = map.Locations.Where(n => n != CurrentLocation).AsReadOnlyList();
		if (candidateLocations.Count == 0)
			return [];

		var candidateRoutes = candidateLocations
			.Select(location =>
			{
				var route = map.GetShortestRoute(CurrentLocation, location);
				var weight = 0.0;
				for (int i = 1; i < route.Count; i++)
					weight += route[i - 1].Coordinates.SquareDistanceTo(route[i].Coordinates);
				return (Route: route, Weight: weight);
			})
			.AsReadOnlyList();
		if (candidateRoutes.Count == 0)
			return [];

		var totalWeight = candidateRoutes.Sum(x => x.Weight);
		var value = rng.NextDouble() * totalWeight;
		for (int i = 0; i < candidateRoutes.Count; i++)
		{
			value -= candidateRoutes[i].Weight;
			if (value <= 0)
				return candidateRoutes[i].Route;
		}
		return candidateRoutes[^1].Route;
	}

	private void UpdateModifiedAttributes()
	{
		m_modifiedAttributes = m_traits
			.SelectMany(x => x.Modifiers)
			.GroupBy(x => x.Attribute)
			.Select(x => new AttributeModifier { Attribute = x.Key, Modifier = x.Sum(modifier => modifier.Modifier) })
			.ToList();
	}

	private const double c_moveProbability = 0.1;
	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(ActorViewModel));

	private readonly List<LocationViewModel> m_currentRoute;
	private LocationViewModel? m_currentLocation;
	private readonly List<Trait> m_traits;
	private List<AttributeModifier> m_modifiedAttributes;
}
