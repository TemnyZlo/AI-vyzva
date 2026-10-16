using System.Text.Json;
using Nachytej.Models;

namespace Nachytej.Services;

/// <summary>
/// Načte scénáře a pravidla z Content/*.json a při startu je zkontroluje.
/// Chybný obsah = aplikace nenastartuje, takže se nedostane k lidem.
/// </summary>
public sealed class ContentStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public IReadOnlyList<Scenario> Scenarios { get; }
    public IReadOnlyList<Rule> Rules { get; }
    private readonly Dictionary<string, Scenario> _byId;
    private readonly Dictionary<int, Rule> _rulesById;

    public ContentStore(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "Content");
        Rules = Load<List<Rule>>(Path.Combine(dir, "rules.json"));
        Scenarios = Load<List<Scenario>>(Path.Combine(dir, "scenarios.json"));
        _byId = Scenarios.ToDictionary(s => s.Id);
        _rulesById = Rules.ToDictionary(r => r.Id);
        Validate();
    }

    public Scenario Get(string id) => _byId[id];
    public Scenario? Find(string id) => _byId.GetValueOrDefault(id);
    public IEnumerable<Rule> RulesFor(Scenario s) => s.RuleIds.Select(id => _rulesById[id]);

    private static T Load<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
        ?? throw new InvalidOperationException($"Prázdný soubor s obsahem: {path}");

    private void Validate()
    {
        var errors = new List<string>();

        var samples = new[]
        {
            new Profile(),
            new Profile { Greeting = "babi", PersonName = "Kristýna", PersonFemale = true, BankName = "Moje Banka" },
        };
        foreach (var raw in Scenarios)
        foreach (var sample in samples)
        {
            var s = Personalizer.Apply(raw, sample);
            if (s.Choices.Count < 3)
                errors.Add($"{s.Id}: potřebuje aspoň 3 odpovědi");
            if (s.Choices.Select(c => c.Action).Distinct().Count() != s.Choices.Count)
                errors.Add($"{s.Id}: dvě odpovědi mají stejnou kategorii akce");
            foreach (var c in s.Choices)
                if (!UserActions.IsValid(c.Action)) errors.Add($"{s.Id}: neznámá akce '{c.Action}' u odpovědi");
            if (s.Choices.Count > 0 && s.MaxChoiceScore < 2)
                errors.Add($"{s.Id}: mezi odpověďmi není žádná plně bezpečná (2 body)");
            if ((s.Sender + s.Subject + s.Text + s.Explanation + s.GuideTip + string.Concat(s.Choices.Select(c => c.Label))).Contains('{'))
                errors.Add($"{s.Id}: neznámá zástupná značka v textu");
            foreach (var a in UserActions.All)
                if (!s.ActionScores.ContainsKey(a.Key))
                    errors.Add($"{s.Id}: chybí bodování akce '{a.Key}'");
            if (s.Kind is not ("scam" or "legit")) errors.Add($"{s.Id}: neznámý druh '{s.Kind}'");
            if (s.Channel is not ("sms" or "email" or "call" or "chat" or "web")) errors.Add($"{s.Id}: neznámý kanál '{s.Channel}'");
            foreach (var r in s.RuleIds)
                if (!_rulesById.ContainsKey(r)) errors.Add($"{s.Id}: neexistující pravidlo {r}");
            var all = s.Sender + "\n" + s.Subject + "\n" + s.Text;
            foreach (var h in s.Highlights)
            {
                if (!all.Contains(h.Fragment, StringComparison.Ordinal))
                    errors.Add($"{s.Id}: zvýraznění '{h.Fragment}' není ve zprávě");
                if (!Signals.IsValid(h.Signal))
                    errors.Add($"{s.Id}: neznámá kategorie znaku '{h.Signal}'");
                else if ((h.Signal == "ok") == s.IsScam)
                    errors.Add($"{s.Id}: kategorie 'ok' patří jen ke skutečným zprávám (a naopak)");
            }
            if (s.ActionScores.Values.Any(v => v is < 0 or > 2)) errors.Add($"{s.Id}: body mimo rozsah 0–2");
        }

        foreach (var g in Scenarios.GroupBy(s => s.PairGroup))
        {
            if (g.Count() != 2) errors.Add($"Skupina '{g.Key}' má {g.Count()} scénářů, musí mít přesně 2");
            if (g.Select(s => s.Kind).Distinct().Count() > 1) errors.Add($"Skupina '{g.Key}' míchá podvod a legitimní zprávu");
            if (g.Select(s => s.Core).Distinct().Count() > 1) errors.Add($"Skupina '{g.Key}' má nejednotný příznak core");
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Chyby v obsahu:\n - " + string.Join("\n - ", errors));
    }
}
