using System.ComponentModel.DataAnnotations;

namespace Quadras.DTO;

public class CriarQuadraDto : IValidatableObject
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Modalidade { get; set; } = string.Empty;

    public decimal ValorPorHora { get; set; }

    [Required]
    public TimeOnly? HorarioAbertura { get; set; }

    [Required]
    public TimeOnly? HorarioFechamento { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ValorPorHora <= 0)
        {
            yield return new ValidationResult(
                "O valor por hora deve ser maior que zero.",
                new[] { nameof(ValorPorHora) });
        }

        if (HorarioAbertura is TimeOnly abertura &&
            HorarioFechamento is TimeOnly fechamento &&
            fechamento <= abertura)
        {
            yield return new ValidationResult(
                "O fechamento deve ser depois da abertura.",
                new[] { nameof(HorarioFechamento) });
        }
    }
}