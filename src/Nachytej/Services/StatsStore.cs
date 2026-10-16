using System.Text.Json;
using Nachytej.Models;

namespace Nachytej.Services;

/// <summary>Anonymní výsledek jednoho tréninku. Žádné jméno, e-mail, telefon ani IP adresa.</summary>
public sealed record StatsRecord(
    Guid Id,
    DateTime CompletedAt,
    string Mode,
    Profile Profile,
    int Round1Score, int Round1Max, int Round1Safe,
    int Round2Score, int Round2Max, int Round2Safe,
    List<AnswerRecord> Answers);

/// <summary>
/// Ukládá anonymní statistiky (jen se souhlasem) jako řádky JSON do jednoho souboru.
/// Pro MVP stačí; později lze vyměnit za SQLite bez změny stránek.
/// </summary>
public sealed class StatsStore
{
    private readonly string _path;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public StatsStore(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Stats:FilePath"];
        _path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "data", "stats.jsonl")
            : configured;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public async Task AppendAsync(TrainingState t)
    {
        var r1 = t.ResultFor(1);
        var r2 = t.ResultFor(2);
        var record = new StatsRecord(t.SessionId, DateTime.UtcNow, t.Mode, (t.Profile ?? new Profile()).Anonymized(),
            r1.Score, r1.Max, r1.Safe, r2.Score, r2.Max, r2.Safe, t.Answers);
        var line = JsonSerializer.Serialize(record) + "\n";

        await Lock.WaitAsync();
        try { await File.AppendAllTextAsync(_path, line); }
        finally { Lock.Release(); }
    }

    public async Task<List<StatsRecord>> ReadAllAsync()
    {
        if (!File.Exists(_path)) return new();
        await Lock.WaitAsync();
        try
        {
            var lines = await File.ReadAllLinesAsync(_path);
            return lines.Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => JsonSerializer.Deserialize<StatsRecord>(l)!)
                .ToList();
        }
        finally { Lock.Release(); }
    }
}
