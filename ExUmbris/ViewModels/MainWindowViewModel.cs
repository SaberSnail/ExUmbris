using GoldenAnvil.Utility.Windows.Async;
using Microsoft.VisualStudio.Threading;

namespace ExUmbris.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
	public static async Task<MainWindowViewModel> CreateAsync(TaskStateController state, Random rng)
	{
		await state.ToSyncContext();

		var factory = new RandomMapFactory();
		var map = new MapViewModel();
		map.Initialize(factory, rng, 25);

		return new MainWindowViewModel(map);
	}

	private MainWindowViewModel(MapViewModel map)
	{
		Map = map;
	}

	public MapViewModel Map { get; }


	public void Dispose()
	{
	}
}
