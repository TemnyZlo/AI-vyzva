using Microsoft.AspNetCore.Mvc;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class DotaznikModel(ScenarioSelector selector) : TrainingPageModel
{
    public sealed record Question(string Key, string Self, string Pair);

    public static readonly IReadOnlyList<Question> Questions = new[]
    {
        new Question("mobileBanking", "Používáte bankovnictví v mobilu nebo v počítači?", "Používá bankovnictví v mobilu nebo v počítači?"),
        new Question("onlineShopping", "Nakupujete na internetu a chodí vám zásilky?", "Nakupuje na internetu a chodí mu/jí zásilky?"),
        new Question("marketplaces", "Prodáváte nebo kupujete na Bazoši, Vintedu a podobně?", "Prodává nebo kupuje na Bazoši, Vintedu a podobně?"),
        new Question("messaging", "Píšete si přes WhatsApp, Messenger nebo podobnou aplikaci?", "Píše si přes WhatsApp, Messenger nebo podobnou aplikaci?"),
        new Question("familyOnPhone", "Píšou vám děti nebo vnoučata na mobil?", "Píšou mu/jí děti nebo vnoučata na mobil?"),
    };

    [BindProperty] public Dictionary<string, string> Answers { get; set; } = new();

    // Volitelná personalizace (jen v session, do statistik se neukládá).
    [BindProperty] public string? Greeting { get; set; }
    [BindProperty] public string? PersonName { get; set; }
    [BindProperty] public string? PersonGender { get; set; }
    [BindProperty] public string? BankName { get; set; }
    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        if (!LoadState()) return RedirectToPage("/Index");
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!LoadState()) return RedirectToPage("/Index");

        if (Questions.Any(q => !Answers.TryGetValue(q.Key, out var v) || v is not ("ano" or "ne")))
        {
            Error = "Odpovězte prosím na všechny otázky (stačí ano / ne).";
            return Page();
        }

        bool Yes(string key) => Answers[key] == "ano";
        State!.Profile = new Profile
        {
            MobileBanking = Yes("mobileBanking"),
            OnlineShopping = Yes("onlineShopping"),
            Marketplaces = Yes("marketplaces"),
            Messaging = Yes("messaging"),
            FamilyOnPhone = Yes("familyOnPhone"),
            Greeting = Personalizer.Greetings.Contains(Greeting) ? Greeting : null,
            PersonName = Personalizer.Clean(PersonName),
            PersonFemale = PersonGender == "zena",
            BankName = Personalizer.Clean(BankName),
        };
        (State.Round1, State.Round2) = selector.Build(State.Profile);
        State.Round = 1;
        State.Index = 0;
        State.Answers.Clear();
        SaveState();
        return RedirectToPage("/Scenar");
    }
}
