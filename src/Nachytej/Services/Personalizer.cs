using System.Text.RegularExpressions;
using Nachytej.Models;

namespace Nachytej.Services;

/// <summary>
/// Dosadí do zprávy údaje, které si člověk volitelně nastavil (oslovení, jméno blízkého, banka).
/// Obyčejné nahrazení textu – žádná AI.
///
/// Zástupné značky v obsahu:
///   {banka}            → název banky, jinak „Banka Modrá“
///   {osloveni}         → „ mami“ / „ tati“ / „ babi“ / „ dědo“ (s mezerou), jinak nic
///   {blizky|výchozí}   → jméno blízkého, jinak výchozí text
///   {g:mužský|ženský}  → tvar podle pohlaví blízkého (bez jména podle scénáře)
/// </summary>
public static partial class Personalizer
{
    public const string DefaultBank = "Banka Modrá";

    [GeneratedRegex(@"\{g:([^|}]*)\|([^}]*)\}")]
    private static partial Regex GenderToken();

    [GeneratedRegex(@"\{blizky\|([^}]*)\}")]
    private static partial Regex PersonToken();

    public static Scenario Apply(Scenario s, Profile? p)
    {
        p ??= new Profile();
        string T(string text) => Fill(text, s, p);
        return s with
        {
            Sender = T(s.Sender),
            Subject = s.Subject is null ? null : T(s.Subject),
            Text = T(s.Text),
            Explanation = T(s.Explanation),
            GuideTip = s.GuideTip is null ? null : T(s.GuideTip),
            Choices = s.Choices.Select(c => c with { Label = T(c.Label) }).ToList(),
            Highlights = s.Highlights.Select(h => h with { Fragment = T(h.Fragment), Reason = T(h.Reason) }).ToList(),
        };
    }

    private static string Fill(string text, Scenario s, Profile p)
    {
        var hasPerson = !string.IsNullOrWhiteSpace(p.PersonName);
        var female = hasPerson ? p.PersonFemale : s.PersonaFemale;

        text = GenderToken().Replace(text, m => female ? m.Groups[2].Value : m.Groups[1].Value);
        text = PersonToken().Replace(text, m => hasPerson ? p.PersonName! : m.Groups[1].Value);
        text = text.Replace("{banka}", string.IsNullOrWhiteSpace(p.BankName) ? DefaultBank : p.BankName);
        text = text.Replace("{osloveni}", string.IsNullOrWhiteSpace(p.Greeting) ? "" : " " + p.Greeting);
        return text;
    }

    /// <summary>Očistí text zadaný uživatelem: oříznout, zkrátit, odstranit znaky šablon.</summary>
    public static string? Clean(string? input, int maxLength = 30)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var t = input.Trim().Replace("{", "").Replace("}", "").Replace("|", "");
        if (t.Length > maxLength) t = t[..maxLength].Trim();
        return t.Length == 0 ? null : t;
    }

    public static readonly IReadOnlyList<string> Greetings = new[] { "mami", "tati", "babi", "dědo" };
}
