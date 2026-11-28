using Oekolariet.Application.Common.Errors;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Games;

public enum HighscorePeriod
{
    Day,
    Month
}

public sealed class GetHighscoresHandler(
    IGameRepository games,
    TimeProvider clock)
{
    private static readonly TimeZoneInfo DanishTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public async Task<List<HighscoreEntryDto>> HandleAsync(
        Guid gameId,
        HighscorePeriod period,
        CancellationToken ct)
    {
        var game = await games.GetByIdAsync(gameId, ct);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var today = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), DanishTimeZone).Date;

        var (start, end) = period switch
        {
            HighscorePeriod.Day => (today, today.AddDays(1)),
            HighscorePeriod.Month => GetMonthRange(today),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

        // Convert each local midnight separately because the UTC offset can change.
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(start, DanishTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(end, DanishTimeZone);

        return await games.GetHighscoresAsync(game.VersionId, fromUtc, toUtc, ct);
    }

    private static (DateTime Start, DateTime End) GetMonthRange(DateTime date)
    {
        var start = new DateTime(date.Year, date.Month, 1);
        return (start, start.AddMonths(1));
    }
}
