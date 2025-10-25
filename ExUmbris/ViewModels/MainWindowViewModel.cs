using GoldenAnvil.Utility.Windows.Async;
using Microsoft.VisualStudio.Threading;

namespace ExUmbris.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
	public static async Task<MainWindowViewModel> CreateAsync(TaskStateController state)
	{
		await state.ToSyncContext();
		return new MainWindowViewModel();
	}

	private MainWindowViewModel()
	{
	}

	public void Dispose()
	{
	}
}
