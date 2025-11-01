namespace ExUmbris.ViewModels;

public sealed class ActorViewModel : ViewModelBase
{
	public ActorViewModel()
	{
		Name = "Unknown Actor";
	}

	public int Id { get; init; }
	public string Name { get; set; }
	public MapNodeViewModel? CurrentNode => VerifyAccess(m_currentNode);

	public void MoveToNode(MapNodeViewModel node)
	{
		if (CurrentNode == node)
			return;

		using var _ = ScopedPropertyChange(nameof(CurrentNode));

		if (m_currentNode is not null)
			m_currentNode.RemoveActor(this);
		m_currentNode = node;
		if (m_currentNode is not null)
			m_currentNode.AddActor(this);
	}

	private MapNodeViewModel? m_currentNode;
}
