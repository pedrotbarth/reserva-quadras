using System.ComponentModel.DataAnnotations;

namespace Clientes.DTO;

public class EditarClienteDto : CriarClienteDto
{
    [Required]
    public bool? Ativo { get; set; }
}