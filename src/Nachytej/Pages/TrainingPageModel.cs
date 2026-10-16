using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

/// <summary>Společný základ stránek tréninku: načtení stavu ze session a navigace.</summary>
public abstract class TrainingPageModel : PageModel
{
    protected TrainingState? State { get; private set; }
    public bool IsPair => State?.IsPair == true;

    protected bool LoadState()
    {
        State = HttpContext.Session.GetTraining();
        return State is not null;
    }

    protected void SaveState() => HttpContext.Session.SetTraining(State!);

    /// <summary>Kam má člověk jít podle stavu tréninku (ochrana proti tlačítku Zpět a přímým odkazům).</summary>
    protected IActionResult RedirectToCurrentStep()
    {
        if (State is null) return RedirectToPage("/Index");
        if (State.Profile is null) return RedirectToPage("/Dotaznik");
        if (State.RoundFinished) return RedirectToPage(State.Round == 1 ? "/Mezikolo" : "/Vysledek");
        var id = State.CurrentScenarioId!;
        return State.AnswerFor(State.Round, id) is null ? RedirectToPage("/Scenar") : RedirectToPage("/Vysvetleni");
    }
}
