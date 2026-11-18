namespace Pagamentos.DTO;

// O que o serviÃ§o de Reservas devolve em GET /api/reservas/{id}. SÃ³ os campos que o Pagamentos usa.
public class ReservaDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
}

