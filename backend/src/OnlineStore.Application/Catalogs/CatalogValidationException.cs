namespace OnlineStore.Application.Catalogs;

public sealed class CatalogValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public CatalogValidationException(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }
}
