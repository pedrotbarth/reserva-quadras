using System.ComponentModel.DataAnnotations;

namespace Reservas.DTO;

public class CriarReservaDto : IValidatableObject
{
    public Guid ClienteId { get; set; }
    public Guid QuadraId { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly HorarioInicio { get; set; }
    public TimeOnly HorarioFim { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ClienteId == Guid.Empty)
            yield return new ValidationResult("Informe o cliente.", new[] { nameof(ClienteId) });

        if (QuadraId == Guid.Empty)
            yield return new ValidationResult("Informe a quadra.", new[] { nameof(QuadraId) });

        if (Data == default)
            yield return new ValidationResult("Informe a data da reserva.", new[] { nameof(Data) });

        if (HorarioFim <= HorarioInicio)
            yield return new ValidationResult(
                "O horário de término deve ser posterior ao de início.",
                new[] { nameof(HorarioFim) });
    }
}