using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomersAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("CustomersDb"));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/customers", async (AppDbContext db, [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] bool desc, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
            {
                var query = db.Customers.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search));

                if (!string.IsNullOrEmpty(sortBy))
                    query = desc ? query.OrderByDescending(e => EF.Property<object>(e, sortBy)) : query.OrderBy(e => EF.Property<object>(e, sortBy));

                var total = await query.CountAsync();
                var customers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return Results.Ok(new { total, customers });
            });

            app.MapPost("/customers", async (AppDbContext db, Customer customer) =>
            {
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
                return Results.Created($"/customers/{customer.Id}", customer);
            });

            app.MapPut("/customers/{id}", async (AppDbContext db, int id, Customer updatedCustomer) =>
            {
                var customer = await db.Customers.FindAsync(id);
                if (customer == null) return Results.NotFound();

                customer.Name = updatedCustomer.Name;
                customer.Email = updatedCustomer.Email;
                await db.SaveChangesAsync();

                return Results.NoContent();
            });

            app.MapDelete("/customers/{id}", async (AppDbContext db, int id) =>
            {
                var customer = await db.Customers.FindAsync(id);
                if (customer == null) return Results.NotFound();

                db.Customers.Remove(customer);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });

            app.Run();
        }
    }

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

}
