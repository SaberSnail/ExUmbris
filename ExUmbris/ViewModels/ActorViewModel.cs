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

	public string Name { get; set; }

	public IReadOnlyList<Trait> Traits => m_traits;

	public double TotalAttributeMagnitude => m_traits.SelectMany(t => t.Modifiers).Sum(e => e.Modifier);

	public IReadOnlyList<AttributeModifier> ModifiedAttributes => m_modifiedAttributes;

	public double GetAttributeModifier(AttributeKind attribute) =>
		m_modifiedAttributes.FirstOrDefault(x => x.Attribute == attribute)?.Modifier ?? 0.0;

	public MapNodeViewModel? CurrentNode
	{
		get => VerifyAccess(m_currentNode);
		set
		{
			if (CurrentNode == value)
				return;

			using var _ = ScopedPropertyChange(nameof(CurrentNode));

			if (m_currentNode is not null)
				m_currentNode.RemoveActor(this);
			m_currentNode = value;
			if (m_currentNode is not null)
				m_currentNode.AddActor(this);
		}
	}

	public IReadOnlyList<MapNodeViewModel> CurrentRoute => VerifyAccess(m_currentRoute);

	public void ProcessTurn(Random rng, MapViewModel map)
	{
		if (CurrentNode is null)
			return;

		if (m_currentRoute.Count == 0)
		{
			if (rng.NextDouble() < c_moveProbability)
			{
				m_currentRoute.AddRange(SelectBestTargetRoute(rng, map));
				Log.Info($"{Name} has selected a new route from {CurrentNode!.Name} to {CurrentRoute[^1].Name}.");
			}
		}

		if (m_currentRoute.Count > 1)
		{
			var nextNode = m_currentRoute[1];
			Log.Info($"{Name} has moves from {CurrentNode!.Name} to {nextNode.Name}.");
			CurrentNode = nextNode;
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

	private IReadOnlyList<MapNodeViewModel> SelectBestTargetRoute(Random rng, MapViewModel map)
	{
		if (CurrentNode is null)
			return [];

		var candidateNodes = map.MapNodes.Where(n => n != CurrentNode).AsReadOnlyList();
		if (candidateNodes.Count == 0)
			return [];

		var candidateRoutes = candidateNodes
			.Select(node =>
			{
				var route = map.GetShortestRoute(CurrentNode, node);
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

	private readonly List<MapNodeViewModel> m_currentRoute;
	private MapNodeViewModel? m_currentNode;
	private readonly List<Trait> m_traits;
	private List<AttributeModifier> m_modifiedAttributes;
}
