using System.Globalization;
using Bogus;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.Feedback;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Api.Dtos.Tables;

namespace Restaurant_Aqa_Framework.Api.Seeding;

/// <summary>
/// Faker-backed builders for seeding the shared live environment. Every visible field (name,
/// address, comment, etc.) is prefixed "AQA-Sample-" so seeded records stay recognizable even
/// where no delete endpoint exists yet (Dishes, Tables) and <see cref="CleanupScope"/> can only
/// be best-effort there.
/// </summary>
public static class SampleDataFactory
{
    public const string Prefix = "AQA-Sample-";

    // Bogus' Faker isn't thread-safe and scenarios run in parallel, so each thread gets its own.
    [ThreadStatic]
    private static Faker? _faker;

    private static Faker Faker => _faker ??= new Faker();

    // Building blocks of a valid registration, also used on their own by the feature-file value macros.
    public static string UniqueEmail() => $"aqa.sample.{Guid.NewGuid():N}@example.com";

    public static string ValidPassword() => $"Aqa-{Faker.Random.AlphaNumeric(8)}1!";

    // Bogus first names can contain characters the backend rejects (it allows Latin letters, - and '),
    // so keep only those.
    public static string FirstName() =>
        Prefix + new string(Faker.Name.FirstName().Where(c => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '-' or '\'').ToArray());

    public static RegisterRequest RegisterRequest()
    {
        var password = ValidPassword();
        return new RegisterRequest(
            FirstName: FirstName(),
            LastName: Faker.Name.LastName().Replace(" ", "-", StringComparison.Ordinal),
            Email: UniqueEmail(),
            Password: password,
            ConfirmPassword: password);
    }

    public static CreateLocationRequest CreateLocationRequest() =>
        new(
            Address: Prefix + Faker.Address.FullAddress(),
            Description: Faker.Lorem.Sentence(),
            TotalCapacity: Faker.Random.Int(20, 200),
            AverageOccupancy: Faker.Random.Int(0, 100).ToString(CultureInfo.InvariantCulture));

    public static CreateDishRequest CreateDishRequest(
        IReadOnlyList<string> locationIds, DishType dishType = DishType.MainCourse) =>
        new(
            Name: Prefix + Faker.Commerce.ProductName(),
            Description: Faker.Lorem.Sentence(),
            DishType: dishType,
            Price: Faker.Random.Decimal(5, 50),
            Weight: $"{Faker.Random.Int(100, 500)}g",
            Calories: Faker.Random.Int(100, 900).ToString(CultureInfo.InvariantCulture),
            Proteins: Faker.Random.Int(1, 50).ToString(CultureInfo.InvariantCulture),
            Fats: Faker.Random.Int(1, 50).ToString(CultureInfo.InvariantCulture),
            Carbohydrates: Faker.Random.Int(1, 100).ToString(CultureInfo.InvariantCulture),
            Vitamins: "A, C",
            LocationIds: locationIds,
            State: DishState.Available);

    public static CreateTableRequest CreateTableRequest(string locationId, int? capacity = null) =>
        new(
            LocationId: locationId,
            Capacity: capacity ?? Faker.Random.Int(2, 8),
            TableNumber: Faker.Random.Int(100, 999));

    public static CreateReservationRequest CreateReservationRequest(
        string tableId, DateTime startTime, int guestCount = 2) =>
        new(
            TableId: tableId,
            StartTime: startTime,
            EndTime: startTime.AddHours(1),
            GuestCount: guestCount);

    public static CreateFeedbackRequest CreateFeedbackRequest(
        string reservationId, int rating = 5, FeedbackType type = FeedbackType.Service) =>
        new(
            ReservationId: reservationId,
            Rating: rating,
            Type: type,
            Comment: Prefix + Faker.Lorem.Sentence());
}
