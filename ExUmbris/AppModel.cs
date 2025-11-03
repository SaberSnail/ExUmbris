using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Logging;
using GoldenAnvil.Utility.Windows.Async;
using GoldenAnvil.Utility;
using GoldenAnvil.Utility.Windows;
using Microsoft.VisualStudio.Threading;

namespace ExUmbris;

public sealed class AppModel : NotifyPropertyChangedDispatcherBase
{
	public static AppModel Instance => s_appModel.Value;

	private AppModel()
	{
		m_taskGroup = new TaskGroup();
		m_rng = new Random(1);
		LogManager.Initialize(new DebugLogDestination());
		m_currentTheme = new Uri(@"/Themes/Default/Default.xaml", UriKind.Relative);
	}

	public TaskGroup TaskGroup => m_taskGroup;

	public Random Rng => m_rng;

	public MainWindowViewModel? MainWindow
	{
		get => VerifyAccess(m_mainWindow);
		set => SetPropertyField(value, ref m_mainWindow);
	}

	public Uri CurrentTheme
	{
		get
		{
			VerifyAccess();
			return m_currentTheme;
		}
		set
		{
			if (SetPropertyField(value, ref m_currentTheme))
				Log.Info($"Changing theme to \"{m_currentTheme.OriginalString}\"");
		}
	}

	public async Task StartupAsync(TaskStateController state)
	{
		await state.ToSyncContext();

		m_mainWindow = await MainWindowViewModel.CreateAsync(state, m_rng).ConfigureAwait(false);
	}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task ShutdownAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		DisposableUtility.Dispose(ref m_mainWindow);
		DisposableUtility.Dispose(ref m_taskGroup);
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(AppModel));
	private static readonly Lazy<AppModel> s_appModel = new(() => new AppModel());
	private readonly Random m_rng;

	private MainWindowViewModel? m_mainWindow;
	private Uri m_currentTheme;
	private TaskGroup m_taskGroup;
}
