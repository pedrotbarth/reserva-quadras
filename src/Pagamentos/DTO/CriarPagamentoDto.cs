using System.ComponentModel.DataAnnotations;
using Pagamentos.Servicos;

namespace Pagamentos.DTO;

public class CriarPagamentoDto : IValidatableObject
{
    [Required]
    public Guid? ReservaId { get; set; }

    public decimal Valor { get; set; }

    [Required]
    public FormaPagamento? FormaPagamento { get; set; }

    // Pagamento simulado: quando true, o pagamento é registrado como recusado.
    public bool SimularRecusa { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Valor <= 0)
        {
            yield return new ValidationResult(
                "O valor deve ser maior que zero.",
                new[] { nameof(Valor) });
        }
    }
}
