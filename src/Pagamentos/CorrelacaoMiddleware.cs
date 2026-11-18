using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Pagamentos;

// Dá a cada requisição um identificador de correlação, que aparece em todos os logs dela,
// volta na resposta e é repassado aos outros serviços.
public partial class CorrelacaoMiddleware(RequestDelegate proximo, ILogger<CorrelacaoMiddleware> logger)
{
    public const string Cabecalho = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        var recebido = contexto.Request.Headers[Cabecalho].ToString();

        // Só aceita o id de fora se for curto e simples, para ninguém injetar texto falso no log.
        var correlationId = IdValido().IsMatch(recebido) ? recebido : Guid.NewGuid().ToString();

        contexto.Items[Cabecalho] = correlationId;
        contexto.Response.Headers[Cabecalho] = correlationId;

        using (logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
        {
            var cronometro = Stopwatch.StartNew();

            await proximo(contexto);

            logger.LogInformation(
                "{Metodo} {Caminho} respondeu {Status} em {Tempo} ms",
                contexto.Request.Method,
                contexto.Request.Path,
                contexto.Response.StatusCode,
                cronometro.ElapsedMilliseconds);
        }
    }

    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
    private static partial Regex IdValido();
}
