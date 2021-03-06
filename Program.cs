using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MinimalApiTest.Models;


const string AllowFrontendOrigin = "_allowFrontendOrigin";

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

builder.Services.AddSqlite<PizzaDb>(connectionString);

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

builder.Services.AddCors(options => 
{
    options.AddPolicy(name: AllowFrontendOrigin,
        config => config.WithOrigins("*"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.UseCors(AllowFrontendOrigin);

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
.Accepts<EditPizzaDto>("application/json")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithName("EditPizza")
.WithTags("Editors");

app.Run();

public record PizzaDto(int Id, string Name, string Description);
public record CreatePizzaDto(string Name);
public record EditPizzaDto(string Name, string Description);