using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Order
{
    [BsonId]
    public ObjectId ObjectId { get; set; }
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int CusId { get; set; }
    public List<int> OrderDetailIds { get; set; } // Changed to store OrderDetailId
}

public class Product
{
    [BsonId]
    public ObjectId ObjectId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } // New property for stock management
}

public class OrderDetail
{
    [BsonId]
    public ObjectId ObjectId { get; set; }
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public virtual Order Order { get; set; }
    public virtual Product Product { get; set; }
}

public class Sequence
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
}
