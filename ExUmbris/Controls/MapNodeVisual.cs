using System.Windows;
using System.Windows.Media;
using ExUmbris.ViewModels;

namespace ExUmbris.Controls;

public sealed class MapNodeVisual : DrawingVisual
{
	public MapNodeVisual(MapNodeViewModel node, double nodeRadius, double pixelsPerDip)
	{
		Node = node;
		m_nodeRadius = nodeRadius;
		m_pixelsPerDip = pixelsPerDip;
	}

	public MapNodeViewModel Node { get; }

	public void Render()
	{
		using var dc = RenderOpen();

		dc.DrawEllipse(Brushes.DarkBlue, new Pen(Brushes.LightBlue, 2), new Point(0, 0), m_nodeRadius, m_nodeRadius);

		var formattedText = new FormattedText(
			Node.Name,
			System.Globalization.CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			new Typeface("Segoe UI"),
		   14,
			Brushes.White,
			m_pixelsPerDip);
		dc.DrawText(formattedText, new Point(-formattedText.Width / 2, m_nodeRadius + 2));
	}

	private readonly double m_nodeRadius;
	private readonly double m_pixelsPerDip;
}
