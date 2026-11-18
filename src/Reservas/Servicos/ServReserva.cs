using Microsoft.EntityFrameworkCore;
using Reservas.DTO;

namespace Reservas.Servicos;

public class HorarioOcupadoException(IReadOnlyList<HorarioSugeridoDto> sugestoes)
    : Exception("A quadra já tem uma reserva nesse horário.")
{
    public IReadOnlyList<HorarioSugeridoDto> Sugestoes { get; } = sugestoes;
}

public class ReservaInvalidaException(string mensagem) : Exception(mensagem) { }

public class ServReserva(DataContext banco, IHttpClientFactory clientesHttp, IHttpContextAccessor acessoContexto)
{
    public async Task<Reserva> CriarAsync(CriarReservaDto dados, CancellationToken ct)
    {
        var clienteId = dados.ClienteId.GetValueOrDefault();
        var quadraId = dados.QuadraId.GetValueOrDefault();
        var data = dados.Data.GetValueOrDefault();
        var inicio = dados.HorarioInicio.GetValueOrDefault();
        var fim = dados.HorarioFim.GetValueOrDefault();

        if (clienteId == Guid.Empty || quadraId == Guid.Empty || fim <= inicio)
            throw new ReservaInvalidaException("Informe cliente, quadra e um horário de fim após o início.");

        var cliente = await ConsultarAsync<ClienteDto>("Clientes", $"api/clientes/{clienteId}", ct);
        if (cliente is not { Ativo: true })
            throw new ReservaInvalidaException("Cliente inexistente ou inativo.");

        var quadra = await ConsultarAsync<QuadraDto>("Quadras", $"api/quadras/{quadraId}", ct);
        if (quadra is not { Ativa: true })
            throw new ReservaInvalidaException("Quadra inexistente ou inativa.");

        if (inicio < quadra.HorarioAbertura || fim > quadra.HorarioFechamento)
            throw new ReservaInvalidaException("O horário solicitado está fora do funcionamento da quadra.");

        await using var transacao = await banco.Database.BeginTransactionAsync(ct);
        var ocupadas = await banco.Reservas.AsNoTracking()
            .Where(reserva => reserva.QuadraId == quadraId && reserva.Data == data &&
                reserva.Status != StatusReserva.Cancelada)
            .ToListAsync(ct);

        if (!HorarioLivre(inicio, fim, ocupadas))
            throw new HorarioOcupadoException(SugerirHorarios(quadra, inicio, fim, ocupadas));

        var reserva = new Reserva
        {
            ClienteId = clienteId,
            QuadraId = quadraId,
            Data = data,
            HorarioInicio = inicio,
            HorarioFim = fim,
            ValorTotal = Math.Round(quadra.ValorPorHora * ((decimal)(fim - inicio).Ticks / TimeSpan.TicksPerHour),
                2, MidpointRounding.AwayFromZero)
        };

        banco.Reservas.Add(reserva);
        await banco.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);
        return reserva;
    }

    private async Task<T?> ConsultarAsync<T>(string servico, string rota, CancellationToken ct) where T : class
    {
        using var cliente = clientesHttp.CreateClient(servico);
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, rota);
        var correlationId = acessoContexto.HttpContext?.Items[CorrelacaoMiddleware.Cabecalho];
        if (correlationId is string id)
            requisicao.Headers.Add(CorrelacaoMiddleware.Cabecalho, id);

        using var resposta = await cliente.SendAsync(requisicao, ct);
        if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new HttpRequestException($"O serviço de {servico} retornou uma resposta vazia.");
    }

    private static bool HorarioLivre(TimeOnly inicio, TimeOnly fim, IEnumerable<Reserva> ocupadas)
    {
        return !ocupadas.Any(reserva => inicio < reserva.HorarioFim && fim > reserva.HorarioInicio);
    }

    private static List<HorarioSugeridoDto> SugerirHorarios(
        QuadraDto quadra, TimeOnly inicio, TimeOnly fim, IReadOnlyList<Reserva> ocupadas)
    {
        var duracao = (fim - inicio).Ticks;
        var primeiroInicio = quadra.HorarioAbertura.Ticks;
        var ultimoInicio = quadra.HorarioFechamento.Ticks - duracao;
        var candidatos = new HashSet<long>();
        for (var horario = primeiroInicio; horario <= ultimoInicio; horario += TimeSpan.TicksPerMinute * 30)
            candidatos.Add(horario);

        candidatos.Add(ultimoInicio);
        foreach (var reserva in ocupadas)
        {
            candidatos.Add(reserva.HorarioFim.Ticks);
            candidatos.Add(reserva.HorarioInicio.Ticks - duracao);
        }

        return candidatos.Where(horario => horario >= primeiroInicio && horario <= ultimoInicio)
            .OrderBy(horario => Math.Abs(horario - inicio.Ticks))
            .ThenBy(horario => horario)
            .Select(horario => new HorarioSugeridoDto(new TimeOnly(horario), new TimeOnly(horario + duracao)))
            .Where(sugestao => HorarioLivre(sugestao.HorarioInicio, sugestao.HorarioFim, ocupadas))
            .Take(3)
            .ToList();
    }
}
