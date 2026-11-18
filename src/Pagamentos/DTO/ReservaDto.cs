namespace Pagamentos.DTO;

// O que o serviço de Reservas devolve em GET /api/reservas/{id}. Só os campos que o Pagamentos usa.
public class ReservaDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
