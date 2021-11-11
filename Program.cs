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
    new (1, "Margherita", DateTime.UtcNow),
    new (2, "Capriciosa", DateTime.UtcNow),
    new (3, "CzyPsy", DateTime.UtcNow),
    new (4, "Wegetariańska", DateTime.UtcNow),
};

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.MapGet("/", () => "Hello World MAC!");

app.MapGet("/pizzas/{id}", (int id) =>
{
    var pizza = pizzas.SingleOrDefault(x => x.Id == id);
    return pizza is not null ? Results.Ok(pizza) : Results.NotFound();
})
.Produces<PizzaDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetPizza")
.WithTags("Getters");

app.MapGet("/pizzas", (int? page, int? pageSize) =>
{
    if (page.HasValue && pageSize.HasValue)
    {
        return pizzas.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
    }

    return pizzas;
})
.Produces<List<PizzaDto>>(StatusCodes.Status200OK)
.WithName("GetAllPizzas")
.WithTags("Getters");

app.MapDelete("/pizzas/{id}", (int id) =>
{ 
    pizzas.RemoveAll(x => x.Id == id);
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent);

app.MapPost("/pizza", (CreatePizzaDto pizza) =>
{
    var id = pizzas.MaxBy(x => x.Id)?.Id + 1 ?? 1;
    var createdPizza = new PizzaDto(id, pizza.Name, DateTime.UtcNow);

    pizzas.Add(createdPizza);

    return Results.Created($"/pizzas/{id}", createdPizza);
})
.Accepts<CreatePizzaDto>("application/json")
.Produces<PizzaDto>(StatusCodes.Status201Created)
.WithName("CreatePizza")
.WithTags("Creators");

app.MapPut("/pizzas/{id}", (int id, EditPizzaDto pizzaEditDto) =>
{
    var pizzaToEditIndex = pizzas.FindIndex(x => x.Id == id);
    var pizzaToEdit = pizzaToEditIndex > -1
        ? pizzas[pizzaToEditIndex]
        : null;

    if (pizzaToEdit is null)
    {
        return Results.NotFound();
    }

    var editedPizza = pizzaToEdit with { Name = pizzaEditDto.Name };

    pizzas[pizzaToEditIndex] = editedPizza;
    return Results.Ok(editedPizza);
})
.Accepts<CreatePizzaDto>("application/json")
.Produces<PizzaDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("EditPizza")
.WithTags("Editors");

app.Run();

public record PizzaDto(int Id, string Name, DateTime CreatedAt);
public record CreatePizzaDto(string Name);
public record EditPizzaDto(string Name);