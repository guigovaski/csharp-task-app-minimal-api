using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TaskAppContext>(options =>
    options.UseInMemoryDatabase("taskdb")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tasks", async (TaskAppContext ctx) => await ctx.Tasks.ToListAsync());

app.MapGet("/tasks/{id:int}", async (int id, TaskAppContext ctx) =>
    await ctx.Tasks.FindAsync(id) is Task task ? Results.Ok(task) : Results.NotFound()
);

app.MapGet("/tasks/done", async (TaskAppContext ctx) => await ctx.Tasks.Where(c => c.IsDone).ToListAsync());

app.MapPost("/tasks", async (Task task, TaskAppContext ctx) =>
{
    var newTask = ctx.Tasks.Add(task);
    await ctx.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", newTask);
});

app.MapPut("/tasks/{id:int}", async (int id, Task task, TaskAppContext ctx) =>
{
    var t = await ctx.Tasks.FindAsync(id);

    if (t is null) return Results.NotFound();

    t.Name = task.Name;
    t.IsDone = task.IsDone;

    await ctx.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tasks/{id:int}", async (int id, TaskAppContext ctx) =>
{
    var task = await ctx.Tasks.FindAsync(id);

    if (task is null) return Results.NotFound();

    ctx.Tasks.Remove(task);
    await ctx.SaveChangesAsync();
    return Results.Ok();
});

app.UseHttpsRedirection();
app.Run();

class Task
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}

class TaskAppContext : DbContext
{
    public TaskAppContext(DbContextOptions<TaskAppContext> options) : base(options)
    {
    }

    public DbSet<Task> Tasks => Set<Task>();
}

