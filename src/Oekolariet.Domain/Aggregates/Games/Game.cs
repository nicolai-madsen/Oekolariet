using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Aggregates.Games;

public sealed class Game
{
    public Guid Id { get; private set; }
    public Guid VersionId { get; private set; }
    public int GuestNumber { get; private set; }
    public string AvatarKey { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int Score { get; private set; }
    private const int PointsPerCorrectAnswer = 10;
    private readonly List<GameAnswer> _answers = [];
    public IReadOnlyCollection<GameAnswer> Answers => _answers.AsReadOnly();

    private Game() { } // Used by EF Core when loading a saved game.

    public static Game Start(
        Guid id,
        Guid versionId,
        int guestNumber,
        string avatarKey,
        DateTime startedAtUtc,
        TimeSpan duration)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id må ikke være tomt.", nameof(id));
        }

        if (versionId == Guid.Empty)
        {
            throw new ArgumentException("Spillet skal have en udgivet version.", nameof(versionId));
        }

        if (guestNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(guestNumber));
        }

        if (string.IsNullOrWhiteSpace(avatarKey))
        {
            throw new ArgumentException("Vælg en avatar.", nameof(avatarKey));
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        return new Game
        {
            Id = id,
            VersionId = versionId,
            GuestNumber = guestNumber,
            AvatarKey = avatarKey.Trim(),
            StartedAt = startedAtUtc,
            EndsAt = startedAtUtc.Add(duration),
            Score = 0
        };
    }

    public bool HasExpired(DateTime nowUtc) => nowUtc >= EndsAt; // The game is still valid at the exact deadline, but not after.

    public void Complete(DateTime completedAtUtc)
    {
        // Keep the finish time from the first completion.
        CompletedAtUtc ??= completedAtUtc;
    }

    // Returns true if the answer was correct, false if it was incorrect.
    public bool SubmitAnswer(SortingItem item, Guid selectedCategoryId, DateTime answeredAtUtc)
    {
        if (HasExpired(answeredAtUtc))
        {
            throw new InvalidOperationException("Tiden er udløbet.");
        }

        if (selectedCategoryId == Guid.Empty)
        {
            throw new ArgumentException("Vælg en kategori.", nameof(selectedCategoryId));
        }

        if (_answers.Any(answer => answer.ItemId == item.Id))
        {
            throw new InvalidOperationException("Genstanden er allerede besvaret i denne runde.");
        }

        bool isCorrect = item.IsCorrectlySortedInto(selectedCategoryId);

        // A wrong answer also uses up the item for this round.
        _answers.Add(new GameAnswer(Id, item.Id));

        if (isCorrect)
        {
            Score += PointsPerCorrectAnswer;
        }

        return isCorrect;
    }
}
