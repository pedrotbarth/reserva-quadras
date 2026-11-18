using System.Net;
using System.Net.Http.Json;
using Reservas.DTO;

namespace Reservas.Servicos;

public class ClientesClient(HttpClient http)
{
    public async Task<ClienteConsultadoDto?> BuscarPorIdAsync(
        Guid id, CancellationToken cancellationToken)
    {
        using var resposta = await http.GetAsync($"api/clientes/{id}", cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        resposta.EnsureSuccessStatusCode();

        return await resposta.Content.ReadFromJsonAsync<ClienteConsultadoDto>(
            cancellationToken: cancellationToken);
    }
}