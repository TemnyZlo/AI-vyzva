using Microsoft.AspNetCore.Mvc.RazorPages;
using Nachytej.Models;
using Nachytej.Services;

namespace Nachytej.Pages;

public class KartickaModel(ContentStore content) : PageModel
{
    public IReadOnlyList<Rule> Rules => content.Rules;
    public void OnGet() { }
}
