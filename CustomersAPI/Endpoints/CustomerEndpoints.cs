using CustomersAPI.DataContext;
using CustomersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CustomersAPI.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void MapRoutes(WebApplication app)
        {
            app.MapGet("/customers", async (AppDbContext db, [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] bool desc, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
            {
                var query = db.Customers.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search));

                if (!string.IsNullOrEmpty(sortBy))
                {
                    var property = typeof(Customer).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        query = desc
                            ? query.OrderByDescending(c => EF.Property<object>(c, property.Name))
                            : query.OrderBy(c => EF.Property<object>(c, property.Name));
                    }
                }

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

                return Results.Ok(customer);
            });

            app.MapDelete("/customers/{id}", async (AppDbContext db, int id) =>
            {
                var customer = await db.Customers.FindAsync(id);
                if (customer == null) return Results.NotFound();

                db.Customers.Remove(customer);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
