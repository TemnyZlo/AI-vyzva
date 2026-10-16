namespace Nachytej.Models;

/// <summary>Pět univerzálních akcí, ze kterých uživatel vybírá u každé zprávy.</summary>
public static class UserActions
{
    public const string ClickOrReply = "click_or_reply";
    public const string CallNumberInMessage = "call_number_in_message";
    public const string VerifyMyself = "verify_myself";
    public const string AskFamily = "ask_family";
    public const string Ignore = "ignore";

    public sealed record ActionInfo(string Key, string Icon, string Label, string Hint);

    public static readonly IReadOnlyList<ActionInfo> All = new[]
    {
        new ActionInfo(ClickOrReply, "👆", "Udělám, co zpráva chce", "kliknu, odpovím, pošlu, nainstaluji"),
        new ActionInfo(CallNumberInMessage, "📞", "Zavolám na číslo ze zprávy", "nebo zavolám zpět na číslo, které volalo"),
        new ActionInfo(VerifyMyself, "🔍", "Ověřím si to sám", "číslo ze zadní strany karty, oficiální web, aplikace"),
        new ActionInfo(AskFamily, "👨‍👩‍👧", "Zeptám se rodiny", "nebo někoho, komu věřím"),
        new ActionInfo(Ignore, "🙅", "Nic neudělám", "jen si to přečtu / zavěsím"),
    };

    public static bool IsValid(string? key) => All.Any(a => a.Key == key);
    public static ActionInfo Get(string key) => All.First(a => a.Key == key);
}
