using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PedidosAPI.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Produto { get; set; }

        [Range(1, 9999)]
        public int Quantidade { get; set; }

        [Range(0.01, 100000)]
        public decimal PrecoUnitario { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantidade * PrecoUnitario;

        // Relação com o pedido
        public int PedidoId { get; set; }

        // Forma que achei para resolver o erro 400 ao tentar fazer POST Request
        [JsonIgnore]    
        public Pedido? Pedido { get; set; }
    }
}