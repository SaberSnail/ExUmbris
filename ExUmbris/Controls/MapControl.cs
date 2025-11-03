using System.Windows;
using System.Windows.Media;
using ExUmbris.Models;
using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Logging;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapControl : FrameworkElement
{
	public MapControl()
	{
		m_visuals = new VisualCollection(this);
		m_nodeVisuals = [];
		m_edgeVisuals = [];
		m_routeVisuals = [];

		m_palette = new MapPalette
		{
			NodeBrush = Brushes.DarkBlue,
			HoverNodeBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0xcb)).Frozen(),
			NodeActorBrush = Brushes.Gold,
			NodePen = new Pen(new SolidColorBrush(Color.FromRgb(0x8d, 0xb8, 0xc6)), 2).Frozen(),
			HoverNodePen = new Pen(Brushes.LightBlue, 2).Frozen(),
			HoverGlowBrush = LocationVisual.CreateHoverGlowBrush(),
		};

		// Enable mouse events for dragging
		MouseLeftButtonDown += MapControl_MouseLeftButtonDown;
		MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
		MouseWheel += MapControl_MouseWheel;
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

	public static readonly DependencyProperty ZoomProperty =
		DependencyPropertyUtility<MapControl>.Register(x => x.Zoom, new PropertyChangedCallback(OnZoomChanged), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 1.0);

	public double Zoom
	{
		get => (double) GetValue(ZoomProperty);
		set => SetValue(ZoomProperty, value);
	}

	private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (MapControl) d;
		control.InvalidateArrange();
	}

	public static readonly DependencyProperty CenterProperty =
		DependencyPropertyUtility<MapControl>.Register(x => x.Center, new PropertyChangedCallback(OnCenterChanged), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new MapCoordinates(0.0, 0.0));

	public MapCoordinates Center
	{
		get => (MapCoordinates) GetValue(CenterProperty);
		set => SetValue(CenterProperty, value);
	}

	private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (MapControl) d;
		control.InvalidateArrange();
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
		if (Map?.Locations is not null)
		{
			var squareBounds = GetSquareBounds(finalSize.Width, finalSize.Height);
			var zoom = Zoom;
			foreach (var nodeVisual in m_nodeVisuals.Values)
			{
				var center = MapToControl(nodeVisual.Node.Coordinates, squareBounds, Center, zoom);
				nodeVisual.Offset = new Vector(center.X, center.Y);
			}

			foreach (var edgeVisual in m_edgeVisuals)
				edgeVisual.Render();

			foreach (var routeVisual in m_routeVisuals)
				routeVisual.Render();
		}
		return base.ArrangeOverride(finalSize);
	}

	protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (Map?.Locations is null)
			return;

		var mousePos = e.GetPosition(this);

		if (m_lastDragPoint != null && m_dragStartCenter != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
		{
			var bounds = GetSquareBounds(ActualWidth, ActualHeight);
			var start = m_lastDragPoint.Value;
			var delta = mousePos - start;
			var zoom = Zoom;
			if (delta.Length > 0)
			{
				var mapDeltaX = 2.0 * (delta.X / bounds.Width) * zoom;
				var mapDeltaY = -2.0 * (delta.Y / bounds.Height) * zoom;
				var newCenter = new MapCoordinates(
					m_dragStartCenter.X - mapDeltaX,
					m_dragStartCenter.Y - mapDeltaY
				);
				Center = newCenter;
			}
			return;
		}

		bool shouldInvalidate = false;
		foreach (var node in m_nodeVisuals.Values)
		{
			if (node.IsHovered)
			{
				UpdateNodeIsHovered(node, false);
				shouldInvalidate = true;
			}
		}

		var hitResult = VisualTreeHelper.HitTest(this, mousePos);
		if (hitResult?.VisualHit is LocationVisual nodeVisual)
		{
			if (!nodeVisual.IsHovered)
			{
				UpdateNodeIsHovered(nodeVisual, true);
				shouldInvalidate = true;
			}
		}

		if (shouldInvalidate)
			InvalidateVisual();
	}

	private void MapControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (Map?.Locations is null)
			return;
		m_lastDragPoint = e.GetPosition(this);
		m_dragStartCenter = Center;
		CaptureMouse();
	}

	private void MapControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		m_lastDragPoint = null;
		m_dragStartCenter = null;
		ReleaseMouseCapture();
	}

	private void MapControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
	{
		if (Map?.Locations is null)
			return;

		var mousePos = e.GetPosition(this);
		var bounds = GetSquareBounds(ActualWidth, ActualHeight);
		var zoomBefore = Zoom;
		var zoomAfter = e.Delta > 0 ? zoomBefore / c_zoomFactor : zoomBefore * c_zoomFactor;

		// Convert mouse position to map coordinates before zoom
		var mapCoordsBefore = ScreenToMap(mousePos, bounds, Center, zoomBefore);

		// Update zoom
		Zoom = zoomAfter;

		// Convert mouse position to map coordinates after zoom
		var mapCoordsAfter = ScreenToMap(mousePos, bounds, Center, zoomAfter);

		// Adjust center so the map coordinate under the mouse stays the same
		var newCenter = new MapCoordinates(
			Center.X + (mapCoordsBefore.X - mapCoordsAfter.X),
			Center.Y + (mapCoordsBefore.Y - mapCoordsAfter.Y)
		);
		Center = newCenter;
	}

	private void UpdateNodeIsHovered(LocationVisual nodeVisual, bool isHovered)
	{
		nodeVisual.IsHovered = isHovered;
		if (isHovered)
		{
			foreach (var actor in nodeVisual.Node.Actors)
			{
				if (actor.CurrentRoute.Count < 2)
					continue;
				var routeNodes = actor.CurrentRoute
					.Select(n => m_nodeVisuals[n.Id])
					.ToList();
				var routeVisual = new MapRouteVisual(routeNodes);
				routeVisual.Render();
				m_routeVisuals.Add(routeVisual);
			}
			RefreshChildVisuals();
		}
		else
		{
			m_routeVisuals.Clear();
			RefreshChildVisuals();
		}
	}

	private void RebuildVisuals()
	{
		m_visuals.Clear();
		m_nodeVisuals.Clear();
		m_edgeVisuals.Clear();
		if ((Map?.Locations?.Count ?? 0) == 0)
		{
			InvalidateVisual();
			return;
		}

		var bounds = GetSquareBounds(ActualWidth, ActualHeight);
		var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		var zoom = Zoom;

		// Create node visuals and index by node ID
		foreach (var node in Map!.Locations)
		{
			var visual = new LocationVisual(node, dpi, m_palette);
			visual.Render();
			var center = MapToControl(node.Coordinates, bounds, Center, zoom);
			visual.Offset = new Vector(center.X, center.Y);
			m_nodeVisuals[node.Id] = visual;
		}

		// Create edge visuals
		var drawn = new HashSet<(int, int)>();
		foreach (var node in Map.Locations)
		{
			foreach (var connected in node.ConnectedLocations)
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

		RefreshChildVisuals();
	}

	private void RefreshChildVisuals()
	{
		m_visuals.Clear();
		foreach (var edge in m_edgeVisuals.Cast<DrawingVisual>().Concat(m_nodeVisuals.Values).Concat(m_routeVisuals))
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

	private static Point MapToControl(MapCoordinates coords, Rect bounds, MapCoordinates center, double zoom)
	{
		// Center is now variable, apply zoom
		var scaledWidth = bounds.Width / zoom;
		var scaledHeight = bounds.Height / zoom;
		var x = bounds.Left + ((coords.X - center.X) + 1.0) / 2.0 * scaledWidth + (bounds.Width - scaledWidth) / 2.0;
		var y = bounds.Top + (1.0 - ((coords.Y - center.Y) + 1.0) / 2.0) * scaledHeight + (bounds.Height - scaledHeight) / 2.0;
		return new Point(x, y);
	}

	private static MapCoordinates ScreenToMap(Point screenPoint, Rect bounds, MapCoordinates center, double zoom)
	{
		var scaledWidth = bounds.Width / zoom;
		var scaledHeight = bounds.Height / zoom;
		var x = ((screenPoint.X - bounds.Left - (bounds.Width - scaledWidth) / 2.0) / scaledWidth) * 2.0 - 1.0 + center.X;
		var y = (1.0 - ((screenPoint.Y - bounds.Top - (bounds.Height - scaledHeight) / 2.0) / scaledHeight) * 2.0) + center.Y;
		return new MapCoordinates(x, y);
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(MapControl));

	private const double c_mapBorderThickness = 4.0;
	private const double c_zoomFactor = 1.15;

	private readonly VisualCollection m_visuals;
	private readonly Dictionary<int, LocationVisual> m_nodeVisuals;
	private readonly List<MapEdgeVisual> m_edgeVisuals;
	private readonly List<MapRouteVisual> m_routeVisuals;
	private readonly MapPalette m_palette;
	private Point? m_lastDragPoint;
	private MapCoordinates? m_dragStartCenter;
}
