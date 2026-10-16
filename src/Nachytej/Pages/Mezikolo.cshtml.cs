using Microsoft.AspNetCore.Mvc;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class MezikoloModel(ContentStore content) : TrainingPageModel
{
    public RoundResult Result { get; private set; } = null!;
    public IReadOnlyList<Rule> Rules => content.Rules;

    public IActionResult OnGet()
    {
        if (!LoadState() || State!.Profile is null || State.Round != 1 || !State.RoundFinished) return RedirectToCurrentStep();
        Result = State.ResultFor(1);
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!LoadState() || State!.Profile is null || State.Round != 1 || !State.RoundFinished) return RedirectToCurrentStep();
        State.Round = 2;
        State.Index = 0;
        SaveState();
        return RedirectToPage("/Scenar");
    }
}
