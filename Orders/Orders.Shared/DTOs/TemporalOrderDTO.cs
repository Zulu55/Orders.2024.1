namespace Orders.Shared.DTOs
{
    public class TemporalOrderDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public float Quantity { get; set; } = 1;

        public string Remarks { get; set; } = string.Empty;
    }
}