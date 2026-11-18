namespace Pagamentos.Servicos;

public class Pagamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReservaId { get; set; }
    public decimal Valor { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public StatusPagamento Status { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
    public bool ReservaConfirmada { get; set; }
}
