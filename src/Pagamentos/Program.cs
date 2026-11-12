using System.Text.Json.Serialization;
using Pagamentos;
using Pagamentos.Servicos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Tira dos logs os ids técnicos de rastreamento (SpanId, TraceId); usamos o nosso CorrelationId.
builder.Logging.Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.None);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Conexão do SQLite não configurada.");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ServPagamento>();

var app = builder.Build();

app.UseMiddleware<CorrelacaoMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
