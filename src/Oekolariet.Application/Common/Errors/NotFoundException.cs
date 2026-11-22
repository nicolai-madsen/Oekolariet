namespace Oekolariet.Application.Common.Errors;

public sealed class NotFoundException(string resourceName, Guid id)
    : Exception($"{resourceName} med id {id} blev ikke fundet.");
