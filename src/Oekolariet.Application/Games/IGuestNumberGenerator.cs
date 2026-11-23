namespace Oekolariet.Application.Games;

public interface IGuestNumberGenerator
{
    // Two games starting at the same time must receive different numbers.
    Task<int> NextAsync(CancellationToken ct);
}
