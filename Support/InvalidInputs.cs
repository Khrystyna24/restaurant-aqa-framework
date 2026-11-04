namespace Restaurant_Aqa_Framework.Support;

/// <summary>The form fields a visitor can get wrong, named as they appear to a person.</summary>
public enum InputField
{
    FirstName,
    LastName,
    EmailAddress,
    Password,
    PasswordConfirmation,
}

/// <summary>What is wrong with a value, in the words feature files use.</summary>
public enum InputProblem
{
    TooShort,
    TooLong,
    Empty,
    WithDigits,
    WithAnUnderscore,
    WithoutAtSign,
    WithoutDomain,
    DifferentFromPassword,
}

/// <summary>What is wrong with a location a manager tries to set up.</summary>
public enum LocationProblem
{
    EmptyAddress,
    NegativeCapacity,
}

/// <summary>
/// The concrete bad values behind phrases like "an invalid first name (too long)". Feature files stay
/// in business language; the exact characters and lengths — which are the backend's/frontend's
/// validation limits, not business rules — live here, in one place.
/// </summary>
public static class InvalidInputs
{
    private const int MaxNameLength = 50;
    private const int MaxPasswordLength = 16;
    private const int MaxEmailLength = 254;

    public static string Value(InputField field, InputProblem problem) => (field, problem) switch
    {
        (InputField.FirstName or InputField.LastName, InputProblem.TooShort) => "A",
        (InputField.FirstName or InputField.LastName, InputProblem.TooLong) => new string('a', MaxNameLength + 1),
        (InputField.FirstName or InputField.LastName, InputProblem.WithDigits) => "Ab1",
        (InputField.FirstName or InputField.LastName, InputProblem.WithAnUnderscore) => "Te_st",
        (InputField.EmailAddress, InputProblem.WithoutAtSign) => "not-an-email",
        (InputField.EmailAddress, InputProblem.WithoutDomain) => "someone@nodot",
        (InputField.EmailAddress, InputProblem.TooLong) => new string('a', MaxEmailLength + 1),
        (InputField.Password, InputProblem.TooShort) => "short",
        (InputField.Password, InputProblem.TooLong) => new string('a', MaxPasswordLength + 1),
        (InputField.PasswordConfirmation, InputProblem.DifferentFromPassword) => "Different1!",
        (_, InputProblem.Empty) => "",
        _ => throw new NotSupportedException($"No invalid value is defined for '{field}' that is '{problem}'."),
    };
}
