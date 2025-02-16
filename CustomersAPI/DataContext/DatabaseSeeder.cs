using CustomersAPI.Models;

namespace CustomersAPI.DataContext
{
    public static class DatabaseSeeder
    {
        public static void Seed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Customers.AddRange(
                new Customer { Name = "John Doe", Email = "john@example.com" },
                new Customer { Name = "Jane Smith", Email = "jane@example.com" },
                new Customer { Name = "Alice Brown", Email = "alice@example.com" }
            );
            db.SaveChanges();
        }
    }
}
