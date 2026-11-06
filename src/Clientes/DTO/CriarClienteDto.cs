using System.ComponentModel.DataAnnotations;

namespace Clientes.DTO;

public class CriarClienteDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Telefone { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}