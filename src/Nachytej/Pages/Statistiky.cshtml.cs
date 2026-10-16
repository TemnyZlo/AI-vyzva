using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

/// <summary>Anonymní souhrn pro autora. Chráněno klíčem v URL: /Statistiky?key=...</summary>
public class StatistikyModel(StatsStore stats, ContentStore content, IConfiguration config) : PageModel
{
    public sealed record ScenarioRow(string Id, bool IsScam, int Answered, int Safe);

    public int Sessions { get; private set; }
    public int Improved { get; private set; }
    public double AvgBefore { get; private set; }
    public double AvgAfter { get; private set; }
    public List<ScenarioRow> Rows { get; private set; } = new();
    public List<(Signals.SignalInfo Info, int Count)> Weak { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(string? key)
    {
        var expected = config["Stats:Key"];
        if (string.IsNullOrWhiteSpace(expected) || key != expected) return NotFound();

        var all = await stats.ReadAllAsync();
        Sessions = all.Count;
        if (Sessions > 0)
        {
            AvgBefore = all.Average(r => r.Round1Max == 0 ? 0 : 100.0 * r.Round1Score / r.Round1Max);
            AvgAfter = all.Average(r => r.Round2Max == 0 ? 0 : 100.0 * r.Round2Score / r.Round2Max);
            Improved = all.Count(r => r.Round2Safe > r.Round1Safe);
        }

        Weak = TrainingState.WeakSignals(all.SelectMany(r => r.Answers), content.Find)
            .Select(x => (Signals.Get(x.Signal), x.Count)).ToList();

        Rows = all.SelectMany(r => r.Answers)
            .GroupBy(a => a.ScenarioId)
            .Select(g => new ScenarioRow(g.Key, content.Find(g.Key)?.IsScam ?? true, g.Count(), g.Count(a => a.Score == a.MaxScore)))
            .OrderBy(r => r.Answered == 0 ? 1 : (double)r.Safe / r.Answered)
            .ToList();
        return Page();
    }
}
