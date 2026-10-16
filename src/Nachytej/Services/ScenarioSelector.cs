using Nachytej.Models;

namespace Nachytej.Services;

/// <summary>
/// Personalizace bez LLM za běhu: vybere scénáře podle profilu.
/// Každá párová skupina dá jeden scénář do 1. kola a druhý do 2. kola,
/// takže srovnání před/po je férové (stejná obtížnost i typ podvodu).
/// </summary>
public sealed class ScenarioSelector(ContentStore store)
{
    public const int ScamsPerRound = 4;
    public const int LegitPerRound = 2;
    public const int MaxProfileSpecific = 1;

    public (List<string> Round1, List<string> Round2) Build(Profile profile, Random? random = null)
    {
        var rnd = random ?? Random.Shared;

        var groups = store.Scenarios
            .Where(s => Matches(s, profile))
            .GroupBy(s => s.PairGroup)
            .Where(g => g.Count() == 2)
            .Select(g => g.OrderBy(_ => rnd.Next()).ToList())
            .ToList();

        // 1) aspoň jeden nejškodlivější typ (falešný bankéř, „vnuk“),
        // 2) nejvýš jeden typ šitý na profil, aby nevytlačily ty obecné,
        // 3) zbytek náhodně.
        var scamGroups = groups.Where(g => g[0].IsScam).OrderBy(_ => rnd.Next()).ToList();
        var scams = new List<List<Scenario>>();
        scams.AddRange(scamGroups.Where(g => g[0].Core).Take(1));
        scams.AddRange(scamGroups.Where(g => g[0].Requires?.Count > 0 && !scams.Contains(g)).Take(MaxProfileSpecific));
        // Doplnit nejdřív obecnými typy; profilové jen tehdy, když obecné nestačí.
        scams.AddRange(scamGroups.Where(g => !scams.Contains(g))
            .OrderBy(g => g[0].Requires?.Count > 0)
            .Take(ScamsPerRound - scams.Count)
            .ToList());
        var legit = groups.Where(g => !g[0].IsScam)
            .OrderBy(_ => rnd.Next())
            .Take(LegitPerRound);

        var chosen = scams.Concat(legit).ToList();
        var r1 = chosen.Select(g => g[0].Id).OrderBy(_ => rnd.Next()).ToList();
        var r2 = chosen.Select(g => g[1].Id).OrderBy(_ => rnd.Next()).ToList();
        return (r1, r2);
    }

    private static bool Matches(Scenario s, Profile p) =>
        s.Requires is null || s.Requires.All(kv => p.Get(kv.Key) == kv.Value);
}
