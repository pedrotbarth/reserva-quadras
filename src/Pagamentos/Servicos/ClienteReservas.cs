using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Pagamentos.DTO;

namespace Pagamentos.Servicos;

// Ãšnico lugar do Pagamentos que conhece o serviÃ§o de Reservas.
public class ClienteReservas(HttpClient http, IHttpContextAccessor acessoContexto, ILogger<ClienteReservas> logger)
{
    // Rotas provisÃ³rias: quando o contrato com o grupo for fechado, basta trocar aqui.
    private const string RotaConsultar = "api/reservas/{0}";
    private const string RotaConfirmar = "api/reservas/{0}/confirmar-pagamento";

    // Status que a reserva precisa ter para poder ser paga.
    public const string StatusAguardandoPagamento = "AguardandoPagamento";

    // Devolve null se a reserva nÃ£o existe. LanÃ§a ReservasIndisponivelException se Reservas nÃ£o responder direito.
    public async Task<ReservaDto?> ConsultarAsync(Guid reservaId)
    {
        using var resposta = await EnviarAsync(HttpMethod.Get, string.Format(RotaConsultar, reservaId));

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!resposta.IsSuccessStatusCode)
            throw new ReservasIndisponivelException();

        try
        {
            return await resposta.Content.ReadFromJsonAsync<ReservaDto>()
                ?? throw new ReservasIndisponivelException();
        }
        catch (JsonException erro)
        {
            logger.LogError(erro, "Reservas devolveu uma resposta que nÃ£o Ã© a esperada para a reserva {ReservaId}", reservaId);
            throw new ReservasIndisponivelException();
        }
    }

    // Devolve true se Reservas confirmou. Nunca lanÃ§a: quem chama decide o que fazer com a falha.
    public async Task<bool> ConfirmarAsync(Guid reservaId, Guid pagamentoId)
    {
        try
        {
            using var resposta = await EnviarAsync(HttpMethod.Patch, string.Format(RotaConfirmar, reservaId), System.Net.Http.Json.JsonContent.Create(new { pagamentoId }));
            return resposta.IsSuccessStatusCode;
        }
        catch (ReservasIndisponivelException)
        {
            return false;
        }
    }

    // Toda chamada passa por aqui: repassa o CorrelationId e mede o tempo.
    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string rota, HttpContent? corpo = null)
    {
        using var requisicao = new HttpRequestMessage(metodo, rota) { Content = corpo };

        if (acessoContexto.HttpContext?.Items[CorrelacaoMiddleware.Cabecalho] is string correlationId)
            requisicao.Headers.Add(CorrelacaoMiddleware.Cabecalho, correlationId);

        var cronometro = Stopwatch.StartNew();

        try
        {
            var resposta = await http.SendAsync(requisicao);

            logger.LogInformation(
                "Chamada a Reservas {Metodo} {Rota} respondeu {Status} em {Tempo} ms",
                metodo, rota, (int)resposta.StatusCode, cronometro.ElapsedMilliseconds);

            return resposta;
        }
        catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException)
        {
            // HttpRequestException: Reservas fora do ar. TaskCanceledException: passou do tempo limite.
            logger.LogError(
                "Chamada a Reservas {Metodo} {Rota} falhou apÃ³s {Tempo} ms: {Erro}",
                metodo, rota, cronometro.ElapsedMilliseconds, erro.Message);

            throw new ReservasIndisponivelException();
        }
    }
}

