using Oekolariet.Domain.Aggregates.Games;

namespace Oekolariet.Domain.Tests;

public sealed class GameTests
{
    [Fact]
    public void Start_CreatesGameWithSelectedAvatarAndZeroScore()
    {
        var id = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var start = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);

        var game = Game.Start(id, versionId, 42, "odder", start, TimeSpan.FromSeconds(30));

        Assert.Equal(id, game.Id);
        Assert.Equal(versionId, game.VersionId);
        Assert.Equal(42, game.GuestNumber);
        Assert.Equal("odder", game.AvatarKey);
        Assert.Equal(0, game.Score);
        Assert.Equal(start, game.StartedAt);
        Assert.Equal(start.AddSeconds(30), game.EndsAt);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void HasExpired_UsesTheExactDeadline(int ticksFromDeadline, bool expected)
    {
        var start = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromSeconds(30);
        var game = Game.Start(Guid.NewGuid(), Guid.NewGuid(), 42, "odder", start, duration);

        // Check just before, exactly at, and just after the deadline.
        var now = start.Add(duration).AddTicks(ticksFromDeadline);

        Assert.Equal(expected, game.HasExpired(now));
    }
}
