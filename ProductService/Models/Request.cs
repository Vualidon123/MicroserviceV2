namespace ProductService.Models
{
    public class OrderRequest
    {
        public DateTime OrderDate { get; set; }
        public int CusId { get; set; }
        public List<OrderDetailRequest> OrderDetails { get; set; }
    }

    public class OrderDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class Remain()
    {
        public int ProductId { get; set; }
        public int remain { get; set; }
    }

}