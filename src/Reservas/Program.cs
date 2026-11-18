using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Reservas;
using Reservas.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Conexão do SQLite não configurada.");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ServReserva>();

foreach (var servico in new[] { "Clientes", "Quadras" })
{
    var url = builder.Configuration[$"{servico}:UrlBase"]
        ?? throw new InvalidOperationException($"URL de {servico} não configurada.");
    builder.Services.AddHttpClient(servico, cliente =>
    {
        cliente.BaseAddress = new Uri(url.TrimEnd('/') + "/");
        cliente.Timeout = TimeSpan.FromSeconds(30);
    });
}

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
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
