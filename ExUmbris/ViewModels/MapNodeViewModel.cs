using ExUmbris.Models;

namespace ExUmbris.ViewModels;

public sealed class MapNodeViewModel : ViewModelBase
{
	public MapNodeViewModel()
	{
		Name = "Unknown Map Node";
		Coordinates = new(0.0, 0.0);
		ConnectedNodes = [];
		m_actors = [];
	}

	public int Id { get; init; }
	public string Name { get; set; }
	public MapCoordinates Coordinates { get; set; }
	public IReadOnlyList<MapNodeViewModel> ConnectedNodes { get; set; }
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
