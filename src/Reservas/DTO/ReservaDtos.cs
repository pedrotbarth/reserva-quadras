using System.ComponentModel.DataAnnotations;

namespace Reservas.DTO;

public record CriarReservaDto(
    [Required] Guid? ClienteId,
    [Required] Guid? QuadraId,
    [Required] DateOnly? Data,
    [Required] TimeOnly? HorarioInicio,
    [Required] TimeOnly? HorarioFim);

public record ConfirmarPagamentoDto([Required] Guid? PagamentoId);
public record HorarioSugeridoDto(TimeOnly HorarioInicio, TimeOnly HorarioFim);

public record ClienteDto(bool Ativo);
public record QuadraDto(bool Ativa, decimal ValorPorHora, TimeOnly HorarioAbertura, TimeOnly HorarioFechamento);
