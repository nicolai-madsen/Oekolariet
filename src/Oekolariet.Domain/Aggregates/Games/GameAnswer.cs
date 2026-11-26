namespace Oekolariet.Domain.Aggregates.Games;

// Each GameAnswer records one item the guest has sorted in this game, whether the answer was correct or incorrect.
public sealed class GameAnswer
{
    public Guid GameId { get; private set; }
    public Guid ItemId { get; private set; }

    private GameAnswer() { } // Used by EF Core when loading a saved answer.

    // Only the game aggregate should record an item as answered.
    internal GameAnswer(Guid gameId, Guid itemId)
    {
        GameId = gameId;
        ItemId = itemId;
    }
}
