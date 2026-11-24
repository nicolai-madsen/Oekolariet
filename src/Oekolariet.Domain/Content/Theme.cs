namespace Oekolariet.Domain.Content;

public sealed record Theme(
    string PrimaryColor,
    string SecondaryColor,
    string FontFamily,
    Guid? LogoMediaId,
    Guid? BackgroundMediaId);
