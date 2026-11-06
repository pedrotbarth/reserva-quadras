using System.ComponentModel.DataAnnotations;

namespace Quadras.DTO;

public class EditarQuadraDto : CriarQuadraDto
{
    [Required]
    public bool? Ativa { get; set; }
}