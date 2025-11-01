using System.Linq;
using System.Windows;
using System.Windows.Media;
using ExUmbris.Models;
using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapControl : FrameworkElement
{
	public static readonly DependencyProperty MapProperty =
		DependencyPropertyUtility<MapControl>.Register(x => x.Map, new PropertyChangedCallback(OnMapChanged));

	public MapViewModel? Map
	{
		get => (MapViewModel?) GetValue(MapProperty);
		set => SetValue(MapProperty, value);
	}

	public MapControl()
	{
		m_visuals = new VisualCollection(this);
		m_nodeVisuals = [];
		m_lineVisuals = [];
	}

	private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (MapControl) d;
		control.RebuildVisuals();
	}

	private void RebuildVisuals()
	{
		m_visuals.Clear();
		m_lineVisuals.Clear();
		if ((Map?.MapNodes?.Count ?? 0) == 0)
			return;

		var bounds = GetSquareBounds(ActualWidth, ActualHeight);
		var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

		// Create node visuals and index by node ID
		foreach (var node in Map.MapNodes)
		{
			var visual = new MapNodeVisual(node, c_nodeRadius, dpi);
			visual.Render();
			var center = MapToControl(node.Coordinates, bounds);
			visual.Offset = new Vector(center.X, center.Y);
			m_nodeVisuals[node.Id] = visual;
		}

		// Create connection visuals
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
				var connectionVisual = new MapConnectionVisual(node1, node2);
				connectionVisual.Render();
				m_lineVisuals.Add(connectionVisual);
			}
		}

		foreach (var visual in m_lineVisuals.Cast<DrawingVisual>().Concat(m_nodeVisuals.Values))
			m_visuals.Add(visual);

		InvalidateVisual();
	}

	protected override int VisualChildrenCount => m_visuals.Count;

	protected override Visual GetVisualChild(int index) => m_visuals[index];

	protected override void OnRender(DrawingContext drawingContext)
	{
		// Draw background
		drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));

		// Draw border outside the map area
		var outerBounds = GetSquareBounds(ActualWidth, ActualHeight);
		outerBounds.Inflate(c_mapBorderThickness, c_mapBorderThickness);
		drawingContext.DrawRectangle(null, new Pen(Brushes.DarkGray, c_mapBorderThickness), outerBounds);

		base.OnRender(drawingContext);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		// Reposition node and line visuals according to new size
		if (Map != null && Map.MapNodes != null)
		{
			var squareBounds = GetSquareBounds(finalSize.Width, finalSize.Height);

			// Update node visuals
			foreach (var node in Map.MapNodes)
			{
				var visual = m_nodeVisuals[node.Id];
				var center = MapToControl(node.Coordinates, squareBounds);
				visual.Offset = new Vector(center.X, center.Y);
			}

			// Update line visuals
			foreach (var lineVisual in m_lineVisuals)
				lineVisual.Render();
		}
		return base.ArrangeOverride(finalSize);
	}

	private Rect GetSquareBounds(double width, double height)
	{
		var size = Math.Max(0, Math.Min(width, height) - 2 * c_mapBorderThickness);
		var x = (width - size) / 2;
		var y = (height - size) / 2;
		return new Rect(x, y, size, size);
	}

	private Point MapToControl(MapCoordinates coords, Rect bounds)
	{
		// Map [-1,1] to bounds (which now excludes border)
		var x = bounds.Left + (coords.X + 1.0) / 2.0 * bounds.Width;
		var y = bounds.Top + (1.0 - (coords.Y + 1.0) / 2.0) * bounds.Height;
		return new Point(x, y);
	}

	private const double c_mapBorderThickness = 4.0;
	const double c_nodeRadius = 10.0;

	private readonly VisualCollection m_visuals;
	private readonly Dictionary<int, MapNodeVisual> m_nodeVisuals;
	private readonly List<MapConnectionVisual> m_lineVisuals;
}
