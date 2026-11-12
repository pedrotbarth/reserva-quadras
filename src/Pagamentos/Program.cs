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

builder.Services.AddHttpContextAccessor();

// A URL de Reservas muda no Render: vem do appsettings ou da variável de ambiente Reservas__UrlBase.
var urlReservas = builder.Configuration["Reservas:UrlBase"]
    ?? throw new InvalidOperationException("URL do serviço de Reservas não configurada.");
var tempoLimiteReservas = builder.Configuration.GetValue("Reservas:TempoLimiteSegundos", 30);

builder.Services.AddHttpClient<ClienteReservas>(cliente =>
{
    cliente.BaseAddress = new Uri(urlReservas.TrimEnd('/') + "/");
    cliente.Timeout = TimeSpan.FromSeconds(tempoLimiteReservas);
});

var app = builder.Build();

// Cria ou atualiza a tabela ao subir. No Render o banco pode começar vazio a cada reinício.
using (var escopo = app.Services.CreateScope())
{
    escopo.ServiceProvider.GetRequiredService<DataContext>().Database.Migrate();
}

app.UseMiddleware<CorrelacaoMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // No Render o HTTPS é feito antes de chegar ao serviço, que recebe HTTP.
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
