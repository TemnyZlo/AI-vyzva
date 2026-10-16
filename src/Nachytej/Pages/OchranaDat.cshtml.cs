using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nachytej.Services;

namespace Nachytej.Pages;

/// <summary>Informace o zpracování osobních údajů (čl. 13 GDPR) + možnost smazat průběh tréninku.</summary>
public class OchranaDatModel(IConfiguration config) : PageModel
{
    public string Controller => Value("Privacy:Controller");
    public string Contact => Value("Privacy:Contact");
    public string Hosting => Value("Privacy:Hosting");
    public string Retention => Value("Privacy:StatsRetention");
    public string Updated => Value("Privacy:Updated");
    public bool Deleted { get; private set; }

    public void OnGet(bool smazano = false) => Deleted = smazano;

    /// <summary>Okamžitě zahodí průběh tréninku uložený v paměti serveru a session cookie.</summary>
    public IActionResult OnPostSmazat()
    {
        HttpContext.Session.ClearTraining();
        HttpContext.Session.Clear();
        Response.Cookies.Delete(".Nachytej.Session");
        return RedirectToPage(new { smazano = true });
    }

    // Nevyplněná hodnota se ukáže výrazně, aby se na ni před zveřejněním nezapomnělo.
    private string Value(string key) =>
        string.IsNullOrWhiteSpace(config[key]) ? $"[DOPLNIT: {key}]" : config[key]!;
}
