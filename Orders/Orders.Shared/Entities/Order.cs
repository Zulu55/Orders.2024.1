using System.ComponentModel.DataAnnotations;
using Orders.Shared.Enums;

namespace Orders.Shared.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        [Display(Name = "Inventario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DateTime Date { get; set; }

        public User? User { get; set; }

        public string? UserId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Comentarios")]
        public string? Remarks { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Líneas")]
        public int Lines => OrderDetails == null || OrderDetails.Count == 0 ? 0 : OrderDetails.Count;

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Cantidad")]
        public float Quantity => OrderDetails == null || OrderDetails.Count == 0 ? 0 : OrderDetails.Sum(sd => sd.Quantity);

        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Display(Name = "Valor")]
        public decimal Value => OrderDetails == null || OrderDetails.Count == 0 ? 0 : OrderDetails.Sum(sd => sd.Value);
    }
}