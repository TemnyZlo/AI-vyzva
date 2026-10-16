namespace Nachytej.Models;

/// <summary>Odpovědi z dotazníku. Jen ano/ne – nic, co by identifikovalo člověka.</summary>
public sealed class Profile
{
    public bool MobileBanking { get; set; }
    public bool OnlineShopping { get; set; }
    public bool Marketplaces { get; set; }
    public bool Messaging { get; set; }
    public bool FamilyOnPhone { get; set; }

    // Volitelná personalizace. Žije jen v session, do statistik se NEukládá (viz Anonymized).
    public string? Greeting { get; set; }
    public string? PersonName { get; set; }
    public bool PersonFemale { get; set; }
    public string? BankName { get; set; }

    /// <summary>Kopie bez osobních údajů – jen odpovědi ano/ne.</summary>
    public Profile Anonymized() => new()
    {
        MobileBanking = MobileBanking,
        OnlineShopping = OnlineShopping,
        Marketplaces = Marketplaces,
        Messaging = Messaging,
        FamilyOnPhone = FamilyOnPhone,
    };

    public bool? Get(string key) => key switch
    {
        "mobileBanking" => MobileBanking,
        "onlineShopping" => OnlineShopping,
        "marketplaces" => Marketplaces,
        "messaging" => Messaging,
        "familyOnPhone" => FamilyOnPhone,
        _ => null,
    };
}

public sealed class AnswerRecord
{
    public string ScenarioId { get; set; } = "";
    public int Round { get; set; }
    public string Action { get; set; } = "";
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Stav tréninku – žije jen v session (cookie), do souboru se nic neukládá bez souhlasu.</summary>
public sealed class TrainingState
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    /// <summary>"sam" nebo "dvojice"</summary>
    public string Mode { get; set; } = "sam";
    public bool Consent { get; set; }
    public Profile? Profile { get; set; }
    public List<string> Round1 { get; set; } = new();
    public List<string> Round2 { get; set; } = new();
    public int Round { get; set; } = 1;
    public int Index { get; set; }
    public List<AnswerRecord> Answers { get; set; } = new();
    public bool Saved { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public bool IsPair => Mode == "dvojice";
    public List<string> CurrentRoundIds => Round == 1 ? Round1 : Round2;
    public string? CurrentScenarioId => Index < CurrentRoundIds.Count ? CurrentRoundIds[Index] : null;
    public bool RoundFinished => Index >= CurrentRoundIds.Count;

    public AnswerRecord? AnswerFor(int round, string scenarioId) =>
        Answers.FirstOrDefault(a => a.Round == round && a.ScenarioId == scenarioId);

    /// <summary>
    /// Kategorie varovných znaků u podvodů, kde člověk nezvolil nejbezpečnější postup.
    /// Každá kategorie se u jedné zprávy počítá jen jednou.
    /// </summary>
    public static List<(string Signal, int Count)> WeakSignals(IEnumerable<AnswerRecord> answers, Func<string, Scenario?> find) =>
        answers.Where(a => a.Score < a.MaxScore)
            .Select(a => find(a.ScenarioId))
            .Where(s => s is { IsScam: true })
            .SelectMany(s => s!.Highlights.Select(h => h.Signal).Distinct())
            .GroupBy(x => x)
            .Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Item2)
            .ToList();

    public RoundResult ResultFor(int round)
    {
        var list = Answers.Where(a => a.Round == round).ToList();
        return new RoundResult(
            Count: list.Count,
            Safe: list.Count(a => a.Score == a.MaxScore),
            Score: list.Sum(a => a.Score),
            Max: list.Sum(a => a.MaxScore));
    }
}

public sealed record RoundResult(int Count, int Safe, int Score, int Max)
{
    public int Percent => Max == 0 ? 0 : (int)Math.Round(100.0 * Score / Max);
}
