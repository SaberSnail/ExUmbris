using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using ExUmbris.ViewModels;
using GoldenAnvil.Utility.Windows;

namespace ExUmbris.Controls;

public sealed class LocationVisual : DrawingVisual
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

	public LocationVisual(LocationViewModel node, double pixelsPerDip, MapPalette palette)
	{
		Node = node;
		m_pixelsPerDip = pixelsPerDip;
		m_palette = palette;
		Edges = [];

		Node.PropertyChanged += OnNodePropertyChanged;
	}

	private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(LocationViewModel.Name) or nameof(LocationViewModel.Actors))
			Render();
	}

	public LocationViewModel Node { get; }

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

		RenderNode();
		RenderActors();
		RenderName();

		void RenderNode()
		{
			if (m_isHovered)
				dc.DrawEllipse(m_palette.HoverGlowBrush, null, new Point(0, 0), c_nodeRadius + c_hoverGlowThickness, c_nodeRadius + c_hoverGlowThickness);

			dc.DrawEllipse(m_palette.GetNodeBrush(m_isHovered), m_palette.GetNodePen(m_isHovered), new Point(0, 0), c_nodeRadius, c_nodeRadius);
		}

		void RenderActors()
		{
			// Draw actors in an arc above the node
			var actors = Node.Actors;
			if (actors.Count > 0)
			{
				var actorsArcRadius = c_nodeRadius + c_actorRadius + c_actorPadding;
				var actorEffectiveRadius = c_actorRadius + c_actorPadding / 2.0;
				var anglePerActor = 2.0 * Math.Asin(actorEffectiveRadius / actorsArcRadius);

				var actorCount = Math.Min(actors.Count, GetMaxActors());
				int GetMaxActors()
				{
					var excludedAngle = Math.Acos(c_nodeRadius / actorsArcRadius);
					var availableAngle = 2.0 * Math.PI - 2.0 * excludedAngle;
					return (int) Math.Floor(availableAngle / anglePerActor);
				}

				var angle = Math.PI / 2.0 + (anglePerActor / 2.0) - (anglePerActor * actorCount / 2.0);
				for (int i = 0; i < actorCount; i++)
				{
					var x = actorsArcRadius * Math.Cos(angle);
					var y = -actorsArcRadius * Math.Sin(angle);
					dc.DrawEllipse(m_palette.NodeActorBrush, null, new Point(x, y), c_actorRadius, c_actorRadius);

					// If not all actors are shown, draw a cross centered on the ellipse of the right-most actor
					if (actorCount < actors.Count && i == 0)
					{
						var crossPen = new Pen(Brushes.Black, 1.0).Frozen();
						dc.DrawLine(crossPen, new Point(x, y - c_actorRadius), new Point(x, y + c_actorRadius));
						dc.DrawLine(crossPen, new Point(x - c_actorRadius, y), new Point(x + c_actorRadius, y));
					}

					angle += anglePerActor;
				}
			}
		}

		void RenderName()
		{
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
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		var point = hitTestParameters.HitPoint;
		var dx = point.X;
		var dy = point.Y;
		var effectiveNodeRadius = c_nodeRadius + c_hoverGlowThickness;
		bool isInEllipse = dx * dx + dy * dy <= effectiveNodeRadius * effectiveNodeRadius;
		bool isInText = m_cachedTextRect.Contains(point);

		return isInEllipse || isInText ? new PointHitTestResult(this, point) : null;
	}

	const double c_nodeRadius = 10.0;
	const double c_actorRadius = 3.0;
	const double c_actorPadding = 2.0;
	private const double c_hoverGlowThickness = 12.0;

	private readonly double m_pixelsPerDip;
	private readonly MapPalette m_palette;
	private bool m_isHovered;
	private Rect m_cachedTextRect;
}
