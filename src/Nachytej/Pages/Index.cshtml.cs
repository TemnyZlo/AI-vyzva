using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class IndexModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnPost(string? mode, bool consent)
    {
        var state = new TrainingState
        {
            Mode = mode == "dvojice" ? "dvojice" : "sam",
            Consent = consent,
        };
        HttpContext.Session.SetTraining(state);
        return RedirectToPage("/Dotaznik");
    }
}
