using Oekolariet.Application.Common;
using Oekolariet.Application.Common.Errors;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Games;

public sealed class SubmitAnswerHandler(
    IGameRepository games,
    IExhibitionVersionRepository versions,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<AnswerResultDto> HandleAsync(
        Guid gameId,
        Guid itemId,
        Guid categoryId,
        CancellationToken ct)
    {
        var game = await games.GetByIdAsync(gameId, ct)
            ?? throw new NotFoundException("Spil", gameId);

        // Use the version the guest started with, even if a newer one has been published.
        var version = await versions.GetByIdAsync(game.VersionId, ct)
            ?? throw new InvalidOperationException(
                $"Spillets udstillingsversion {game.VersionId} mangler i databasen.");

        var content = version.Content.Game;
        var item = content.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("Genstand", itemId);

        if (!content.Categories.Any(category => category.Id == categoryId))
        {
            throw new ArgumentException(
                "Den valgte kategori findes ikke i denne spilrunde.", nameof(categoryId));
        }

        bool isCorrect;

        var answeredAtUtc = clock.GetUtcNow().UtcDateTime;

        try
        {
            isCorrect = game.SubmitAnswer(item, categoryId, answeredAtUtc);
        }
        catch (InvalidOperationException ex)
        {
            // An expired round or a second answer for the same item is an HTTP 409 conflict.
            throw new ConflictException(ex.Message);
        }

        var answeredCount = game.Answers.Count;
        var totalCount = content.Items.Count;
        var isGameCompleted = answeredCount == totalCount || game.HasExpired(answeredAtUtc);

        if (isGameCompleted)
        {
            game.Complete(answeredAtUtc);
        }

        // Save the final answer and completion time in the same transaction.
        await unitOfWork.SaveChangesAsync(ct);

        return new AnswerResultDto(
            IsCorrect: isCorrect,
            CorrectCategoryId: item.CorrectCategoryId,
            Explanation: item.Explanation,
            AnsweredCount: answeredCount,
            TotalCount: totalCount,
            IsGameCompleted: isGameCompleted);
    }
}
