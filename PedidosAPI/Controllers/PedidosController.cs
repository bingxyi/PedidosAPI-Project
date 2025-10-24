using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosAPI.Data;
using PedidosAPI.Models;


// Utilização de controllers para fazer as HTTP Requests
namespace PedidosAPI.Controllers
{

    // O atributo [ApiController] ativa a validação automática do modelo e 
    // gera respostas 400 automaticamente se os dados não forem válidos. (400 / 404)
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .ToListAsync();
        }

        // GET: api/pedidos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);

            // 404 - Not Found
            if (pedido == null)
                return NotFound();

            return pedido;
        }

        // POST: api/pedidos
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            // Utilização do foreach para vincular corretamente cada item
            foreach (var item in pedido.Itens)
                item.Pedido = pedido;
            pedido.Total = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
        }

        // PUT: api/pedidos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedidoAtualizado)
        {
            // 400 - BadRequest
            if (id != pedidoAtualizado.Id)
                return BadRequest();

            var pedidoExistente = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoExistente == null)
                return NotFound();
            
            pedidoExistente.Cliente = pedidoAtualizado.Cliente;
            pedidoExistente.Itens = pedidoAtualizado.Itens;
            pedidoExistente.Total = pedidoAtualizado.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/pedidos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}