using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MinimalApiTest.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("Pizzas"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PizzaStore API", 
        Description = "Making the pizzas you love!", 
        Version = "v1" 
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var pizzas = new List<PizzaDto> {
    new (1, "Margherita", ""),
    new (2, "Capriciosa", ""),
    new (3, "CzyPsy", ""),
    new (4, "Wegetariańska", ""),
};

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.MapGet("/pizzas/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(pizza);
})
.Produces<PizzaDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetPizza")
.WithTags("Getters");

app.MapGet("/pizzas", async (PizzaDb db) =>
{
    var pizzas = await db.Pizzas.ToListAsync();
    return pizzas.Select(x => new PizzaDto(x.Id, x.Name, x.Description));
})
.Produces<List<PizzaDto>>(StatusCodes.Status200OK)
.WithName("GetAllPizzas")
.WithTags("Getters");

app.MapDelete("/pizzas/{id}", async (int id, PizzaDb db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);

    if (pizza is null) return Results.NoContent();

    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.WithName("DeletePizza");

app.MapPost("/pizza", async (PizzaDb db, CreatePizzaDto pizza) =>
{
    var createdPizza = new Pizza { Name = pizza.Name };

    await db.Pizzas.AddAsync(createdPizza);
    await db.SaveChangesAsync();

    return Results.Created($"/pizzas/{createdPizza.Id}", createdPizza);
})
.Accepts<CreatePizzaDto>("application/json")
.Produces<PizzaDto>(StatusCodes.Status201Created)
.WithName("CreatePizza")
.WithTags("Creators");

app.MapPut("/pizzas/{id}", async (int id, EditPizzaDto pizzaEditDto, PizzaDb db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);

    if (pizza is null) return Results.NotFound();

    pizza.Name = pizzaEditDto.Name;
    pizza.Description = pizzaEditDto.Description;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.Accepts<CreatePizzaDto>("application/json")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithName("EditPizza")
.WithTags("Editors");

app.Run();

public record PizzaDto(int Id, string Name, string Description);
public record CreatePizzaDto(string Name);
public record EditPizzaDto(string Name, string Description);