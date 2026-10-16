namespace Nachytej.Models;

/// <summary>
/// Kategorie varovných znaků. Každé zvýraznění ve zprávě patří do jedné kategorie,
/// takže umíme člověku na konci říct, na jaký typ triku je nejvíc náchylný.
/// </summary>
public static class Signals
{
    public sealed record SignalInfo(string Key, string Icon, string Label, string Tip);

    public static readonly IReadOnlyList<SignalInfo> All = new[]
    {
        new SignalInfo("spech", "⏱️", "Spěch a nátlak", "Když někdo spěchá, zpomalte. Skutečná banka ani úřad vás nehoní – a spěch je trik, který lidé přehlížejí nejčastěji."),
        new SignalInfo("penize", "💸", "Žádost o peníze nebo poplatek", "Jakákoli žádost o peníze je důvod si to ověřit jinou cestou – i když jde o „malý poplatek“."),
        new SignalInfo("udaje", "🔑", "Žádost o údaje, kódy nebo kartu", "PIN, hesla, kódy z SMS ani údaje z karty nikomu. Na přijetí peněz stačí číslo účtu."),
        new SignalInfo("odkaz", "🔗", "Podezřelý odkaz nebo adresa", "Na web nebo do aplikace jděte sami, ne přes odkaz ve zprávě. Adresa může jménu firmy jen připomínat."),
        new SignalInfo("identita", "🎭", "Vydává se za někoho", "Banka, policie, technik i vnuk se dají napodobit – dnes i hlasem. Ověřte si to na čísle, které znáte vy."),
        new SignalInfo("utajeni", "🤫", "Utajení a bránění v ověření", "„Nikomu to neříkej“ nebo „nevolej mi“ je silný varovný signál. Zeptejte se rodiny."),
        new SignalInfo("aplikace", "📲", "Instalace programu na žádost", "Nic neinstalujte na žádost cizího člověka. Program na dálkové ovládání mu dá váš telefon i bankovnictví."),
        new SignalInfo("lakadlo", "🎁", "Příliš dobré, aby to byla pravda", "Výhra, o kterou jste nesoutěžili, zaručený zisk nebo obří sleva bývají návnada."),
        new SignalInfo("ok", "✅", "Znak skutečné zprávy", "Nechce peníze, údaje ani klikání a nespěchá."),
    };

    private static readonly Dictionary<string, SignalInfo> ByKey = All.ToDictionary(s => s.Key);

    public static bool IsValid(string? key) => key is not null && ByKey.ContainsKey(key);
    public static SignalInfo Get(string key) => ByKey[key];
}
