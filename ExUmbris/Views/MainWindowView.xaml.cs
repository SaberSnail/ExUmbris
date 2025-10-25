using ExUmbris.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ExUmbris.Views;

public partial class MainWindowView : Window
{
	public MainWindowView(MainWindowViewModel? viewModel)
	{
		ViewModel = viewModel;
		InitializeComponent();
		MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
	}

	public MainWindowViewModel? ViewModel { get; }
}
