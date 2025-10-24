using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosAPI.Data;
using PedidosAPI.Models;


namespace PedidosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItensController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItensController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/itens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItens()
        {
            return await _context.Itens.ToListAsync();
        }

        // GET: api/itens/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Itens.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // PUT: api/itens/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, Item itemAtualizado)
        {
            // Verifica se o ID do caminho corresponde ao ID do corpo
            if (id != itemAtualizado.Id)
                return BadRequest("O ID do item não corresponde.");

            // Verifica se o item existe no banco
            var itemExistente = await _context.Itens.FindAsync(id);
            if (itemExistente == null)
                return NotFound($"Item com ID {id} não encontrado.");

            // Atualiza campos específicos
            itemExistente.Produto = itemAtualizado.Produto;
            itemExistente.Quantidade = itemAtualizado.Quantidade;
            itemExistente.PrecoUnitario = itemAtualizado.PrecoUnitario;
            itemExistente.PedidoId = itemAtualizado.PedidoId;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 - Atualizado com sucesso
        }

        // DELETE: api/itens/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Itens.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Itens.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    // OBS.: Ao fazer solicitações GET, não julgue o preço, o gerente ficou maluco...
}