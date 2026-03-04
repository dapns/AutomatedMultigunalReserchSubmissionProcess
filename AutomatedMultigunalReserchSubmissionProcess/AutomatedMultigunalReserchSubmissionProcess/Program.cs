var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
{
    services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    services.AddOpenApi();
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}

