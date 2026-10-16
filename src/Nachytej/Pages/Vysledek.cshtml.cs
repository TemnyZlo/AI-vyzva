using Microsoft.AspNetCore.Mvc;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class VysledekModel(StatsStore stats, ContentStore content) : TrainingPageModel
{
    public RoundResult Before { get; private set; } = null!;
    public RoundResult After { get; private set; } = null!;
    public bool Consent { get; private set; }
    /// <summary>Až 3 kategorie triků, na které se člověk chytil nejčastěji.</summary>
    public List<(Signals.SignalInfo Info, int Count)> WeakSpots { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!LoadState() || State!.Profile is null || State.Round != 2 || !State.RoundFinished) return RedirectToCurrentStep();

        Before = State.ResultFor(1);
        After = State.ResultFor(2);
        Consent = State.Consent;
        WeakSpots = TrainingState.WeakSignals(State.Answers, content.Find)
            .Take(3)
            .Select(x => (Signals.Get(x.Signal), x.Count))
            .ToList();

        if (State.Consent && !State.Saved)
        {
            await stats.AppendAsync(State);
            State.Saved = true;
            SaveState();
        }
        return Page();
    }

    public IActionResult OnPostRestart()
    {
        HttpContext.Session.ClearTraining();
        return RedirectToPage("/Index");
    }
}
