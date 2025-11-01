using System.Windows;
using System.Windows.Media;
using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class MapNodeVisual : DrawingVisual
{
	public static RadialGradientBrush CreateHoverGlowBrush()
	{
		var gradientBrush = new RadialGradientBrush();
		gradientBrush.GradientOrigin = new Point(0.5, 0.5);
		gradientBrush.Center = new Point(0.5, 0.5);
		gradientBrush.RadiusX = 0.5;
		gradientBrush.RadiusY = 0.5;
		gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 30, 144, 255), 0.0));
		var ellipseEdge = c_nodeRadius / (c_nodeRadius + c_hoverGlowThickness);
		gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 30, 144, 255), ellipseEdge));
		gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 30, 144, 255), 1.0));
		return gradientBrush.Frozen();
	}

	public MapNodeVisual(MapNodeViewModel node, double pixelsPerDip, MapNodePalette palette)
	{
		Node = node;
		m_pixelsPerDip = pixelsPerDip;
		m_palette = palette;
		Edges = [];
	}

	public MapNodeViewModel Node { get; }

	public List<MapEdgeVisual> Edges { get; }

	public bool IsHovered
	{
		get => m_isHovered;
		set
		{
			if (m_isHovered != value)
			{
				m_isHovered = value;
				Render();
				foreach (var edge in Edges)
					edge.OnNodeIsHoveredChanged();
			}
		}
	}

	public void Render()
	{
		using var dc = RenderOpen();

		if (m_isHovered)
			dc.DrawEllipse(m_palette.HoverGlowBrush, null, new Point(0, 0), c_nodeRadius + c_hoverGlowThickness, c_nodeRadius + c_hoverGlowThickness);

		dc.DrawEllipse(m_palette.GetNodeBrush(m_isHovered), m_palette.GetNodePen(m_isHovered), new Point(0, 0), c_nodeRadius, c_nodeRadius);

		var formattedText = new FormattedText(
			Node.Name,
			System.Globalization.CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			new Typeface("Segoe UI"),
			12,
			Brushes.White,
			m_pixelsPerDip);
		var textRect = new Rect(
			-formattedText.Width / 2,
			c_nodeRadius + 2,
			formattedText.Width,
			formattedText.Height);
		m_cachedTextRect = textRect;

		var backgroundBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
		backgroundBrush.Freeze();
		dc.DrawRectangle(backgroundBrush, null, textRect);

		dc.DrawText(formattedText, new Point(textRect.Left, textRect.Top));
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		var point = hitTestParameters.HitPoint;
		var dx = point.X;
		var dy = point.Y;
		var effectiveNodeRadius = c_nodeRadius + c_hoverGlowThickness;
		bool isInEllipse = dx * dx + dy * dy <= effectiveNodeRadius * effectiveNodeRadius;
		bool isInText = m_cachedTextRect.Contains(point);

		return isInEllipse || isInText ?  new PointHitTestResult(this, point) : null;
	}

	const double c_nodeRadius = 10.0;
	private const double c_hoverGlowThickness = 12.0;

	private readonly double m_pixelsPerDip;
	private readonly MapNodePalette m_palette;
	private bool m_isHovered;
	private Rect m_cachedTextRect;
}
