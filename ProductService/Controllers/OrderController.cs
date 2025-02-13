using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductService.Datas;
using ProductService.Models;
using ProductService.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly Repository<Order> _orderRepository;
        private readonly Repository<OrderDetail> _orderDetailRepository;
        private readonly Repository<Product> _productRepository;
        private readonly ILogger<OrderController> _logger;
        
        private readonly Publis _publis;
        public OrderController(ILogger<OrderController> logger, MongoDbContext context, Publis publis)
        {
            _logger = logger;
            _context = context;
            _orderRepository = new Repository<Order>(context);
            _productRepository = new Repository<Product>(context);
            _orderDetailRepository = new Repository<OrderDetail>(context);
            _publis = publis;
           
        }
        [HttpGet]
        public async Task<IEnumerable<Order>> GetOrders()
        {
            var result = await _orderRepository.GetAllAsync();
            return result;
        }
        [HttpPost("place-order")]
        public async Task<ActionResult<Order>> PlaceOrder([FromBody] OrderRequest orderRequest)
        {                      
                try
                {
                    var orderId = await _context.GetNextSequenceValue("OrderId");
                    var order = new Order
                    {
                        ObjectId = ObjectId.GenerateNewId(),
                        OrderId = orderId,
                        OrderDate = orderRequest.OrderDate,
                        CusId = orderRequest.CusId,
                        OrderDetailIds = new List<int>()
                    };

                    foreach (var detail in orderRequest.OrderDetails)
                    {
                        var product = await _productRepository.GetByIdAsync(detail.ProductId);
                        if (product == null || product.Quantity < detail.Quantity)//Kiem tra so luong cua don hang so voi so luong san pham
                        {
                            return BadRequest($"Product {detail.ProductId} is not available or insufficient stock.");
                        }

                        var orderDetailId = await _context.GetNextSequenceValue("OrderDetailId");
                        var orderDetail = new OrderDetail
                        {
                            ObjectId = ObjectId.GenerateNewId(),
                            OrderDetailId = orderDetailId,
                            OrderId = order.OrderId,
                            ProductId = detail.ProductId,   
                            Quantity = detail.Quantity,
                            UnitPrice = product.Price,
                            Order = order,
                            Product = product
                        };                                              
                        await _orderDetailRepository.AddAsync(orderDetail);//Cap nhat chi tiet thong tin dong hang
                        order.OrderDetailIds.Add(orderDetail.OrderDetailId);
                    }
                await _orderRepository.AddAsync(order);//Cap nhat don hang vao db
                await _publis.UpdateRequest(orderRequest.OrderDetails);
                await _publis.SendMessageAsync(orderRequest);
               /* await _consumer.UpdateProductQuantity();
                await _consumer.ConsumeMessageAsync();*/
                return Ok(order);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex.Message);   
                    return BadRequest();
                }
           
        }
      
    }
}