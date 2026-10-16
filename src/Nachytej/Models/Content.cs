namespace Nachytej.Models;

/// <summary>Varovný znak ve zprávě: kategorie (viz <see cref="Signals"/>), úsek textu a vysvětlení.</summary>
public sealed record Highlight(string Signal, string Fragment, string Reason);

/// <summary>Jedna odpověď u konkrétní zprávy: přirozený text + kategorie akce, podle které se boduje.</summary>
public sealed record Choice(string Action, string Label);

/// <summary>Jedno zlaté pravidlo. Pevný, ručně ověřený text – negeneruje ho AI.</summary>
public sealed record Rule(int Id, string Title, string Text);

/// <summary>
/// Jeden tréninkový scénář. Obsah navrhla AI a zkontroloval člověk.
/// Správné odpovědi (ActionScores) jsou pevně dané šablonou, nezávisí na AI.
/// </summary>
public sealed record Scenario
{
    public string Id { get; init; } = "";
    /// <summary>"scam" nebo "legit"</summary>
    public string Kind { get; init; } = "scam";
    /// <summary>"sms", "email", "call", "chat", "web"</summary>
    public string Channel { get; init; } = "sms";
    /// <summary>Dva scénáře stejné obtížnosti – jeden jde do 1. kola, druhý do 2. kola.</summary>
    public string PairGroup { get; init; } = "";
    public int Difficulty { get; init; } = 1;
    /// <summary>Nejškodlivější typy podvodů – v každém tréninku je aspoň jeden.</summary>
    public bool Core { get; init; }
    /// <summary>Postava ve zprávě je žena (když si člověk nezadal vlastní jméno blízkého).</summary>
    public bool PersonaFemale { get; init; }
    /// <summary>Podmínky na profil, např. { "mobileBanking": true }. Null = pro všechny.</summary>
    public Dictionary<string, bool>? Requires { get; init; }
    public string Sender { get; init; } = "";
    public string? Subject { get; init; }
    public string Text { get; init; } = "";
    public List<Highlight> Highlights { get; init; } = new();
    /// <summary>Body za každou z 5 akcí (0–2).</summary>
    public Dictionary<string, int> ActionScores { get; init; } = new();
    /// <summary>Odpovědi, které k téhle zprávě dávají smysl (zobrazí se zamíchané).</summary>
    public List<Choice> Choices { get; init; } = new();
    public string Explanation { get; init; } = "";
    /// <summary>Tip pro průvodce (režim ve dvojici).</summary>
    public string? GuideTip { get; init; }
    public List<int> RuleIds { get; init; } = new();
    public string Source { get; init; } = "";
    public string ReviewedAt { get; init; } = "";

    public bool IsScam => Kind == "scam";
    public int MaxScore => ActionScores.Count == 0 ? 0 : ActionScores.Values.Max();
    public int ScoreFor(string action) => ActionScores.TryGetValue(action, out var s) ? s : 0;
    /// <summary>Nejvyšší skóre mezi nabízenými odpověďmi.</summary>
    public int MaxChoiceScore => Choices.Count == 0 ? MaxScore : Choices.Max(c => ScoreFor(c.Action));
    public Choice? ChoiceFor(string action) => Choices.FirstOrDefault(c => c.Action == action);
}
