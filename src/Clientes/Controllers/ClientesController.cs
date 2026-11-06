using Clientes.DTO;
using Clientes.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(ServCliente servico) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Cliente>> Cadastrar(CriarClienteDto dados)
    {
        var cliente = await servico.CadastrarAsync(dados);
        return CreatedAtAction(nameof(BuscarPorId), new { id = cliente.Id }, cliente);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Cliente>> BuscarPorId(Guid id)
    {
        var cliente = await servico.BuscarPorIdAsync(id);

        if (cliente is null)
            return NotFound();

        return Ok(cliente);
    }

    [HttpGet]
    public async Task<ActionResult<List<Cliente>>> Listar()
    {
        return Ok(await servico.ListarAsync());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Cliente>> Editar(Guid id, EditarClienteDto dados)
    {
        var cliente = await servico.EditarAsync(id, dados);

        if (cliente is null)
            return NotFound();

        return Ok(cliente);
    }
}