using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PedidosAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Cliente { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; } = 0;

        public List<Item> Itens { get; set; } = new();
    }
}