namespace ExUmbris.ViewModels;

public sealed class ActorManagerViewModel : ViewModelBase
{
	public ActorManagerViewModel()
	{
		m_actors = [];
	}

	public IReadOnlyList<ActorViewModel> Actors => VerifyAccess(m_actors);

	public void Initialize(IGameFactory gameFactory, Random rng, int actorCount, MapViewModel map)
	{
		using var _ = ScopedPropertyChange(nameof(Actors));
		m_actors = gameFactory.CreateActors(rng, actorCount, map);
	}

	private IReadOnlyList<ActorViewModel> m_actors;
}
