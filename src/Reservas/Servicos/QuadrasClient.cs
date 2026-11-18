using System.Net;
using System.Net.Http.Json;
using Reservas.DTO;

namespace Reservas.Servicos;

public class QuadrasClient(HttpClient http)
{
    public async Task<QuadraConsultadaDto?> BuscarPorIdAsync(
        Guid id, CancellationToken cancellationToken)
    {
        using var resposta = await http.GetAsync($"api/quadras/{id}", cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        resposta.EnsureSuccessStatusCode();

        return await resposta.Content.ReadFromJsonAsync<QuadraConsultadaDto>(
            cancellationToken: cancellationToken);
    }
}