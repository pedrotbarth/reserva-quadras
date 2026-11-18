namespace Reservas.DTO;

public record ClienteConsultadoDto(Guid Id, bool Ativo);

public record QuadraConsultadaDto(
    Guid Id,
    bool Ativa,
    decimal ValorPorHora,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento);