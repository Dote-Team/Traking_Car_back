using Bogus;
using Microsoft.EntityFrameworkCore;
using TrakingCar.Data;
using TrakingCar.Models;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Locations.AnyAsync() || await context.Ownerships.AnyAsync() || await context.Cars.AnyAsync())
            return;

        var now = DateTime.Now;

        // 🔹 توليد مواقع (Locations) بالعربية
        var locationFaker = new Faker<Location>("ar")
            .RuleFor(l => l.Id, f => Guid.NewGuid())
            .RuleFor(l => l.Name, f => f.Address.City())
            .RuleFor(l => l.Detailes, f => f.Lorem.Sentence())
            .RuleFor(l => l.LocationName, f => f.Address.FullAddress())
            .RuleFor(l => l.CreatedAt, f => now)
            .RuleFor(l => l.UpdatedAt, f => now);

        var locations = locationFaker.Generate(100);
        await context.Locations.AddRangeAsync(locations);

        // 🔹 توليد ملاك (Ownerships) بالعربية
        var ownershipFaker = new Faker<Ownership>("ar")
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.Name, f => f.Person.FullName)
            .RuleFor(o => o.Detailes, f => f.Lorem.Sentence())
            .RuleFor(o => o.LocationName, f => f.Address.City())
            .RuleFor(o => o.CreatedAt, f => now)
            .RuleFor(o => o.UpdatedAt, f => now);

        var ownerships = ownershipFaker.Generate(100);
        await context.Ownerships.AddRangeAsync(ownerships);

        await context.SaveChangesAsync();

        // 🔹 توليد سيارات (Cars) بالعربية
        var carFaker = new Faker<Car>("ar")
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.CarType, f => f.Vehicle.Type())
            .RuleFor(c => c.ChassisNumber, f => f.Vehicle.Vin())
            .RuleFor(c => c.CarNumber, f => $"{f.Random.String2(2, "أبتثجحخ")}-{f.Random.Number(1000, 9999)}")
            .RuleFor(c => c.Status, f => f.PickRandom("نشط", "غير نشط", "صيانة"))
            .RuleFor(c => c.BodyCondition, f => f.PickRandom("ممتاز", "جيد", "متضرر"))
            .RuleFor(c => c.ReceiptDate, f => f.Date.Past(3))
            .RuleFor(c => c.Note, f => f.Lorem.Sentence())
            .RuleFor(c => c.TrackingCode, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.CreatedAt, f => now)
            .RuleFor(c => c.UpdatedAt, f => now)
            .RuleFor(c => c.LocationId, f => f.PickRandom(locations).Id)
            .RuleFor(c => c.OwnershipId, f => f.PickRandom(ownerships).Id);

        var cars = carFaker.Generate(1000);
        await context.Cars.AddRangeAsync(cars);

        await context.SaveChangesAsync();
    }
}
