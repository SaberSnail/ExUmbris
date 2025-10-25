using System.Windows.Threading;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.ViewModels;

public abstract class ViewModelBase : NotifyPropertyChangedDispatcherBase
{
	protected ViewModelBase()
	{
	}

	protected ViewModelBase(Dispatcher dispatcher)
		: base(dispatcher)
	{
	}
}
