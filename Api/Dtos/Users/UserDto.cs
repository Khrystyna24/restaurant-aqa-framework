namespace Restaurant_Aqa_Framework.Api.Dtos.Users;

/// <summary>Shape returned by GET /users. Mirrors backend Models/Users/UserDto.cs: "Id" is the
/// Cognito sub, surfaced because it's the only stable handle a presumed delete endpoint could key
/// on (there is no Dynamo-backed user table to look up a synthetic id from).</summary>
public sealed record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role);
