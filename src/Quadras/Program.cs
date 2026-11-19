using Microsoft.EntityFrameworkCore;
using Quadras;
using Quadras.Servicos;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Conexão do SQLite não configurada.");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ServQuadra>();
builder.Services.AddHealthChecks().AddCheck<SqliteHealthCheck>("sqlite");
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.UseUtcTimestamp = true;
});

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    await escopo.ServiceProvider.GetRequiredService<DataContext>().Database.MigrateAsync();
}

app.UseMiddleware<CorrelacaoMiddleware>();
app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();
