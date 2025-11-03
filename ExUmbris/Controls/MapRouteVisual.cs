using System.Windows;
using System.Windows.Media;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapRouteVisual : DrawingVisual
{
	public MapRouteVisual(IReadOnlyList<LocationVisual> nodes)
	{
		Nodes = nodes;
		var pen = new Pen(new SolidColorBrush(Color.FromRgb(255,0,0)), 1.0).Frozen();
		m_routePen = pen;
	}

	public IReadOnlyList<LocationVisual> Nodes { get; }

	public void Render()
	{
		using var context = RenderOpen();
		for (int i =0; i < Nodes.Count -1; i++)
		{
			var point1 = Nodes[i].Offset;
			var point2 = Nodes[i +1].Offset;
			context.DrawLine(m_routePen, new Point(point1.X, point1.Y), new Point(point2.X, point2.Y));
		}
	}

	private readonly Pen m_routePen;
}
