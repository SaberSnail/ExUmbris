using System.IO;
using System.Text.Json;
using ExUmbris.Models;
using GoldenAnvil.Utility;
using GoldenAnvil.Utility.Logging;
using GoldenAnvil.Utility.Windows.Async;

namespace ExUmbris.Data;

internal class NameHelper
{
	public async static Task InitializeAsync(TaskStateController state)
	{
		string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "names.json");
		if (!File.Exists(filePath))
			throw new FileNotFoundException($"names.json not found at {filePath}");

		try
		{
			using var stream = File.OpenRead(filePath);
			var json = await JsonDocument.ParseAsync(stream, cancellationToken: state.CancellationToken);
			var dict = new Dictionary<NameKind, IReadOnlyList<string>>();
			foreach (var prop in json.RootElement.EnumerateObject())
			{
				if (Enum.TryParse<NameKind>(prop.Name, out var kind))
				{
					var names = prop.Value.EnumerateArray()
						.Select(x => x.GetString() ?? string.Empty)
						.Where(x => x.Length != 0)
						.AsReadOnlyList();
					dict[kind] = names;
				}
				else
				{
					Log.Error($"Unknown NameKind \"{prop.Name}\" in names.json");
				}
			}
			s_names = dict;
		}
		catch (JsonException ex)
		{
			Log.Error($"Failed to parse names.json: {ex}");
			throw;
		}
	}

	public static string GetRandomSystem(Random rng) => GetRandomName(NameKind.System, rng);

	public static string GetRandomPlanet(Random rng) => GetRandomName(NameKind.Planet, rng);

	public static (string First, string Last) GetRandomActor(GenderKind gender, Random rng)
	{
		var firstNameKind = gender switch
		{
			GenderKind.Male => NameKind.ActorMale,
			GenderKind.Female => NameKind.ActorFemale,
			_ => throw new NotImplementedException($"Unhandled GenderKind: {gender}"),
		};
		string first = GetRandomName(firstNameKind, rng);
		string last = GetRandomName(NameKind.ActorLastName, rng);
		return (first, last);
	}

	public static (string First, string Last) GetRandomFemaleActor(Random rng)
	{
		string first = GetRandomName(NameKind.ActorFemale, rng);
		string last = GetRandomName(NameKind.ActorLastName, rng);
		return (first, last);
	}

	public static string GetRandomJumpStation(string systemName, Random rng)
	{
		var baseName = GetRandomName(NameKind.LocationJumpStation, rng);
		return baseName.Replace("{System}", systemName);
	}

	public static string GetRandomLocation(LocationKind location, string planetName, Random rng)
	{
		var nameKind = location switch
		{
			LocationKind.PlanetaryHub => NameKind.LocationPlanetaryHub,
			LocationKind.Commercial => NameKind.LocationCommercial,
			LocationKind.ResourceExtraction => NameKind.LocationResourceExtraction,
			LocationKind.Farming => NameKind.LocationFarming,
			LocationKind.Governmental => NameKind.LocationGovernmental,
			LocationKind.Industrial => NameKind.LocationIndustrial,
			LocationKind.Residential => NameKind.LocationResidential,
			LocationKind.Tourism => NameKind.LocationTourism,
			LocationKind.Wilderness => NameKind.LocationWilderness,
			LocationKind.JumpStation => throw new ArgumentException("Use GetRandomJumpStation for jump stations.", nameof(location)),
			_ => throw new NotImplementedException($"Unhandled LocationKind value: {location}"),
		};
		var baseName = GetRandomName(nameKind, rng);
		return baseName.Replace("{Planet}", planetName);
	}

	private static string GetRandomName(NameKind kind, Random rng)
	{
		var names = s_names[kind];
		int index = rng.Next(names.Count);
		return names[index];
	}

	public enum NameKind
	{
		System,
		Planet,
		ActorMale,
		ActorFemale,
		ActorLastName,
		LocationJumpStation,
		LocationPlanetaryHub,
		LocationCommercial,
		LocationResourceExtraction,
		LocationFarming,
		LocationGovernmental,
		LocationIndustrial,
		LocationResidential,
		LocationTourism,
		LocationWilderness,
	}

	private static ILogSource Log { get; } = LogManager.CreateLogSource(nameof(NameHelper));

	private static Dictionary<NameKind, IReadOnlyList<string>> s_names = new();
}
