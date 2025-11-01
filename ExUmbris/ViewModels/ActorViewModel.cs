namespace ExUmbris.ViewModels;

public sealed class ActorViewModel : ViewModelBase
{
	public ActorViewModel()
	{
		Name = "Unknown Actor";
	}

	public int Id { get; init; }
	public string Name { get; set; }
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

	private MapNodeViewModel? m_currentNode;
}
