using System.Linq;
using System.Windows;
using System.Windows.Media;
using ExUmbris.Models;
using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapControl : FrameworkElement
{
	public MapControl()
	{
		m_visuals = new VisualCollection(this);
		m_nodeVisuals = [];
		m_edgeVisuals = [];

		m_nodePalette = new MapNodePalette
		{
			NodeBrush = Brushes.DarkBlue,
			HoverNodeBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0xcb)).Frozen(),
			NodeActorBrush = Brushes.Gold,
			NodePen = new Pen(new SolidColorBrush(Color.FromRgb(0x8d, 0xb8, 0xc6)), 2).Frozen(),
			HoverNodePen = new Pen(Brushes.LightBlue, 2).Frozen(),
			HoverGlowBrush = MapNodeVisual.CreateHoverGlowBrush(),
		};
	}

	public static readonly DependencyProperty MapProperty =
		DependencyPropertyUtility<MapControl>.Register(x => x.Map, new PropertyChangedCallback(OnMapChanged));

	public MapViewModel? Map
	{
		get => (MapViewModel?) GetValue(MapProperty);
		set => SetValue(MapProperty, value);
	}

	private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (MapControl) d;
		control.RebuildVisuals();
	}

	protected override int VisualChildrenCount => m_visuals.Count;

	protected override Visual GetVisualChild(int index) => m_visuals[index];

	protected override void OnRender(DrawingContext drawingContext)
	{
		// Background
		drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));

		// Border outside the map area
		var outerBounds = GetSquareBounds(ActualWidth, ActualHeight);
		outerBounds.Inflate(c_mapBorderThickness, c_mapBorderThickness);
		drawingContext.DrawRectangle(null, new Pen(Brushes.DarkGray, c_mapBorderThickness), outerBounds);

		base.OnRender(drawingContext);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		if (Map?.MapNodes is not null)
		{
			var squareBounds = GetSquareBounds(finalSize.Width, finalSize.Height);

			foreach (var nodeVisual in m_nodeVisuals.Values)
			{
				var center = MapToControl(nodeVisual.Node.Coordinates, squareBounds);
				nodeVisual.Offset = new Vector(center.X, center.Y);
			}

			foreach (var edgeVisual in m_edgeVisuals)
				edgeVisual.Render();
		}
		return base.ArrangeOverride(finalSize);
	}

	protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (Map?.MapNodes is null)
			return;

		var mousePos = e.GetPosition(this);
		bool shouldInvalidate = false;

		foreach (var node in m_nodeVisuals.Values)
		{
			node.IsHovered = false;
			shouldInvalidate = true;
		}

		var hitResult = VisualTreeHelper.HitTest(this, mousePos);
		if (hitResult?.VisualHit is MapNodeVisual nodeVisual)
		{
			nodeVisual.IsHovered = true;
			shouldInvalidate = true;
		}

		if (shouldInvalidate)
			InvalidateVisual();
	}

	private void RebuildVisuals()
	{
		m_visuals.Clear();
		m_nodeVisuals.Clear();
		m_edgeVisuals.Clear();
		if ((Map?.MapNodes?.Count ?? 0) == 0)
			return;

		var bounds = GetSquareBounds(ActualWidth, ActualHeight);
		var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

		// Create node visuals and index by node ID
		foreach (var node in Map!.MapNodes)
		{
			var visual = new MapNodeVisual(node, dpi, m_nodePalette);
			visual.Render();
			var center = MapToControl(node.Coordinates, bounds);
			visual.Offset = new Vector(center.X, center.Y);
			m_nodeVisuals[node.Id] = visual;
		}

		// Create edge visuals
		var drawn = new HashSet<(int, int)>();
		foreach (var node in Map.MapNodes)
		{
			foreach (var connected in node.ConnectedNodes)
			{
				int id1 = node.Id, id2 = connected.Id;
				var key = id1 < id2 ? (id1, id2) : (id2, id1);
				if (drawn.Contains(key))
					continue;
				drawn.Add(key);
				var node1 = m_nodeVisuals[id1];
				var node2 = m_nodeVisuals[id2];
				var edgeVisual = new MapEdgeVisual(node1, node2);
				edgeVisual.Render();
				m_edgeVisuals.Add(edgeVisual);

				// Add connection to both nodes
				node1.Edges.Add(edgeVisual);
				node2.Edges.Add(edgeVisual);
			}
		}

		foreach (var edge in m_edgeVisuals.Cast<DrawingVisual>().Concat(m_nodeVisuals.Values))
			m_visuals.Add(edge);

		InvalidateVisual();
	}

	private static Rect GetSquareBounds(double width, double height)
	{
		var size = Math.Max(0, Math.Min(width, height) - 2 * c_mapBorderThickness);
		var x = (width - size) / 2;
		var y = (height - size) / 2;
		return new Rect(x, y, size, size);
	}

	private static Point MapToControl(MapCoordinates coords, Rect bounds)
	{
		var x = bounds.Left + (coords.X + 1.0) / 2.0 * bounds.Width;
		var y = bounds.Top + (1.0 - (coords.Y + 1.0) / 2.0) * bounds.Height;
		return new Point(x, y);
	}

	private const double c_mapBorderThickness = 4.0;

	private readonly VisualCollection m_visuals;
	private readonly Dictionary<int, MapNodeVisual> m_nodeVisuals;
	private readonly List<MapEdgeVisual> m_edgeVisuals;
	private readonly MapNodePalette m_nodePalette;
}
