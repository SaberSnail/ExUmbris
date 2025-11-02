using System.IO;
using System.Text.Json;
using ExUmbris.Models;
using GoldenAnvil.Utility.Logging;

namespace ExUmbris.ViewModels;

public sealed class ActorManagerViewModel : ViewModelBase
{
	public ActorManagerViewModel()
	{
		m_actors = [];
		m_traits = [];
	}

	public IReadOnlyList<ActorViewModel> Actors
	{
		get => VerifyAccess(m_actors);
		private set => SetPropertyField(value, ref m_actors);
	}

	public IReadOnlyList<Trait> Traits => m_traits;

	public void Initialize(IGameFactory gameFactory, Random rng, int actorCount, MapViewModel map)
	{
		LoadTraits();
		Actors = gameFactory.CreateActors(rng, actorCount, map, m_traits);
	}

	public void ProcessActors(Random rng, MapViewModel map)
	{
		VerifyAccess();
		foreach (var actor in m_actors)
			actor.ProcessTurn(rng, map);
	}

	private void LoadTraits()
	{
		var outputDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "traits.json");
		var traitsJson = File.ReadAllText(outputDataPath);

		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

		try
		{
			m_traits = JsonSerializer.Deserialize<List<Trait>>(traitsJson, options) ?? [];
		}
		catch (JsonException ex)
		{
			Log.Error($"Failed to deserialize traits from traits.json: {ex}");
			m_traits = [];
		}
		if (m_traits.Count == 0)
			Log.Error("No traits were loaded from traits.json.");
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(ActorManagerViewModel));

	private IReadOnlyList<ActorViewModel> m_actors;
	private IReadOnlyList<Trait> m_traits;
}
