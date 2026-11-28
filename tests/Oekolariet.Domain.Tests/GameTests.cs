using Oekolariet.Domain.Aggregates.Games;
using Oekolariet.Domain.Content;

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

    [Theory]
    [InlineData(true, 10)]
    [InlineData(false, 0)]
    public void SubmitAnswer_BeforeDeadline_ReturnsResultAndAwardsPoints(
    bool selectsCorrectCategory, int expectedScore)
    {
        var startedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var game = Game.Start(Guid.NewGuid(), Guid.NewGuid(), 1, "odder",
            startedAt, TimeSpan.FromSeconds(30));

        var plasticId = Guid.NewGuid();
        var paperId = Guid.NewGuid();
        var item = new SortingItem(Guid.NewGuid(), "Plastflaske", null, plasticId, null);
        var selectedCategoryId = selectsCorrectCategory ? plasticId : paperId;

        var isCorrect = game.SubmitAnswer(item, selectedCategoryId, startedAt.AddSeconds(5));

        Assert.Equal(selectsCorrectCategory, isCorrect);
        Assert.Equal(expectedScore, game.Score);
    }

    [Fact]
    public void SubmitAnswer_AtDeadline_RejectsAnswerAndKeepsScore()
    {
        var startedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var game = Game.Start(Guid.NewGuid(), Guid.NewGuid(), 1, "odder",
            startedAt, TimeSpan.FromSeconds(30));

        var plasticId = Guid.NewGuid();
        var item = new SortingItem(Guid.NewGuid(), "Plastflaske", null, plasticId, null);

        // Even a correct answer is too late at the exact end of the round.
        Assert.Throws<InvalidOperationException>(() =>
            game.SubmitAnswer(item, plasticId, game.EndsAt));

        Assert.Equal(0, game.Score);
    }

    [Fact]
    public void Start_NewGame_HasNoCompletionTime()
    {
        var startedAtUtc = new DateTime(2026, 11, 28, 10, 0, 0, DateTimeKind.Utc);

        var game = Game.Start(
            id: Guid.NewGuid(),
            versionId: Guid.NewGuid(),
            guestNumber: 1,
            avatarKey: "odder",
            startedAtUtc: startedAtUtc,
            duration: TimeSpan.FromSeconds(30));

        Assert.Null(game.CompletedAtUtc);
    }

    [Fact]
    public void Complete_CalledTwice_KeepsFirstCompletionTime()
    {
        var startedAtUtc = new DateTime(2026, 11, 28, 10, 0, 0, DateTimeKind.Utc);
        var completedAtUtc = startedAtUtc.AddSeconds(20);

        var game = Game.Start(
            id: Guid.NewGuid(),
            versionId: Guid.NewGuid(),
            guestNumber: 1,
            avatarKey: "odder",
            startedAtUtc: startedAtUtc,
            duration: TimeSpan.FromSeconds(30));

        game.Complete(completedAtUtc);
        game.Complete(completedAtUtc.AddSeconds(5));

        Assert.Equal(completedAtUtc, game.CompletedAtUtc);
    }
}
