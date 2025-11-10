namespace UserService.Model.Utilities;

public static class Sanitizer
{
    public static string Sanitize(string input) =>
        input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
}