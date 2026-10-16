using Microsoft.AspNetCore.Mvc;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class ScenarModel(ContentStore content) : TrainingPageModel
{
    public RenderedMessage Message { get; private set; } = null!;
    public List<Choice> Choices { get; private set; } = new();
    public int Round { get; private set; }
    public int Number { get; private set; }
    public int Total { get; private set; }

    public IActionResult OnGet()
    {
        if (!LoadState() || State!.Profile is null || State.RoundFinished) return RedirectToCurrentStep();
        if (State.AnswerFor(State.Round, State.CurrentScenarioId!) is not null) return RedirectToPage("/Vysvetleni");
        Fill();
        return Page();
    }

    public IActionResult OnPost(string? choice)
    {
        if (!LoadState() || State!.Profile is null || State.RoundFinished) return RedirectToCurrentStep();
        var id = State.CurrentScenarioId!;
        if (State.AnswerFor(State.Round, id) is null && content.Get(id).ChoiceFor(choice ?? "") is not null)
        {
            var s = content.Get(id);
            State.Answers.Add(new AnswerRecord
            {
                ScenarioId = id,
                Round = State.Round,
                Action = choice!,
                Score = s.ScoreFor(choice!),
                MaxScore = s.MaxChoiceScore,
            });
            SaveState();
        }
        return RedirectToCurrentStep();
    }

    /// <summary>
    /// Zamíchá odpovědi, aby bezpečná nebyla pořád na stejném místě.
    /// Pořadí je pro danou session a zprávu stálé (obnovení stránky ho nezmění).
    /// </summary>
    private static List<Choice> Shuffle(List<Choice> choices, Guid session, string scenarioId)
    {
        var seed = BitConverter.ToInt32(session.ToByteArray(), 0);
        foreach (var ch in scenarioId) seed = unchecked(seed * 31 + ch);
        var rnd = new Random(seed);
        return choices.OrderBy(_ => rnd.Next()).ToList();
    }

    private void Fill()
    {
        var s = Personalizer.Apply(content.Get(State!.CurrentScenarioId!), State.Profile);
        Message = HighlightRenderer.Plain(s);
        Choices = Shuffle(s.Choices, State.SessionId, s.Id);
        Round = State.Round;
        Number = State.Index + 1;
        Total = State.CurrentRoundIds.Count;
    }
}
