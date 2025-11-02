namespace ExUmbris.Models;

public sealed class Trait
{
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required IReadOnlyList<AttributeModifier> Modifiers { get; set; }
	public required double Frequency { get; set; }
}
