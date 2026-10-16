using Microsoft.AspNetCore.Mvc;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class VysvetleniModel(ContentStore content) : TrainingPageModel
{
    public RenderedMessage Message { get; private set; } = null!;
    public AnswerRecord Answer { get; private set; } = null!;
    public List<Rule> Rules { get; private set; } = new();
    public bool IsLastInRound { get; private set; }
    public int Round { get; private set; }

    public Scenario Scenario => Message.Scenario;
    public bool BestAnswer => Answer.Score == Answer.MaxScore;

    public IActionResult OnGet()
    {
        if (!LoadState() || State!.Profile is null || State.RoundFinished) return RedirectToCurrentStep();
        var id = State.CurrentScenarioId!;
        var answer = State.AnswerFor(State.Round, id);
        if (answer is null) return RedirectToPage("/Scenar");

        var s = Personalizer.Apply(content.Get(id), State.Profile);
        Message = HighlightRenderer.WithHighlights(s);
        Answer = answer;
        Rules = content.RulesFor(s).ToList();
        Round = State.Round;
        IsLastInRound = State.Index == State.CurrentRoundIds.Count - 1;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!LoadState() || State!.Profile is null || State.RoundFinished) return RedirectToCurrentStep();
        // Posunout se jen tehdy, když je aktuální zpráva zodpovězená (dvojklik nepřeskočí zprávu).
        if (State.AnswerFor(State.Round, State.CurrentScenarioId!) is not null)
        {
            State.Index++;
            SaveState();
        }
        return RedirectToCurrentStep();
    }
}
