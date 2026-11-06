namespace Quadras.Servicos;

public class Quadra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public decimal ValorPorHora { get; set; }
    public TimeOnly HorarioAbertura { get; set; }
    public TimeOnly HorarioFechamento { get; set; }
    public bool Ativa { get; set; } = true;
}