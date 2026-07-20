namespace Records;

public sealed record ShaclValidationOutcome(bool Conforms, IReadOnlyList<string> Messages)
{
    public static readonly ShaclValidationOutcome Success = new(true, []);
}
