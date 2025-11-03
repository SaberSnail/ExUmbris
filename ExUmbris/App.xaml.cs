using System.Diagnostics;
using System.Windows;
using ExUmbris.Views;
using GoldenAnvil.Utility.Logging;
using GoldenAnvil.Utility.Windows.Async;
using Microsoft.VisualStudio.Threading;

namespace ExUmbris;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e) => TaskWatcher.Execute(c => OnStartupAsync(e, c), AppModel.Instance.TaskGroup);

	private async Task OnStartupAsync(StartupEventArgs e, TaskStateController state)
	{
		var stopwatch = Stopwatch.StartNew();

		base.OnStartup(e);

		FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
		{
			DefaultValue = FindResource(typeof(Window))
		});

		await AppModel.Instance.StartupAsync(state).ConfigureAwait(false);

		await state.ToSyncContext();

		new MainWindowView(AppModel.Instance.MainWindow).Show();

		Log.Info($"Finished starting up in {stopwatch.Elapsed}");
	}

#pragma warning disable VSTHRD100 // Avoid async void methods
	protected override async void OnExit(ExitEventArgs e) => await OnShutdownAsync(e).ConfigureAwait(false);
#pragma warning restore VSTHRD100 // Avoid async void methods

	private async Task OnShutdownAsync(ExitEventArgs e)
	{
		using var logScope = Log.TimedInfo("Shutting down");

		await AppModel.Instance.ShutdownAsync().ConfigureAwait(false);

		base.OnExit(e);
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(App));
}
