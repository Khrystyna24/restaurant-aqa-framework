namespace Restaurant_Aqa_Framework.Api.Dtos.Auth;

/// <summary>POST /register always provisions a Customer-role account — there is no self-service
/// Waiter/Admin signup. Password: 8-16 chars, upper+lower+digit+special.</summary>
public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword);
