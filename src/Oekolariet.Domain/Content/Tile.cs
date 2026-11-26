namespace Oekolariet.Domain.Content;

public sealed record Tile(
    TileType Type,
    string? Title,
    Guid? MediaId);

//Use the TileType for navigation purposes, so the right info is at the right place
public enum TileType
{
    Hierarchy,
    Quiz,
    Journeys,
    Offers,
    Home
}
