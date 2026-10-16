using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Nachytej.Models;

namespace Nachytej.Services;

/// <summary>Zpráva připravená k zobrazení, případně se zvýrazněnými a očíslovanými varovnými znaky.</summary>
public sealed class RenderedMessage
{
    public required Scenario Scenario { get; init; }
    public required IHtmlContent Sender { get; init; }
    public IHtmlContent? Subject { get; init; }
    public required IHtmlContent Body { get; init; }
    /// <summary>Varovné znaky v pořadí čísel, která se zobrazí ve zprávě.</summary>
    public List<Highlight> Marks { get; init; } = new();
}

public static class HighlightRenderer
{
    public static RenderedMessage Plain(Scenario s) => new()
    {
        Scenario = s,
        Sender = new HtmlString(Encode(s.Sender)),
        Subject = s.Subject is null ? null : new HtmlString(Encode(s.Subject)),
        Body = new HtmlString(Encode(s.Text)),
    };

    public static RenderedMessage WithHighlights(Scenario s)
    {
        var marks = new List<Highlight>();
        var remaining = s.Highlights.ToList();

        // Každé zvýraznění se použije v prvním poli, kde se vyskytuje: odesílatel → předmět → text.
        IHtmlContent Field(string text)
        {
            var hits = remaining
                .Select(h => (h, idx: text.IndexOf(h.Fragment, StringComparison.Ordinal)))
                .Where(x => x.idx >= 0)
                .OrderBy(x => x.idx)
                .ToList();

            var sb = new StringBuilder();
            var pos = 0;
            foreach (var (h, idx) in hits)
            {
                if (idx < pos) continue; // překryv – přeskočit
                remaining.Remove(h);
                marks.Add(h);
                sb.Append(Encode(text[pos..idx]));
                sb.Append($"<mark class=\"warn\"><span class=\"warn-text\">{Encode(h.Fragment)}</span><sup class=\"warn-num\">{marks.Count}</sup></mark>");
                pos = idx + h.Fragment.Length;
            }
            sb.Append(Encode(text[pos..]));
            return new HtmlString(sb.ToString());
        }

        var sender = Field(s.Sender);
        var subject = s.Subject is null ? null : Field(s.Subject);
        var body = Field(s.Text);
        return new RenderedMessage { Scenario = s, Sender = sender, Subject = subject, Body = body, Marks = marks };
    }

    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    // Mezera uvnitř čísla („25 000 Kč“, „+420 000 111“) a před „Kč“ nesmí zalomit řádek.
    private static readonly Regex NumberSpace = new(@"(?<=\d) (?=\d|Kč)", RegexOptions.Compiled);

    // Odkaz ve zprávě vypadá jako odkaz, „[ TEXT ]“ jako tlačítko – ale nejsou klikací.
    private static readonly Regex Link = new(@"https?://[^\s<]+", RegexOptions.Compiled);
    private static readonly Regex FakeButton = new(@"\[ (.+?) \]", RegexOptions.Compiled);

    private static string Encode(string text) =>
        string.Join("<br>", text.Replace("\r\n", "\n").Split('\n')
            .Select(line =>
            {
                var html = Encoder.Encode(NumberSpace.Replace(line, "\u00A0"));
                html = Link.Replace(html, m => $"<span class=\"fake-link\">{m.Value}</span>");
                html = FakeButton.Replace(html, m => $"<span class=\"fake-button\">{m.Groups[1].Value}</span>");
                return html;
            }));
}
