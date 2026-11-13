namespace OnlineStore.Application.Auth;

public static class PasswordPolicy
{
    public const int MinimumPasswordLength = 8;

    public static List<string> Validate(string password)
    {
        var errors = new List<string>();
        if (password.Length < MinimumPasswordLength)
        {
            errors.Add($"Passwords must be at least {MinimumPasswordLength} characters.");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Passwords must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            errors.Add("Passwords must contain at least one lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Passwords must contain at least one digit.");
        }

        return errors;
    }
}
