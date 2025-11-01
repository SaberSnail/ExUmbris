using GoldenAnvil.Utility.Windows.Async;
using Microsoft.VisualStudio.Threading;

namespace ExUmbris.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
	public static async Task<MainWindowViewModel> CreateAsync(TaskStateController state, Random rng)
	{
		await state.ToSyncContext();

		var factory = new RandomGameFactory();
		var map = new MapViewModel();
		map.Initialize(factory, rng, 25);

		var actorManager = new ActorManagerViewModel();
		actorManager.Initialize(factory, rng, 10, map);

		return new MainWindowViewModel(map, actorManager);
	}

	private MainWindowViewModel(MapViewModel map, ActorManagerViewModel actorManager)
	{
		Map = map;
		ActorManager = actorManager;
	}

	public MapViewModel Map { get; }

	public ActorManagerViewModel ActorManager { get; }

	public void ProcessTurn()
	{
		var rng = AppModel.Instance.Rng;
		ActorManager.ProcessActors(rng, Map);
	}

	public void Dispose()
	{
	}
}
