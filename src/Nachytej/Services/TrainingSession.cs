using System.Text.Json;
using Nachytej.Models;

namespace Nachytej.Services;

public static class TrainingSessionExtensions
{
    private const string Key = "training";

    public static TrainingState? GetTraining(this ISession session)
    {
        var json = session.GetString(Key);
        return json is null ? null : JsonSerializer.Deserialize<TrainingState>(json);
    }

    public static void SetTraining(this ISession session, TrainingState state) =>
        session.SetString(Key, JsonSerializer.Serialize(state));

    public static void ClearTraining(this ISession session) => session.Remove(Key);
}
