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

	private IReadOnlyList<MapNodeViewModel> m_mapNodes;
}
