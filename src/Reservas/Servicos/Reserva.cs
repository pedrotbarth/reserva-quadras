namespace Reservas.Servicos;

public enum StatusReserva
{
    AguardandoPagamento,
    Confirmada,
    Cancelada
}

public class Reserva
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public Guid QuadraId { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly HorarioInicio { get; set; }
    public TimeOnly HorarioFim { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.AguardandoPagamento;
    public Guid? PagamentoId { get; set; }
}
