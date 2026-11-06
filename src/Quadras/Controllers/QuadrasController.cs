using Microsoft.AspNetCore.Mvc;
using Quadras.DTO;
using Quadras.Servicos;

namespace Quadras.Controllers;

[ApiController]
[Route("api/quadras")]
public class QuadrasController(ServQuadra servico) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Quadra>> Cadastrar(CriarQuadraDto dados)
    {
        var quadra = await servico.CadastrarAsync(dados);
        return CreatedAtAction(nameof(BuscarPorId), new { id = quadra.Id }, quadra);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Quadra>> BuscarPorId(Guid id)
    {
        var quadra = await servico.BuscarPorIdAsync(id);

        if (quadra is null)
            return NotFound();

        return Ok(quadra);
    }
    [HttpGet]
    public async Task<ActionResult<List<Quadra>>> Listar()
    {
        return Ok(await servico.ListarAsync());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Quadra>> Editar(Guid id, EditarQuadraDto dados)
    {
        var quadra = await servico.EditarAsync(id, dados);

        if (quadra is null)
            return NotFound();

        return Ok(quadra);
    }
}