using ExUmbris.Models;

namespace ExUmbris.ViewModels;

public sealed class LocationViewModel : ViewModelBase
{
	public LocationViewModel()
	{
		Name = "Unknown Location";
		Coordinates = new(0.0, 0.0);
		ConnectedLocations = [];
		m_actors = [];
	}

	public int Id { get; init; }
	public LocationKind Kind { get; set; }
	public string Name { get; set; }
	public MapCoordinates Coordinates { get; set; }
	public IReadOnlyList<LocationViewModel> ConnectedLocations { get; set; }
	public IReadOnlyList<ActorViewModel> Actors => VerifyAccess(m_actors);

	public void RemoveActor(ActorViewModel actor)
	{
		VerifyAccess();
		if (m_actors.Contains(actor))
		{
			using var _ = ScopedPropertyChange(nameof(Actors));
			m_actors.Remove(actor);
		}
	}

	public void AddActor(ActorViewModel actor)
	{
		VerifyAccess();
		if (!m_actors.Contains(actor))
		{
			using var _ = ScopedPropertyChange(nameof(Actors));
			m_actors.Add(actor);
		}
	}

	private readonly List<ActorViewModel> m_actors;
}
