using System.Windows.Media;

namespace ExUmbris.Controls;

public sealed class MapPalette
{
	public required Brush NodeBrush { get; init; }
	public required Brush HoverNodeBrush { get; init; }
	public required Brush NodeActorBrush { get; init; }
	public required Pen NodePen { get; init; }
	public required Pen HoverNodePen { get; init; }
	public required Brush HoverGlowBrush { get; init; }

	public Brush GetNodeBrush(bool isHovered) => isHovered ? HoverNodeBrush : NodeBrush;
	public Pen GetNodePen(bool isHovered) => isHovered ? HoverNodePen : NodePen;
}
