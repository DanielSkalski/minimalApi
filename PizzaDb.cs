using Microsoft.EntityFrameworkCore;

namespace MinimalApiTest.Models;

class PizzaDb : DbContext
{
    public PizzaDb(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Pizza> Pizzas { get; set; }
}