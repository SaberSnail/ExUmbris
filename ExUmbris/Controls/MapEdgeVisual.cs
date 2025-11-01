using System.Windows;
using System.Windows.Media;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapEdgeVisual : DrawingVisual
{
	public MapEdgeVisual(MapNodeVisual node1, MapNodeVisual node2)
	{
		Node1 = node1;
		Node2 = node2;
		m_linePen = new Pen(new SolidColorBrush(Color.FromRgb(184, 134, 11)), 2.0).Frozen();
		m_nodeHoveredLinePen = new Pen(new SolidColorBrush(Color.FromRgb(255, 215, 0)), 3.0).Frozen();
	}

	public MapNodeVisual Node1 { get; }
	public MapNodeVisual Node2 { get; }

	public void OnNodeIsHoveredChanged() => Render();

	public void Render()
	{
		using var context = RenderOpen();
		var point1 = Node1.Offset;
		var point2 = Node2.Offset;
		var pen = (Node1.IsHovered || Node2.IsHovered) ? m_nodeHoveredLinePen : m_linePen;
		context.DrawLine(pen, new Point(point1.X, point1.Y), new Point(point2.X, point2.Y));
	}

	private readonly Pen m_linePen;
	private readonly Pen m_nodeHoveredLinePen;
}
